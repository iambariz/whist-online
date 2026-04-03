using WhistOnline.API.Data;
using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Actions;
using WhistOnline.API.Repositories;
using WhistOnline.API.Services;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<GameRepository>();
builder.Services.AddScoped<PlayerRepository>();
builder.Services.AddScoped<BidRepository>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<LobbyService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<TrickService>();
builder.Services.AddScoped<ScoringService>();
builder.Services.AddScoped<GameRules>();
builder.Services.AddScoped<ScoreBoardService>();
builder.Services.AddScoped<DeckService>();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();