using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubscriptionTracker.Models;
using System;
using FluentAssertions;

namespace SubscriptionTracker.UnitTests
{
    [TestClass]
    public class SubscriptionTests
    {
        [TestMethod]
        public void DaysRemaining_PaymentDateThisMonth_CalculatesCorrectly()
        {
            // Arrange
            var today = DateTime.Today;
            var subscription = new Subscription
            {
                Name = "Test Service",
                Cost = 9.99m,
                PaymentDate = today.AddDays(5)
            };

            // Act
            int daysRemaining = subscription.DaysRemaining;

            // Assert
            daysRemaining.Should().Be(5);
        }

        [TestMethod]
        public void DaysRemaining_PaymentDatePassed_CalculatesNextMonth()
        {
            // Arrange
            var today = DateTime.Today;
            var subscription = new Subscription
            {
                Name = "Test Service",
                Cost = 9.99m,
                PaymentDate = today.AddDays(-5) // 5天前
            };

            // Act
            int daysRemaining = subscription.DaysRemaining;

            // Assert
            var expectedDate = today.AddDays(-5).AddMonths(1);
            var expectedDays = (expectedDate - today).Days;
            daysRemaining.Should().Be(expectedDays);
        }

        [TestMethod]
        public void Constructor_ValidData_CreatesSuccessfully()
        {
            // Arrange
            string name = "Netflix";
            decimal cost = 12.99m;
            var paymentDate = DateTime.Today;

            // Act
            var subscription = new Subscription
            {
                Name = name,
                Cost = cost,
                PaymentDate = paymentDate
            };

            // Assert
            subscription.Should().NotBeNull();
            subscription.Name.Should().Be(name);
            subscription.Cost.Should().Be(cost);
            subscription.PaymentDate.Should().Be(paymentDate);
        }
    }
}