using FrontOffice.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FrontOffice.Controllers;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = _httpClientFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"{_configuration["ApiBaseUrl"]}/auth/login",
            model
        );

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Email ou mot de passe incorrect");
            return View(model);
        }

        var token = await response.Content.ReadAsStringAsync();

        // Stockage du JWT
        HttpContext.Session.SetString("JWT", token);

        return RedirectToAction("Index", "Home");
    }
}