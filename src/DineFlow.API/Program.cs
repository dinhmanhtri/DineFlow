using System.Text;
using DineFlow.API.Middleware;
using DineFlow.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// [KIẾN THỨC] DI Container Setup
// builder.Services = IServiceCollection
// Tất cả .Add...() ở đây đăng ký services vào DI container
// =============================================

// ===== Controllers & JSON =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Camel case cho JSON response: { "tableNumber": 1 } thay vì { "TableNumber": 1 }
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;

        // Serialize enum as string: "Available" thay vì 0
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ===== Infrastructure (EF Core, Redis, Repositories, Services, AutoMapper) =====
builder.Services.AddInfrastructure(builder.Configuration);

// ===== JWT Authentication =====
// [KIẾN THỨC] Authentication vs Authorization:
// - Authentication (Xác thực): "Bạn là ai?" → JWT Bearer token
// - Authorization  (Phân quyền): "Bạn được làm gì?" → [Authorize(Roles = "Admin")]
var jwtSection  = builder.Configuration.GetSection("Jwt");
var secretKey   = jwtSection["SecretKey"]!;
var issuer      = jwtSection["Issuer"]!;
var audience    = jwtSection["Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,  // Kiểm tra exp (expiry time)
        ValidateIssuerSigningKey = true,
        ValidIssuer              = issuer,
        ValidAudience            = audience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew                = TimeSpan.Zero  // Không cho phép sai lệch thời gian (default 5 phút)
    };

    // Trả về 401 JSON thay vì redirect (API không cần redirect)
    options.Events = new JwtBearerEvents
    {
        OnChallenge = ctx =>
        {
            ctx.HandleResponse();
            ctx.Response.StatusCode = 401;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync("{\"statusCode\":401,\"message\":\"Bạn cần đăng nhập để thực hiện thao tác này.\"}");
        }
    };
});

builder.Services.AddAuthorization();

// ===== Swagger / OpenAPI =====
// [KIẾN THỨC] Swagger:
// - UI cho phép test API trực tiếp trên browser
// - Tự gen từ [ApiController] attributes
// - Cấu hình Security Definition để gửi JWT token từ Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title       = "DineFlow API",
        Version     = "v1",
        Description = "Restaurant Management System REST API"
    });

    // Thêm JWT security definition để test từ Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description  = "Nhập: Bearer {your-jwt-token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// ===== CORS (dành cho MVC Web app và Frontend) =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("DineFlowPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:7001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =============================================
// Build app và cấu hình Middleware Pipeline
// [KIẾN THỨC] Thứ tự middleware RẤT QUAN TRỌNG:
// 1. Error Handler — phải đầu tiên để bắt mọi exception
// 2. Swagger — chỉ dev
// 3. HTTPS Redirection
// 4. CORS
// 5. Authentication — xác định user từ JWT
// 6. Authorization  — check [Authorize] attribute
// 7. MapControllers — route request tới controller
// =============================================
var app = builder.Build();

// 1. Global error handler (phải ĐẦUTIÊN)
app.UseGlobalErrorHandler();

// 2. Swagger (chỉ development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DineFlow API v1");
        options.RoutePrefix = "swagger"; // Truy cập: /swagger
    });
}

// 3. HTTPS redirect
app.UseHttpsRedirection();

// 4. CORS
app.UseCors("DineFlowPolicy");

// 5 & 6. Auth (phải theo thứ tự: Authentication trước Authorization)
app.UseAuthentication();
app.UseAuthorization();

// 7. Route controllers
app.MapControllers();

// ===== Auto Migration (Development only) =====
if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDatabaseAsync();
}

app.Run();
