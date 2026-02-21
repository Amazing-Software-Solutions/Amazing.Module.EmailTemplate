# EmailTemplate Module - Integration Guide for Developers

## ?? Purpose

This guide shows **other Oqtane module developers** how to use the EmailTemplate module to send templated emails from their modules.

---

## ?? **Why Use This Module?**

Instead of:
- ? Writing SMTP code in every module
- ? Hardcoding email templates in code
- ? Managing HTML email formatting manually
- ? Duplicating variable replacement logic

You get:
- ? **Centralized email templates** - Managed via UI
- ? **Rich HTML editing** - WYSIWYG editor
- ? **Variable replacement** - Dynamic content
- ? **SMTP abstraction** - Uses Oqtane's notification system
- ? **Template reusability** - One template, many uses
- ? **Non-technical user management** - Site admins can edit templates

---

## ?? Quick Start

### Step 1: Inject the Service

In your module's **Server Service**, inject `IEmailSendingService`:

```csharp
using Amazing.Module.EmailTemplate.Services;

namespace YourModule.Services
{
    public class YourModuleService
    {
        private readonly IEmailSendingService _emailService;

        public YourModuleService(IEmailSendingService emailService)
        {
            _emailService = emailService;
        }

        // Your methods...
    }
}
```

### Step 2: Send Email

```csharp
public async Task SendWelcomeEmail(User user)
{
    var variables = new Dictionary<string, string>
    {
        { "FirstName", user.FirstName },
        { "LastName", user.LastName },
        { "Email", user.Email }
    };

    var result = await _emailService.SendEmailByTemplateNameAsync(
        siteId: user.SiteId,
        templateName: "User Welcome Email",
        toEmail: user.Email,
        variables: variables,
        toDisplayName: user.DisplayName
    );

    if (!result.Success)
    {
        // Handle error
        _logger.LogError("Email failed: {Message}", result.Message);
    }
}
```

---

## ?? **Complete API Reference**

### **Method 1: Send by Template Name** (Recommended)

```csharp
Task<EmailSendResult> SendEmailByTemplateNameAsync(
    int siteId,              // Current site ID
    string templateName,     // Template name (e.g., "User Welcome Email")
    string toEmail,          // Recipient email
    Dictionary<string, string> variables,  // Variables to replace
    string toDisplayName = null  // Optional recipient display name
)
```

**When to use**: When template name is known and stable.

**Example**:
```csharp
await _emailService.SendEmailByTemplateNameAsync(
    PageState.Site.SiteId,
    "Order Confirmation",
    customer.Email,
    new Dictionary<string, string>
    {
        { "OrderNumber", order.Id.ToString() },
        { "OrderTotal", order.Total.ToString("C") },
        { "CustomerName", customer.Name }
    },
    customer.Name
);
```

---

### **Method 2: Send by Template ID**

```csharp
Task<EmailSendResult> SendEmailByTemplateIdAsync(
    int siteId,
    int templateId,          // Specific template ID
    string toEmail,
    Dictionary<string, string> variables,
    string toDisplayName = null
)
```

**When to use**: When template ID is stored in configuration or database.

**Example**:
```csharp
// Template ID stored in module settings
int templateId = int.Parse(await _settingService.GetModuleSettingAsync(
    moduleId, 
    "WelcomeEmailTemplateId", 
    "0"
));

await _emailService.SendEmailByTemplateIdAsync(
    siteId,
    templateId,
    user.Email,
    variables
);
```

---

### **Method 3: Get Available Templates**

```csharp
Task<List<TemplateInfo>> GetAvailableTemplatesAsync(int siteId)
```

**Returns**: List of all active templates with metadata.

**Example**:
```csharp
var templates = await _emailService.GetAvailableTemplatesAsync(siteId);

foreach (var template in templates)
{
    Console.WriteLine($"{template.Name} - {template.Category}");
    Console.WriteLine($"Variables: {string.Join(", ", template.RequiredVariables)}");
}
```

---

### **Method 4: Get Templates by Category**

```csharp
Task<List<TemplateInfo>> GetTemplatesByCategoryAsync(
    int siteId,
    string category          // Category name (e.g., "Onboarding")
)
```

**When to use**: When organizing templates by functional area.

