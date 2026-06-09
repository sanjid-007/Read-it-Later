using Business.DTOs;
using Business.Interfaces;
namespace Business.Services
{
    public class ControlArticleMetadata : IControlArticleMetadata
    {
        private readonly IArticleRepository _articleRepository;

        private readonly IParseArticleMetadata _parseArticleMetadata;
        public ControlArticleMetadata(IArticleRepository articleRepository, IParseArticleMetadata parseArticleMetadata)
        {
            _articleRepository = articleRepository;
            _parseArticleMetadata = parseArticleMetadata;
        }
        public Task<ArticleResponse> GetArticleAsync(int id)
        {
            return _articleRepository.GetArticleAsync(id);
        }

        public async Task SaveArticleAsync(ArticleDto article)
        {
            var articleResponse = await _parseArticleMetadata.ParseMetadata(article.Url);
            if (articleResponse == null)
            {
                throw new Exception("Failed to parse article metadata.");
            }
            await _articleRepository.AddArticleAsync(articleResponse);
        }
    }
}