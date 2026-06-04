using fk_news_detector.Models.ViewModels;
using fk_news_detector.Services;
using Microsoft.AspNetCore.Mvc;
using fk_news_detector.UnitOfWork;
using fk_news_detector.Models;

namespace fk_news_detector.Controllers;

public class ArticleController : Controller
{
    private readonly INewsExtractionService _extractor;
    private readonly IDetectionService _detector;
    private readonly IUnitOfWork _unitOfWork;


    public ArticleController(INewsExtractionService extractor,  IDetectionService detector,  IUnitOfWork unitOfWork)
    {
        _extractor = extractor;
        _detector = detector;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Submit()
    {
        return View(new ArticleSubmitVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(ArticleSubmitVM vm)
    {
        if (vm.InputMode == "url" || !string.IsNullOrWhiteSpace(vm.ArticleUrl))
        {
            if (string.IsNullOrWhiteSpace(vm.ArticleUrl))
            {
                ModelState.AddModelError("ArticleUrl", "Please enter a URL.");
                return View(vm);
            }

            var extracted = await _extractor.ExtractAsync(vm.ArticleUrl);
            vm.Extracted = extracted;

            if (!extracted.Success)
            {
                ModelState.AddModelError("ArticleUrl", extracted.ErrorMessage ?? "Extraction failed.");
                return View(vm);
            }

            ModelState.Clear();
            vm.Title = extracted.Title;
            vm.Content = extracted.Content;
            vm.SourceUrl = extracted.SourceUrl;
            var detection = await _detector.AnalyzeAsync(
                extracted.Title,
                extracted.Content,
                extracted.SourceUrl);
            var articleId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var article = new Article
            {
                ArticleId = articleId,
                Title = extracted.Title,
                Content = extracted.Content,
                SourceUrl = extracted.SourceUrl,
                SubmittedAt = now,
                Verdict = detection.Label,
                ConfidenceScore = detection.Confidence ?? 0
            };

            await _unitOfWork.Articles.AddAsync(article);
            
            var articleByDate = new ArticleByDate
            {
                Bucket = now.ToString("yyyy-MM"),
                SubmittedAt = now,
                ArticleId = articleId,
                Title = article.Title,
                SourceUrl = article.SourceUrl,
                Verdict = article.Verdict,
                ConfidenceScore = article.ConfidenceScore
            };

            await _unitOfWork.Articles.AddToHistoryAsync(articleByDate);
            
            var result = new DetectionResultEntity
            {
                ArticleId = articleId,
                ResultId = Guid.NewGuid(),
                AnalyzedAt = now,
                ModelName = "FastAPI-ML",
                Verdict = detection.Label,
                Confidence = detection.Confidence ?? 0
            };

            await _unitOfWork.DetectionResults.AddAsync(result);
            await _unitOfWork.CommitAsync();

            ViewBag.Label = detection.Label;
            ViewBag.Confidence = detection.Confidence;
            ViewBag.IsFake = detection.IsFake;

            return View(vm);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(vm.Content))
            {
                ModelState.AddModelError("Content", "Please enter the article content.");
                return View(vm);
            }

            vm.Extracted = new fk_news_detector.Models.ExtractedArticle
            {
                Title = vm.Title ?? string.Empty,
                Content = vm.Content,
                SourceUrl = vm.SourceUrl ?? string.Empty,
                Success = true
            };
            // TODO: pass to DetectionService, persist via UnitOfWork
            var detection = await _detector.AnalyzeAsync(
                vm.Title ?? string.Empty,
                vm.Content,
                vm.SourceUrl);

            ViewBag.Label = detection.Label;
            ViewBag.Confidence = detection.Confidence;
            ViewBag.IsFake = detection.IsFake;

            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FetchFromUrl([FromForm] string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return BadRequest(new { success = false, error = "URL is required." });

        var result = await _extractor.ExtractAsync(url);

        if (!result.Success)
            return Ok(new { success = false, error = result.ErrorMessage });

        return Ok(new
        {
            success = true,
            title = result.Title,
            content = result.Content,
            author = result.Author,
            publishedDate = result.PublishedDate?.ToString("o")
        });
    }
}
