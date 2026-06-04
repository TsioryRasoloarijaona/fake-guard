using System.Net.Http.Json;
using System.Text.Json.Serialization;
using fk_news_detector.Models;

namespace fk_news_detector.Services;

public class DetectionService : IDetectionService
{
    private readonly HttpClient _httpClient;

    public DetectionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DetectionResponse> AnalyzeAsync(
        string title,
        string content,
        string? source = null)
    {
        var request = new PredictionRequest
        {
            Title = title,
            Article = content,
            SiteWeb = source
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/predict",
            request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PredictionResponse>();

        if (result == null)
            throw new Exception("Empty response from detection API.");

        return new DetectionResponse
        {
            Prediction = result.Prediction,
            Label = result.Label,
            IsFake = result.IsFake,
            Confidence = result.Confidence
        };
    }

    private class PredictionRequest
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