using fk_news_detector.Models.ViewModels;
using fk_news_detector.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace fk_news_detector.Controllers;

public class ResultController : Controller
{
    private readonly IUnitOfWork _uow;

    public ResultController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var article = await _uow.Articles.GetByIdAsync(id);

        if (article is null)
            return NotFound();

        var vm = new ResultDisplayVM
        {
            ArticleId    = article.ArticleId,
            Title        = article.Title,
            Content      = article.Content,
            SourceUrl    = article.SourceUrl,
            SubmittedAt  = article.SubmittedAt,
            Verdict      = article.Verdict,
            Confidence   = article.Confidence,
            ModelName    = article.ModelName
        };

        return View(vm);
    }
}
