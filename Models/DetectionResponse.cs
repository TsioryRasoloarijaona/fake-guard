namespace fk_news_detector.Models;

public class DetectionResponse
{
    public int Prediction { get; set; }

    public string Label { get; set; } = string.Empty;

    public bool IsFake { get; set; }

    public double? Confidence { get; set; }
}