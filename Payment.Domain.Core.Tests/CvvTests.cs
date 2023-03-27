using System;
using FluentAssertions;
using Xunit;

namespace Payment.Domain.Core.Tests;

public class CvvTests
{
    [Theory]
    [InlineData("123")]
    [InlineData("000")]
    [InlineData("999")]
    public void Cvv_ShouldContainExactly3digits(string cvv)
    {
        Action createCvv = () => new Cvv(cvv);

        createCvv.Should().NotThrow();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("23445")]
    [InlineData("1s1")]
    public void CardNumber_ShouldThrowsForShortOrLongNumbers(string cvv)
    {
        Action createCvv = () => new Cvv(cvv);

        createCvv.Should().Throw<ArgumentException>();
    }    
}