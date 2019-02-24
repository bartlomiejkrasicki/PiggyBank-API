using System;

namespace PiggyBank_API.Models
{
    public class Transaction
    {
        public DateTime AddDate { get; set; }
        public double Amount { get; set; }
        public string Category { get; set; }
        public int Id { get; set; }
        public string Type { get; set; }
    }
}