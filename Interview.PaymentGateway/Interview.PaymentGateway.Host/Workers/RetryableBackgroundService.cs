using Polly;
using Polly.Retry;

namespace Interview.PaymentGateway.Host.Workers;

public abstract class RetryableBackgroundService : BackgroundService
{
    private const int NumberOfRetries = 3;
    private readonly TimeSpan _pollingDelay = TimeSpan.FromSeconds(5);

    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly string _serviceTypeName;
    private readonly ILogger _logger;

    protected RetryableBackgroundService(
        ILogger logger)
    {
        _logger = logger;

        _serviceTypeName = GetType().Name;
        _retryPolicy = Policy
            .Handle<Exception>(ex => ex is not ApplicationException && ex is not UnauthorizedAccessException)
            .WaitAndRetryAsync(
                NumberOfRetries,
                _ => TimeSpan.FromSeconds(1), 
                (ex, sleep) => logger.LogWarning($"{_serviceTypeName} execute failed: {ex}. Next retry after {sleep}."));
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await Task.Delay(_pollingDelay, token);
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _retryPolicy.ExecuteAsync(() => OnExecuteAsync(token));
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    $"{_serviceTypeName} execute failed after retries: {exception}. Sleeping till next scheduled start after {_pollingDelay}");
            }

            await Task.Delay(_pollingDelay, token);
        }
    }

    protected abstract Task OnExecuteAsync(CancellationToken token);
}