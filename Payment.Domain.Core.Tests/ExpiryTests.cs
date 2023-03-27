using System;
using FluentAssertions;
using Xunit;

namespace Payment.Domain.Core.Tests;

public sealed class ExpiryTests
{
    [Theory]
    [InlineData("01/23")]
    [InlineData("12/23")]
    [InlineData("02/35")]
    [InlineData("02/00")]
    public void Expiry_ShouldSatisfyMask(string expiry)
    {
        Action createExpiry= () => new Expiry(expiry);

        createExpiry.Should().NotThrow();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    [InlineData(@"12\23")]
    [InlineData("12_23")]
    [InlineData("1223")]
    [InlineData("asdf")]
    [InlineData("asdfsdfgdf")]
    [InlineData("13/24")]
    [InlineData("00/24")]
    public void Expiry_ShouldThrowIfNotSatisfyMask(string expiry)
    {
        Action createExpiry= () => new Expiry(expiry);

        createExpiry.Should().Throw<ArgumentException>();
    }
    
}