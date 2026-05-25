using Microsoft.AspNetCore.Mvc;

namespace fk_news_detector.Controllers;

public class ResultController : Controller
{
    [HttpGet]
    public IActionResult Details()
    {
        return View();
    }
}
