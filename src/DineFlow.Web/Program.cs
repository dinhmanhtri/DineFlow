using DineFlow.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== MVC + Views =====
builder.Services.AddControllersWithViews();

// ===== Session (dùng để lưu JWT token sau khi login) =====
// [KIẾN THỨC] Session vs Cookie:
// - Session: lưu data phía server, client chỉ lưu session ID
// - Cookie: lưu data phía client (browser)
// → Dùng Session để lưu JWT, tránh expose token ra client-side JS
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(8);  // Khớp với JWT ExpiryHours
    options.Cookie.HttpOnly    = true;   // Không cho JS đọc cookie (XSS protection)
    options.Cookie.IsEssential = true;   // Không cần cookie consent
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite     = SameSiteMode.Lax;
});

// ===== HttpClient → DineFlow.API =====
// [KIẾN THỨC] Typed HttpClient Pattern:
// - Đăng ký IMenuApiClient → MenuApiClient (typed client)
// - BaseAddress được set từ appsettings "ApiBaseUrl"
// - HttpClient được quản lý bởi IHttpClientFactory (tránh socket exhaustion)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5186";

builder.Services.AddHttpClient<IMenuApiClient, MenuApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ===== IHttpContextAccessor (để đọc Session trong ApiClient) =====
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ===== Middleware Pipeline =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session phải trước Authorization
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
