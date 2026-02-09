using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Text;
using OllamaSharp;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SECURITY & IDENTITY ---
var secretKey = "KUNCI_RAHASIA_SECURAI_PRO_2026_TUGAS_KAMPUS"; 
var keyBytes = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => {
        opt.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

// --- 2. CORS (Izinkan Web UI di Port 5073) ---
builder.Services.AddCors(options => {
    options.AddPolicy("AllowUI", p => p
        .WithOrigins("http://localhost:5073") // Port Web Lu
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// --- 3. INFRASTRUCTURE PROTECTION (Rate Limit) ---
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", opt => {
        opt.PermitLimit = 3; // Maks 3 request [cite: 2026-01-22]
        opt.Window = TimeSpan.FromSeconds(10); // Per 10 detik [cite: 2026-01-22]
        opt.QueueLimit = 0;
    });
});

builder.Services.AddEndpointsApiExplorer();

// --- 4. SWAGGER (Konfigurasi Ikon Gembok) ---
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SecurAI Cloud Gateway", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "Format: Bearer [spasi] token_lu",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// --- 5. OLLAMA CLIENT ---
builder.Services.AddSingleton<IOllamaApiClient>(sp => 
    new OllamaApiClient(new Uri("http://localhost:11434"), "gpt-oss:120b-cloud"));

var app = builder.Build();

// --- 6. MIDDLEWARE PIPELINE ---
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowUI");       // Wajib di atas Authentication
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// --- 7. ENDPOINTS ---

// LOGIN: Menghasilkan Token JWT [cite: 2026-01-22]
app.MapPost("/api/login", ([FromBody] LoginRequest login) => {
    if (login.Username == "admin" && login.Password == "admin") {
        var handler = new JwtSecurityTokenHandler();
        var desc = new SecurityTokenDescriptor {
            Subject = new System.Security.Claims.ClaimsIdentity(new[] { 
                new System.Security.Claims.Claim("user", login.Username) 
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(desc);
        // Penting: Nama field adalah "Token" (T Besar)
        return Results.Ok(new { Token = handler.WriteToken(token) });
    }
    return Results.Unauthorized();
});

// CHAT: Proxy ke Ollama Cloud [cite: 2026-01-22]
app.MapPost("/api/chat", async (IOllamaApiClient ollama, [FromBody] ChatRequest req) => {
    try {
        var chat = new Chat(ollama);
        var res = "";
        await foreach (var s in chat.SendAsync(req.Prompt)) res += s;
        return Results.Ok(new { Response = res });
    }
    catch (Exception ex) { return Results.Problem(ex.Message); }
})
.RequireAuthorization()
.RequireRateLimiting("fixed");

app.Run();

public record LoginRequest(string Username, string Password);
public record ChatRequest(string Prompt);