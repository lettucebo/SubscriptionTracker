using System;
using System.ComponentModel.DataAnnotations;

namespace SubscriptionTracker.Models
{
    /// <summary>
    /// Represents a subscription service with details about cost and payment dates
    /// </summary>
    public class Subscription
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "服務名稱是必填的")]
        [Display(Name = "服務名稱")]
        public string Name { get; set; }

        [Required(ErrorMessage = "費用是必填的")]
        [Range(0.01, double.MaxValue, ErrorMessage = "費用必須大於0")]
        [Display(Name = "月費")]
        [DataType(DataType.Currency)]
        public decimal Cost { get; set; }

        [Required(ErrorMessage = "繳費日期是必填的")]
        [Display(Name = "繳費日期")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Calculates the number of days remaining until the next payment date
        /// </summary>
        public int DaysRemaining 
        { 
            get 
            { 
                // Calculate next payment date based on the day of month
                var today = DateTime.Today;
                var nextPaymentDate = new DateTime(today.Year, today.Month, PaymentDate.Day);
                
                // If the payment date for this month has passed, get next month's payment date
                if (nextPaymentDate < today)
                {
                    nextPaymentDate = nextPaymentDate.AddMonths(1);
                }
                
                return (nextPaymentDate - today).Days;
            } 
        }
    }
}