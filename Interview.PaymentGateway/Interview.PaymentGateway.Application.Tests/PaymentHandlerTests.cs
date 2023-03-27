using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Interview.PaymentGateway.Application.Events;
using Interview.PaymentGateway.Application.Handlers;
using Interview.PaymentGateway.Domain;
using NSubstitute;
using Payment.Domain.Core;
using Xunit;

namespace Interview.PaymentGateway.Application.Tests;

public sealed class PaymentHandlerTests
{
    private readonly Fixture _fixture = new Fixture();
    private PaymentHandler _paymentHandler;
    private IPaymentRepository _paymentRepository;
    private IPaymentHandledNotifier _paymentHandledNotifier;

    public PaymentHandlerTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _paymentHandledNotifier = Substitute.For<IPaymentHandledNotifier>();
        
        _paymentHandler = new PaymentHandler(_paymentRepository, _paymentHandledNotifier);
    }
    
    [Fact]
    public async Task HandlePaymentCompleted_CompletesPayment()
    {
        var payment = new Domain.Payment(
            _fixture.Create<PaymentId>(),
            new CardInformation(
                new CardNumber("0000000000000000"),
                new Expiry("03/25"),
                new Cvv("023"),
                _fixture.Create<string?>()),
            new Money(123, Currency.EUR),
            _fixture.Create<MerchantId>());

        _paymentRepository
            .Get(Arg.Any<PaymentId>(), CancellationToken.None)
            .Returns(Task.FromResult(payment));

        await _paymentHandler.Handle(new PaymentCompleted { PaymentId = payment.Id.Value }, CancellationToken.None);

        payment.Status.Should().Be(PaymentStatus.Completed);
        await _paymentRepository.Received().Update(payment, CancellationToken.None);
        _paymentHandledNotifier.Received().NotifyCompleted(payment.Id);
    }
    
    [Fact]
    public async Task HandlePaymentRejected_RejectPayment()
    {
        var payment = new Domain.Payment(
            _fixture.Create<PaymentId>(),
            new CardInformation(
                new CardNumber("0000000000000000"),
                new Expiry("03/25"),
                new Cvv("023"),
                _fixture.Create<string?>()),
            new Money(123, Currency.EUR),
            _fixture.Create<MerchantId>());

        _paymentRepository
            .Get(Arg.Any<PaymentId>(), CancellationToken.None)
            .Returns(Task.FromResult(payment));

        var reason = _fixture.Create<string>();
        await _paymentHandler.Handle(
            new PaymentRejected
            {
                PaymentId = payment.Id.Value,
                Reason = reason
            }, 
            CancellationToken.None);

        payment.Status.Should().Be(PaymentStatus.Error);
        payment.Reason.Should().Be(reason);
        
        await _paymentRepository.Received().Update(payment, CancellationToken.None);
        _paymentHandledNotifier.Received().NotifyRejected(payment.Id, reason);
    }    
}