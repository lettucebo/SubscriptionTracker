using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubscriptionTracker.Controllers;
using SubscriptionTracker.Data;
using SubscriptionTracker.Models;
using System;
using System.Threading.Tasks;
using FluentAssertions;

namespace SubscriptionTracker.UnitTests.Controllers
{
    [TestClass]
    public class SubscriptionsControllerTests
    {
        private ApplicationDbContext _context;
        private SubscriptionsController _controller;

        [TestInitialize]
        public void Setup()
        {
            // 設定 InMemory 資料庫
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new SubscriptionsController(_context);
        }

        [TestMethod]
        public async Task Create_ValidSubscription_ReturnsRedirectToActionResult()
        {
            // Arrange
            var subscription = new Subscription
            {
                Name = "Netflix",
                Cost = 12.99m,
                PaymentDate = DateTime.Today
            };

            // Act
            var result = await _controller.Create(subscription);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be(nameof(SubscriptionsController.Index));
        }

        [TestMethod]
        public async Task Details_ExistingSubscription_ReturnsSubscription()
        {
            // Arrange
            var subscription = new Subscription
            {
                Name = "Netflix",
                Cost = 12.99m,
                PaymentDate = DateTime.Today
            };
            _context.Add(subscription);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Details(subscription.Id);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<Subscription>().Subject;
            
            model.Name.Should().Be(subscription.Name);
            model.Cost.Should().Be(subscription.Cost);
        }

        [TestMethod]
        public async Task Details_NonExistentSubscription_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task Edit_ValidSubscription_UpdatesAndRedirects()
        {
            // Arrange
            var subscription = new Subscription
            {
                Name = "Netflix",
                Cost = 12.99m,
                PaymentDate = DateTime.Today
            };
            _context.Add(subscription);
            await _context.SaveChangesAsync();

            var updatedSubscription = new Subscription
            {
                Id = subscription.Id,
                Name = "Netflix Premium",
                Cost = 15.99m,
                PaymentDate = DateTime.Today.AddDays(1)
            };

            // Act
            var result = await _controller.Edit(subscription.Id, updatedSubscription);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be(nameof(SubscriptionsController.Index));

            _context.ChangeTracker.Clear(); // 清除實體追蹤器
            var editedSubscription = await _context.Subscriptions.FindAsync(subscription.Id);
            editedSubscription.Should().NotBeNull();
            editedSubscription.Name.Should().Be(updatedSubscription.Name);
            editedSubscription.Cost.Should().Be(updatedSubscription.Cost);
            editedSubscription.PaymentDate.Should().Be(updatedSubscription.PaymentDate);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }
    }
}