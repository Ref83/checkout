using AutoFixture;
using FluentAssertions;
using Payment.Domain.Core;
using Xunit;

namespace Interview.PaymentGateway.Domain.Tests;

public sealed class PaymentTests
{
    private readonly Fixture _fixture = new Fixture();

    [Fact]
    public void Complete_ChangesStatusToCompleted()
    {
        var payment = new Payment(
            _fixture.Create<PaymentId>(),
            new CardInformation(
                new CardNumber("0000000000000000"),
                new Expiry("03/25"),
                new Cvv("023"),
                _fixture.Create<string?>()),
            new Money(123, Currency.EUR),
            _fixture.Create<MerchantId>());

        payment.Complete();
        payment.Status.Should().Be(PaymentStatus.Completed);
    }
    
    [Fact]
    public void Reject_ChangesStatusToError()
    {
        var payment = new Payment(
            _fixture.Create<PaymentId>(),
            new CardInformation(
                new CardNumber("0000000000000000"),
                new Expiry("03/25"),
                new Cvv("023"),
                _fixture.Create<string?>()),
            new Money(123, Currency.EUR),
            _fixture.Create<MerchantId>());

        var reason = _fixture.Create<string>();
        payment.Reject(reason);
        
        payment.Status.Should().Be(PaymentStatus.Error);
        payment.Reason.Should().Be(reason);
    }  
    
    [Fact]
    public void Reject_CompletedPayment_DoesNotChangesStatus()
    {
        var payment = new Payment(
            _fixture.Create<PaymentId>(),
            new CardInformation(
                new CardNumber("0000000000000000"),
                new Expiry("03/25"),
                new Cvv("023"),
                _fixture.Create<string?>()),
            new Money(123, Currency.EUR),
            _fixture.Create<MerchantId>());

        payment.Complete();
        payment.Reject(_fixture.Create<string>());
        
        payment.Status.Should().Be(PaymentStatus.Completed);
    }    
}