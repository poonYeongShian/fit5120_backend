using Dapper;
using Deploy.Interfaces;
using Deploy.Models;
using Npgsql;

namespace Deploy.Repositories;

public class TtsRepository : ITtsRepository
{
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<TtsRepository> _logger;

    public TtsRepository(NpgsqlConnection connection, ILogger<TtsRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<TtsCacheEntry?> GetByKeyAsync(string textHash, string voiceId, string modelId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();
        await EnsureTableAsync(connection);

        const string sql =
            """
            SELECT text_hash    AS TextHash,
                   text         AS Text,
                   voice_id     AS VoiceId,
                   model_id     AS ModelId,
                   speed        AS Speed,
                   mime_type    AS MimeType,
                   audio_content AS AudioContent,
                   timings_json::text AS TimingsJson
            FROM   public.tts_audio_cache
            WHERE  text_hash = @TextHash
              AND  voice_id = @VoiceId
              AND  model_id = @ModelId
            """;

        try
        {
            return await connection.QuerySingleOrDefaultAsync<TtsCacheEntry>(
                sql,
                new { TextHash = textHash, VoiceId = voiceId, ModelId = modelId });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "TTS cache lookup failed. TextHash={TextHash}, VoiceId={VoiceId}, ModelId={ModelId}, Sql={Sql}",
                textHash,
                voiceId,
                modelId,
                sql);
            throw;
        }
    }

    public async Task<TtsCacheEntry?> GetByTextHashAsync(string textHash)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();
        await EnsureTableAsync(connection);

        const string sql =
            """
            SELECT text_hash    AS TextHash,
                   text         AS Text,
                   voice_id     AS VoiceId,
                   model_id     AS ModelId,
                   speed        AS Speed,
                   mime_type    AS MimeType,
                   audio_content AS AudioContent,
                   timings_json::text AS TimingsJson
            FROM   public.tts_audio_cache
            WHERE  text_hash = @TextHash
            """;

        try
        {
            return await connection.QuerySingleOrDefaultAsync<TtsCacheEntry>(
                sql,
                new { TextHash = textHash });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TTS audio fetch failed. TextHash={TextHash}, Sql={Sql}", textHash, sql);
            throw;
        }
    }

    public async Task SaveAsync(TtsCacheEntry entry)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();
        await EnsureTableAsync(connection);

        const string sql =
            """
            INSERT INTO public.tts_audio_cache (
                text_hash,
                text,
                voice_id,
                model_id,
                speed,
                mime_type,
                audio_content,
                timings_json,
                created_at,
                updated_at
            )
            VALUES (
                @TextHash,
                @Text,
                @VoiceId,
                @ModelId,
                @Speed,
                @MimeType,
                @AudioContent,
                CAST(@TimingsJson AS jsonb),
                NOW(),
                NOW()
            )
            ON CONFLICT (text_hash, voice_id, model_id)
            DO UPDATE SET
                text = EXCLUDED.text,
                mime_type = EXCLUDED.mime_type,
                audio_content = EXCLUDED.audio_content,
                timings_json = EXCLUDED.timings_json,
                updated_at = NOW()
            """;

        try
        {
            await connection.ExecuteAsync(sql, entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "TTS cache save failed. TextHash={TextHash}, VoiceId={VoiceId}, ModelId={ModelId}, Sql={Sql}",
                entry.TextHash,
                entry.VoiceId,
                entry.ModelId,
                sql);
            throw;
        }
    }

    private static async Task EnsureTableAsync(NpgsqlConnection connection)
    {
        await connection.ExecuteAsync(
            """
            CREATE TABLE IF NOT EXISTS public.tts_audio_cache (
                text_hash     TEXT NOT NULL,
                text          TEXT NOT NULL,
                voice_id      TEXT NOT NULL,
                model_id      TEXT NOT NULL,
                speed         NUMERIC(5,2) NOT NULL DEFAULT 1.0,
                mime_type     TEXT NOT NULL DEFAULT 'audio/mpeg',
                audio_content BYTEA NOT NULL,
                timings_json  JSONB NOT NULL DEFAULT '[]'::jsonb,
                created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE (text_hash, voice_id, model_id)
            );

            ALTER TABLE public.tts_audio_cache
            ADD COLUMN IF NOT EXISTS speed NUMERIC(5,2) NOT NULL DEFAULT 1.0;

            ALTER TABLE public.tts_audio_cache
            ADD COLUMN IF NOT EXISTS timings_json JSONB NOT NULL DEFAULT '[]'::jsonb;
            """);
    }
}
