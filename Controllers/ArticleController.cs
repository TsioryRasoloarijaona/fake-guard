using fk_news_detector.Models;
using fk_news_detector.Models.ViewModels;
using fk_news_detector.Services;
using Microsoft.AspNetCore.Mvc;

namespace fk_news_detector.Controllers;

public class ArticleController : Controller
{
    private readonly INewsExtractionService _extractor;
    private readonly IDetectionService _detector;

    public ArticleController(INewsExtractionService extractor, IDetectionService detector)
    {
        _extractor = extractor;
        _detector = detector;
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
        if (vm.InputMode == "url")
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

            var result = await _detector.DetectAsync(extracted.Content, extracted.Title, extracted.SourceUrl);
            vm.Detection = result;

            if (!result.Success)
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Detection failed.");

            return View(vm);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(vm.Content))
            {
                ModelState.AddModelError("Content", "Please enter the article content.");
                return View(vm);
            }

            vm.Extracted = new ExtractedArticle
            {
                Title = vm.Title ?? string.Empty,
                Content = vm.Content,
                SourceUrl = vm.SourceUrl ?? string.Empty,
                Success = true
            };

            var result = await _detector.DetectAsync(vm.Content, vm.Title, vm.SourceUrl);
            vm.Detection = result;

            if (!result.Success)
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Detection failed.");

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
