using System.Text.Json;
using DineFlow.Web.Models;
using DineFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Web.Controllers;

/// <summary>
/// AuthController — Xử lý Login/Logout cho MVC Web
/// 
/// [KIẾN THỨC] Session-based JWT Storage:
/// - Login thành công → lưu JWT vào Session (server-side, secure)
/// - Logout → xóa Session
/// - Mỗi request API → MenuApiClient đọc JWT từ Session và attach vào Header
/// </summary>
public class AuthController(
    IAuthApiClient authApiClient) : Controller
{
    private const string SessionKeyToken    = "jwt_token";
    private const string SessionKeyFullName = "user_fullname";
    private const string SessionKeyRole     = "user_role";
    private const string SessionKeyEmail    = "user_email";

    // GET /Auth/Login
    public IActionResult Login(string? returnUrl = null)
    {
        // Nếu đã login rồi → redirect về home
        if (HttpContext.Session.GetString(SessionKeyToken) is not null)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    // POST /Auth/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var response = await authApiClient.LoginAsync(model.Email, model.Password);

        if (response is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        // Lưu JWT và user info vào Session
        HttpContext.Session.SetString(SessionKeyToken,    response.Token);
        HttpContext.Session.SetString(SessionKeyFullName, response.FullName);
        HttpContext.Session.SetString(SessionKeyRole,     response.Role);
        HttpContext.Session.SetString(SessionKeyEmail,    response.Email);

        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Home");
    }

    // POST /Auth/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
