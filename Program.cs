using EventManager.Api.Data;
using EventManager.Api.Services;
using EventManager.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.RateLimiting;
using EventManager.Api.Validators;
using FluentValidation;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using EventManager.Api;
using System.Text.Json.Serialization; // Added for JsonNamingPolicy and JsonIgnoreCondition

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UserRegisterDtoValidator>();
builder.Services.AddDbContext<EventManagerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS policy (restrict in production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // Updated to assumed frontend port
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
            throw new InvalidOperationException("JWT Key is not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPlannerService, PlannerService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information); // Change to Information in production
});

var app = builder.Build();

// Global exception handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"message\":\"An unexpected error occurred.\"}");
    });
});

// Log all incoming requests
app.Use(async (context, next) =>
{
    app.Logger.LogDebug("Incoming request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next.Invoke();
    app.Logger.LogDebug("Response status: {StatusCode}", context.Response.StatusCode);
});

app.UseRouting();
app.UseCors("AllowSpecificOrigins");
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EventManagerDbContext>();
    await SeedData.Initialize(dbContext);
}

#pragma warning disable ASP0014
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    var endpointDataSource = endpoints.DataSources.FirstOrDefault();
    if (endpointDataSource != null)
    {
        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (endpoint is Microsoft.AspNetCore.Routing.RouteEndpoint routeEndpoint)
            {
                app.Logger.LogDebug("Registered endpoint: {Method} {Route}", routeEndpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Http.EndpointMetadataCollection>()?.GetMetadata<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>()?.HttpMethods?.FirstOrDefault() ?? "Unknown", routeEndpoint.RoutePattern.RawText);
            }
        }
    }
});
#pragma warning restore ASP0014

app.Run("http://localhost:5203");