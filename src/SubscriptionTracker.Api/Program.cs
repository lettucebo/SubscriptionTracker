using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Filters;
using SubscriptionTracker.Service.Data;
using SubscriptionTracker.Service.Models;
using SubscriptionTracker.Service.Services;
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container using MVC pattern.
builder.Services.AddControllersWithViews();

// Configure SQL Server database.
builder.Services.AddDbContext<SubscriptionTracker.Service.Data.SubscriptionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Swagger/OpenAPI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SubscriptionTracker API",
        Version = "v1",
        Description = "API for tracking subscription services."
    });

    // 加入 XML 註解支援
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // 啟用範例功能
    options.ExampleFilters();

    // Configure Swagger to use JWT Bearer Authentication
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{builder.Configuration["AzureAd:Instance"]}{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"{builder.Configuration["AzureAd:Instance"]}{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/token"),
                Scopes = { { "api://bff69ff1-dbac-43ef-88a1-f2a0c9aba3dc/access_as_user", "Access the API as a user" } }
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { "api://bff69ff1-dbac-43ef-88a1-f2a0c9aba3dc/access_as_user" }
        }
    });
});

// Configure CORS policy.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Configure Entra ID authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Add user service
builder.Services.AddScoped<IUserService, UserService>();

// Configure Azure Application Insights with OpenTelemetry
string? appInsightsConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    // Add Azure Monitor OpenTelemetry
    builder.Services.AddOpenTelemetry().UseAzureMonitor(o =>
    {
        o.ConnectionString = appInsightsConnectionString;
    });
}

var app = builder.Build();

// 初始化資料庫
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<SubscriptionDbContext>();
    // Apply any pending migrations
    context.Database.Migrate();
}

// Home redirect to Swagger
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger", permanent: true);
        return;
    }
    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SubscriptionTracker API v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Configure route for MVC.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