**Example**:
```csharp
// Get all onboarding email templates
var onboardingTemplates = await _emailService.GetTemplatesByCategoryAsync(
    siteId, 
    "Onboarding"
);

// Use first template
if (onboardingTemplates.Any())
{
    await _emailService.SendEmailByTemplateIdAsync(
        siteId,
        onboardingTemplates.First().TemplateId,
        user.Email,
        variables
    );
}
```

---

## ?? **Response Model: EmailSendResult**

```csharp
public class EmailSendResult
{
    public bool Success { get; set; }       // True if queued successfully
    public string Message { get; set; }     // Success/error message
    public int? NotificationId { get; set; } // Oqtane Notification ID (for tracking)
}
```

**Success Response**:
```csharp
{
    Success = true,
    Message = "Email queued for delivery",
    NotificationId = 12345
}
```

**Error Responses**:
```csharp
// Template not found
{ Success = false, Message = "Template 'XYZ' not found or inactive", NotificationId = null }

// Template inactive
{ Success = false, Message = "Template 'XYZ' is inactive", NotificationId = null }

// Site mismatch
{ Success = false, Message = "Site ID mismatch - cannot send cross-site emails", NotificationId = null }
```

---

## ?? **Template Info Model**

```csharp
public class TemplateInfo
{
    public int TemplateId { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public List<string> RequiredVariables { get; set; }  // Parsed from TemplateVariables
}
```

**Example**:
```csharp
{
    TemplateId = 5,
    Name = "User Welcome Email",
    Category = "Onboarding",
    Description = "Sent to new users after registration",
    RequiredVariables = ["FirstName", "LastName", "Email"]
}
```

---

## ?? **Usage Patterns**

### **Pattern 1: Simple Send (No Variables)**

```csharp
await _emailService.SendEmailByTemplateNameAsync(
    siteId,
    "Simple Notification",
    user.Email,
    null  // No variables
);
```

---

### **Pattern 2: With Variables**

```csharp
var variables = new Dictionary<string, string>
{
    { "FirstName", user.FirstName },
    { "LastName", user.LastName },
    { "Email", user.Email },
    { "ActivationLink", activationUrl }
};

await _emailService.SendEmailByTemplateNameAsync(
    siteId,
    "Account Activation",
    user.Email,
    variables,
    user.DisplayName
);
```

---

### **Pattern 3: Dynamic Template Selection**

```csharp
public async Task SendUserNotification(User user, string action)
{
    string templateName = action switch
    {
        "register" => "User Welcome Email",
        "reset" => "Password Reset",
        "activate" => "Account Activation",
        _ => "General Notification"
    };

    await _emailService.SendEmailByTemplateNameAsync(
        user.SiteId,
        templateName,
        user.Email,
        GetUserVariables(user)
    );
}
```

---

### **Pattern 4: Error Handling**

```csharp
var result = await _emailService.SendEmailByTemplateNameAsync(
    siteId,
    templateName,
    user.Email,
    variables
);

if (result.Success)
{
    _logger.LogInformation(
        "Email sent to {Email} using template {Template}, NotificationId: {Id}",
        user.Email,
        templateName,
        result.NotificationId
    );
}
else
{
    _logger.LogError(
        "Failed to send email to {Email}: {Error}",
        user.Email,
        result.Message
    );
    
    // Fallback: Send plain text notification
    await SendFallbackNotification(user, result.Message);
}
```

---

### **Pattern 5: Template Discovery**

```csharp
// Get all available templates
var templates = await _emailService.GetAvailableTemplatesAsync(siteId);

// Let user select template via dropdown
var templateOptions = templates
    .OrderBy(t => t.Category)
    .ThenBy(t => t.Name)
    .Select(t => new 
    { 
        Value = t.TemplateId,
        Text = $"{t.Category} - {t.Name}"
    });

// Use selected template
await _emailService.SendEmailByTemplateIdAsync(
    siteId,
    selectedTemplateId,
    recipient.Email,
    variables
);
```

---

### **Pattern 6: Bulk Sending**

```csharp
public async Task SendBulkEmails(List<User> users, string templateName)
{
    foreach (var user in users)
    {
        var variables = new Dictionary<string, string>
        {
            { "FirstName", user.FirstName },
            { "LastName", user.LastName },
            { "Email", user.Email }
        };

        var result = await _emailService.SendEmailByTemplateNameAsync(
            user.SiteId,
            templateName,
            user.Email,
            variables,
            user.DisplayName
        );

        if (!result.Success)
        {
            _logger.LogWarning("Email to {User} failed: {Error}", 
                user.Username, result.Message);
        }

        // Add delay to avoid overwhelming SMTP server
        await Task.Delay(100);
    }
}
```

