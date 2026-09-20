using Chess.Application.Games.Commands.CreateGame;
using Chess.Application.Games.Commands.JoinGame;
using Chess.Application.Games.Commands.MakeMove;
using Chess.Application.Games.Commands.StartGame;
using Chess.Application.Games.Queries.GetLegalMoves;
using Chess.Application.Games.Queries.ViewGame;
using Chess.Domain.Interfaces;
using Chess.Domain.Services;
using Chess.Infrastructure.Persistence;
using Chess.Infrastructure.Persistence.Repositories;
using Chess.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ChessDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ChessDbContext>();

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

builder.Services.AddScoped<MakeMoveCommandHandler>();
builder.Services.AddScoped<JoinGameCommandHandler>();
builder.Services.AddScoped<StartGameCommandHandler>();

builder.Services.AddScoped<ViewGameQueryHandler>();
builder.Services.AddScoped<GetLegalMovesQueryHandler>();

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

app.MapRazorPages()
   .WithStaticAssets();

app.MapHub<GameHub>("/gameHub");

app.Run();
