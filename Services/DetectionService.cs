using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using fk_news_detector.Models;

namespace fk_news_detector.Services;

public class DetectionService : IDetectionService
{
    private readonly HttpClient _http;
    private const double ConfidenceThreshold = 0.75;

    public DetectionService(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("ML");
        var endpoint = config["ML:Endpoint"] ?? "http://localhost:5000";
        _http.BaseAddress = new Uri(endpoint.TrimEnd('/').Replace("/predict", "") + "/");
    }

    public async Task<DetectionResult> DetectAsync(string content, string? title = null, string? sourceUrl = null)
    {
        try
        {
            var payload = new ArticleRequest
            {
                SiteWeb = sourceUrl,
                Title = title,
                Article = content
            };

            var response = await _http.PostAsJsonAsync("predict", payload);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return Fail($"ML service returned {(int)response.StatusCode}: {body}");
            }

            var prediction = await response.Content.ReadFromJsonAsync<PredictionResponse>();

            if (prediction is null)
                return Fail("ML service returned an empty response.");

            var confidence = prediction.Confidence;
            var verdict = confidence >= ConfidenceThreshold
                ? (prediction.IsFake ? "FAKE" : "REAL")
                : "UNCERTAIN";

            return new DetectionResult
            {
                Verdict = verdict,
                Confidence = confidence,
                Success = true
            };
        }
        catch (TaskCanceledException)
        {
            return Fail("ML service timed out.");
        }
        catch (HttpRequestException ex)
        {
            return Fail($"Could not reach ML service: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Fail($"Invalid response from ML service: {ex.Message}");
        }
    }

    private static DetectionResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };

    private class ArticleRequest
    {
        [JsonPropertyName("site_web")]
        public string? SiteWeb { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("article")]
        public string? Article { get; set; }
    }

    private class PredictionResponse
    {
        [JsonPropertyName("site_web")]
        public string? SiteWeb { get; set; }

        [JsonPropertyName("prediction")]
        public int Prediction { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("is_fake")]
        public bool IsFake { get; set; }

        [JsonPropertyName("confidence")]
        public double? Confidence { get; set; }
    }
}
