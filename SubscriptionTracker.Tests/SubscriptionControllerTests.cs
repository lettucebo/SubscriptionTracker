using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubscriptionTracker.Api.Controllers;
using SubscriptionTracker.Service.Data;
using SubscriptionTracker.Service.Models;

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
            // Create new in-memory database options.
            var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
                .UseInMemoryDatabase(databaseName: "TestSubscriptionDB")
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
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
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
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
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
            Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
        }
    }
}
