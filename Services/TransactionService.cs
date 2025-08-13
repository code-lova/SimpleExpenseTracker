using SimpleExpenseTracker.Data;
using Blazored.LocalStorage;

namespace SimpleExpenseTracker.Services
{
    public class TransactionService
    {
        private readonly AuthService _authService;
        private readonly CategoryService _categoryService;
        private readonly ILocalStorageService _localStorage;

        public TransactionService(AuthService authService, CategoryService categoryService, ILocalStorageService localStorage)
        {
            _authService = authService;
            _categoryService = categoryService;
            _localStorage = localStorage;
        }

        private async Task<List<Income>> GetAllIncomesAsync()
        {
            var incomes = await _localStorage.GetItemAsync<List<Income>>("incomes");
            return incomes ?? new List<Income>();
        }

        private async Task<List<Expense>> GetAllExpensesAsync()
        {
            var expenses = await _localStorage.GetItemAsync<List<Expense>>("expenses");
            return expenses ?? new List<Expense>();
        }

        private int GetNextIncomeId(List<Income> incomes)
        {
            return incomes.Any() ? incomes.Max(i => i.Id) + 1 : 1;
        }

        private int GetNextExpenseId(List<Expense> expenses)
        {
            return expenses.Any() ? expenses.Max(e => e.Id) + 1 : 1;
        }

        // Income Methods
        public async Task<List<Income>> GetIncomeForCurrentUserAsync()
        {
            if (_authService.CurrentUser == null) return new List<Income>();

            var incomes = await GetAllIncomesAsync();
            var userIncomes = incomes.Where(i => i.UserId == _authService.CurrentUser.Id).ToList();
            
            // Populate category information
            foreach (var income in userIncomes)
            {
                income.Category = await _categoryService.GetCategoryByIdAsync(income.CategoryId);
            }

            return userIncomes.OrderByDescending(i => i.Date).ToList();
        }

        public async Task<Income?> GetIncomeByIdAsync(int id)
        {
            if (_authService.CurrentUser == null) return null;

            var incomes = await GetAllIncomesAsync();
            var income = incomes.FirstOrDefault(i => i.Id == id && i.UserId == _authService.CurrentUser.Id);
            if (income != null)
            {
                income.Category = await _categoryService.GetCategoryByIdAsync(income.CategoryId);
            }
            return income;
        }

        public async Task<bool> AddIncomeAsync(Income income)
        {
            if (_authService.CurrentUser == null) return false;

            var incomes = await GetAllIncomesAsync();
            
            income.Id = GetNextIncomeId(incomes);
            income.UserId = _authService.CurrentUser.Id;
            incomes.Add(income);
            await _localStorage.SetItemAsync("incomes", incomes);

            return true;
        }

        public async Task<bool> UpdateIncomeAsync(Income income)
        {
            if (_authService.CurrentUser == null) return false;

            var incomes = await GetAllIncomesAsync();

            var existingIncome = incomes.FirstOrDefault(i => i.Id == income.Id && i.UserId == _authService.CurrentUser.Id);
            if (existingIncome == null) return false;

            existingIncome.Description = income.Description;
            existingIncome.Amount = income.Amount;
            existingIncome.Date = income.Date;
            existingIncome.CategoryId = income.CategoryId;
            existingIncome.Notes = income.Notes;

            await _localStorage.SetItemAsync("incomes", incomes);
            return true;
        }

        public async Task<bool> DeleteIncomeAsync(int incomeId)
        {
            if (_authService.CurrentUser == null) return false;

            var incomes = await GetAllIncomesAsync();

            var income = incomes.FirstOrDefault(i => i.Id == incomeId && i.UserId == _authService.CurrentUser.Id);
            if (income == null) return false;

            incomes.Remove(income);
            await _localStorage.SetItemAsync("incomes", incomes);
            return true;
        }

        // Expense Methods
        public async Task<List<Expense>> GetExpensesForCurrentUserAsync()
        {
            if (_authService.CurrentUser == null) return new List<Expense>();

            var expenses = await GetAllExpensesAsync();
            var userExpenses = expenses.Where(e => e.UserId == _authService.CurrentUser.Id).ToList();
            
            // Populate category information
            foreach (var expense in userExpenses)
            {
                expense.Category = await _categoryService.GetCategoryByIdAsync(expense.CategoryId);
            }

            return userExpenses.OrderByDescending(e => e.Date).ToList();
        }

        public async Task<Expense?> GetExpenseByIdAsync(int id)
        {
            if (_authService.CurrentUser == null) return null;

            var expenses = await GetAllExpensesAsync();
            var expense = expenses.FirstOrDefault(e => e.Id == id && e.UserId == _authService.CurrentUser.Id);
            if (expense != null)
            {
                expense.Category = await _categoryService.GetCategoryByIdAsync(expense.CategoryId);
            }
            return expense;
        }

        public async Task<bool> AddExpenseAsync(Expense expense)
        {
            if (_authService.CurrentUser == null) return false;

            var expenses = await GetAllExpensesAsync();
            
            expense.Id = GetNextExpenseId(expenses);
            expense.UserId = _authService.CurrentUser.Id;
            expenses.Add(expense);
            await _localStorage.SetItemAsync("expenses", expenses);

            return true;
        }

        public async Task<bool> UpdateExpenseAsync(Expense expense)
        {
            if (_authService.CurrentUser == null) return false;

            var expenses = await GetAllExpensesAsync();

            var existingExpense = expenses.FirstOrDefault(e => e.Id == expense.Id && e.UserId == _authService.CurrentUser.Id);
            if (existingExpense == null) return false;

            existingExpense.Description = expense.Description;
            existingExpense.Amount = expense.Amount;
            existingExpense.Date = expense.Date;
            existingExpense.CategoryId = expense.CategoryId;
            existingExpense.Notes = expense.Notes;

            await _localStorage.SetItemAsync("expenses", expenses);
            return true;
        }

        public async Task<bool> DeleteExpenseAsync(int expenseId)
        {
            if (_authService.CurrentUser == null) return false;

            var expenses = await GetAllExpensesAsync();

            var expense = expenses.FirstOrDefault(e => e.Id == expenseId && e.UserId == _authService.CurrentUser.Id);
            if (expense == null) return false;

            expenses.Remove(expense);
            await _localStorage.SetItemAsync("expenses", expenses);
            return true;
        }

        // Summary Methods
        public async Task<decimal> GetTotalIncomeAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (_authService.CurrentUser == null) return 0;

            var incomes = await GetAllIncomesAsync();
            var query = incomes.Where(i => i.UserId == _authService.CurrentUser.Id);

            if (startDate.HasValue)
                query = query.Where(i => i.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(i => i.Date <= endDate.Value);

            return query.Sum(i => i.Amount);
        }

        public async Task<decimal> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (_authService.CurrentUser == null) return 0;

            var expenses = await GetAllExpensesAsync();
            var query = expenses.Where(e => e.UserId == _authService.CurrentUser.Id);

            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate.Value);

            return query.Sum(e => e.Amount);
        }

        public async Task<decimal> GetNetIncomeAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var totalIncome = await GetTotalIncomeAsync(startDate, endDate);
            var totalExpenses = await GetTotalExpensesAsync(startDate, endDate);
            return totalIncome - totalExpenses;
        }
    }
}
