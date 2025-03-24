using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubscriptionTracker.Api.Controllers;
using SubscriptionTracker.Service.Data;
using SubscriptionTracker.Service.Models;
using FluentAssertions;

namespace SubscriptionTracker.Tests
{
    /// <summary>
    /// Unit tests for SubscriptionController.
    /// </summary>
    [TestClass]
    public class SubscriptionControllerTests
    {
        private SubscriptionController _controller;
        private SubscriptionDbContext _context;

        /// <summary>
        /// Initializes the test context and seeds test data.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Create new in-memory database options with a unique database name.
            var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            // Initialize the context.
            _context = new SubscriptionDbContext(options);
            
            // Seed a test subscription.
            _context.Subscriptions.Add(new Subscription 
            { 
                Id = 1, 
                Name = "Test Service", 
                Fee = 10.0m, 
                PaymentDate = DateTime.Today.AddDays(5), 
                Category = "Test" 
            });
            _context.SaveChanges();

            // Initialize the controller with the test context.
            _controller = new SubscriptionController(_context);
        }
 
        /// <summary>
        /// Tests retrieving an existing subscription returns an OkObjectResult.
        /// </summary>
        [TestMethod]
        public async Task GetSubscription_ReturnsOkResult_WithSubscription()
        {
            // Arrange.
            int testId = 1;
 
            // Act.
            var result = await _controller.GetSubscription(testId);
 
            // Assert.
            result.Should().BeOfType<OkObjectResult>();
        }
 
        /// <summary>
        /// Tests retrieving a non-existing subscription returns a NotFoundResult.
        /// </summary>
        [TestMethod]
        public async Task GetSubscription_ReturnsNotFound_WhenSubscriptionDoesNotExist()
        {
            // Arrange.
            int testId = 999;
 
            // Act.
            var result = await _controller.GetSubscription(testId);
 
            // Assert.
            result.Should().BeOfType<NotFoundResult>();
        }
     
        /// <summary>
        /// Tests creating a new subscription returns a CreatedAtActionResult.
        /// </summary>
        [TestMethod]
        public async Task CreateSubscription_ReturnsCreatedAtActionResult()
        {
            // Arrange.
            var newSubscription = new Subscription 
            {
                Id = 2,
                Name = "New Service",
                Fee = 20.0m,
                PaymentDate = DateTime.Today.AddDays(10),
                Category = "Test"
            };
 
            // Act.
            var result = await _controller.CreateSubscription(newSubscription);
 
            // Assert.
            result.Should().BeOfType<CreatedAtActionResult>();
        }
 
        /// <summary>
        /// Tests updating an existing subscription returns NoContentResult.
        /// </summary>
        [TestMethod]
        public async Task UpdateSubscription_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange: create a subscription to update.
            var subscriptionToUpdate = new Subscription 
            {
                Id = 3,
                Name = "Update Test",
                Fee = 30.0m,
                PaymentDate = DateTime.Today.AddDays(15),
                Category = "Test"
            };
            _context.Subscriptions.Add(subscriptionToUpdate);
            _context.SaveChanges();
 
            // Modify subscription.
            subscriptionToUpdate.Fee = 35.0m;
 
            // Act.
            var result = await _controller.UpdateSubscription(3, subscriptionToUpdate);
 
            // Assert.
            result.Should().BeOfType<NoContentResult>();
        }
 
        /// <summary>
        /// Tests deleting an existing subscription returns NoContentResult.
        /// </summary>
        [TestMethod]
        public async Task DeleteSubscription_ReturnsNoContent_WhenDeletionIsSuccessful()
        {
            // Arrange: create a subscription to delete.
            var subscriptionToDelete = new Subscription
            {
                Id = 4,
                Name = "Delete Test",
                Fee = 40.0m,
                PaymentDate = DateTime.Today.AddDays(20),
                Category = "Test"
            };
            _context.Subscriptions.Add(subscriptionToDelete);
            _context.SaveChanges();
 
            // Act.
            var result = await _controller.DeleteSubscription(4);
 
            // Assert.
            result.Should().BeOfType<NoContentResult>();
        }
 
        /// <summary>
        /// Cleanup after each test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
