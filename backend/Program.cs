using WhistOnline.API.Data;
using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Actions;
using WhistOnline.API.Repositories;
using WhistOnline.API.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WhistOnline.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
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
builder.Services.AddSignalR();
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
        
        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                var requestPath = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(token) && requestPath.StartsWithSegments("/hubs"))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {                                                                                                                              
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()                                                                                                    
            .AllowAnyMethod();                                                                                                   
    });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GameHub>("/hubs/game");
app.Run();