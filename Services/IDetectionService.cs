using fk_news_detector.Models;

namespace fk_news_detector.Services;

public interface IDetectionService
{
    Task<DetectionResult> DetectAsync(string content, string? title = null, string? sourceUrl = null);
}
