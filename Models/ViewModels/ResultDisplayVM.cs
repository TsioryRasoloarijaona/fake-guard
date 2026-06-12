namespace fk_news_detector.Models.ViewModels;

public class ResultDisplayVM
{
    public Guid ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public string Verdict { get; set; } = "UNCERTAIN";
    public double Confidence { get; set; }
    public string? ModelName { get; set; }

    public string BadgeClass => Verdict switch
    {
        "FAKE" => "danger",
        "REAL" => "success",
        _ => "warning"
    };

    public string VerdictIcon => Verdict switch
    {
        "FAKE" => "bi-x-circle-fill",
        "REAL" => "bi-check-circle-fill",
        _ => "bi-question-circle-fill"
    };

    public int ConfidencePct => (int)Math.Round(Confidence * 100);
}
