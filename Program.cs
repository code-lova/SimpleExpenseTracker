using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SimpleExpenseTracker;
using SimpleExpenseTracker.Services;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Blazored LocalStorage for browser persistence
builder.Services.AddBlazoredLocalStorage();

// Register notification service
builder.Services.AddScoped<NotificationService>();

// Register our services as scoped (since ILocalStorageService is scoped)
builder.Services.AddScoped<AuthService>(provider => 
{
    var localStorage = provider.GetRequiredService<ILocalStorageService>();
    return new AuthService(localStorage);
});
builder.Services.AddScoped<CategoryService>(provider => 
{
    var authService = provider.GetRequiredService<AuthService>();
    var localStorage = provider.GetRequiredService<ILocalStorageService>();
    var categoryService = new CategoryService(authService, localStorage);
    
    // Set up the circular dependency
    authService.SetCategoryService(categoryService);
    
    return categoryService;
});
builder.Services.AddScoped<TransactionService>(provider =>
{
    var authService = provider.GetRequiredService<AuthService>();
    var categoryService = provider.GetRequiredService<CategoryService>();
    var localStorage = provider.GetRequiredService<ILocalStorageService>();
    var transactionService = new TransactionService(authService, categoryService, localStorage);
    
    // Set up the circular dependency
    authService.SetTransactionService(transactionService);
    
    return transactionService;
});

await builder.Build().RunAsync();
