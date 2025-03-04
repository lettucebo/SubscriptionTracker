using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubscriptionTracker.Controllers;
using SubscriptionTracker.Data;
using SubscriptionTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace SubscriptionTracker.UnitTests.Controllers
{
    [TestClass]
    public class ReportControllerTests
    {
        private ApplicationDbContext _context;
        private ReportController _controller;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new ReportController(_context);

            // 新增測試資料
            _context.Subscriptions.AddRange(
                new Subscription { Name = "Netflix", Cost = 12.99m, PaymentDate = DateTime.Today },
                new Subscription { Name = "Spotify", Cost = 9.99m, PaymentDate = DateTime.Today.AddDays(5) },
                new Subscription { Name = "Amazon Prime", Cost = 8.99m, PaymentDate = DateTime.Today.AddDays(10) }
            );
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task Index_ReturnsViewWithSubscriptions()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Subscription>>().Subject;
            model.Should().HaveCount(3);
        }

        [TestMethod]
        public async Task Index_CalculatesTotalMonthlyCostCorrectly()
        {
            // Act
            var result = await _controller.Index() as ViewResult;

            // Assert
            decimal expectedTotalCost = 12.99m + 9.99m + 8.99m;
            result.ViewData["TotalMonthlyCost"].Should().Be(expectedTotalCost);
        }

        [TestMethod]
        public async Task Index_SortsByNameCorrectly()
        {
            // Act
            var result = await _controller.Index("nameAsc") as ViewResult;
            var subscriptions = result.Model.Should().BeAssignableTo<IEnumerable<Subscription>>().Subject;

            // Assert
            var sortedList = subscriptions.ToList();
            sortedList[0].Name.Should().Be("Amazon Prime");
            sortedList[1].Name.Should().Be("Netflix");
            sortedList[2].Name.Should().Be("Spotify");
        }

        [TestMethod]
        public async Task Index_SortsByCostCorrectly()
        {
            // Act
            var result = await _controller.Index("costAsc") as ViewResult;
            var subscriptions = result.Model.Should().BeAssignableTo<IEnumerable<Subscription>>().Subject;

            // Assert
            var sortedList = subscriptions.ToList();
            sortedList[0].Cost.Should().Be(8.99m);
            sortedList[1].Cost.Should().Be(9.99m);
            sortedList[2].Cost.Should().Be(12.99m);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }
    }
}