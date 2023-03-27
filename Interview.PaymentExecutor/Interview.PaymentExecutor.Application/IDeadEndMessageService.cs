namespace Interview.PaymentExecutor.Application;

public interface IDeadEndMessageService
{
    Task Send(
        string paymentId,
        string reason,
        CancellationToken token);
}