---

## ?? **Real-World Examples**

### **Example 1: User Registration Module**

```csharp
namespace YourModule.Registration.Services
{
    public class RegistrationService
    {
        private readonly IEmailSendingService _emailService;
        private readonly IUserService _userService;

        public RegistrationService(
            IEmailSendingService emailService,
            IUserService userService)
        {
            _emailService = emailService;
            _userService = userService;
        }

        public async Task<User> RegisterUserAsync(RegistrationDto dto, int siteId)
        {
            // Create user
            var user = await _userService.AddUserAsync(dto.ToUser(), siteId);

            // Send welcome email
            var variables = new Dictionary<string, string>
            {
                { "FirstName", user.FirstName },
                { "LastName", user.LastName },
                { "Email", user.Email },
                { "Username", user.Username },
                { "SiteName", dto.SiteName },
                { "ActivationLink", GenerateActivationLink(user) }
            };

            var result = await _emailService.SendEmailByTemplateNameAsync(
                siteId,
                "User Welcome Email",
                user.Email,
                variables,
                user.DisplayName
            );

            return user;
        }
    }
}
```

---

### **Example 2: Ecommerce Order Module**

```csharp
namespace YourModule.Ecommerce.Services
{
    public class OrderService
    {
        private readonly IEmailSendingService _emailService;

        public OrderService(IEmailSendingService emailService)
        {
            _emailService = emailService;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            // Save order to database
            order = await SaveOrder(order);

            // Send confirmation email
            var variables = new Dictionary<string, string>
            {
                { "CustomerName", order.CustomerName },
                { "OrderNumber", order.OrderNumber },
                { "OrderDate", order.OrderDate.ToString("MM/dd/yyyy") },
                { "OrderTotal", order.Total.ToString("C") },
                { "ItemCount", order.Items.Count.ToString() },
                { "ShippingAddress", FormatAddress(order.ShippingAddress) },
                { "TrackingUrl", GetTrackingUrl(order) }
            };

            await _emailService.SendEmailByTemplateNameAsync(
                order.SiteId,
                "Order Confirmation",
                order.CustomerEmail,
                variables,
                order.CustomerName
            );

            return order;
        }

        public async Task SendOrderShippedEmail(Order order)
        {
            var variables = new Dictionary<string, string>
            {
                { "CustomerName", order.CustomerName },
                { "OrderNumber", order.OrderNumber },
                { "TrackingNumber", order.TrackingNumber },
                { "Carrier", order.ShippingCarrier },
                { "TrackingUrl", GetCarrierTrackingUrl(order) }
            };

            await _emailService.SendEmailByTemplateNameAsync(
                order.SiteId,
                "Order Shipped",
                order.CustomerEmail,
                variables,
                order.CustomerName
            );
        }
    }
}
```

---

### **Example 3: Password Reset Module**

```csharp
public class PasswordResetService
{
    private readonly IEmailSendingService _emailService;

    public PasswordResetService(IEmailSendingService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendPasswordResetEmailAsync(User user, string resetToken)
    {
        var resetUrl = $"https://{_alias.Name}/reset-password?token={resetToken}";
        var expiryTime = DateTime.Now.AddHours(24);

        var variables = new Dictionary<string, string>
        {
            { "FirstName", user.FirstName },
            { "Email", user.Email },
            { "ResetLink", resetUrl },
            { "ExpiryTime", expiryTime.ToString("g") },
            { "ExpiryHours", "24" }
        };

        var result = await _emailService.SendEmailByTemplateNameAsync(
            user.SiteId,
            "Password Reset Request",
            user.Email,
            variables,
            user.DisplayName
        );

        if (!result.Success)
        {
            throw new Exception($"Failed to send password reset email: {result.Message}");
        }
    }
}
```

---

### **Example 4: Event Notification Module**

