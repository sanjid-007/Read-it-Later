
using Business.Interfaces;
using Business.Services;
using Business.Constants;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]));

builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IControlArticleMetadata, ControlArticleMetadata>();
builder.Services.AddScoped<IParseArticleMetadata, ParseArticleMetadata>();

builder.Services.AddHostedService<ArticleJob>();


builder.Services.AddHttpClient(HttpClientConstant.ArticleParser, client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
        "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

});

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
