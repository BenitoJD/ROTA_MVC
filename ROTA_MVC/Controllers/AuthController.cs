using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ROTA_MVC.ViewModels;
using System.Security.Claims;
using System;
using ROTA_MVC.Models;
using System.IdentityModel.Tokens.Jwt;

namespace ROTA_MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor; // To store/retrieve token

        public AuthController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var apiClient = _httpClientFactory.CreateClient("RotaApiClient");

            try
            {
                var apiLoginDto = new LoginDto { Username = model.Username, Password = model.Password };
                var response = await apiClient.PostAsJsonAsync("api/auth/login", apiLoginDto);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

                    if (loginResponse != null && loginResponse.Success && !string.IsNullOrEmpty(loginResponse.Token))
                    {

                        _httpContextAccessor.HttpContext?.Session.SetString("JWToken", loginResponse.Token);

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, model.Username), 
                            new Claim(ClaimTypes.Role, "Admin"), 
                            new Claim(ClaimTypes.NameIdentifier, "1") 
                        };

                        
                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadToken(loginResponse.Token) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                        if (jsonToken != null)
                        {
                            claims.AddRange(jsonToken.Claims);                                                               
                            var nameClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name);
                            if (nameClaim != null) claims.RemoveAll(c => c.Type == ClaimTypes.Name);
                        }
                        else
                        { 
                            claims.Add(new Claim(ClaimTypes.Role, "Unknown")); 
                        }

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true, 
                            ExpiresUtc = loginResponse.Expiration ?? DateTimeOffset.UtcNow.AddMinutes(60) 
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, loginResponse?.Message ?? "Invalid login attempt.");
                        return View(model);
                    }
                }
                else 
                {
                    ModelState.AddModelError(string.Empty, $"API Error: {(int)response.StatusCode} - {response.ReasonPhrase}");
                    return View(model);
                }
            }
            catch (HttpRequestException httpEx)
            {
                ModelState.AddModelError(string.Empty, $"Network error connecting to API. Please try again later. {httpEx.Message}");
                return View(model);
            }
            catch (Exception ex) 
            {
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred. {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            _httpContextAccessor.HttpContext?.Session.Remove("JWToken");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home"); 
        }


        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
