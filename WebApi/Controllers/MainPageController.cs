using Microsoft.AspNetCore.Mvc;

namespace EthereumAPIBalance.WebApi.Controllers;

/// <summary>
/// Redirection from main page to Swagger
/// </summary>
[Route("")]
public class MainPageController : ControllerBase
{
    /// <summary>
    /// Redirection from main page to Swagger
    /// </summary>
    /// <returns></returns>
    [HttpGet("/")]
    public IActionResult OpenMainPage()
    {
        return RedirectPermanent("/swagger/");
    }
}