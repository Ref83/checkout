using System.Collections.Concurrent;
using Payment.Domain.Core;

namespace Interview.PaymentGateway.Application;

public sealed class PaymentHandledNotifier : IPaymentHandledNotifier, IPaymentHandledAwaiter
{
    private readonly TimeSpan DefaultPaymentTimeout = TimeSpan.FromSeconds(30);
    
    private readonly ConcurrentDictionary<PaymentId, TaskCompletionSource> _notifiers = new ();

    public void NotifyCompleted(PaymentId paymentId)
    {
        if (_notifiers.TryRemove(paymentId, out var notifier))
            notifier.TrySetResult();
    }

    public void NotifyRejected(PaymentId paymentId, string reason)
    {
        if (_notifiers.TryRemove(paymentId, out var notifier))
            notifier.TrySetException(new ApplicationException(reason));
    }

    public Task Await(PaymentId paymentId, CancellationToken token)
    {
        var taskCompletionSource = _notifiers.GetOrAdd(
            paymentId,
            _ => new TaskCompletionSource());

        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(DefaultPaymentTimeout);
        var combined = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token, token);        
        
        return taskCompletionSource.Task.WaitAsync(combined.Token);
    }
}