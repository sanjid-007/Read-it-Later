using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Domain.Entities;

namespace Business.Services
{
    public class SummaryJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SummaryJob> _logger;
        private const int BatchSize = 5;
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);
        public SummaryJob(IServiceScopeFactory scopeFactory, ILogger<SummaryJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConvertProcessingToPendingSummariesOnStartup();
            while (!stoppingToken.IsCancellationRequested)
            {
                var processedCount = 0;
                try
                {
                    processedCount = await ProcessSummaryTasks(stoppingToken);
                    _logger.LogInformation("SummaryJob is running at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing summaries.");
                }
                try
                {
                    if (processedCount == BatchSize)
                    {
                        _logger.LogInformation("Processed {Count} summaries, continuing without delay", processedCount);
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
        private async Task<int> ProcessSummaryTasks(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var summaryService = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
            var parsedArticles = await summaryService.GetParsedArticlesAsync();
            var processedCount = 0;
            foreach (var article in parsedArticles)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                var claimed = await summaryService.TryClaimSummaryAsync(article.Id);
                if (!claimed)
                {
                    continue;
                }
                try
                {
                    var summary = await GenerateSummaryAsync(article.Content);
                    await summaryService.UpdateSummaryAsync(article.Id, SummaryStatus.Completed, summary: summary);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate summary for article ID {ArticleId}", article.Id);
                    await summaryService.UpdateSummaryAsync(article.Id,SummaryStatus.Failed);
                }
                processedCount++;
            }

            return processedCount;
        }
        private async Task ConvertProcessingToPendingSummariesOnStartup()
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
            var count = await repository.ConvertProcessingToPendingSummariesOnStartupAsync();
            if (count > 0)
            {
                _logger.LogInformation("Converted {Count} processing summaries to pending", count);
            }
        }

        // TODO: replace with real summarization (e.g. Claude API call) — placeholder so the build passes.
        private Task<string> GenerateSummaryAsync(string? content)
        {
            return Task.FromResult("Summary not yet implemented.");
        }
    }
}