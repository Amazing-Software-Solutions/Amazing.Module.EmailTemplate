# EmailTemplate Module - Integration Guide for Developers

## Purpose

This guide shows **other Oqtane module developers** how to use the EmailTemplate module to send templated emails from their modules.

---

## Why Use This Module

Instead of:

- Writing SMTP code in every module
- Hardcoding email templates in code
- Managing HTML email formatting manually
- Duplicating variable replacement logic

You get:

- **Centralized email templates** - Managed via UI
- **Rich HTML editing** - WYSIWYG editor
- **Variable replacement** - Dynamic content
- **SMTP abstraction** - Uses Oqtane notification system
- **Template reusability** - One template, many uses across site
- **Non-technical user management** - Site admins can edit templates
- **Site-scoped sharing** - Create once, use everywhere (v1.0.2)

---

## Quick Start

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

## Complete API Reference

### Method 1: Send by Template Name (Recommended)

```csharp
Task<EmailSendResult> SendEmailByTemplateNameAsync(
    int siteId,                          // Current site ID
    string templateName,                 // Template name (e.g., "User Welcome Email")
    string toEmail,                      // Recipient email
    Dictionary<string, string> variables,  // Variables to replace
    string toDisplayName = null          // Optional recipient display name
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

### Method 2: Send by Template ID

```csharp
Task<EmailSendResult> SendEmailByTemplateIdAsync(
    int siteId,
    int templateId,                      // Specific template ID
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

### Method 3: Get Available Templates

```csharp
Task<List<TemplateInfo>> GetAvailableTemplatesAsync(int siteId)
```

**Returns**: List of all active templates for the site with metadata.

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

### Method 4: Get Templates by Category

```csharp
Task<List<TemplateInfo>> GetTemplatesByCategoryAsync(
    int siteId,
    string category                      // Category name (e.g., "Onboarding")
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

## Response Model: EmailSendResult

```csharp
public class EmailSendResult
{
    public bool Success { get; set; }         // True if queued successfully
    public string Message { get; set; }       // Success/error message
    public int? NotificationId { get; set; }  // Oqtane Notification ID (for tracking)
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

## Template Info Model

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

## Module File Structure

```
Amazing.Module.EmailTemplate/
|-- Shared/                           # Models and interfaces
|   |-- Models/
|   |   |-- EmailTemplate.cs         # 8 fields + audit, SiteId
|   |   |-- EmailSendResult.cs
|   |   |-- TemplateInfo.cs
|   |   |-- TestEmailRequest.cs
|   |   +-- TestEmailResult.cs
|   +-- Services/
|       +-- IEmailSendingService.cs   # Public API for integration
|
|-- Server/                           # Business logic and API
|   |-- Controllers/
|   |   +-- EmailTemplateController.cs
|   |-- Services/
|   |   |-- EmailTemplateService.cs  # Internal UI management
|   |   +-- EmailSendingService.cs   # Public email sending
|   |-- Repository/
|   |   |-- EmailTemplateContext.cs
|   |   +-- EmailTemplateRepository.cs  # Site-scoped queries
|   +-- Manager/
|       +-- EmailTemplateManager.cs
|
+-- Client/                           # Blazor UI components
    |-- Modules/Amazing.Module.EmailTemplate/
    |   |-- Index.razor
    |   +-- Edit.razor
    +-- Services/
        +-- EmailTemplateService.cs
```

---

## Usage Patterns

### Pattern 1: Simple Send (No Variables)

```csharp
await _emailService.SendEmailByTemplateNameAsync(
    siteId,
    "Simple Notification",
    user.Email,
    null  // No variables
);
```

---

### Pattern 2: With Variables

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

### Pattern 3: Dynamic Template Selection

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

### Pattern 4: Error Handling

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

### Pattern 5: Template Discovery

```csharp
// Get all available templates for site
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

### Pattern 6: Bulk Sending

```csharp
public async Task SendBulkEmails(List<User> users, string templateName, int siteId)
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
            siteId,
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

## Real-World Examples

*See EXAMPLE_INTEGRATION.md for complete working examples including:*

- User Registration Module
- Ecommerce Order Module
- Password Reset Module
- Event Notification Module

---

## Security & Authorization

### No Permission Required to Send

The `IEmailSendingService` is **site-scoped**, not permission-based:

- Any authenticated module can send emails
- Site ID validated (no cross-site sending)
- Template must be Active
- All sends are logged for audit

**Rationale**: Email sending is a functional capability, not a security boundary. Authorization should be at the **calling module level** (e.g., only admins can trigger password resets).

### Best Practices

```csharp
// GOOD: Authorization in your module
[Authorize(Policy = PolicyNames.EditModule)]
public async Task SendAdminAlert(int moduleId, string message)
{
    // Authorization checked above
    await _emailService.SendEmailByTemplateNameAsync(...);
}

// BAD: No authorization check
public async Task SendEmail(string to, string template)
{
    // Anyone can call this!
    await _emailService.SendEmailByTemplateNameAsync(...);
}
```

---

## Error Handling

### Common Errors

| Error Message | Cause | Solution |
|---------------|-------|----------|
| Template 'XYZ' not found | Template doesn't exist or typo | Check template name in EmailTemplate module |
| Template 'XYZ' is inactive | Template exists but IsActive = false | Activate template in EmailTemplate module |
| Site ID mismatch | Trying to use template from different site | Use correct siteId parameter |
| Email queued but not delivered | SMTP not configured | Configure SMTP in Admin :arrow_right: Site Settings |

### Defensive Coding

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

## Template Variable Guidelines

### Variable Naming Conventions

```csharp
// GOOD: PascalCase, descriptive
"FirstName", "LastName", "OrderNumber", "ActivationLink"

// BAD: lowercase, cryptic
"fname", "lname", "ordnum", "link1"
```

### Variable Documentation

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

## Configuration Requirements

### EmailTemplate Module Must Be Installed

1. Install EmailTemplate module on the site
2. Create templates via UI (Admin adds module to page)
3. Your module can now use those templates
4. Templates are shared across entire site (v1.0.2)

### SMTP Must Be Configured

Emails use Oqtane's Notification system, which requires:

- **SMTP Settings**: Admin :arrow_right: Site Settings :arrow_right: SMTP Section
- **NotificationJob Enabled**: Admin :arrow_right: Scheduled Jobs

**If SMTP not configured**:

- `SendEmailByTemplateNameAsync` still returns `Success = true`
- Email queued in Notification table
- NotificationJob will log SMTP errors
- Email won't be delivered

**Your module doesn't need to check SMTP** - that's handled by Oqtane's infrastructure.

---

## Template Design Guidelines

### Creating Templates for Your Module

**Template Name**: Use clear, descriptive names

- GOOD: "User Welcome Email", "Order Confirmation"
- BAD: "Email1", "Template"

**Category**: Use your module name or functional area

- "YourModuleName"
- "Registration"
- "Ecommerce"
- "Events"

**Variables**: Document in Description and TemplateVariables fields

```
Description: 
Sent when user completes registration.

TemplateVariables: {{FirstName}},{{LastName}},{{Email}},{{ActivationLink}}
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

## Testing Your Integration

### Unit Test Example

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

## Integration Checklist

### For Module Developers

- [ ] Reference Amazing.Module.EmailTemplate.Shared project (or DLL)
- [ ] Inject `IEmailSendingService` in your server services
- [ ] Create email templates via EmailTemplate module UI
- [ ] Document required variables for your templates
- [ ] Handle send failures gracefully (don't crash on email errors)
- [ ] Test with real SMTP configuration
- [ ] Log email sends for debugging
- [ ] Verify site isolation (multi-tenant)

### For Site Administrators

- [ ] Install EmailTemplate module
- [ ] Add module to admin page (once per site)
- [ ] Create templates for integrations
- [ ] Set templates to Active
- [ ] Test templates using "Send Test Email" feature
- [ ] Configure SMTP in Site Settings
- [ ] Enable NotificationJob
- [ ] Understand templates are site-wide (v1.0.2)

---

## Performance Considerations

### Email Sending is Async

- Emails queued via Notification system
- Delivered by background NotificationJob (every minute)
- Your code doesn't wait for delivery
- No performance impact on user requests

### Template Lookup

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

## Troubleshooting

### Problem: Template Not Found

```csharp
Result: { Success = false, Message = "Template 'XYZ' not found or inactive" }
```

**Solutions**:

1. Check template name spelling (case-sensitive)
2. Verify template exists in EmailTemplate module
3. Ensure template IsActive = true
4. Check you're using correct SiteId
5. Verify template was created for the same site (v1.0.2)

---

### Problem: Email Not Delivered

```csharp
Result: { Success = true, NotificationId = 123 }
// But email never arrives
```

**Check**:

1. SMTP configured: Admin :arrow_right: Site Settings :arrow_right: SMTP
2. NotificationJob enabled: Admin :arrow_right: Scheduled Jobs
3. Check Oqtane Event Log for SMTP errors
4. Check spam folder
5. Verify SMTP credentials are correct

---

### Problem: Variables Not Replaced

**Check variable naming**:

```csharp
// Template has: {{FirstName}}
// But you send:
variables["firstname"] = "John";  // Wrong case

// Fix:
variables["FirstName"] = "John";  // Correct
```

---

## Best Practices

### DO

- Use descriptive template names  
- Document required variables  
- Handle send failures gracefully  
- Log email sends for debugging  
- Use SendByName for stable integrations  
- Provide fallback templates  
- Rate limit bulk sends  
- Understand site scoping (v1.0.2)

### DON'T

- Crash on email failures  
- Hardcode template IDs  
- Send without variable validation  
- Assume email delivered immediately  
- Send emails in loops without delays  
- Expose template management to end users  
- Try to send cross-site emails

---

## Dependencies

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

## Getting Started Checklist

### One-Time Setup

- [ ] Install EmailTemplate module
- [ ] Create your templates via UI (once per site)
- [ ] Test templates using "Send Test Email"
- [ ] Configure SMTP
- [ ] Enable NotificationJob

### In Your Module

- [ ] Add reference to Amazing.Module.EmailTemplate.Shared
- [ ] Inject `IEmailSendingService` in your server services
- [ ] Call `SendEmailByTemplateNameAsync` where needed
- [ ] Handle errors gracefully
- [ ] Test end-to-end

---

## Example Templates to Create

### For User Management Modules

- "User Welcome Email" (Category: Registration)
- "Password Reset Request" (Category: Security)
- "Account Locked" (Category: Security)
- "Email Verification" (Category: Registration)

### For Ecommerce Modules

- "Order Confirmation" (Category: Ecommerce)
- "Order Shipped" (Category: Ecommerce)
- "Payment Received" (Category: Ecommerce)
- "Refund Processed" (Category: Ecommerce)

### For Content Modules

- "Comment Notification" (Category: Content)
- "Content Approved" (Category: Content)
- "Subscription Digest" (Category: Content)

---

## Support

### Common Integration Issues

**Q: Service not found in DI container?**  
A: Ensure EmailTemplate module is installed and ServerStartup.ConfigureServices registers it.

**Q: Template not found at runtime?**  
A: Template name is case-sensitive. Verify exact name in EmailTemplate module. Check template is for correct site (v1.0.2).

**Q: Variables not replacing?**  
A: Dictionary keys must match template exactly: `{{FirstName}}` requires `variables["FirstName"]`

**Q: Email not arriving?**  
A: Check SMTP configuration and NotificationJob. Email sending is asynchronous (1-2 min delay).

**Q: Can I share templates across sites?**  
A: No. Templates are site-scoped (v1.0.2). Each site has its own templates. Export/import between sites if needed.

---

## Site Scoping (v1.0.2)

### What Changed

Templates are now **site-scoped** instead of module-scoped:

- **Before (v1.0.0-1.0.1)**: Templates isolated per module instance
- **After (v1.0.2)**: Templates shared across entire site

### Benefits

- Create template once, available to all modules in site
- Centralized management
- No template duplication
- Proper multi-tenant architecture

### For Developers

**No code changes required!** The IEmailSendingService API already used `siteId` parameter.

---

## Next Steps

1. **Review EXAMPLE_INTEGRATION.md** for copy-paste code
2. **Create your templates** in EmailTemplate module UI
3. **Add service injection** to your module
4. **Test end-to-end** with real email delivery
5. **Document variables** your templates need

---

**You're ready to integrate! Start sending templated emails from your modules now!**