```csharp
public class EventNotificationService
{
    private readonly IEmailSendingService _emailService;

    public async Task SendEventReminder(Event evt, List<User> attendees)
    {
        // Get reminder template
        var templates = await _emailService.GetTemplatesByCategoryAsync(
            evt.SiteId, 
            "Events"
        );

        var reminderTemplate = templates.FirstOrDefault(t => t.Name.Contains("Reminder"));

        if (reminderTemplate == null)
        {
            _logger.LogWarning("Event reminder template not found");
            return;
        }

        // Send to all attendees
        foreach (var attendee in attendees)
        {
            var variables = new Dictionary<string, string>
            {
                { "AttendeeFirstName", attendee.FirstName },
                { "EventName", evt.Name },
                { "EventDate", evt.StartDate.ToString("MMMM dd, yyyy") },
                { "EventTime", evt.StartDate.ToString("h:mm tt") },
                { "EventLocation", evt.Location },
                { "EventLink", GetEventUrl(evt) }
            };

            await _emailService.SendEmailByTemplateIdAsync(
                evt.SiteId,
                reminderTemplate.TemplateId,
                attendee.Email,
                variables,
                attendee.DisplayName
            );

            await Task.Delay(100); // Rate limiting
        }
    }
}
```

---

## ?? **Advanced Scenarios**

### **Scenario 1: Template Settings in Module**

Allow users to select which template your module uses:

```csharp
// In your module's Settings.razor
<Section Name="EmailSettings" Heading="Email Settings">
    <div class="row mb-1">
        <Label Class="col-sm-3">Welcome Email Template:</Label>
        <div class="col-sm-9">
            <select class="form-select" @bind="_welcomeTemplateId">
                <option value="0">-- Select Template --</option>
                @foreach (var template in _emailTemplates)
                {
                    <option value="@template.TemplateId">
                        @template.Category - @template.Name
                    </option>
                }
            </select>
        </div>
    </div>
</Section>

@code {
    private List<TemplateInfo> _emailTemplates;
    private int _welcomeTemplateId;

    protected override async Task OnInitializedAsync()
    {
        // Get available templates (client-side needs API call)
        // Or store in settings and use server-side
        _emailTemplates = await EmailSendingService.GetAvailableTemplatesAsync(
            PageState.Site.SiteId
        );

        // Load saved setting
        _welcomeTemplateId = int.Parse(
            await SettingService.GetModuleSettingAsync(
                ModuleState.ModuleId,
                "WelcomeEmailTemplateId",
                "0"
            )
        );
    }

    private async Task SaveSettings()
    {
        await SettingService.SetModuleSettingAsync(
            ModuleState.ModuleId,
            "WelcomeEmailTemplateId",
            _welcomeTemplateId.ToString()
        );
    }
}
```

---

### **Scenario 2: Conditional Template Usage**

```csharp
public async Task SendNotification(User user, NotificationType type)
{
    var templates = await _emailService.GetAvailableTemplatesAsync(user.SiteId);

    string templateName = type switch
    {
        NotificationType.Welcome => "User Welcome Email",
        NotificationType.PasswordReset => "Password Reset Request",
        NotificationType.AccountLocked => "Account Locked",
        NotificationType.SubscriptionExpiring => "Subscription Expiring",
        _ => "General Notification"
    };

    // Check if template exists
    if (!templates.Any(t => t.Name == templateName))
    {
        _logger.LogWarning("Template {Template} not found, using default", templateName);
        templateName = "General Notification";
    }

    await _emailService.SendEmailByTemplateNameAsync(
        user.SiteId,
        templateName,
        user.Email,
        BuildVariables(user, type)
    );
}
```

---

### **Scenario 3: Multi-Language Templates**

```csharp
public async Task SendLocalizedEmail(User user, string baseTemplateName)
{
    // Template naming convention: "TemplateName-en", "TemplateName-es", etc.
    var userLanguage = await GetUserLanguage(user);
    var localizedTemplateName = $"{baseTemplateName}-{userLanguage}";

    // Try localized template first
    var result = await _emailService.SendEmailByTemplateNameAsync(
        user.SiteId,
        localizedTemplateName,
        user.Email,
        variables
    );

    // Fallback to default if localized not found
    if (!result.Success && result.Message.Contains("not found"))
    {
        await _emailService.SendEmailByTemplateNameAsync(
            user.SiteId,
            baseTemplateName,  // Default English template
            user.Email,
            variables
        );
    }
}
```

---

## ?? **Security & Authorization**

### **No Permission Required to Send**

The `IEmailSendingService` is **site-scoped**, not permission-based:
- ? Any authenticated module can send emails
- ? Site ID validated (no cross-site sending)
- ? Template must be Active
- ? All sends are logged for audit

