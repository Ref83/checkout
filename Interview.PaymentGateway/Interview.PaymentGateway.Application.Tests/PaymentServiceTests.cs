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

public sealed class PaymentServiceTests
{
    private readonly Fixture _fixture = new Fixture();
    private PaymentService _paymentService;
    private IPaymentRepository _paymentRepository;
    private IPaymentHandledAwaiter _paymentHandledAwaiter;
    private string _topic;

    public PaymentServiceTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _paymentHandledAwaiter = Substitute.For<IPaymentHandledAwaiter>();
        _topic = _fixture.Create<string>();
        
        _paymentService = new PaymentService(_paymentRepository, _paymentHandledAwaiter, _topic);
    }

    [Fact]
    public async Task Pay_CreatesNewPayment()
    {
        Domain.Payment? savedPayment = null;
        Event savedEvent = default;
        
        _paymentRepository
            .TryAdd(Arg.Any<Domain.Payment>(), Arg.Any<Event>(), CancellationToken.None)
            .Returns(Task.FromResult(true))
            .AndDoes(c =>
            {
                savedPayment = (Domain.Payment)c[0];
                savedEvent = (Event)c[1];
            });

        var paymentId = _fixture.Create<PaymentId>();
        var cardInformation = new CardInformation(
            new CardNumber("0000000000000000"),
            new Expiry("03/25"),
            new Cvv("023"),
            _fixture.Create<string?>());
        var amount = new Money(123, Currency.EUR);
        var merchantId = _fixture.Create<MerchantId>();

        await _paymentService.Pay(paymentId, cardInformation, amount, merchantId, CancellationToken.None);

        savedPayment?.Id.Should().Be(paymentId);
        savedPayment?.Card.Should().Be(cardInformation);
        savedPayment?.Amount.Should().Be(amount);
        savedPayment?.MerchantId.Should().Be(merchantId);
        savedPayment?.Status.Should().Be(PaymentStatus.Processing);

        var expectedEvent = new Event(
            new PaymentCreated
            {
                PaymentId = paymentId.Value,
                
                CardNumber = cardInformation.CardNumber.Value,
                Expiry = cardInformation.Expiry.Value,
                Cvv = cardInformation.Cvv.Value,
                CardHolder = cardInformation.CardHolder,
                
                Amount = amount.Amount,
                Currency = amount.Currency.ToString(),
                
                MerchantId = merchantId.Value
            },
            _topic);
        
        savedEvent.Topic.Should().BeEquivalentTo(expectedEvent.Topic);
        savedEvent.Message.Should().BeEquivalentTo(expectedEvent.Message);
        
        await _paymentRepository.Received().TryAdd(Arg.Any<Domain.Payment>(), Arg.Any<Event>(), CancellationToken.None);
        await _paymentHandledAwaiter.Received().Await(paymentId, CancellationToken.None);
    }
    
    [Fact]
    public async Task PaySecondTime_DoesntCreatesNewPayment()
    {
        Domain.Payment? savedPayment = null;
        Event savedEvent = default;
        
        _paymentRepository
            .TryAdd(Arg.Any<Domain.Payment>(), Arg.Any<Event>(), CancellationToken.None)
            .Returns(Task.FromResult(false))
            .AndDoes(c =>
            {
                savedPayment = (Domain.Payment)c[0];
                savedEvent = (Event)c[1];
            });

        var paymentId = _fixture.Create<PaymentId>();
        var cardInformation = new CardInformation(
            new CardNumber("0000000000000000"),
            new Expiry("03/25"),
            new Cvv("023"),
            _fixture.Create<string?>());
        var amount = new Money(123, Currency.EUR);
        var merchantId = _fixture.Create<MerchantId>();

        await _paymentService.Pay(paymentId, cardInformation, amount, merchantId, CancellationToken.None);

        savedPayment?.Id.Should().Be(paymentId);
        savedPayment?.Card.Should().Be(cardInformation);
        savedPayment?.Amount.Should().Be(amount);
        savedPayment?.MerchantId.Should().Be(merchantId);
        savedPayment?.Status.Should().Be(PaymentStatus.Processing);

        var expectedEvent = new Event(
            new PaymentCreated
            {
                PaymentId = paymentId.Value,
                
                CardNumber = cardInformation.CardNumber.Value,
                Expiry = cardInformation.Expiry.Value,
                Cvv = cardInformation.Cvv.Value,
                CardHolder = cardInformation.CardHolder,
                
                Amount = amount.Amount,
                Currency = amount.Currency.ToString(),
                
                MerchantId = merchantId.Value
            },
            _topic);
        
        savedEvent.Topic.Should().BeEquivalentTo(expectedEvent.Topic);
        savedEvent.Message.Should().BeEquivalentTo(expectedEvent.Message);
        
        await _paymentRepository.Received().TryAdd(Arg.Any<Domain.Payment>(), Arg.Any<Event>(), CancellationToken.None);
        await _paymentHandledAwaiter.DidNotReceive().Await(paymentId, CancellationToken.None);
    }    
}