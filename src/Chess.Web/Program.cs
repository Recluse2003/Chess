using Chess.Application.Games.Commands.CreateGame;
using Chess.Application.Interfaces;
using Chess.Domain.Services;
using Chess.Infrastructure.Persistence;
using Chess.Infrastructure.Persistence.Models;
using Chess.Infrastructure.Persistence.Repositories;
using Chess.Web.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ChessDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ChessDbContext>();

builder.Services.AddRazorPages()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddSignalR();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateGameCommand).Assembly));

builder.Services.AddScoped<ChessRulesService>();

builder.Services.AddScoped<IChessGameRepository, ChessGameRepository>();
builder.Services.AddScoped<IGameCodeRepository, GameCodeRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapGet("/", () => Results.Redirect("/Game/Create"));

app.MapHub<ChessHub>("/chessHub");
app.MapHub<LobbyHub>("/lobbyHub");

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