**Rationale**: Email sending is a functional capability, not a security boundary. Authorization should be at the **calling module level** (e.g., only admins can trigger password resets).

### **Best Practices**:

```csharp
// ? GOOD: Authorization in your module
[Authorize(Policy = PolicyNames.EditModule)]
public async Task SendAdminAlert(int moduleId, string message)
{
    // Authorization checked above
    await _emailService.SendEmailByTemplateNameAsync(...);
}

// ? BAD: No authorization check
public async Task SendEmail(string to, string template)
{
    // Anyone can call this!
    await _emailService.SendEmailByTemplateNameAsync(...);
}
```

---

## ?? **Error Handling**

### **Common Errors**:

| Error Message | Cause | Solution |
|---------------|-------|----------|
| Template 'XYZ' not found | Template doesn't exist or typo | Check template name in EmailTemplate module |
| Template 'XYZ' is inactive | Template exists but IsActive = false | Activate template in EmailTemplate module |
| Site ID mismatch | Trying to use template from different site | Use correct siteId parameter |
| Email queued but not delivered | SMTP not configured | Configure SMTP in Admin ? Site Settings |

### **Defensive Coding**:

```csharp
public async Task SendEmailSafely(User user)
{
    try
    {
        var result = await _emailService.SendEmailByTemplateNameAsync(
            user.SiteId,
            "Welcome Email",
            user.Email,
            GetVariables(user)
        );

        if (!result.Success)
        {
            _logger.LogWarning(
                "Email not sent to {User}: {Error}. User experience may be degraded.",
                user.Username,
                result.Message
            );
            
            // Don't fail the entire operation
            // Email is supplementary, not critical
        }
    }
    catch (Exception ex)
    {
        // Never let email failure crash your module
        _logger.LogError(ex, "Email service error for {User}", user.Username);
    }
}
```

---

## ?? **Template Variable Guidelines**

### **Variable Naming Conventions**:

```csharp
// ? GOOD: PascalCase, descriptive
"FirstName", "LastName", "OrderNumber", "ActivationLink"

// ? BAD: lowercase, cryptic
"fname", "lname", "ordnum", "link1"
```

### **Variable Documentation**:

Create a constants class for your module:

```csharp
public static class EmailVariables
{
    // User-related
    public const string FirstName = "FirstName";
    public const string LastName = "LastName";
    public const string Email = "Email";
    public const string Username = "Username";

    // Order-related
    public const string OrderNumber = "OrderNumber";
    public const string OrderTotal = "OrderTotal";
    public const string OrderDate = "OrderDate";

    // Event-related
    public const string EventName = "EventName";
    public const string EventDate = "EventDate";
    public const string EventLocation = "EventLocation";

    public static Dictionary<string, string> CreateUserVariables(User user)
    {
        return new Dictionary<string, string>
        {
            { FirstName, user.FirstName },
            { LastName, user.LastName },
            { Email, user.Email },
            { Username, user.Username }
        };
    }
}
```

**Usage**:
```csharp
var variables = EmailVariables.CreateUserVariables(user);
variables.Add(EmailVariables.OrderNumber, order.Id.ToString());

await _emailService.SendEmailByTemplateNameAsync(
    siteId,
    "Order Confirmation",
    user.Email,
    variables
);
```

---

## ?? **Configuration Requirements**

### **EmailTemplate Module Must Be Installed**

1. Install EmailTemplate module on the site
2. Create templates via UI (Admin adds module to page)
3. Your module can now use those templates

### **SMTP Must Be Configured**

Emails use Oqtane's Notification system, which requires:
- **SMTP Settings**: Admin ? Site Settings ? SMTP Section
- **NotificationJob Enabled**: Admin ? Scheduled Jobs

**If SMTP not configured**:
- `SendEmailByTemplateNameAsync` still returns `Success = true`
- Email queued in Notification table
- NotificationJob will log SMTP errors
- Email won't be delivered

**Your module doesn't need to check SMTP** - that's handled by Oqtane's infrastructure.

---

## ?? **Template Design Guidelines**

### **Creating Templates for Your Module**:

**Template Name**: Use clear, descriptive names
- ? "User Welcome Email"
- ? "Order Confirmation"
- ? "Email1"
- ? "Template"

**Category**: Use your module name or functional area
- "YourModuleName"
- "Registration"
- "Ecommerce"
- "Events"

