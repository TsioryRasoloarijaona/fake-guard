using Microsoft.AspNetCore.Mvc;

namespace fk_news_detector.Controllers;

public class ArticleController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Submit()
    {
        return View();
    }
}
