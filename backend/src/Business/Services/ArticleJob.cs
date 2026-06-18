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
        private const int BatchSize = 5;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ArticleJob> _logger;

        public ArticleJob(IServiceScopeFactory scopeFactory, ILogger<ArticleJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConvertProcessingToPendingOnStartup();
            while (!stoppingToken.IsCancellationRequested)
            {
                var processedCount = 0;
                try
                {
                    processedCount = await ProcessPendingArticlesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while polling for pending articles");
                }

                try
                {
                    if (processedCount == BatchSize)
                    {
                        _logger.LogInformation("Processed {Count} articles, continuing without delay", processedCount);
                        continue;
                    }
                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task<int> ProcessPendingArticlesAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
            var parser = scope.ServiceProvider.GetRequiredService<IParseArticleMetadata>();

            var pending = await repository.GetPendingArticlesAsync(BatchSize);
            var processedCount = pending.Count;
            foreach (var article in pending)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                var won = await repository.TryClaimAsync(article.Id);
                if (!won)
                {
                    continue;
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
            return processedCount;
        }

        private async Task ConvertProcessingToPendingOnStartup()
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
            var count = await repository.ConvertProcessingToPendingOnStartupAsync();
            if (count > 0)
            {
                _logger.LogInformation("Converted {Count} processing articles to pending", count);
            }
        }
    }
}
