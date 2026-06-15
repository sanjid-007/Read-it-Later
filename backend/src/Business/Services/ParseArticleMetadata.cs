
using Business.DTOs;
using Business.Interfaces;
using Business.Constants;
using HtmlAgilityPack;
using SmartReader;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Business.Services
{
    public class ParseArticleMetadata : IParseArticleMetadata
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ParseArticleMetadata> logger;
        public ParseArticleMetadata(IHttpClientFactory httpClientFactory, ILogger<ParseArticleMetadata> logger)
        {
            _httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public async Task<ArticleResponse> ParseMetadata(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }
            var client = _httpClientFactory.CreateClient(HttpClientConstant.ArticleParser);
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                var content = await response.Content.ReadAsStringAsync();
                logger.LogInformation($"Fetched content from {url} in {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Restart();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var title = ExtractTitle(doc);


                var actualContent = ExtractContent(url,content);
                logger.LogInformation($"Extracted metadata from {url} in {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Stop();

                return new ArticleResponse
                {
                    Url = url,
                    Title = title,
                    Content = actualContent
                };

            }
            catch
            {
                return null;
            }
        }


        private string ExtractTitle(HtmlDocument doc)
        {
            var ogTitle = doc.DocumentNode
                .SelectSingleNode("//meta[@property='og:title']")?.GetAttributeValue("content", null);
            var titleTag = doc.DocumentNode
                .SelectSingleNode("//title")?.InnerText?.Trim();

            return ogTitle ?? titleTag ?? "Untitled";
        }
        private string ExtractContent(string url, string content)
        {
            try
            {
                var reader = new Reader(url, content);
                var article = reader.GetArticle();
                return article?.TextContent ?? "Content could not be extracted.";
            }
            
            catch
            {
                return "Content could not be extracted.";
            }
        }
    }
}