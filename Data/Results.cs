namespace SimpleExpenseTracker.Data
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public User? User { get; set; }
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; set; }
    }

    public class SortOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class DateRange
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
