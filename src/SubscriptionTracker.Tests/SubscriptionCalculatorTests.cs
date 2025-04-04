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
        [DynamicData(nameof(GetYearlyTestData), DynamicDataSourceType.Method)]
        public void CalculateEffectiveMonthlyPrice_ForYearly_ReturnsCorrectPrice(string billingCycle, decimal amount, decimal discountRate, decimal expectedMonthlyPrice)
        {
            // Act
            var result = SubscriptionCalculator.CalculateEffectiveMonthlyPrice(billingCycle, amount, discountRate);
            // Assert
            result.Should().BeApproximately(expectedMonthlyPrice, 0.01m);
        }

        /// <summary>
        /// Verifies monthly subscriptions return the same amount without discount
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(GetMonthlyTestData), DynamicDataSourceType.Method)]
        public void CalculateEffectiveMonthlyPrice_ForMonthly_ReturnsSameAmount(string billingCycle, decimal amount)
        {
            // Act
            var result = SubscriptionCalculator.CalculateEffectiveMonthlyPrice(billingCycle, amount, 0);
            // Assert
            result.Should().Be(amount);
        }

        /// <summary>
        /// Tests invalid billing cycle handling
        /// </summary>
        [TestMethod]
        public void CalculateEffectiveMonthlyPrice_InvalidBillingCycle_ThrowsArgumentException()
        {
            // Act
            Action act = () => SubscriptionCalculator.CalculateEffectiveMonthlyPrice("invalid", 100, 0);
            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Invalid billing cycle: invalid");
        }

        /// <summary>
        /// Tests remaining days calculation with fixed end date
        /// </summary>
        [TestMethod]
        public void CalculateRemainingDays_WithEndDate_ReturnsDaysDifference()
        {
            // Arrange
            var today = DateOnly.FromDateTime(DateTime.Today);
            var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
            // Act
            var result = SubscriptionCalculator.CalculateRemainingDays(today, endDate, "monthly");
            // Assert
            result.Should().Be(10);
        }

        /// <summary>
        /// Tests yearly subscription remaining days calculation without end date
        /// </summary>
        [TestMethod]
        public void CalculateRemainingDays_ForYearly_NoEndDate_ReturnsDaysUntilNextYear()
        {
            // Arrange
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            // Act
            var result = SubscriptionCalculator.CalculateRemainingDays(startDate, null, "yearly");
            var expected = DateOnly.FromDateTime(DateTime.Today.AddYears(1)).DayNumber - startDate.DayNumber;
            // Assert
            result.Should().Be(expected);
        }

        /// <summary>
        /// Tests monthly subscription remaining days calculation without end date
        /// </summary>
        [TestMethod]
        public void CalculateRemainingDays_ForMonthly_NoEndDate_ReturnsDaysUntilNextMonth()
        {
            // Arrange
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            // Act
            var result = SubscriptionCalculator.CalculateRemainingDays(startDate, null, "monthly");
            var expected = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)).DayNumber - startDate.DayNumber;
            // Assert
            result.Should().Be(expected);
        }

        /// <summary>
        /// Tests invalid billing cycle handling when no end date exists
        /// </summary>
        [TestMethod]
        public void CalculateRemainingDays_InvalidBillingCycle_NoEndDate_ReturnsZero()
        {
            // Arrange
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            // Act
            var result = SubscriptionCalculator.CalculateRemainingDays(startDate, null, "invalid");
            // Assert
            result.Should().Be(0);
        }

        // Additional test cases for increased coverage

        /// <summary>
        /// Additional test cases for yearly subscription price calculation
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(GetYearlyAdditionalTestData), DynamicDataSourceType.Method)]
        public void CalculateEffectiveMonthlyPrice_ForYearly_AdditionalTest(string billingCycle, decimal amount, decimal discountRate, decimal expectedMonthlyPrice)
        {
            // Act
            var result = SubscriptionCalculator.CalculateEffectiveMonthlyPrice(billingCycle, amount, discountRate);
            // Assert
            result.Should().BeApproximately(expectedMonthlyPrice, 0.01m);
        }

        /// <summary>
        /// Additional test cases for monthly subscription price verification
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(GetMonthlyAdditionalTestData), DynamicDataSourceType.Method)]
        public void CalculateEffectiveMonthlyPrice_ForMonthly_AdditionalTest(string billingCycle, decimal amount)
        {
            // Act
            var result = SubscriptionCalculator.CalculateEffectiveMonthlyPrice(billingCycle, amount, 0);
            // Assert
            result.Should().Be(amount);
        }

        /// <summary>
        /// Tests remaining days calculation with past end date
        /// </summary>
        [TestMethod]
        public void CalculateRemainingDays_WithPastEndDate_ReturnsNegativeDays()
        {
            // Arrange
            var today = DateOnly.FromDateTime(DateTime.Today);
            var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));
            // Act
            var result = SubscriptionCalculator.CalculateRemainingDays(today, pastDate, "monthly");
            // Assert (negative difference)
            result.Should().Be(-5);
        }

        /// <summary>
        /// Provides test data for yearly price calculation
        /// - 120/year with 10% discount = 9/month
        /// </summary>
        public static IEnumerable<object[]> GetYearlyTestData()
        {
            yield return new object[] { "yearly", 120M, 0.1M, 9.0M };
        }

        /// <summary>
        /// Provides test data for monthly price verification
        /// - 15/month with no discount
        /// </summary>
        public static IEnumerable<object[]> GetMonthlyTestData()
        {
            yield return new object[] { "monthly", 15M };
        }

        /// <summary>
        /// Provides additional test data for yearly price calculation
        /// - 240/year with 20% discount = 16/month
        /// </summary>
        public static IEnumerable<object[]> GetYearlyAdditionalTestData()
        {
            yield return new object[] { "yearly", 240M, 0.2M, 16.0M };
        }

        /// <summary>
        /// Provides additional test data for monthly price verification
        /// - 50/month with no discount
        /// </summary>
        public static IEnumerable<object[]> GetMonthlyAdditionalTestData()
        {
            yield return new object[] { "monthly", 50M };
        }
    }
}
