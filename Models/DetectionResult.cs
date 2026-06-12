namespace fk_news_detector.Models;

public class DetectionResult
{
    public string Verdict { get; set; } = "UNCERTAIN"; 
    public double? Confidence { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
