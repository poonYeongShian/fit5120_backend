using System.Net;
using System.Net.Http.Json;
using Deploy.DTOs;
using Deploy.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deploy.Tests;

public class ApiIntegrationTests
{
    [Fact]
    public async Task CreateProfile_Returns201_WithProfileCodeAndPin()
    {
        using var harness = TestHarness.Create();

        var created = await CreateProfileAsync(harness.Client, "Ava", "1234");

        Assert.False(string.IsNullOrWhiteSpace(created.ProfileCode));
        Assert.Equal("1234", created.Pin);
        Assert.Equal("Ava", created.DisplayName);
        Assert.False(string.IsNullOrWhiteSpace(created.SessionToken));
    }

    [Fact]
    public async Task RestoreProfile_WithCodeAndPin_Returns200_WithCorrectUserData()
    {
        using var harness = TestHarness.Create();
        var created = await CreateProfileAsync(harness.Client, "Noah", "4321");

        var restoreResponse = await harness.Client.PostAsJsonAsync("/api/v1/profiles/restore", new
        {
            profileCode = created.ProfileCode,
            pin = created.Pin,
            deviceInfo = "new-device"
        });

        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        var restored = await restoreResponse.Content.ReadFromJsonAsync<RestoreProfileResponseDto>();
        Assert.NotNull(restored);
        Assert.Equal(created.ProfileId, restored.ProfileId);
        Assert.Equal(created.ProfileCode, restored.ProfileCode);
        Assert.Equal("Noah", restored.DisplayName);
        Assert.False(string.IsNullOrWhiteSpace(restored.SessionToken));
    }

    [Fact]
    public async Task AutoLogin_WithSessionToken_Returns200()
    {
        using var harness = TestHarness.Create();
        var created = await CreateProfileAsync(harness.Client, "Liam", "5555");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/profiles");
        request.Headers.Add("X-Session-Token", created.SessionToken);

        var response = await harness.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<ProfileAutoLoginDto>();
        Assert.NotNull(profile);
        Assert.Equal(created.ProfileId, profile.ProfileId);
    }

    [Fact]
    public async Task AutoLogin_WithoutSessionToken_Returns401()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/profiles");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_DeletesSession_AndSubsequentAutoLoginFails()
    {
        using var harness = TestHarness.Create();
        var created = await CreateProfileAsync(harness.Client, "Mia", "7777");

        using var logout = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/profiles/session");
        logout.Headers.Add("X-Session-Token", created.SessionToken);

        var logoutResponse = await harness.Client.SendAsync(logout);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        using var autologin = new HttpRequestMessage(HttpMethod.Get, "/api/v1/profiles");
        autologin.Headers.Add("X-Session-Token", created.SessionToken);
        var autoLoginResponse = await harness.Client.SendAsync(autologin);
        Assert.Equal(HttpStatusCode.Unauthorized, autoLoginResponse.StatusCode);
    }

