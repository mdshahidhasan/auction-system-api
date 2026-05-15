using AuctionSystem.API.Authentication;
using AuctionSystem.API.Hubs;
using AuctionSystem.API.Mapping;
using AuctionSystem.API.Services.RealtimeNotification;
using AuctionSystem.App.Apps;
using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Interfaces.ExternalServices;
using AuctionSystem.Core.Interfaces.SecurityServices;
using AuctionSystem.Core.Interfaces.Services;
using AuctionSystem.Infra.Services.Data;
using AuctionSystem.Infra.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using AutoMapper;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMapper>(_ =>
    new MapperConfiguration(cfg => cfg.AddMaps(typeof(BidProfile).Assembly)).CreateMapper());

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing in configuration.");

builder.Services.AddScoped<MySqlConnection>(_ =>
    new MySqlConnection(connectionString));


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

        // Configure JWT authentication for SignalR connections
        // SignalR uses WebSocket/Server-Sent Events which don't support custom headers
        // Instead, the JWT token is passed as a query string parameter named "access_token"
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = SignalRJwtQueryStringTokenEventHandler.OnMessageReceivedAsync
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            // SignalR requires WebSocket support in CORS
            .AllowCredentials());
});


builder.Services.AddSignalR(options =>
{

    options.MaximumReceiveMessageSize = 32 * 1024;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);

    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});


builder.Services.AddScoped<IAuthApp, AuthApp>();
builder.Services.AddScoped<IUserApp, UserApp>();
builder.Services.AddScoped<IUserAdminApp, UserAdminApp>();
builder.Services.AddScoped<IProductApp, ProductApp>();
builder.Services.AddScoped<IBidApp, BidApp>();

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserDataService, UserDataService>();
builder.Services.AddScoped<IProductDataService, ProductDataService>();
builder.Services.AddScoped<IBidDataService, BidDataService>();
builder.Services.AddScoped<IProductPhotoDataService, ProductPhotoDataService>();


builder.Services.AddScoped<IAuctionRealtimeNotificationService, AuctionRealtimeNotificationService>();

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

app.MapHub<AuctionHub>("/hubs/auction");

app.Run();
