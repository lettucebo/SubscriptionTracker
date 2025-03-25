using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubscriptionTracker.Service.Services;

namespace SubscriptionTracker.Tests
{
    [TestClass]
    public class SubscriptionCalculatorTests
    {
        [DataTestMethod]
        [DataRow("yearly", 120, 0.1, 9.0)]
        public void CalculateEffectiveMonthlyPrice_ForYearly_ReturnsCorrectPrice(string billingCycle, decimal amount, decimal discountRate, decimal expectedMonthlyPrice)
        {
            // Act
            var result = SubscriptionCalculator.CalculateEffectiveMonthlyPrice(billingCycle, amount, discountRate);
            // Assert
            result.Should().BeApproximately(expectedMonthlyPrice, 0.01m);
        }

        [DataTestMethod]
        [DataRow("monthly", 15)]
        public void CalculateEffectiveMonthlyPrice_ForMonthly_ReturnsSameAmount(string billingCycle, decimal amount)
        {
            // Act
            var result = SubscriptionCalculator.CalculateEffectiveMonthlyPrice(billingCycle, amount, 0);
            // Assert
            result.Should().Be(amount);
        }

        [TestMethod]
        public void CalculateEffectiveMonthlyPrice_InvalidBillingCycle_ThrowsArgumentException()
        {
            // Act
            Action act = () => SubscriptionCalculator.CalculateEffectiveMonthlyPrice("invalid", 100, 0);
            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Invalid billing cycle: invalid");
        }

        [TestMethod]
        public void CalculateRemainingDays_WithEndDate_ReturnsDaysDifference()
        {
            // Arrange
            var today = DateTime.Today;
            var endDate = today.AddDays(10);
            // Act
            var result = SubscriptionCalculator.CalculateRemainingDays(today, endDate, "monthly");
            // Assert
            result.Should().Be(10);
        }

        [TestMethod]
        public void CalculateRemainingDays_ForYearly_NoEndDate_ReturnsDaysUntilNextYear()
        {
            // Arrange
            var startDate = DateTime.Today;
            // Act
            var result = SubscriptionCalculator.CalculateRemainingDays(startDate, null, "yearly");
            var expected = (startDate.AddYears(1) - DateTime.Today).Days;
            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void CalculateRemainingDays_ForMonthly_NoEndDate_ReturnsDaysUntilNextMonth()
        {
            // Arrange
            var startDate = DateTime.Today;
            // Act
            var result = SubscriptionCalculator.CalculateRemainingDays(startDate, null, "monthly");
            var expected = (startDate.AddMonths(1) - DateTime.Today).Days;
            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void CalculateRemainingDays_InvalidBillingCycle_NoEndDate_ReturnsZero()
        {
            // Arrange
            var startDate = DateTime.Today;
            // Act
            var result = SubscriptionCalculator.CalculateRemainingDays(startDate, null, "invalid");
            // Assert
            result.Should().Be(0);
        }

        // Additional test cases for increased coverage

        [DataTestMethod]
        [DataRow("yearly", 240, 0.2, 16.0)] // Additional test for yearly cycle with different parameters
        public void CalculateEffectiveMonthlyPrice_ForYearly_AdditionalTest(string billingCycle, decimal amount, decimal discountRate, decimal expectedMonthlyPrice)
        {
            // Act
            var result = SubscriptionCalculator.CalculateEffectiveMonthlyPrice(billingCycle, amount, discountRate);
            // Assert
            result.Should().BeApproximately(expectedMonthlyPrice, 0.01m);
        }

        [DataTestMethod]
        [DataRow("monthly", 50)] // Additional test for monthly cycle
        public void CalculateEffectiveMonthlyPrice_ForMonthly_AdditionalTest(string billingCycle, decimal amount)
        {
            // Act
            var result = SubscriptionCalculator.CalculateEffectiveMonthlyPrice(billingCycle, amount, 0);
            // Assert
            result.Should().Be(amount);
        }

        [TestMethod]
        public void CalculateRemainingDays_WithPastEndDate_ReturnsNegativeDays()
        {
            // Arrange
            var today = DateTime.Today;
            var pastDate = today.AddDays(-5);
            // Act
            var result = SubscriptionCalculator.CalculateRemainingDays(today, pastDate, "monthly");
            // Assert (negative difference)
            result.Should().Be(-5);
        }
    }
}
