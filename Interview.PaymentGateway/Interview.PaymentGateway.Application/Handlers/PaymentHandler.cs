using Eventso.Subscription;
using Interview.PaymentGateway.Application.Events;
using Interview.PaymentGateway.Domain;
using Payment.Domain.Core;

namespace Interview.PaymentGateway.Application.Handlers;

public sealed class PaymentHandler : 
    IMessageHandler<PaymentCompleted>,
    IMessageHandler<PaymentRejected>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentHandledNotifier _paymentHandledNotifier;

    public PaymentHandler(IPaymentRepository paymentRepository, IPaymentHandledNotifier paymentHandledNotifier)
    {
        _paymentRepository = paymentRepository;
        _paymentHandledNotifier = paymentHandledNotifier;
    }

    public async Task Handle(PaymentCompleted message, CancellationToken token)
    {
        var paymentId = new PaymentId(message.PaymentId);
        var payment = await _paymentRepository.Get(paymentId, token);
        
        payment.Complete();

        await _paymentRepository.Update(payment, token);
        _paymentHandledNotifier.NotifyCompleted(paymentId);
    }

    public async Task Handle(PaymentRejected message, CancellationToken token)
    {
        var paymentId = new PaymentId(message.PaymentId);
        var payment = await _paymentRepository.Get(paymentId, token);
        
        payment.Reject(message.Reason);

        await _paymentRepository.Update(payment, token);
        _paymentHandledNotifier.NotifyRejected(paymentId, message.Reason);
    }
}