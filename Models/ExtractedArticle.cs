namespace fk_news_detector.Models;

public class ExtractedArticle
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string? Author { get; set; }
    public DateTime? PublishedDate { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
