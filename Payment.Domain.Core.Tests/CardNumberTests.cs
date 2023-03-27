using System;
using FluentAssertions;
using Xunit;

namespace Payment.Domain.Core.Tests;

public class CardNumberTests
{
    [Theory]
    [InlineData("0000000000000000")]
    [InlineData("1234123412341234")]
    [InlineData("9999999999999999")]
    public void CardNumber_ShouldContainExactly16digits(string number)
    {
        Action createCardNumber = () => new CardNumber(number);

        createCardNumber.Should().NotThrow();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    [InlineData("123412341234")]
    [InlineData("12341234123412341234")]
    [InlineData("999999999999999a")]
    public void CardNumber_ShouldThrowsForShortOrLongNumbers(string number)
    {
        Action createCardNumber = () => new CardNumber(number);

        createCardNumber.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Mask_ReturnsMaskedNumber()
    {
        var cardNumber = new CardNumber("0000000000001234");
        cardNumber.Mask().Should().Be("*1234");
    }
}