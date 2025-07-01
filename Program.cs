using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using studentdb.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Database configuration
string? connectionString;

// Azure Key Vault integration (production)
if (!builder.Environment.IsDevelopment())
{
    var keyVaultEndpoint = new Uri(builder.Configuration["VaultUri"] 
        ?? throw new ArgumentNullException("VaultUri is not configured"));
    
    builder.Configuration.AddAzureKeyVault(
        keyVaultEndpoint,
        new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = builder.Configuration["AZURE_CLIENT_ID"]
        }));
    
    connectionString = builder.Configuration["StudentDbConnectionString"] 
        ?? throw new ArgumentNullException("StudentDbConnectionString is not configured");
}
else
{
    // Explicitly block local DB usage as per your requirement
    throw new InvalidOperationException("Local development is disabled. Use Azure deployment only.");
}

// DbContext configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 3)));

var app = builder.Build();

// Pipeline configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
