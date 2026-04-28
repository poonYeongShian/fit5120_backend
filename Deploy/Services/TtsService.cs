using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using Deploy.DTOs;
using Deploy.Interfaces;
using Deploy.Models;

namespace Deploy.Services;

public class TtsService : ITtsService
{
    private const string DefaultVoiceId = "RaFzMbMIfqBcIurH6XF9";
    private const string DefaultModelId = "eleven_multilingual_v2";
    private const string DefaultBaseUrl = "https://api.elevenlabs.io";
    private const string DefaultMimeType = "audio/mpeg";

    private readonly ITtsRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TtsService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public TtsService(
        ITtsRepository repository,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<TtsService> logger)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<GenerateTtsAudioResponseDto> GenerateOrGetAudioAsync(GenerateTtsAudioRequestDto request, string audioUrlBase)
    {
        var text = request.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required.");

        var voiceId = request.VoiceId?.Trim();
        if (string.IsNullOrWhiteSpace(voiceId))
            voiceId = _configuration["Tts:DefaultVoice"] ?? DefaultVoiceId;

        var modelId = request.ModelId?.Trim();
        if (string.IsNullOrWhiteSpace(modelId))
            modelId = _configuration["Tts:ModelId"] ?? DefaultModelId;

        var speed = request.Speed ?? 1.0m;
        if (speed <= 0m)
            throw new ArgumentException("Speed must be greater than 0.");

        var textHash = ComputeHash(text, voiceId, modelId, speed);
        _logger.LogInformation(
            "TTS generate requested. TextLength={TextLength}, VoiceId={VoiceId}, ModelId={ModelId}, Speed={Speed}, TextHash={TextHash}",
            text.Length,
            voiceId,
            modelId,
            speed,
            textHash);

        var cached = await _repository.GetByKeyAsync(textHash, voiceId, modelId);
        if (cached is not null)
        {
            _logger.LogInformation("TTS cache hit for TextHash={TextHash}", textHash);
            return new GenerateTtsAudioResponseDto
            {
                TextHash = textHash,
                VoiceId = voiceId,
                ModelId = modelId,
                Speed = cached.Speed,
                MimeType = cached.MimeType,
                Cached = true,
                AudioUrl = $"{audioUrlBase.TrimEnd('/')}/{textHash}",
                Timings = ParseTimings(cached.TimingsJson)
            };
        }

        _logger.LogInformation("TTS cache miss for TextHash={TextHash}; calling ElevenLabs", textHash);

        var apiKey = _configuration["Tts:ElevenLabsApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Missing ElevenLabs API key in configuration.");

        var baseUrl = _configuration["Tts:BaseUrl"] ?? DefaultBaseUrl;

        var generated = await CallElevenLabsAsync(baseUrl, apiKey, text, voiceId, modelId, speed);
        var timingsJson = JsonSerializer.Serialize(generated.Timings, _jsonOptions);
        _logger.LogInformation(
            "ElevenLabs returned audio for TextHash={TextHash}. AudioBytes={AudioBytes}, Timings={TimingCount}",
            textHash,
            generated.AudioBytes.Length,
            generated.Timings.Count);

        var entry = new TtsCacheEntry
        {
            TextHash = textHash,
            Text = text,
            VoiceId = voiceId,
            ModelId = modelId,
            Speed = speed,
            MimeType = DefaultMimeType,
            AudioContent = generated.AudioBytes,
            TimingsJson = timingsJson
        };

        await _repository.SaveAsync(entry);
        _logger.LogInformation("Saved TTS cache entry for TextHash={TextHash}", textHash);

        return new GenerateTtsAudioResponseDto
        {
            TextHash = textHash,
            VoiceId = voiceId,
            ModelId = modelId,
            Speed = speed,
            MimeType = DefaultMimeType,
            Cached = false,
            AudioUrl = $"{audioUrlBase.TrimEnd('/')}/{textHash}",
            Timings = generated.Timings
        };
    }

    public async Task<(byte[] AudioBytes, string MimeType)?> GetAudioByTextHashAsync(string textHash)
    {
        _logger.LogInformation("Fetching audio by TextHash={TextHash}", textHash);
        var entry = await _repository.GetByTextHashAsync(textHash);
        if (entry is null)
        {
            _logger.LogWarning("No cached audio found for TextHash={TextHash}", textHash);
            return null;
        }

        return (entry.AudioContent, entry.MimeType);
    }

    private async Task<TtsGeneratedAudio> CallElevenLabsAsync(string baseUrl, string apiKey, string text, string voiceId, string modelId, decimal speed)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{baseUrl.TrimEnd('/')}/v1/text-to-speech/{voiceId}/with-timestamps";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("xi-api-key", apiKey);

