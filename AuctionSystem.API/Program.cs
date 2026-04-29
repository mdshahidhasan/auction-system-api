using AuctionSystem.API.Mapping;
using AuctionSystem.App.Apps;
using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Interfaces.SecurityServices;
using AuctionSystem.Core.Interfaces.Services;
using AuctionSystem.Infra.Services.Data;
using AuctionSystem.Infra.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using AutoMapper;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers and API documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application services
builder.Services.AddSingleton<IMapper>(_ =>
    new MapperConfiguration(cfg => cfg.AddMaps(typeof(BidProfile).Assembly)).CreateMapper());

// Database (Dapper)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing in configuration.");

builder.Services.AddScoped<MySqlConnection>(_ =>
    new MySqlConnection(connectionString));

// JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing in configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is missing in configuration.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is missing in configuration.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// CORS for the Vue frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Application layer
builder.Services.AddScoped<IAuthApp, AuthApp>();
builder.Services.AddScoped<IUserApp, UserApp>();
builder.Services.AddScoped<IUserAdminApp, UserAdminApp>();
builder.Services.AddScoped<IProductApp, ProductApp>();
builder.Services.AddScoped<IBidApp, BidApp>();

// Infrastructure and security services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserDataService, UserDataService>();
builder.Services.AddScoped<IProductDataService, ProductDataService>();
builder.Services.AddScoped<IBidDataService, BidDataService>();
builder.Services.AddScoped<IProductPhotoDataService, ProductPhotoDataService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
