using SimpleExpenseTracker.Data;
using Blazored.LocalStorage;

namespace SimpleExpenseTracker.Services
{
    public class CategoryService
    {
        private readonly AuthService _authService;
        private readonly ILocalStorageService _localStorage;

        public CategoryService(AuthService authService, ILocalStorageService localStorage)
        {
            _authService = authService;
            _localStorage = localStorage;
        }

        public async Task CreateDefaultCategoriesForUserAsync(int userId)
        {
            var categories = await GetAllCategoriesAsync();
            
            // Check if user already has categories
            var hasCategories = categories.Any(c => c.UserId == userId);
            if (hasCategories) return;

            // Get starting ID
            int nextId = categories.Any() ? categories.Max(c => c.Id) + 1 : 1;

            // Add 4 expense categories and 4 income categories for this user
            var defaultCategories = new List<Category>
            {
                // 4 Expense Categories
                new() { Id = nextId++, Name = "Food & Dining", Description = "Meals, groceries, restaurants", Color = "#ff6b6b", Type = "Expense", UserId = userId },
                new() { Id = nextId++, Name = "Transportation", Description = "Gas, public transport, car maintenance", Color = "#4ecdc4", Type = "Expense", UserId = userId },
                new() { Id = nextId++, Name = "Entertainment", Description = "Movies, games, hobbies, fun activities", Color = "#45b7d1", Type = "Expense", UserId = userId },
                new() { Id = nextId++, Name = "Utilities & Bills", Description = "Electric, water, internet, phone", Color = "#f9ca24", Type = "Expense", UserId = userId },
                
                // 4 Income Categories
                new() { Id = nextId++, Name = "Salary", Description = "Regular employment income", Color = "#6c5ce7", Type = "Income", UserId = userId },
                new() { Id = nextId++, Name = "Freelance", Description = "Freelance and contract work", Color = "#a29bfe", Type = "Income", UserId = userId },
                new() { Id = nextId++, Name = "Investment", Description = "Dividends, interest, capital gains", Color = "#00b894", Type = "Income", UserId = userId },
                new() { Id = nextId++, Name = "Other Income", Description = "Gifts, bonuses, miscellaneous", Color = "#00cec9", Type = "Income", UserId = userId }
            };

            categories.AddRange(defaultCategories);
            await _localStorage.SetItemAsync("categories", categories);
        }

        private int GetNextId(List<Category> categories)
        {
            return categories.Any() ? categories.Max(c => c.Id) + 1 : 1;
        }

        private async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = await _localStorage.GetItemAsync<List<Category>>("categories");
            if (categories == null)
            {
                categories = new List<Category>();
                // Initialize with default categories for demo user
                await InitializeDemoDataAsync(categories);
            }
            return categories;
        }

        private async Task InitializeDemoDataAsync(List<Category> categories)
        {
            // Add default categories for demo user (ID = 1)
            var demoCategories = new List<Category>
            {
                // Income categories
                new() { Id = 1, Name = "Salary", Description = "Monthly salary and wages", Color = "#28a745", Type = "Income", UserId = 1 },
                new() { Id = 2, Name = "Freelance", Description = "Freelance work income", Color = "#17a2b8", Type = "Income", UserId = 1 },
                new() { Id = 3, Name = "Investment", Description = "Investment returns and dividends", Color = "#ffc107", Type = "Income", UserId = 1 },
                new() { Id = 4, Name = "Other Income", Description = "Other sources of income", Color = "#6c757d", Type = "Income", UserId = 1 },

                // Expense categories
                new() { Id = 5, Name = "Food & Dining", Description = "Restaurants, groceries, and food expenses", Color = "#dc3545", Type = "Expense", UserId = 1 },
                new() { Id = 6, Name = "Transportation", Description = "Gas, public transport, car maintenance", Color = "#fd7e14", Type = "Expense", UserId = 1 },
                new() { Id = 7, Name = "Shopping", Description = "Clothes, electronics, and general shopping", Color = "#e83e8c", Type = "Expense", UserId = 1 },
                new() { Id = 8, Name = "Entertainment", Description = "Movies, games, hobbies", Color = "#6f42c1", Type = "Expense", UserId = 1 },
                new() { Id = 9, Name = "Bills & Utilities", Description = "Rent, electricity, internet, phone", Color = "#20c997", Type = "Expense", UserId = 1 },
                new() { Id = 10, Name = "Healthcare", Description = "Medical expenses, insurance, pharmacy", Color = "#0dcaf0", Type = "Expense", UserId = 1 }
            };

            categories.AddRange(demoCategories);
            await _localStorage.SetItemAsync("categories", categories);
        }

        public async Task<List<Category>> GetCategoriesForCurrentUserAsync()
        {
            if (_authService.CurrentUser == null) return new List<Category>();
            
            var categories = await GetAllCategoriesAsync();
            return categories.Where(c => c.UserId == _authService.CurrentUser.Id).ToList();
        }

        public async Task<List<Category>> GetIncomeCategoriesForCurrentUserAsync()
        {
            if (_authService.CurrentUser == null) return new List<Category>();
            
            var categories = await GetAllCategoriesAsync();
            return categories.Where(c => c.UserId == _authService.CurrentUser.Id && c.Type == "Income").ToList();
        }

        public async Task<List<Category>> GetExpenseCategoriesForCurrentUserAsync()
        {
            if (_authService.CurrentUser == null) return new List<Category>();
            
            var categories = await GetAllCategoriesAsync();
            return categories.Where(c => c.UserId == _authService.CurrentUser.Id && c.Type == "Expense").ToList();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            if (_authService.CurrentUser == null) return null;
            
            var categories = await GetAllCategoriesAsync();
            return categories.FirstOrDefault(c => c.Id == id && c.UserId == _authService.CurrentUser.Id);
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            if (_authService.CurrentUser == null) return false;

            var categories = await GetAllCategoriesAsync();

            // Check if category name already exists for this user
            var exists = categories.Any(c => 
                c.Name.ToLower() == category.Name.ToLower() && 
                c.UserId == _authService.CurrentUser.Id);

            if (exists) return false;

            category.Id = GetNextId(categories);
            category.UserId = _authService.CurrentUser.Id;
            categories.Add(category);
            await _localStorage.SetItemAsync("categories", categories);

            return true;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            if (_authService.CurrentUser == null) return false;

            var categories = await GetAllCategoriesAsync();

            var existingCategory = categories.FirstOrDefault(c => c.Id == category.Id && c.UserId == _authService.CurrentUser.Id);
            if (existingCategory == null) return false;

            // Check if new name conflicts with another category
            var conflicts = categories.Any(c => 
                c.Id != category.Id && 
                c.Name.ToLower() == category.Name.ToLower() && 
                c.UserId == _authService.CurrentUser.Id);

            if (conflicts) return false;

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.Color = category.Color;
            existingCategory.Type = category.Type;

            await _localStorage.SetItemAsync("categories", categories);
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            if (_authService.CurrentUser == null) return false;

            var categories = await GetAllCategoriesAsync();

            var category = categories.FirstOrDefault(c => c.Id == categoryId && c.UserId == _authService.CurrentUser.Id);
            if (category == null) return false;

            categories.Remove(category);
            await _localStorage.SetItemAsync("categories", categories);
            return true;
        }
    }
}
