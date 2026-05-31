
using Business.Interfaces;
using Business.Services;
using Business.Constants;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IParseArticleMetadata, ParseArticleMetadata>();
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
