using Rescheduler.Api.Models;
using Xunit;

namespace Rescheduler.Api.Tests.Models;

public class ValidateCronAttributeTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("*/10 * * * *", true)]
    [InlineData("*+100 * * * *", false)]
    [InlineData("*/10 * * * * *", false)]
    public void TestValidate_ShouldPass(object cron, bool expected)
    {
        // Arrange
        var validator = new ValidateCronAttribute();

        // Act
        var actual = validator.IsValid(cron);

        // Assert
        Assert.Equal(expected, actual);
    }
}