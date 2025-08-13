namespace SimpleExpenseTracker.Data
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#007bff"; // Default blue color
        public string Type { get; set; } = "Expense"; // "Income" or "Expense"
        public int UserId { get; set; } // Each user has their own categories
    }
}
