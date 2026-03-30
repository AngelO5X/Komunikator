using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add controllers
builder.Services.AddControllers();

// FluentValidation integration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DmsDbContext>(options =>
    options.UseSqlite("Data Source=speaknow.db"));

// CORS: allow only localhost dev origin by default. Configure in production using configuration.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy.WithOrigins("https://localhost:5001").AllowAnyHeader().AllowAnyMethod());
});

// Rate limiting - basic fixed window limiter
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("global", cfg =>
    {
        cfg.PermitLimit = 100; // 100 requests
        cfg.Window = TimeSpan.FromMinutes(1); // per minute
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

// JWT signing key should come from configuration or environment in production
var jwtKey = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "super_secret_jwt_signing_key_for_dev_only";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Keep issuer/audience validation off until tokens are configured with proper values.
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"].FirstOrDefault();
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ws/signal"))
                {
                    ctx.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();


// Enable OpenAPI/Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add HSTS in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Simple security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    await next();
});

// CORS: allow only localhost dev origin by default. Configure in production using configuration.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy.WithOrigins("https://localhost:5001").AllowAnyHeader().AllowAnyMethod());
});

// Rate limiting - basic fixed window limiter
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("global", cfg =>
    {
        cfg.PermitLimit = 100; // 100 requests
        cfg.Window = TimeSpan.FromMinutes(1); // per minute
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

app.UseCors("Default");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var webSocketOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(30) };
app.UseWebSockets(webSocketOptions);

app.MapGet("/ws/voice/{sessionId}/{userId}", async (HttpContext http, string sessionId, string userId) =>
{
    await VoiceWebSocketHandler.Handle(http, sessionId, userId);
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DmsDbContext>();
    db.Database.EnsureCreated(); 
}

app.Run();

