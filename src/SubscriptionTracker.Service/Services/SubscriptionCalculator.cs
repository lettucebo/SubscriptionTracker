using System;
using SubscriptionTracker.Service.Models;

namespace SubscriptionTracker.Service.Services
{
    public class SubscriptionCalculator
    {
        public static decimal CalculateEffectiveMonthlyPrice(string billingCycle, decimal amount, decimal discountRate)
        {
            return billingCycle.ToLower() switch
            {
                "yearly" => Math.Round((amount * (1 - discountRate)) / 12, 2),
                "monthly" => amount,
                _ => throw new ArgumentException($"Invalid billing cycle: {billingCycle}")
            };
        }

        public static int CalculateRemainingDays(DateTime startDate, DateTime? endDate, string billingCycle)
        {
            if (endDate.HasValue)
            {
                return (endDate.Value - DateTime.Today).Days;
            }

            return billingCycle.ToLower() switch
            {
                "yearly" => (startDate.AddYears(1) - DateTime.Today).Days,
                "monthly" => (startDate.AddMonths(1) - DateTime.Today).Days,
                _ => 0
            };
        }
    }
}
