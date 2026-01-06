// Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using UserManagementAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI: in-memory repository
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// JWT settings (from appsettings.json)
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? "PLEASE_CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY_32+_CHARS";
var jwtIssuer = jwtSection["Issuer"] ?? "TechHiveSolutions";
var jwtAudience = jwtSection["Audience"] ?? "TechHiveSolutionsUsers";

// Token-based authentication (JWT Bearer) + authorization
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// -------------------- Middleware pipeline --------------------
// STEP 5 order required by the assignment:
// 1) Error-handling middleware first
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
        var ex = feature?.Error;

        logger.LogError(ex, "Unhandled exception caught by global handler");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            error = "Internal server error.",
            traceId = context.TraceIdentifier
        });

        await context.Response.WriteAsync(payload);
    });
});

// Swagger (dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 2) Authentication middleware next (token-based)
app.UseAuthentication();
app.UseAuthorization();

// 3) Logging middleware last (logs final status codes for auditing)
app.Use(async (context, next) =>
{
    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("AuditLogging");

    var sw = Stopwatch.StartNew();
    logger.LogInformation("Incoming {Method} {Path}", context.Request.Method, context.Request.Path);

    try
    {
        await next();

        sw.Stop();
        logger.LogInformation("Outgoing {Method} {Path} -> {StatusCode} in {Elapsed}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
    catch (Exception ex)
    {
        sw.Stop();
        logger.LogError(ex, "Unhandled exception while processing {Method} {Path} after {Elapsed}ms",
            context.Request.Method,
            context.Request.Path,
            sw.ElapsedMilliseconds);

        // Let UseExceptionHandler format the JSON error response
        throw;
    }
});

// -------------------- Test endpoints (dev) --------------------
if (app.Environment.IsDevelopment())
{
    // Public endpoint to get a token for testing (no [Authorize] here)
    app.MapPost("/token", (IConfiguration config) =>
    {
        var jwt = config.GetSection("Jwt");

        var key = jwt["Key"] ?? "PLEASE_CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY_32+_CHARS";
        var issuer = jwt["Issuer"] ?? "TechHiveSolutions";
        var audience = jwt["Audience"] ?? "TechHiveSolutionsUsers";

        var expiresMinutes = 60;
        if (int.TryParse(jwt["ExpiresMinutes"], out var parsed))
            expiresMinutes = parsed;

        // Real app: validate user credentials here
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TechHiveUser"),
            new Claim(ClaimTypes.Role, "User")
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Results.Ok(new { token = tokenString });
    });

    // Endpoint to trigger an exception and verify error middleware
    app.MapGet("/throw", () =>
    {
        throw new Exception("Test unhandled exception");
    });
}

// Controllers (put [Authorize] on controllers/actions you want secured)
app.MapControllers();

app.Run();

/*
Add this to appsettings.json:

{
  "Jwt": {
    "Key": "PLEASE_CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY_32+_CHARS",
    "Issuer": "TechHiveSolutions",
    "Audience": "TechHiveSolutionsUsers",
    "ExpiresMinutes": 60
  }
}

Testing:
1) POST /token -> copy token
2) Call protected endpoints with header:
   Authorization: Bearer <token>

To secure endpoints:
- Add [Authorize] to your controller or specific actions.
*/
