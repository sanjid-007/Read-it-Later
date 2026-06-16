using System;
using System.Threading;
using System.Threading.Tasks;
using Business.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Business.Services
{
    public class ArticleJob : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ArticleJob> _logger;

        public ArticleJob(IServiceScopeFactory scopeFactory, ILogger<ArticleJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingArticlesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while polling for pending articles");
                }

                try
                {
                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task ProcessPendingArticlesAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
            var parser = scope.ServiceProvider.GetRequiredService<IParseArticleMetadata>();

            var pending = await repository.GetPendingArticlesAsync();

            foreach (var article in pending)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await repository.UpdateArticleAsync(article.Id, ArticleStatus.Processing);

                try
                {
                    var parsed = await parser.ParseMetadata(article.Url!);
                    if (parsed == null)
                    {
                        await repository.UpdateArticleAsync(article.Id, ArticleStatus.Failed);
                        _logger.LogWarning("Could not parse article {Id} ({Url})", article.Id, article.Url);
                        continue;
                    }

                    await repository.UpdateArticleAsync(
                        article.Id, ArticleStatus.Completed, parsed.Title, parsed.Content);
                    _logger.LogInformation("Processed article {Id} ({Url})", article.Id, article.Url);
                }
                catch (Exception ex)
                {
                    await repository.UpdateArticleAsync(article.Id, ArticleStatus.Failed);
                    _logger.LogError(ex, "Error processing article {Id} ({Url})", article.Id, article.Url);
                }
            }
        }
    }
}
