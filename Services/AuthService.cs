using SimpleExpenseTracker.Data;
using Blazored.LocalStorage;

namespace SimpleExpenseTracker.Services
{
    public class AuthService
    {
        private readonly ILocalStorageService _localStorage;
        private User? _currentUser;
        private CategoryService? _categoryService;
        private TransactionService? _transactionService;

        public event Action? StateHasChanged;

        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        // Method to set services after all services are created
        public void SetCategoryService(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public void SetTransactionService(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public AuthService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return new AuthResult { Success = false, Error = "Username is required." };

                if (string.IsNullOrWhiteSpace(password))
                    return new AuthResult { Success = false, Error = "Password is required." };

                var users = await GetAllUsersAsync();
                
                var user = users.FirstOrDefault(u => 
                    u.Username.ToLower() == username.ToLower() && 
                    u.Password == password);

                if (user != null)
                {
                    _currentUser = user;
                    StateHasChanged?.Invoke();
                    return new AuthResult { Success = true, User = user };
                }

                return new AuthResult { Success = false, Error = "Invalid username or password." };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return new AuthResult { Success = false, Error = "An unexpected error occurred during login. Please try again." };
            }
        }

        public async Task<AuthResult> RegisterAsync(string username, string email, string password)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(username))
                    return new AuthResult { Success = false, Error = "Username is required." };

                if (string.IsNullOrWhiteSpace(email))
                    return new AuthResult { Success = false, Error = "Email is required." };

                if (string.IsNullOrWhiteSpace(password))
                    return new AuthResult { Success = false, Error = "Password is required." };

                if (username.Length < 3)
                    return new AuthResult { Success = false, Error = "Username must be at least 3 characters long." };

                if (password.Length < 6)
                    return new AuthResult { Success = false, Error = "Password must be at least 6 characters long." };

                if (!IsValidEmail(email))
                    return new AuthResult { Success = false, Error = "Please enter a valid email address." };

                var users = await GetAllUsersAsync();

                // Check if user already exists
                var existingUsername = users.Any(u => u.Username.ToLower() == username.ToLower());
                if (existingUsername)
                    return new AuthResult { Success = false, Error = "Username is already taken. Please choose a different username." };

                var existingEmail = users.Any(u => u.Email.ToLower() == email.ToLower());
                if (existingEmail)
                    return new AuthResult { Success = false, Error = "Email is already registered. Please use a different email or try logging in." };

                var newUser = new User
                {
                    Id = users.Any() ? users.Max(u => u.Id) + 1 : 1,
                    Username = username,
                    Email = email,
                    Password = password, // In real app, hash this!
                    CreatedDate = DateTime.Now
                };

                users.Add(newUser);
                await _localStorage.SetItemAsync("users", users);
            
                _currentUser = newUser;
            
                // Create default categories for the new user
                try
                {
                    if (_categoryService != null)
                    {
                        await _categoryService.CreateDefaultCategoriesForUserAsync(newUser.Id);
                    }
                    else
                    {
                        Console.WriteLine("CategoryService is null during registration");
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the registration
                    Console.WriteLine($"Failed to create default categories for user {newUser.Id}: {ex.Message}");
                }
            
                StateHasChanged?.Invoke();

                return new AuthResult { Success = true, User = newUser };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return new AuthResult { Success = false, Error = "An unexpected error occurred during registration. Please try again." };
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await Task.Delay(50); // Simulate async operation
            _currentUser = null;
            StateHasChanged?.Invoke();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = await _localStorage.GetItemAsync<List<User>>("users");
            if (users == null)
            {
                // Initialize empty user list - no demo user
                users = new List<User>();
                await _localStorage.SetItemAsync("users", users);
            }
            return users;
        }
    }
}
