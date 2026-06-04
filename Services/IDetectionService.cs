namespace fk_news_detector.Services;
using fk_news_detector.Models;
public interface IDetectionService
{
    Task<DetectionResponse> AnalyzeAsync(
        string title,
        string content,
        string? source = null);
}