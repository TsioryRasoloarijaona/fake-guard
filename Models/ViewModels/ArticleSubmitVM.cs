using System.ComponentModel.DataAnnotations;
using fk_news_detector.Models;

namespace fk_news_detector.Models.ViewModels;

public class ArticleSubmitVM
{
    public string InputMode { get; set; } = "text";
    
    [Url]
    public string? ArticleUrl { get; set; }
    
    public string? Title { get; set; }
    public string? Content { get; set; }

    [Url]
    public string? SourceUrl { get; set; }
    
    public ExtractedArticle? Extracted { get; set; }
    public DetectionResult? Detection { get; set; }
}
