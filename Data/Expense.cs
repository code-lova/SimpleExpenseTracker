namespace SimpleExpenseTracker.Data
{
    public class Expense
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string? Notes { get; set; } = string.Empty;
        
        // Navigation properties (for easier data access)
        public Category? Category { get; set; }
    }
}
