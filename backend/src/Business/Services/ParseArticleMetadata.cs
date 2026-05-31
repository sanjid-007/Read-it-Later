using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Business.DTOs;
using Business.Interfaces;
using Business.Constants;
using HtmlAgilityPack;
namespace Business.Services
{
    public class ParseArticleMetadata : IParseArticleMetadata
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public ParseArticleMetadata(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                var content = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var title = ExtractTitle(doc);
                var actualContent = ExtractContent(doc);

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
        private string ExtractContent(HtmlDocument doc)
        {
            var contentNode = doc.DocumentNode.SelectNodes("//p")
                            ?.Select(node => node.InnerText.Trim())
                            .Where(text => !string.IsNullOrEmpty(text))
                            .ToList();
            return string.Join("\n\n", contentNode ?? new List<string> { "No content available." });
        }
    }
}