        var body = JsonSerializer.Serialize(new
        {
            text,
            model_id = modelId,
            voice_settings = new
            {
                speed
            }
        });
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request);
        var bytes = await response.Content.ReadAsByteArrayAsync();

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = Encoding.UTF8.GetString(bytes);
            _logger.LogError(
                "ElevenLabs request failed. StatusCode={StatusCode}, VoiceId={VoiceId}, ModelId={ModelId}, Response={Response}",
                (int)response.StatusCode,
                voiceId,
                modelId,
                errorBody);
            throw new InvalidOperationException($"ElevenLabs request failed ({(int)response.StatusCode}): {errorBody}");
        }

        var payload = JsonSerializer.Deserialize<ElevenLabsTimestampResponse>(bytes, _jsonOptions)
                      ?? throw new InvalidOperationException("Invalid ElevenLabs response payload.");

        if (string.IsNullOrWhiteSpace(payload.AudioBase64))
            throw new InvalidOperationException("ElevenLabs response did not include audio.");

        var timings = ToWordTimings(payload.Alignment);

        return new TtsGeneratedAudio
        {
            AudioBytes = Convert.FromBase64String(payload.AudioBase64),
            Timings = timings
        };
    }

    private static string ComputeHash(string text, string voiceId, string modelId, decimal speed)
    {
        var normalizedSpeed = speed.ToString("0.##", CultureInfo.InvariantCulture);
        var raw = $"{text}|{voiceId}|{modelId}|{normalizedSpeed}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private IReadOnlyList<TtsWordTimingDto> ParseTimings(string? timingsJson)
    {
        if (string.IsNullOrWhiteSpace(timingsJson))
            return Array.Empty<TtsWordTimingDto>();

        var timings = JsonSerializer.Deserialize<List<TtsWordTimingDto>>(timingsJson, _jsonOptions);
        if (timings is null)
            return Array.Empty<TtsWordTimingDto>();

        return timings;
    }

    private sealed class TtsGeneratedAudio
    {
        public byte[] AudioBytes { get; set; } = Array.Empty<byte>();
        public IReadOnlyList<TtsWordTimingDto> Timings { get; set; } = Array.Empty<TtsWordTimingDto>();
    }

    private sealed class ElevenLabsTimestampResponse
    {
        [JsonPropertyName("audio_base64")]
        public string AudioBase64 { get; set; } = string.Empty;

        [JsonPropertyName("alignment")]
        public ElevenLabsAlignment? Alignment { get; set; }
    }

    private sealed class ElevenLabsAlignment
    {
        [JsonPropertyName("characters")]
        public List<string> Characters { get; set; } = new();

        [JsonPropertyName("character_start_times_seconds")]
        public List<decimal> CharacterStartTimesSeconds { get; set; } = new();

        [JsonPropertyName("character_end_times_seconds")]
        public List<decimal> CharacterEndTimesSeconds { get; set; } = new();
    }

    private static IReadOnlyList<TtsWordTimingDto> ToWordTimings(ElevenLabsAlignment? alignment)
    {
        if (alignment is null || alignment.Characters.Count == 0)
            return Array.Empty<TtsWordTimingDto>();

        var results = new List<TtsWordTimingDto>();
        var currentWord = new StringBuilder();
        decimal? currentStart = null;
        decimal currentEnd = 0m;

        for (var i = 0; i < alignment.Characters.Count; i++)
        {
            var ch = alignment.Characters[i];
            var start = alignment.CharacterStartTimesSeconds.ElementAtOrDefault(i);
            var end = alignment.CharacterEndTimesSeconds.ElementAtOrDefault(i);

            if (string.IsNullOrWhiteSpace(ch))
            {
                if (currentWord.Length > 0 && currentStart.HasValue)
                {
                    results.Add(new TtsWordTimingDto
                    {
                        Word = currentWord.ToString(),
                        Start = currentStart.Value,
                        End = currentEnd
                    });
                    currentWord.Clear();
                    currentStart = null;
                    currentEnd = 0m;
                }

                continue;
            }

            if (!currentStart.HasValue)
                currentStart = start;

            currentWord.Append(ch);
            currentEnd = end;
        }

        if (currentWord.Length > 0 && currentStart.HasValue)
        {
            results.Add(new TtsWordTimingDto
            {
                Word = currentWord.ToString(),
                Start = currentStart.Value,
                End = currentEnd
            });
        }

        return results;
    }
}
