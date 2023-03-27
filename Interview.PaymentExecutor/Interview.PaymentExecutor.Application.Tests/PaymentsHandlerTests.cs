using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Interview.PaymentExecutor.Application.Events;
using Xunit;
using Interview.PaymentExecutor.Application.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Payment.Domain.Core;

namespace Interview.PaymentExecutor.Application.Tests;

public class PaymentsHandlerTests
{
    private readonly Fixture _fixture = new Fixture();
    private readonly PaymentsHandler _paymentsHandler;
    private readonly IBankService _bankService;
    private readonly IPaymentRetryService _paymentRetryService;
    private readonly IDeadEndMessageService _deadEndMessageService;
    private readonly IPaymentCompletedNotifier _paymentCompletedNotifier;
    private readonly ILogger<PaymentsHandler> _logger;

    public PaymentsHandlerTests()
    {
        _bankService = Substitute.For<IBankService>();
        _paymentRetryService = Substitute.For<IPaymentRetryService>();
        _deadEndMessageService = Substitute.For<IDeadEndMessageService>();
        _paymentCompletedNotifier = Substitute.For<IPaymentCompletedNotifier>();
        _logger = Substitute.For<ILogger<PaymentsHandler>>();

        _paymentsHandler = new PaymentsHandler(
            _bankService,
            _paymentRetryService,
            _deadEndMessageService,
            _paymentCompletedNotifier,
            _logger);
    }
    
    [Fact]
    public async Task HandlePaymentCreated_InvokesBankService()
    {
        var currency = Currency.EUR;
        var paymentCreated = new PaymentCreated
        {
            PaymentId = _fixture.Create<string>(),
            
            CardNumber = "0000000000000000",
            Expiry = "03/26",
            Cvv = "234",
            CardHolder = _fixture.Create<string?>(),
            
            Amount = _fixture.Create<decimal>(),
            Currency = currency.ToString(),
            
            MerchantId = _fixture.Create<string>()
        };

        await _paymentsHandler.Handle(paymentCreated, CancellationToken.None);

        var paymentId = new PaymentId(paymentCreated.PaymentId);
        var card = new CardInformation(
            new CardNumber(paymentCreated.CardNumber),
            new Expiry(paymentCreated.Expiry),
            new Cvv(paymentCreated.Cvv),
            paymentCreated.CardHolder);
        var amount = new Money(paymentCreated.Amount, currency);
        var merchantId = new MerchantId(paymentCreated.MerchantId);

        await _bankService.Received().Pay(paymentId, card, amount, merchantId, CancellationToken.None);
        await _paymentCompletedNotifier.Received().Notify(paymentId, CancellationToken.None);
    }
    
    [Fact]
    public async Task HandlePaymentCreated_ArgumentException_SendsMessageToDeadEnd()
    {
        var paymentCreated = new PaymentCreated
        {
            PaymentId = _fixture.Create<string>(),
            
            CardNumber = "invalid card number",
            Expiry = "03/26",
            Cvv = "234",
            CardHolder = _fixture.Create<string?>(),
            
            Amount = _fixture.Create<decimal>(),
            Currency = Currency.EUR.ToString(),
            
            MerchantId = _fixture.Create<string>()
        };

        await _paymentsHandler.Handle(paymentCreated, CancellationToken.None);

        await _deadEndMessageService.Received().Send(paymentCreated.PaymentId, Arg.Any<string>(), CancellationToken.None);
    }    
    
    [Fact]
    public async Task HandlePaymentCreated_BankPaymentException_SendsMessageRetry()
    {
        var paymentCreated = new PaymentCreated
        {
            PaymentId = _fixture.Create<string>(),
            
            CardNumber = "0000000000000000",
            Expiry = "03/26",
            Cvv = "234",
            CardHolder = _fixture.Create<string?>(),
            
            Amount = _fixture.Create<decimal>(),
            Currency = Currency.EUR.ToString(),
            
            MerchantId = _fixture.Create<string>()
        };

        _bankService.Pay(
                Arg.Any<PaymentId>(),
                Arg.Any<CardInformation>(),
                Arg.Any<Money>(),
                Arg.Any<MerchantId>(),
                CancellationToken.None)
            .Throws(new InvalidOperationException("test exception"));
        
        await _paymentsHandler.Handle(paymentCreated, CancellationToken.None);

        await _paymentRetryService.Received().Send(paymentCreated, CancellationToken.None);
    }
    
    [Fact]
    public async Task HandlePaymentFailed_InvokesBankService()
    {
        var currency = Currency.EUR;
        var paymentFailed = new PaymentFailed
        {
            PaymentId = _fixture.Create<string>(),
            
            CardNumber = "0000000000000000",
            Expiry = "03/26",
            Cvv = "234",
            CardHolder = _fixture.Create<string?>(),
            
            Amount = _fixture.Create<decimal>(),
            Currency = currency.ToString(),
            
            MerchantId = _fixture.Create<string>()
        };

        await _paymentsHandler.Handle(paymentFailed, CancellationToken.None);

        var paymentId = new PaymentId(paymentFailed.PaymentId);
        var card = new CardInformation(
            new CardNumber(paymentFailed.CardNumber),
            new Expiry(paymentFailed.Expiry),
            new Cvv(paymentFailed.Cvv),
            paymentFailed.CardHolder);
        var amount = new Money(paymentFailed.Amount, currency);
        var merchantId = new MerchantId(paymentFailed.MerchantId);

        await _bankService.Received().Pay(paymentId, card, amount, merchantId, CancellationToken.None);
        await _paymentCompletedNotifier.Received().Notify(paymentId, CancellationToken.None);
    }    
    
    [Fact]
    public async Task HandlePaymentFailed_BankPaymentException_SendsMessageToDeadEnd()
    {
        var paymentFailed = new PaymentFailed
        {
            PaymentId = _fixture.Create<string>(),
            
            CardNumber = "0000000000000000",
            Expiry = "03/26",
            Cvv = "234",
            CardHolder = _fixture.Create<string?>(),
            
            Amount = _fixture.Create<decimal>(),
            Currency = Currency.EUR.ToString(),
            
            MerchantId = _fixture.Create<string>()
        };

        _bankService.Pay(
                Arg.Any<PaymentId>(),
                Arg.Any<CardInformation>(),
                Arg.Any<Money>(),
                Arg.Any<MerchantId>(),
                CancellationToken.None)
            .Throws(new InvalidOperationException("test exception"));
        
        await _paymentsHandler.Handle(paymentFailed, CancellationToken.None);

        await _deadEndMessageService.Received().Send(paymentFailed.PaymentId, Arg.Any<string>(), CancellationToken.None);
    }    
}