**Variables**: Document in Description field
```
Description: 
Sent when user completes registration.
Variables: {{FirstName}}, {{LastName}}, {{Email}}, {{ActivationLink}}
```

**Subject Examples**:
```
Welcome to {{SiteName}}, {{FirstName}}!
Your Order #{{OrderNumber}} Has Shipped
Password Reset Request for {{Email}}
```

**Body Best Practices**:
- Use HTML for formatting
- Include all variable placeholders: `{{VariableName}}`
- Test with sample data before production
- Keep it mobile-friendly

---

## ?? **Testing Your Integration**

### **Unit Test Example**:

```csharp
[Fact]
public async Task SendEmail_WithValidTemplate_ReturnsSuccess()
{
    // Arrange
    var mockEmailService = new Mock<IEmailSendingService>();
    mockEmailService
        .Setup(s => s.SendEmailByTemplateNameAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<string>()
        ))
        .ReturnsAsync(new EmailSendResult 
        { 
            Success = true, 
            NotificationId = 123 
        });

    var service = new YourService(mockEmailService.Object);

    // Act
    await service.RegisterUser(testUser);

    // Assert
    mockEmailService.Verify(s => s.SendEmailByTemplateNameAsync(
        It.IsAny<int>(),
        "User Welcome Email",
        testUser.Email,
        It.IsAny<Dictionary<string, string>>(),
        testUser.DisplayName
    ), Times.Once);
}
```

---

## ? **Integration Checklist**

### **For Module Developers**:

