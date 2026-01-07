using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FrontOffice.Dto;
using FrontOffice.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontOffice.Controllers;
public class FrontAuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public FrontAuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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

        // ⚠️ Appel de l’API BO
        var response = await client.PostAsJsonAsync(
            $"{_configuration["ApiBaseUrl"]}/auth/login",
            model
        );

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Email ou mot de passe incorrect");
            return View(model);
        }

        // 🔐 Récupération du JWT
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        var token = result!.Token;

        // 🔎 Lecture du JWT
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // 🧩 Transformation du JWT → Claims MVC
        var claims = jwt.Claims.ToList();

        // Important : type d’authentification = Cookies
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var principal = new ClaimsPrincipal(identity);

        // 🍪 Connexion MVC
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        return RedirectToAction("Index", "Home");
    }
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove("JWT");

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return RedirectToAction("Login");
    }

}