using System.Text;
using CampusEats.Api.Data;
using CampusEats.Api.Models;
using CampusEats.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Database Context (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Scoped Service
builder.Services.AddScoped<IMenuService, MenuService>();

// CORS configuration (Lab 11 Task 03)
builder.Services.AddCors(o => o.AddPolicy("Frontend", p =>
    p.SetIsOriginAllowed(origin => true) // allow Vercel frontend, local dev, preview
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

// JWT Authentication (Lab 04 Task 04)
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? "dev-only-secret-change-me-min-32-characters!!");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => opt.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidIssuer = jwt["Issuer"] ?? "CampusEats",
        ValidateAudience = true,
        ValidAudience = jwt["Audience"] ?? "CampusEatsUsers",
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    });

builder.Services.AddAuthorization();

// Swagger with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste ONLY the token - no 'Bearer ' prefix."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Enable Swagger in Development and Cloud environments
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Menu on Startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        if (!db.MenuItems.Any())
        {
            db.MenuItems.AddRange(
                new MenuItem { Name = "Kottu Roti", Price = 750m, Category = "Mains", Available = true },
                new MenuItem { Name = "Fried Rice", Price = 850m, Category = "Mains", Available = true },
                new MenuItem { Name = "Watalappan", Price = 350m, Category = "Dessert", Available = true });
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization notice: {ex.Message}");
    }
}

app.Run();
