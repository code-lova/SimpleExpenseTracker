namespace SimpleExpenseTracker.Data
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // In real app, this should be hashed
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
