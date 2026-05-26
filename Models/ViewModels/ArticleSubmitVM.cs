using System.ComponentModel.DataAnnotations;
using fk_news_detector.Models;

namespace fk_news_detector.Models.ViewModels;

public class ArticleSubmitVM
{
    public string InputMode { get; set; } = "text";

    // URL mode
    [Url]
    public string? ArticleUrl { get; set; }

    // Text mode
    public string? Title { get; set; }
    public string? Content { get; set; }

    [Url]
    public string? SourceUrl { get; set; }

    // Populated after extraction — drives the preview section in the view
    public ExtractedArticle? Extracted { get; set; }
}