    [Fact]
    public async Task GetAnimals_Returns200_WithArray()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/animals");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var animals = await response.Content.ReadFromJsonAsync<List<AnimalCardDto>>();
        Assert.NotNull(animals);
        Assert.NotEmpty(animals);
    }

    [Fact]
    public async Task GetAnimals_WithClassFilter_ReturnsOnlyMatchingClass()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/animals?animalClass=Reptiles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var animals = await response.Content.ReadFromJsonAsync<List<AnimalCardDto>>();
        Assert.NotNull(animals);
        Assert.NotEmpty(animals);
        Assert.All(animals, animal => Assert.Equal("Reptiles", animal.AnimalClass));
    }

    [Fact]
    public async Task GetAnimals_WithInvalidClass_Returns400()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/animals?animalClass=Aliens");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAnimalById_Returns200_ForKnownId()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/animals/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var animal = await response.Content.ReadFromJsonAsync<AnimalCardDetailDto>();
        Assert.NotNull(animal);
        Assert.Equal("Koala", animal.CommonName);
    }

    [Fact]
    public async Task GetAnimalById_Returns404_ForUnknownId()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/animals/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAnimalOccurrences_Returns200_ForKnownId()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/animals/1/occurrence");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var occurrences = await response.Content.ReadFromJsonAsync<List<AnimalOccurrenceDto>>();
        Assert.NotNull(occurrences);
        Assert.NotEmpty(occurrences);
    }

    [Fact]
    public async Task GetAnimalOccurrences_Returns404_ForUnknownId()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/animals/999/occurrence");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllQuizzes_Returns200_WithArray()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/quizzes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var quizzes = await response.Content.ReadFromJsonAsync<List<QuizDto>>();
        Assert.NotNull(quizzes);
        Assert.NotEmpty(quizzes);
    }

    [Fact]
    public async Task GetRandomQuizQuestions_Returns200()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/quizzes/questions/random?count=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var questions = await response.Content.ReadFromJsonAsync<List<QuizQuestionDto>>();
        Assert.NotNull(questions);
        Assert.Equal(2, questions.Count);
    }

    [Fact]
    public async Task GetRandomQuizQuestions_WithInvalidCount_Returns400()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/quizzes/questions/random?count=0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRandomQuizQuestionsByAnimal_Returns200()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/quizzes/animals/2/questions?count=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var questions = await response.Content.ReadFromJsonAsync<List<QuizQuestionDto>>();
        Assert.NotNull(questions);
        Assert.NotEmpty(questions);
        Assert.All(questions, question => Assert.Equal(2, question.AnimalId));
    }

    [Fact]
    public async Task GetQuizQuestions_Returns200_ForKnownQuizId()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/quizzes/1/questions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var questions = await response.Content.ReadFromJsonAsync<List<QuizQuestionDto>>();
        Assert.NotNull(questions);
        Assert.NotEmpty(questions);
    }

    [Fact]
    public async Task GetQuizQuestions_Returns404_ForUnknownQuizId()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/quizzes/999/questions");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SaveQuizProgress_ValidCompletion_Returns200_AndIncreasesTotalPoints()
    {
        using var harness = TestHarness.Create();
        var created = await CreateProfileAsync(harness.Client, "Lily", "2468");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/quizzes/save-progress")
        {
            Content = JsonContent.Create(new
            {
                totalQuestions = 10,
                correctAnswers = 8
            })
        };
        request.Headers.Add("X-Session-Token", created.SessionToken);

        var response = await harness.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<SaveQuizProgressResponseDto>();
        Assert.NotNull(result);
        Assert.True(result.TotalPoints > 0);
        Assert.True(result.NewLevel >= 1);
    }

    [Fact]
    public async Task SaveQuizProgress_DuplicateSubmission_DoesNotIncreasePointsOrLevel()
    {
        using var harness = TestHarness.Create();
        var created = await CreateProfileAsync(harness.Client, "Mila", "6789");

        async Task<SaveQuizProgressResponseDto?> SubmitQuizAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/quizzes/save-progress")
            {
                Content = JsonContent.Create(new
                {
                    totalQuestions = 8,
                    correctAnswers = 7
                })
            };
            request.Headers.Add("X-Session-Token", created.SessionToken);
            var response = await harness.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.Content.ReadFromJsonAsync<SaveQuizProgressResponseDto>();
        }

        var first = await SubmitQuizAsync();
        var second = await SubmitQuizAsync();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first.TotalPoints, second.TotalPoints);
        Assert.Equal(first.NewLevel, second.NewLevel);
    }

    [Fact]
    public async Task SaveQuizProgress_WithoutSessionToken_Returns401()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.PostAsJsonAsync("/api/v1/quizzes/save-progress", new
        {
            totalQuestions = 5,
            correctAnswers = 3
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllFunFacts_WithToken_Returns200()
    {
        using var harness = TestHarness.Create();
        var created = await CreateProfileAsync(harness.Client, "Zoe", "1010");
        var response = await GetWithTokenAsync(harness.Client, "/api/v1/fun-facts", created.SessionToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var facts = await response.Content.ReadFromJsonAsync<List<AnimalFunFactDto>>();
        Assert.NotNull(facts);
        Assert.NotEmpty(facts);
    }

    [Fact]
    public async Task GetAnimalFunFacts_WithToken_Returns200()
    {
        using var harness = TestHarness.Create();
        var created = await CreateProfileAsync(harness.Client, "Zoe", "1010");
        var response = await GetWithTokenAsync(harness.Client, "/api/v1/fun-facts/1", created.SessionToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var facts = await response.Content.ReadFromJsonAsync<List<AnimalFunFactDto>>();
        Assert.NotNull(facts);
        Assert.NotEmpty(facts);
    }

    [Fact]
    public async Task GetAllFunFacts_WithoutToken_Returns401()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/fun-facts");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBadgeCollection_WithToken_Returns200()
    {
        using var harness = TestHarness.Create();
        var created = await CreateProfileAsync(harness.Client, "Eli", "3030");
        var response = await GetWithTokenAsync(harness.Client, "/api/v1/badges", created.SessionToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var badges = await response.Content.ReadFromJsonAsync<List<BadgeCollectionDto>>();
        Assert.NotNull(badges);
        Assert.NotEmpty(badges);
    }

    [Fact]
    public async Task GetBadgeCollection_WithoutToken_Returns401()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/badges");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WeatherAdaptiveMission_AvailableMission_Returns200()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/missions/weather-adaptive?weatherCode=0&isDay=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var mission = await response.Content.ReadFromJsonAsync<WeatherMissionDto>();
        Assert.NotNull(mission);
        Assert.True(mission.Id > 0);
        Assert.False(string.IsNullOrWhiteSpace(mission.Title));
    }

    [Fact]
    public async Task WeatherAdaptiveMission_Nighttime_ReturnsNotFoundSleepModeBehavior()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.GetAsync("/api/v1/missions/weather-adaptive?weatherCode=0&isDay=false");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        Assert.NotNull(error);
        Assert.Equal("MISSION_NOT_FOUND", error.ErrorCode);
    }

    [Fact]
    public async Task MissionAssignStartHistoryComplete_Flow_ReturnsExpectedStatuses()
    {
        using var harness = TestHarness.Create();
        var created = await CreateProfileAsync(harness.Client, "Kai", "9090");

        var assignRequest = new AssignMissionRequestDto
        {
            MissionId = 101,
            WeatherCode = 0,
            IsDay = true
        };
        var assignResponse = await PostWithTokenAsync(harness.Client, "/api/v1/missions/assign", created.SessionToken, assignRequest);
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        var assigned = await assignResponse.Content.ReadFromJsonAsync<AssignMissionResponseDto>();
        Assert.NotNull(assigned);
        Assert.True(assigned.ProfileMissionId > 0);

        var startResponse = await PostWithTokenAsync(harness.Client, "/api/v1/missions/start", created.SessionToken,
            new StartMissionRequestDto { ProfileMissionId = assigned.ProfileMissionId });
        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);

        var completeResponse = await PostWithTokenAsync(harness.Client, "/api/v1/missions/complete", created.SessionToken,
            new CompleteMissionRequestDto { ProfileMissionId = assigned.ProfileMissionId });
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        var completed = await completeResponse.Content.ReadFromJsonAsync<CompleteMissionResponseDto>();
        Assert.NotNull(completed);
        Assert.True(completed.TotalPoints > 0);

        var historyResponse = await GetWithTokenAsync(harness.Client, "/api/v1/missions/history", created.SessionToken);
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
        var history = await historyResponse.Content.ReadFromJsonAsync<List<CompletedMissionHistoryDto>>();
        Assert.NotNull(history);
        Assert.NotEmpty(history);
    }

    [Fact]
    public async Task MissionAssign_WithoutToken_Returns401()
    {
        using var harness = TestHarness.Create();
        var response = await harness.Client.PostAsJsonAsync("/api/v1/missions/assign", new AssignMissionRequestDto { MissionId = 101 });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MissionStart_WithUnknownProfileMissionId_Returns404()
    {
        using var harness = TestHarness.Create();
        var created = await CreateProfileAsync(harness.Client, "Uma", "8181");

        var response = await PostWithTokenAsync(harness.Client, "/api/v1/missions/start", created.SessionToken,
            new StartMissionRequestDto { ProfileMissionId = 9999 });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MissionComplete_WithUnknownProfileMissionId_Returns404()
    {
        using var harness = TestHarness.Create();
        var created = await CreateProfileAsync(harness.Client, "Uma", "8181");

        var response = await PostWithTokenAsync(harness.Client, "/api/v1/missions/complete", created.SessionToken,
            new CompleteMissionRequestDto { ProfileMissionId = 9999 });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<CreateProfileResponseDto> CreateProfileAsync(HttpClient client, string name, string pin)
    {
        var response = await client.PostAsJsonAsync("/api/v1/profiles", new
        {
            displayName = name,
            pin,
            deviceInfo = "integration-test-device"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateProfileResponseDto>();
        Assert.NotNull(body);
        return body;
    }

    private static async Task<HttpResponseMessage> GetWithTokenAsync(HttpClient client, string path, string sessionToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("X-Session-Token", sessionToken);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PostWithTokenAsync<T>(HttpClient client, string path, string sessionToken, T payload)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("X-Session-Token", sessionToken);
        return await client.SendAsync(request);
    }

    private sealed class TestHarness : IDisposable
    {
        public HttpClient Client { get; }
        private readonly WebApplicationFactory<Program> _factory;

        private TestHarness(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            Client = factory.CreateClient();
        }

        public static TestHarness Create()
        {
            var profileService = new FakeProfileService();
            var animalService = new FakeAnimalService();
            var quizService = new FakeQuizService(profileService);
            var missionService = new FakeMissionService();
            var funFactService = new FakeFunFactService();
            var badgeService = new FakeBadgeService();

            var factory = new TestApiFactory(
                profileService,
                animalService,
                quizService,
                missionService,
                funFactService,
                badgeService);

            return new TestHarness(factory);
        }

        public void Dispose()
        {
            Client.Dispose();
            _factory.Dispose();
        }
    }

    private sealed class TestApiFactory : WebApplicationFactory<Program>
    {
        private readonly IProfileService _profileService;
        private readonly IAnimalService _animalService;
        private readonly IQuizService _quizService;
        private readonly IMissionService _missionService;
        private readonly IFunFactService _funFactService;
        private readonly IBadgeService _badgeService;

        public TestApiFactory(
            IProfileService profileService,
            IAnimalService animalService,
            IQuizService quizService,
            IMissionService missionService,
            IFunFactService funFactService,
            IBadgeService badgeService)
        {
            _profileService = profileService;
            _animalService = animalService;
            _quizService = quizService;
            _missionService = missionService;
            _funFactService = funFactService;
            _badgeService = badgeService;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProfileService>();
                services.RemoveAll<IAnimalService>();
                services.RemoveAll<IQuizService>();
                services.RemoveAll<IMissionService>();
                services.RemoveAll<IFunFactService>();
                services.RemoveAll<IBadgeService>();

                services.AddSingleton(_profileService);
                services.AddSingleton(_animalService);
                services.AddSingleton(_quizService);
                services.AddSingleton(_missionService);
                services.AddSingleton(_funFactService);
                services.AddSingleton(_badgeService);
            });
        }
    }

    private sealed class FakeProfileService : IProfileService
    {
        private readonly Dictionary<Guid, ProfileRecord> _profiles = new();
        private readonly Dictionary<string, Guid> _sessionToProfile = new(StringComparer.Ordinal);

        public Task<CreateProfileResponseDto> CreateProfileAsync(CreateProfileRequestDto request)
        {
            var profileId = Guid.NewGuid();
            var profileCode = $"PC{profileId.ToString("N")[..6].ToUpperInvariant()}";
            var sessionToken = $"session-{Guid.NewGuid():N}";

            _profiles[profileId] = new ProfileRecord
            {
                ProfileId = profileId,
                DisplayName = request.DisplayName ?? string.Empty,
                ProfileCode = profileCode,
                Pin = request.Pin ?? string.Empty,
                SessionToken = sessionToken,
                CurrentLevel = 1,
                TotalPoints = 0
            };

            _sessionToProfile[sessionToken] = profileId;

            return Task.FromResult(new CreateProfileResponseDto
            {
                ProfileId = profileId,
                DisplayName = request.DisplayName ?? string.Empty,
                ProfileCode = profileCode,
                Pin = request.Pin ?? string.Empty,
                SessionToken = sessionToken
            });
        }

        public Task<ProfileAutoLoginDto?> AutoLoginAsync(string sessionToken)
        {
            if (!_sessionToProfile.TryGetValue(sessionToken, out var profileId))
            {
                return Task.FromResult<ProfileAutoLoginDto?>(null);
            }

            var profile = _profiles[profileId];
            return Task.FromResult<ProfileAutoLoginDto?>(new ProfileAutoLoginDto
            {
                ProfileId = profile.ProfileId,
                DisplayName = profile.DisplayName,
                ProfileCode = profile.ProfileCode,
                CurrentLevel = profile.CurrentLevel,
                TotalPoints = profile.TotalPoints,
                TotalMissions = 0,
                TotalQuizzes = 0,
                StreakDays = 0
            });
        }

        public Task<bool> LogoutAsync(string sessionToken)
        {
            var removed = _sessionToProfile.Remove(sessionToken);
            return Task.FromResult(removed);
        }

        public Task<RestoreProfileResponseDto?> RestoreProfileAsync(RestoreProfileRequestDto request)
        {
            var profile = _profiles.Values.FirstOrDefault(p =>
                string.Equals(p.ProfileCode, request.ProfileCode, StringComparison.Ordinal) &&
                string.Equals(p.Pin, request.Pin, StringComparison.Ordinal));

            if (profile is null)
            {
                return Task.FromResult<RestoreProfileResponseDto?>(null);
            }

            var newToken = $"session-{Guid.NewGuid():N}";
            profile.SessionToken = newToken;
            _sessionToProfile[newToken] = profile.ProfileId;

            return Task.FromResult<RestoreProfileResponseDto?>(new RestoreProfileResponseDto
            {
                ProfileId = profile.ProfileId,
                DisplayName = profile.DisplayName,
                ProfileCode = profile.ProfileCode,
                CurrentLevel = profile.CurrentLevel,
                TotalPoints = profile.TotalPoints,
                StreakDays = 0,
                SessionToken = newToken
            });
        }

        public ProfileRecord? GetProfileById(Guid profileId)
        {
            _profiles.TryGetValue(profileId, out var profile);
            return profile;
        }
    }

    private sealed class FakeAnimalService : IAnimalService
    {
        private readonly List<AnimalCardDto> _animals =
        [
            new() { Id = 1, CommonName = "Koala", AnimalClass = "Mammals" },
            new() { Id = 2, CommonName = "Saltwater Crocodile", AnimalClass = "Reptiles" },
            new() { Id = 3, CommonName = "Frilled Lizard", AnimalClass = "Reptiles" }
        ];

        public Task<IEnumerable<AnimalCardDto>> GetAllAnimalCardsAsync(string? animalClass = null)
        {
            if (string.IsNullOrWhiteSpace(animalClass))
            {
                return Task.FromResult<IEnumerable<AnimalCardDto>>(_animals);
            }

            var filtered = _animals.Where(a =>
                string.Equals(a.AnimalClass, animalClass, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult<IEnumerable<AnimalCardDto>>(filtered.ToList());
        }

        public Task<AnimalCardDetailDto?> GetAnimalCardDetailAsync(int animalId)
        {
            if (animalId != 1)
            {
                return Task.FromResult<AnimalCardDetailDto?>(null);
            }

            return Task.FromResult<AnimalCardDetailDto?>(new AnimalCardDetailDto
            {
                CommonName = "Koala",
                ScientificName = "Phascolarctos cinereus",
                ClassName = "Mammals",
                ConservationCode = "VU",
                ConservationLabel = "Vulnerable"
            });
        }

        public Task<IEnumerable<AnimalOccurrenceDto>?> GetAnimalOccurrencesAsync(int animalId)
        {
            if (animalId != 1)
            {
                return Task.FromResult<IEnumerable<AnimalOccurrenceDto>?>(null);
            }

            return Task.FromResult<IEnumerable<AnimalOccurrenceDto>?>(
            [
                new AnimalOccurrenceDto { Latitude = -37.8136m, Longitude = 144.9631m }
            ]);
        }
    }

    private sealed class FakeQuizService : IQuizService
    {
        private readonly FakeProfileService _profileService;
        private readonly Dictionary<Guid, HashSet<string>> _seenSubmissions = new();

        public FakeQuizService(FakeProfileService profileService)
        {
            _profileService = profileService;
        }

        public Task<IEnumerable<QuizDto>> GetAllQuizzesAsync()
        {
            return Task.FromResult<IEnumerable<QuizDto>>(
            [
                new QuizDto { Id = 1, Title = "Australian Wildlife Basics", Topic = "Animals" }
            ]);
        }

        public Task<IEnumerable<QuizQuestionDto>?> GetQuestionsByQuizIdAsync(int quizId)
        {
            if (quizId != 1)
            {
                return Task.FromResult<IEnumerable<QuizQuestionDto>?>(null);
            }

            return Task.FromResult<IEnumerable<QuizQuestionDto>?>(
            [
                BuildQuestion(1, null),
                BuildQuestion(2, null)
            ]);
        }

        public Task<IEnumerable<QuizQuestionDto>> GetRandomQuestionsAsync(int count)
        {
            var questions = Enumerable.Range(1, count).Select(i => BuildQuestion(i, null)).ToList();
            return Task.FromResult<IEnumerable<QuizQuestionDto>>(questions);
        }

        public Task<IEnumerable<QuizQuestionDto>> GetRandomQuestionsByAnimalIdAsync(int animalId, int count)
        {
            var questions = Enumerable.Range(1, count).Select(i => BuildQuestion(i, animalId)).ToList();
            return Task.FromResult<IEnumerable<QuizQuestionDto>>(questions);
        }

        public Task<SaveQuizProgressResponseDto?> SaveQuizProgressAsync(Guid profileId, SaveQuizProgressRequestDto request)
        {
            var profile = _profileService.GetProfileById(profileId);
            if (profile is null)
            {
                return Task.FromResult<SaveQuizProgressResponseDto?>(null);
            }

            if (!_seenSubmissions.TryGetValue(profileId, out var seen))
            {
                seen = [];
                _seenSubmissions[profileId] = seen;
            }

            var signature = $"{request.TotalQuestions}:{request.CorrectAnswers}";
            var isDuplicate = !seen.Add(signature);

            if (!isDuplicate)
            {
                profile.TotalPoints += request.CorrectAnswers * 5;
                profile.CurrentLevel = Math.Max(1, (profile.TotalPoints / 50) + 1);
            }

            return Task.FromResult<SaveQuizProgressResponseDto?>(new SaveQuizProgressResponseDto
            {
                TotalPoints = profile.TotalPoints,
                NewLevel = profile.CurrentLevel,
                LeveledUp = false,
                NewFactsUnlocked = 0,
                NewBadges = []
            });
        }

        private static QuizQuestionDto BuildQuestion(int questionId, int? animalId)
        {
            return new QuizQuestionDto
            {
                QuestionId = questionId,
                AnimalId = animalId,
                QuestionText = "Which animal is a marsupial?",
                Difficulty = "Easy",
                Choices =
                [
                    new QuizChoiceDto { ChoiceLabel = 'A', ChoiceText = "Koala", IsCorrect = true },
                    new QuizChoiceDto { ChoiceLabel = 'B', ChoiceText = "Crocodile", IsCorrect = false }
                ]
            };
        }
    }

    private sealed class FakeMissionService : IMissionService
    {
        private int _nextProfileMissionId = 1;
        private readonly Dictionary<int, MissionState> _missions = new();

        public Task<WeatherMissionDto?> GetWeatherAdaptiveMissionAsync(int weatherCode, bool isDay)
        {
            if (!isDay)
            {
                return Task.FromResult<WeatherMissionDto?>(null);
            }

            return Task.FromResult<WeatherMissionDto?>(new WeatherMissionDto
            {
                Id = 101,
                Title = "Morning Nature Spot",
                Description = "Spot one bird and one plant in your backyard.",
                IsOutdoor = true,
                IsDayOnly = true,
                PointsReward = 20,
                TypeName = "Outdoor"
            });
        }

        public Task<int?> AssignMissionAsync(Guid profileId, int missionId, int? weatherCode, bool? isDay, decimal? weatherTemp, decimal? locationLat, decimal? locationLon)
        {
            var profileMissionId = _nextProfileMissionId++;
            _missions[profileMissionId] = new MissionState
            {
                ProfileId = profileId,
                MissionId = missionId,
                PointsReward = 20,
                IsStarted = false,
                IsCompleted = false
            };
            return Task.FromResult<int?>(profileMissionId);
        }

        public Task<bool> StartMissionAsync(int profileMissionId)
        {
            if (!_missions.TryGetValue(profileMissionId, out var mission) || mission.IsCompleted || mission.IsStarted)
            {
                return Task.FromResult(false);
            }

            mission.IsStarted = true;
            return Task.FromResult(true);
        }

        public Task<CompleteMissionResponseDto?> CompleteMissionAsync(int profileMissionId)
        {
            if (!_missions.TryGetValue(profileMissionId, out var mission) || !mission.IsStarted || mission.IsCompleted)
            {
                return Task.FromResult<CompleteMissionResponseDto?>(null);
            }

            mission.IsCompleted = true;
            return Task.FromResult<CompleteMissionResponseDto?>(new CompleteMissionResponseDto
            {
                TotalPoints = mission.PointsReward,
                NewLevel = 1,
                LeveledUp = false,
                NewFactsUnlocked = 0,
                NewBadges = []
            });
        }

        public Task<List<CompletedMissionHistoryDto>> GetCompletedMissionHistoryAsync(Guid profileId)
        {
            var history = _missions
                .Where(pair => pair.Value.ProfileId == profileId && pair.Value.IsCompleted)
                .Select(pair => new CompletedMissionHistoryDto
                {
                    MissionId = pair.Value.MissionId,
                    Title = "Morning Nature Spot",
                    Description = "Spot one bird and one plant in your backyard.",
                    TypeName = "Outdoor",
                    PointsEarned = pair.Value.PointsReward,
                    IsOutdoor = true,
                    CompletedAt = DateTimeOffset.UtcNow
                })
                .ToList();

            return Task.FromResult(history);
        }
    }

    private sealed class FakeFunFactService : IFunFactService
    {
        public Task<IEnumerable<AnimalFunFactDto>?> GetFunFactsByAnimalAsync(int animalId, Guid profileId)
        {
            return Task.FromResult<IEnumerable<AnimalFunFactDto>?>(
            [
                new AnimalFunFactDto
                {
                    Id = 1,
                    FactText = "Koalas sleep for up to 20 hours a day.",
                    AccessStatus = "unlocked",
                    UserLevel = 1,
                    UserPoints = 0
                }
            ]);
        }

        public Task<IEnumerable<AnimalFunFactDto>?> GetAllFunFactsAsync(Guid profileId)
        {
            return Task.FromResult<IEnumerable<AnimalFunFactDto>?>(
            [
                new AnimalFunFactDto
                {
                    Id = 1,
                    FactText = "Koalas sleep for up to 20 hours a day.",
                    AccessStatus = "unlocked",
                    UserLevel = 1,
                    UserPoints = 0
                },
                new AnimalFunFactDto
                {
                    Id = 2,
                    FactText = "Crocodiles can hold their breath for over an hour.",
                    AccessStatus = "locked",
                    UserLevel = 1,
                    UserPoints = 0
                }
            ]);
        }
    }

    private sealed class FakeBadgeService : IBadgeService
    {
        public Task<IEnumerable<BadgeCollectionDto>?> GetBadgeCollectionAsync(Guid profileId)
        {
            return Task.FromResult<IEnumerable<BadgeCollectionDto>?>(
            [
                new BadgeCollectionDto
                {
                    Id = 1,
                    BadgeName = "First Steps",
                    BadgeType = "Level",
                    IsUnlocked = true,
                    CurrentLevel = 1,
                    TotalPoints = 10,
                    TotalMissions = 0,
                    ProgressPercentage = 100
                }
            ]);
        }
    }

    private sealed class MissionState
    {
        public Guid ProfileId { get; init; }
        public int MissionId { get; init; }
        public int PointsReward { get; init; }
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
    }

    private sealed class ProfileRecord
    {
        public Guid ProfileId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string ProfileCode { get; init; } = string.Empty;
        public string Pin { get; init; } = string.Empty;
        public string SessionToken { get; set; } = string.Empty;
        public int CurrentLevel { get; set; }
        public int TotalPoints { get; set; }
    }
}
