using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Payment.Domain.Core;
using Xunit;

namespace Interview.PaymentGateway.Application.Tests;

public class PaymentHandledNotifierTests
{
    private readonly PaymentHandledNotifier _notifier = new PaymentHandledNotifier();
    private readonly Fixture _fixture = new Fixture();
    
    [Fact]
    public async Task NotifyCompleted_ShouldNotThrowException()
    {
        var paymentId = _fixture.Create<PaymentId>();
        var awaitingTask = _notifier.Await(paymentId, CancellationToken.None);
        
        _notifier.NotifyCompleted(paymentId);

        Func<Task> awaitCompleted = () => awaitingTask;
        await awaitCompleted.Should().NotThrowAsync();
    }

    [Fact]
    public void NotifyCompletedWithoutAwaiting_ShouldNotThrowException()
    {
        var paymentId = _fixture.Create<PaymentId>();
        
        Action notifyAction = () => _notifier.NotifyCompleted(paymentId);
        notifyAction.Should().NotThrow();
    }
    
    [Fact]
    public async Task NotifyCompletedTwice_ShouldNotThrowException()
    {
        var paymentId = _fixture.Create<PaymentId>();
        var awaitingTask = _notifier.Await(paymentId, CancellationToken.None);
        
        _notifier.NotifyCompleted(paymentId);
        
        Action secondNotifyAction = () => _notifier.NotifyCompleted(paymentId);
        secondNotifyAction.Should().NotThrow();

        Func<Task> awaitCompleted = () => awaitingTask;
        await awaitCompleted.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotifyRejected_ShouldThrowException()
    {
        var paymentId = _fixture.Create<PaymentId>();
        var reason = _fixture.Create<string>();

        var awaitingTask = _notifier.Await(paymentId, CancellationToken.None);
        
        _notifier.NotifyRejected(paymentId, reason);

        Func<Task> awaitCompleted = () => awaitingTask;
        await awaitCompleted.Should().ThrowExactlyAsync<ApplicationException>().WithMessage(reason);
    }
    
    [Fact]
    public void NotifyRejectedWithoutAwaiting_ShouldNotThrowException()
    {
        var paymentId = _fixture.Create<PaymentId>();
        
        Action notifyAction = () => _notifier.NotifyRejected(paymentId, _fixture.Create<string>());
        notifyAction.Should().NotThrow();
    }    
    
    [Fact]
    public async Task NotifyRejectedTwice_ShouldNotThrowException()
    {
        var paymentId = _fixture.Create<PaymentId>();
        var firstReason = _fixture.Create<string>();
        var secondReason = _fixture.Create<string>();

        var awaitingTask = _notifier.Await(paymentId, CancellationToken.None);
        
        _notifier.NotifyRejected(paymentId, firstReason);
        
        Action secondNotifyAction = () => _notifier.NotifyRejected(paymentId, secondReason);
        secondNotifyAction.Should().NotThrow();

        Func<Task> awaitCompleted = () => awaitingTask;
        await awaitCompleted.Should().ThrowExactlyAsync<ApplicationException>().WithMessage(firstReason);
    }
}