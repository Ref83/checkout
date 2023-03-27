using System;
using FluentAssertions;
using Xunit;

namespace Payment.Domain.Core.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Money_CantBeNegative()
    {
        Action createMoney = () => new Money(-12.32m, Currency.EUR);
        createMoney.Should().Throw<ArgumentException>();
    }
}