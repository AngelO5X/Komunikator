using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Net;
using System.Net.Sockets;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy.WithOrigins("https://localhost:5001").AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("global", cfg =>
    {
        cfg.PermitLimit = 100; 
        cfg.Window = TimeSpan.FromMinutes(1);
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

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

if (builder.Environment.IsDevelopment())
{
    int port;
    var listener = new TcpListener(IPAddress.Loopback, 0);
    try
    {
        listener.Start();
        port = ((IPEndPoint)listener.LocalEndpoint).Port;
    }
    finally
    {
        listener.Stop();
    }

    var url = $"http://localhost:{port}";
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", url);
    Console.WriteLine($"[DEV] Overriding ASPNETCORE_URLS and using {url} to avoid HTTPS binding issues.");
}

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    await next();
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

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine("Application failed to start. Exception: " + ex);
    if (ex.InnerException != null)
    {
        Console.Error.WriteLine("Inner exception: " + ex.InnerException);
    }
    throw;
}