- [ ] Reference Amazing.Module.EmailTemplate.Shared project (or DLL)
- [ ] Inject `IEmailSendingService` in your server services
- [ ] Create email templates via EmailTemplate module UI
- [ ] Document required variables for your templates
- [ ] Handle send failures gracefully (don't crash on email errors)
- [ ] Test with real SMTP configuration
- [ ] Log email sends for debugging

### **For Site Administrators**:

- [ ] Install EmailTemplate module
- [ ] Add module to admin page
- [ ] Create templates for each integration
- [ ] Set templates to Active
- [ ] Test templates using "Send Test Email" feature
- [ ] Configure SMTP in Site Settings
- [ ] Enable NotificationJob

---

## ? **Performance Considerations**

### **Email Sending is Async**:
- Emails queued via Notification system
- Delivered by background NotificationJob (every minute)
- Your code doesn't wait for delivery
- No performance impact on user requests

### **Template Lookup**:
- Database query per send
- Consider caching if sending many emails:

```csharp
// Cache template info for bulk sends
var templates = await _emailService.GetAvailableTemplatesAsync(siteId);
var welcomeTemplate = templates.First(t => t.Name == "Welcome Email");

foreach (var user in users)
{
    await _emailService.SendEmailByTemplateIdAsync(
        siteId,
        welcomeTemplate.TemplateId,  // No lookup needed
        user.Email,
        variables
    );
}
```

---

## ?? **Troubleshooting**

### **Problem: Template Not Found**

```csharp
Result: { Success = false, Message = "Template 'XYZ' not found or inactive" }
```

**Solutions**:
1. Check template name spelling (case-sensitive)
2. Verify template exists in EmailTemplate module
3. Ensure template IsActive = true
4. Check you're using correct SiteId

---

### **Problem: Email Not Delivered**

```csharp
Result: { Success = true, NotificationId = 123 }
// But email never arrives
```

**Check**:
1. SMTP configured: Admin ? Site Settings ? SMTP
2. NotificationJob enabled: Admin ? Scheduled Jobs
3. Check Oqtane Event Log for SMTP errors
4. Check spam folder
5. Verify SMTP credentials are correct

---

### **Problem: Variables Not Replaced**

**Check variable naming**:
```csharp
// Template has: {{FirstName}}
// But you send:
variables["firstname"] = "John";  // ? Wrong case

// Fix:
variables["FirstName"] = "John";  // ? Correct
```

---

## ?? **Complete Working Example**

### **Your Module Structure**:

```csharp
// Server/Services/YourModuleService.cs
using Amazing.Module.EmailTemplate.Services;

namespace YourModule.Services
{
    public class ServerYourModuleService : IYourModuleService
    {
        private readonly IEmailSendingService _emailService;
        private readonly IUserService _userService;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public ServerYourModuleService(
            IEmailSendingService emailService,
            IUserService userService,
            ITenantManager tenantManager,
            ILogManager logger)
        {
            _emailService = emailService;
            _userService = userService;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        public async Task<User> RegisterUser(string username, string email, string firstName, string lastName)
        {
            // Create user
            var user = await _userService.AddUserAsync(new User
            {
                SiteId = _alias.SiteId,
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            });

            // Send welcome email
            var variables = new Dictionary<string, string>
            {
                { "FirstName", firstName },
                { "LastName", lastName },
                { "Email", email },
                { "Username", username },
                { "SiteName", _alias.Name }
            };

            var emailResult = await _emailService.SendEmailByTemplateNameAsync(
                _alias.SiteId,
                "User Welcome Email",
                email,
                variables,
                $"{firstName} {lastName}"
            );

            if (emailResult.Success)
            {
                _logger.Log(LogLevel.Information, this, LogFunction.Create,
                    "Welcome email sent to {Email}, NotificationId: {Id}",
                    email, emailResult.NotificationId);
            }
            else
            {
                _logger.Log(LogLevel.Warning, this, LogFunction.Create,
                    "Welcome email failed for {Email}: {Error}",
                    email, emailResult.Message);
            }

            return user;
        }
    }
}
```

---

## ?? **Best Practices**

### **DO**:
? Use descriptive template names  
? Document required variables  
? Handle send failures gracefully  
? Log email sends for debugging  
? Use SendByName for stable integrations  
? Provide fallback templates  
? Rate limit bulk sends  

### **DON'T**:
? Crash on email failures  
? Hardcode template IDs  
? Send without variable validation  
? Assume email delivered immediately  
? Send emails in loops without delays  
? Expose template management to end users  

---

## ?? **Dependencies**

Your module needs:

```xml
<!-- In YourModule.Server.csproj -->
<ItemGroup>
    <Reference Include="Amazing.Module.EmailTemplate.Shared">
        <HintPath>path\to\Amazing.Module.EmailTemplate.Shared.Oqtane.dll</HintPath>
    </Reference>
</ItemGroup>
```

**OR** if both modules in same solution:

```xml
<ItemGroup>
    <ProjectReference Include="..\Amazing.Module.EmailTemplate\Shared\Amazing.Module.EmailTemplate.Shared.csproj" />
</ItemGroup>
```

---

## ? **Getting Started Checklist**

### **One-Time Setup**:
- [ ] Install EmailTemplate module
- [ ] Create your templates via UI
- [ ] Test templates using "Send Test Email"
- [ ] Configure SMTP
- [ ] Enable NotificationJob

### **In Your Module**:
- [ ] Add reference to Amazing.Module.EmailTemplate.Shared
- [ ] Inject `IEmailSendingService` in your server services
- [ ] Call `SendEmailByTemplateNameAsync` where needed
- [ ] Handle errors gracefully
- [ ] Test end-to-end

---

## ?? **Example Templates to Create**

### **For User Management Modules**:
- "User Welcome Email" (Category: Registration)
- "Password Reset Request" (Category: Security)
- "Account Locked" (Category: Security)
- "Email Verification" (Category: Registration)

### **For Ecommerce Modules**:
- "Order Confirmation" (Category: Ecommerce)
- "Order Shipped" (Category: Ecommerce)
- "Payment Received" (Category: Ecommerce)
- "Refund Processed" (Category: Ecommerce)

### **For Content Modules**:
- "Comment Notification" (Category: Content)
- "Content Approved" (Category: Content)
- "Subscription Digest" (Category: Content)

---

## ?? **Support**

### **Common Integration Issues**:

**Q: Service not found in DI container?**  
A: Ensure EmailTemplate module is installed and ServerStartup.ConfigureServices registers it.

**Q: Template not found at runtime?**  
A: Template name is case-sensitive. Verify exact name in EmailTemplate module.

**Q: Variables not replacing?**  
A: Dictionary keys must match template exactly: `{{FirstName}}` requires `variables["FirstName"]`

**Q: Email not arriving?**  
A: Check SMTP configuration and NotificationJob. Email sending is asynchronous (1-2 min delay).

---

## ?? **Next Steps**

1. **Review INTEGRATION_GUIDE.md** for complete API reference
2. **Create your templates** in EmailTemplate module UI
3. **Add service injection** to your module
4. **Test end-to-end** with real email delivery
5. **Document variables** your templates need

---

**You're ready to integrate! Start sending templated emails from your module now!** ??
