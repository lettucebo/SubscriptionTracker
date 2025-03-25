namespace SubscriptionTracker.Service.Services
{
    /// <summary>
    /// Provides financial calculation utilities for subscription management
    /// </summary>
    /// <remarks>
    /// <para>This service handles:</para>
    /// <list type="bullet">
    /// <item><description>Price conversions between annual/monthly billing cycles</description></item>
    /// <item><description>Effective cost calculations with discounts</description></item>
    /// <item><description>Remaining time/value calculations for subscriptions</description></item>
    /// </list>
    /// </remarks>
    public class SubscriptionCalculator
    {
        /// <summary>
        /// Calculates the effective monthly price considering annual discounts
        /// </summary>
        /// <param name="billingCycle">Payment interval type
        /// <list type="bullet">
        /// <item><description>"yearly": Annual billing with discount applied</description></item>
        /// <item><description>"monthly": Monthly billing without discount</description></item>
        /// </list>
        /// </param>
        /// <param name="amount">Base subscription amount in local currency (positive value)</param>
        /// <param name="discountRate">Annual discount rate 
        /// <list type="bullet">
        /// <item><description>Range: 0.0 (0%) to 1.0 (100%)</description></item>
        /// <item><description>Example: 0.2 represents 20% discount</description></item>
        /// </list>
        /// </param>
        /// <returns>Effective monthly price rounded to 2 decimal places</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// <list type="bullet">
        /// <item><description>Invalid billing cycle provided</description></item>
        /// <item><description>Negative amount value</description></item>
        /// <item><description>Discount rate outside 0-1 range</description></item>
        /// </list>
        /// </exception>
        /// <example>
        /// <code>
        /// var monthlyPrice = CalculateEffectiveMonthlyPrice("yearly", 1200m, 0.2m);
        /// // Returns (1200 * 0.8) / 12 = 80.00
        /// </code>
        /// </example>
        public static decimal CalculateEffectiveMonthlyPrice(string billingCycle, decimal amount, decimal discountRate)
        {
            return billingCycle.ToLower() switch
            {
                "yearly" => Math.Round((amount * (1 - discountRate)) / 12, 2),
                "monthly" => amount,
                _ => throw new ArgumentException($"Invalid billing cycle: {billingCycle}")
            };
        }

        /// <summary>
        /// Calculates remaining days in current billing period
        /// </summary>
        /// <param name="startDate">Subscription activation date (UTC recommended)</param>
        /// <param name="endDate">Optional termination date (UTC recommended)
        /// <description>- When provided, overrides billing cycle calculation</description>
        /// </param>
        /// <param name="billingCycle">Billing frequency type
        /// <list type="bullet">
        /// <item><description>"yearly": 365-day billing period</description></item>
        /// <item><description>"monthly": 30-day billing period</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// <para>Days remaining until:</para>
        /// <list type="bullet">
        /// <item><description>Termination date (if provided)</description></item>
        /// <item><description>Next billing date (calculated from start date)</description></item>
        /// </list>
        /// <para>Returns 0 for:</para>
        /// <list type="bullet">
        /// <item><description>Invalid billing cycles</description></item>
        /// <item><description>Elapsed end dates</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>Date handling notes:</para>
        /// <list type="bullet">
        /// <item><description>Uses local server time for calculations</description></item>
        /// <item><description>Negative values indicate elapsed periods</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var daysLeft = CalculateRemainingDays(new DateTime(2025,1,1), null, "yearly");
        /// // Returns days between today and 2026-01-01
        /// </code>
        /// </example>
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
