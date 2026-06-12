using fk_news_detector.Models;
using fk_news_detector.Models.ViewModels;
using fk_news_detector.Services;
using fk_news_detector.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace fk_news_detector.Controllers;

public class ArticleController : Controller
{
    private readonly INewsExtractionService _extractor;
    private readonly IArticleService _articleService;
    private readonly IUnitOfWork _uow;

    public ArticleController(INewsExtractionService extractor, IArticleService articleService, IUnitOfWork uow)
    {
        _extractor = extractor;
        _articleService = articleService;
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var articles = await _uow.Articles.GetAllAsync();
        var vm = articles
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => new ResultDisplayVM
            {
                ArticleId   = a.ArticleId,
                Title       = a.Title,
                SourceUrl   = a.SourceUrl,
                SubmittedAt = a.SubmittedAt,
                Verdict     = a.Verdict,
                Confidence  = a.Confidence,
                ModelName   = a.ModelName
            })
            .ToList();
        return View(vm);
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
        ExtractedArticle extracted;

        if (vm.InputMode == "url")
        {
            if (string.IsNullOrWhiteSpace(vm.ArticleUrl))
            {
                ModelState.AddModelError("ArticleUrl", "Please enter a URL.");
                return View(vm);
            }

            extracted = await _extractor.ExtractAsync(vm.ArticleUrl);

            if (!extracted.Success)
            {
                ModelState.AddModelError("ArticleUrl", extracted.ErrorMessage ?? "Extraction failed.");
                return View(vm);
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(vm.Content))
            {
                ModelState.AddModelError("Content", "Please enter the article content.");
                return View(vm);
            }

            extracted = new ExtractedArticle
            {
                Title = vm.Title ?? string.Empty,
                Content = vm.Content,
                SourceUrl = vm.SourceUrl ?? string.Empty,
                Success = true
            };
        }

        var article = await _articleService.GetOrAnalyzeAsync(extracted);
        return RedirectToAction("Details", "Result", new { id = article.ArticleId });
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
