# Example: Using EmailTemplate Module in Your Module

## Simple Integration Example

This is a **complete, copy-paste ready example** showing how to use the EmailTemplate module from another Oqtane module.

---

## File Structure

```
YourModule/
|-- Shared/
|   +-- Models/
|       +-- UserRegistration.cs
|
|-- Server/
|   |-- Services/
|   |   +-- RegistrationService.cs    # Email integration here
|   |-- Controllers/
|   |   +-- RegistrationController.cs
|   +-- Startup/
|       +-- ServerStartup.cs
|
+-- Client/
    +-- Modules/YourModule/
        +-- Register.razor
```

---

## Complete Code Example

### 1. Server Service (Business Logic)

```csharp
// Server/Services/RegistrationService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Amazing.Module.EmailTemplate.Services;  // :arrow_left: Import this

namespace YourModule.Services
{
    public interface IRegistrationService
    {
        Task<User> RegisterUserAsync(string username, string email, string firstName, string lastName, int siteId);
    }

    public class ServerRegistrationService : IRegistrationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailSendingService _emailService;  // :arrow_left: Inject this
        private readonly ILogManager _logger;

        public ServerRegistrationService(
            IUserRepository userRepository,
            IEmailSendingService emailService,  // :arrow_left: Add to constructor
            ILogManager logger)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<User> RegisterUserAsync(string username, string email, string firstName, string lastName, int siteId)
        {
            try
            {
                // Step 1: Create user
                var user = _userRepository.AddUser(new User
                {
                    SiteId = siteId,
                    Username = username,
                    Email = email,
                    DisplayName = $"{firstName} {lastName}",
                    EmailConfirmed = false
                });

                _logger.Log(LogLevel.Information, this, LogFunction.Create, 
                    "User Registered {Username}", username);

                // Step 2: Send welcome email using template
                var variables = new Dictionary<string, string>
                {
                    { "FirstName", firstName },
                    { "LastName", lastName },
                    { "Email", email },
                    { "Username", username }
                };

                var emailResult = await _emailService.SendEmailByTemplateNameAsync(
                    siteId,
                    "User Welcome Email",  // :arrow_left: Template name
                    email,
                    variables,
                    user.DisplayName
                );

                // Step 3: Log result (don't fail registration if email fails)
                if (emailResult.Success)
                {
                    _logger.Log(LogLevel.Information, this, LogFunction.Create,
                        "Welcome Email Sent To {Email}, NotificationId: {Id}", 
                        email, emailResult.NotificationId);
                }
                else
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Create,
                        "Welcome Email Failed For {Email}: {Error}", 
                        email, emailResult.Message);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, 
                    "Error Registering User {Username} {Error}", username, ex.Message);
                throw;
            }
        }
    }
}
```

---

### 2. Controller (API)

```csharp
// Server/Controllers/RegistrationController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using YourModule.Services;

namespace YourModule.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class RegistrationController : ModuleControllerBase
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(
            IRegistrationService registrationService, 
            ILogManager logger) : base(logger)
        {
            _registrationService = registrationService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<User> Post([FromBody] RegistrationRequest request)
        {
            return await _registrationService.RegisterUserAsync(
                request.Username,
                request.Email,
                request.FirstName,
                request.LastName,
                request.SiteId
            );
        }
    }
}
```

---

### 3. Startup (Service Registration)

```csharp
// Server/Startup/ServerStartup.cs
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using YourModule.Services;

namespace YourModule.Startup
{
    public class ServerStartup : IServerStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IRegistrationService, ServerRegistrationService>();
            // IEmailSendingService is already registered by EmailTemplate module
        }

        // ... other methods ...
    }
}
```

**Note**: You DON'T need to register `IEmailSendingService` - it's already registered globally by the EmailTemplate module's ServerStartup.

---

### 4. Client UI Component

```razor
<!-- Client/Modules/YourModule/Register.razor -->
@using YourModule.Services
@namespace YourModule
@inherits ModuleBase
@inject IRegistrationService RegistrationService

<form>
    <div class="mb-3">
        <label>Username:</label>
        <input class="form-control" @bind="_username" />
    </div>
    <div class="mb-3">
        <label>Email:</label>
        <input type="email" class="form-control" @bind="_email" />
    </div>
    <div class="mb-3">
        <label>First Name:</label>
        <input class="form-control" @bind="_firstName" />
    </div>
    <div class="mb-3">
        <label>Last Name:</label>
        <input class="form-control" @bind="_lastName" />
    </div>
    <button type="button" class="btn btn-primary" @onclick="Register">
        Register
    </button>
</form>

@code {
    private string _username = "";
    private string _email = "";
    private string _firstName = "";
    private string _lastName = "";

    private async Task Register()
    {
        try
        {
            var user = await RegistrationService.RegisterUserAsync(
                _username,
                _email,
                _firstName,
                _lastName,
                PageState.Site.SiteId
            );

            AddModuleMessage(
                "Registration successful! Check your email for welcome message.", 
                MessageType.Success
            );
        }
        catch (Exception ex)
        {
            await logger.LogError(ex, "Registration error");
            AddModuleMessage("Registration failed", MessageType.Error);
        }
    }
}
```

---

## Required Email Template

Site admin creates this template in EmailTemplate module:

**Template Details**:

- **Name**: `User Welcome Email`
- **Category**: `Registration`
- **Description**: `Sent when new user registers`
- **Subject**: `Welcome to Our Site, {{FirstName}}!`
- **TemplateVariables**: `{{FirstName}},{{LastName}},{{Email}},{{Username}}`
- **Active**: :white_check_mark: Checked
- **Body** (use Rich Text Editor):

```html
<h1>Welcome {{FirstName}} {{LastName}}!</h1>

<p>Thank you for registering on our site.</p>

<p><strong>Your Account Details:</strong></p>
<ul>
    <li>Username: {{Username}}</li>
    <li>Email: {{Email}}</li>
</ul>

<p>Please verify your email address by clicking the link we sent you.</p>

<p>Best regards,<br>
The Team</p>
```

---

## Testing the Integration

### Step 1: Install Both Modules

1. Install EmailTemplate module
2. Install Your module
3. Both are now on the site

### Step 2: Create Template

1. Add EmailTemplate module to admin page
2. Create "User Welcome Email" template
3. Test it using "Send Test Email" feature
4. Verify email arrives
5. Set Active = true

### Step 3: Test Your Module

1. Add Your module to a page
2. Register a test user
3. Check logs: "Welcome Email Sent To..."
4. Check email inbox (1-2 minutes)
5. Verify personalized content

---

## Variable Mapping Helper

Create a helper class for consistent variable mapping:

```csharp
// Server/Helpers/EmailVariableHelper.cs
namespace YourModule.Helpers
{
    public static class EmailVariableHelper
    {
        public static Dictionary<string, string> CreateUserVariables(User user)
        {
            return new Dictionary<string, string>
            {
                { "FirstName", user.FirstName ?? string.Empty },
                { "LastName", user.LastName ?? string.Empty },
                { "Email", user.Email ?? string.Empty },
                { "Username", user.Username ?? string.Empty },
                { "DisplayName", user.DisplayName ?? string.Empty }
            };
        }

        public static Dictionary<string, string> AddCustomVariables(
            Dictionary<string, string> baseVariables,
            params (string key, string value)[] additionalVariables)
        {
            var result = new Dictionary<string, string>(baseVariables);
            
            foreach (var (key, value) in additionalVariables)
            {
                result[key] = value ?? string.Empty;
            }

            return result;
        }
    }
}
```

**Usage**:

```csharp
var variables = EmailVariableHelper.CreateUserVariables(user);
variables = EmailVariableHelper.AddCustomVariables(
    variables,
    ("ActivationLink", activationUrl),
    ("SiteName", siteName)
);

await _emailService.SendEmailByTemplateNameAsync(siteId, templateName, email, variables);
```

---

## Migration from Hardcoded Emails

### Before (Hardcoded)

```csharp
var smtp = new SmtpClient("smtp.gmail.com", 587);
smtp.Credentials = new NetworkCredential("user", "pass");

var message = new MailMessage
{
    From = new MailAddress("noreply@site.com"),
    Subject = $"Welcome {user.FirstName}!",
    Body = $"<h1>Welcome {user.FirstName} {user.LastName}!</h1><p>Thanks for joining!</p>",
    IsBodyHtml = true
};
message.To.Add(user.Email);

await smtp.SendMailAsync(message);
```

### After (Template-Based)

```csharp
var variables = new Dictionary<string, string>
{
    { "FirstName", user.FirstName },
    { "LastName", user.LastName }
};

await _emailService.SendEmailByTemplateNameAsync(
    user.SiteId,
    "User Welcome Email",
    user.Email,
    variables
);
```

**Benefits**:

- No SMTP configuration in code
- Template editable without redeployment
- Uses Oqtane's notification infrastructure
- Consistent styling across all emails
- Logging and audit trail
- Site-scoped reusability (v1.0.2)

---

## Support

### Common Integration Issues

**Q: Service not found in DI container?**  
A: Ensure EmailTemplate module is installed and ServerStartup.ConfigureServices registers it.

**Q: Template not found at runtime?**  
A: Template name is case-sensitive. Verify exact name in EmailTemplate module. Verify template exists for the correct site.

**Q: Variables not replacing?**  
A: Dictionary keys must match template exactly: `{{FirstName}}` requires `variables["FirstName"]`

**Q: Email not arriving?**  
A: Check SMTP configuration and NotificationJob. Email sending is asynchronous (1-2 min delay).

**Q: Can I use templates from another site?**  
A: No. Templates are site-scoped (v1.0.2) for security and multi-tenant isolation.

---

## Site Scoping Notes (v1.0.2)

### Key Points

- Templates are **site-scoped** (not module-scoped)
- All modules in a site can access templates
- Templates isolated per site (multi-tenant)
- Create template once per site, use everywhere
- Site admin manages templates centrally

### Example

```
Site A (SiteId=1):
  - EmailTemplate: "Welcome Email" (SiteId=1)
  - All modules in Site A can use this template
  
Site B (SiteId=2):
  - EmailTemplate: "Welcome Email" (SiteId=2)
  - Separate template, isolated from Site A
```

---

## Next Steps

1. **Review INTEGRATION_GUIDE.md** for complete API reference
2. **Create your templates** in EmailTemplate module UI
3. **Add service injection** to your module
4. **Test end-to-end** with real email delivery
5. **Document variables** your templates need

---

**You're ready to integrate! Start sending templated emails from your module now!**
