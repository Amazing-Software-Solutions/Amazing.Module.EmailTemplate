# Amazing.Module.EmailTemplate

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Oqtane](https://img.shields.io/badge/Oqtane-10.0.4-green.svg)](https://www.oqtane.org)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)

A professional email template management module for Oqtane Framework with rich HTML editing, template variables, test email capabilities, and a **public API for inter-module email sending**.

---

## Features

### Email Template Management
- **CRUD Operations** - Create, read, update, and delete email templates
- **Rich HTML Editor** - QuillJS-powered WYSIWYG editor with formatting toolbar
- **Template Variables** - Dynamic content replacement using `{{VariableName}}` syntax
- **Category Organization** - Group templates by functional area
- **Active/Inactive Status** - Control template availability
- **Search Integration** - Full-text search on template content
- **Module Portability** - Complete import/export support

### Test Email Capability
- **Send Test Emails** - Validate templates before production use
- **Sample Variable Input** - Test variable replacement with JSON data
- **Instant Feedback** - Success/error messages with detailed logging
- **Preview Your Templates** - See exactly what recipients will receive

### Public Email Service (For Developers)
- **IEmailSendingService** - Reusable service for other Oqtane modules
- **Simple API** - Send templated emails with 3 lines of code
- **Template Discovery** - List available templates by name or category
- **Variable Replacement** - Automatic processing of template variables
- **Notification Integration** - Uses Oqtane's built-in SMTP infrastructure

---

## Quick Start

### Installation

1. **Download** the module package from [Releases](../../releases)
2. **Install** via Oqtane Admin ? Module Management ? Upload Module
3. **Add** the module to a page
4. **Create** your first email template

### Creating Your First Template

1. Click **"Add EmailTemplate"**
2. Fill in the form:
   - **Name**: "Welcome Email"
   - **Subject**: `Welcome {{FirstName}}!`
   - **Body**: Use the rich text editor to create formatted content
   - **Variables**: `{{FirstName}},{{LastName}},{{Email}}`
   - **Active**: Check the box
3. Click **Save**

### Testing Your Template

1. Click **Edit** on your template
2. Expand the **"Test Email"** section
3. Enter your email address
4. Add sample JSON variables:
   ```json
   {
     "FirstName": "John",
     "LastName": "Doe",
     "Email": "john@example.com"
   }
   ```
5. Click **"Send Test Email"**
6. Check your inbox in 1-2 minutes

---

## Documentation

### For Site Administrators
- **[Quick Start Guide](QUICKSTART_GUIDE.md)** - Get started with test email feature
- **[Complete Module Summary](MODULE_COMPLETE_SUMMARY.md)** - Full feature overview

### For Module Developers
- **[Integration Guide](INTEGRATION_GUIDE.md)** - Complete API reference and patterns
- **[Example Integration](EXAMPLE_INTEGRATION.md)** - Copy-paste ready code examples

### Technical Documentation
- **[Data Model Enhancement](DATAMODEL_ENHANCEMENT.md)** - Database schema and migrations
- **[Rich Text Editor Integration](RICHTEXTEDITOR_INTEGRATION.md)** - QuillJS implementation details

---

## Using This Module in Your Code

Other Oqtane modules can use this module as a centralized email service!

### Simple Example

```csharp
using Amazing.Module.EmailTemplate.Services;

public class YourModuleService
{
    private readonly IEmailSendingService _emailService;

    public YourModuleService(IEmailSendingService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendWelcomeEmail(User user)
    {
        var variables = new Dictionary<string, string>
        {
            { "FirstName", user.FirstName },
            { "Email", user.Email }
        };

        var result = await _emailService.SendEmailByTemplateNameAsync(
            siteId: user.SiteId,
            templateName: "Welcome Email",
            toEmail: user.Email,
            variables: variables
        );

        if (result.Success)
        {
            _logger.LogInformation("Email sent: {Id}", result.NotificationId);
        }
    }
}
```

**See [Integration Guide](INTEGRATION_GUIDE.md) for complete API documentation!**

---

## Use Cases

### User Management
- Registration welcome emails
- Email verification
- Password reset requests
- Account locked notifications

### Ecommerce
- Order confirmations
- Shipping notifications
- Payment receipts
- Refund confirmations

### Content Management
- Comment notifications
- Content approval
- Subscription digests
- Newsletter campaigns

### Events
- Event registration confirmation
- Event reminders
- Event cancellation
- Post-event surveys

---

## Architecture

### Technology Stack
- **Framework**: Oqtane 10.0.4 on .NET 10
- **UI**: Blazor Server + WebAssembly
- **Database**: Multi-database support (SQL Server, MySQL, PostgreSQL, SQLite)
- **Email**: Oqtane Notification System + MailKit
- **Editor**: QuillJS Rich Text Editor

### Module Structure
```
Amazing.Module.EmailTemplate/
??? Shared/               # Models and interfaces
?   ??? Models/
?   ?   ??? EmailTemplate.cs
?   ?   ??? EmailSendResult.cs
?   ?   ??? TemplateInfo.cs
?   ??? Services/
?       ??? IEmailSendingService.cs  (Public API)
?
??? Server/               # Business logic and API
?   ??? Controllers/
?   ?   ??? EmailTemplateController.cs
?   ??? Services/
?   ?   ??? EmailTemplateService.cs    (Internal)
?   ?   ??? EmailSendingService.cs     (Public)
?   ??? Repository/
?       ??? EmailTemplateRepository.cs
?
??? Client/               # Blazor UI components
    ??? Modules/Amazing.Module.EmailTemplate/
        ??? Index.razor
        ??? Edit.razor
```

---

## Security

### Authorization
- **View Permission** - See template list and preview
- **Edit Permission** - Create, edit, delete, and send test emails
- **Site-Scoped** - Templates isolated per site
- **Public API** - No permission required (authorization in calling module)

### Data Protection
- Multi-tenant isolation
- No cross-site email sending
- Audit trail for all operations
- Active/inactive status control

---

## Configuration Requirements

### SMTP Setup (Required for Email Delivery)

1. Navigate to **Admin ? Site Settings ? SMTP Settings**
2. Configure:
   - **Host**: Your SMTP server (e.g., `smtp.gmail.com`)
   - **Port**: SMTP port (e.g., `587`)
   - **SSL**: SSL/TLS options
   - **Username/Password**: SMTP credentials
   - **Sender**: From email address

### NotificationJob (Required)

1. Navigate to **Admin ? Scheduled Jobs**
2. Find **"Notification Job"**
3. Set **Enabled** = Yes
4. Set **Frequency** = Minute, **Interval** = 1

---

## Database Schema

**Table**: `AmazingEmailTemplate`

| Column | Type | Description |
|--------|------|-------------|
| EmailTemplateId | int | Primary key |
| ModuleId | int | Module reference |
| Name | nvarchar(100) | Template name |
| Subject | nvarchar(200) | Email subject (with variables) |
| Body | nvarchar(MAX) | HTML email content |
| Category | nvarchar(50) | Template category |
| IsActive | bit | Active status |
| TemplateVariables | nvarchar(2000) | Available variables |
| Description | nvarchar(500) | Template notes |
| CreatedBy, CreatedOn, ModifiedBy, ModifiedOn | | Audit fields |

---

## Template Variable Syntax

Templates support dynamic content via double-brace variables:

**Subject**: `Welcome {{FirstName}} {{LastName}}!`

**Body**:
```html
<h1>Hello {{FirstName}}!</h1>
<p>Your email address is: {{Email}}</p>
<p>Order #{{OrderNumber}} total: ${{OrderTotal}}</p>
```

**Sample Data** (JSON):
```json
{
  "FirstName": "John",
  "LastName": "Doe",
  "Email": "john@example.com",
  "OrderNumber": "12345",
  "OrderTotal": "99.99"
}
```

**Result**:
```
Subject: Welcome John Doe!
Body: 
Hello John!
Your email address is: john@example.com
Order #12345 total: $99.99
```

---

## API Reference

### IEmailSendingService Methods

#### 1. SendEmailByTemplateNameAsync (Recommended)
```csharp
Task<EmailSendResult> SendEmailByTemplateNameAsync(
    int siteId,
    string templateName,
    string toEmail,
    Dictionary<string, string> variables,
    string toDisplayName = null
)
```

#### 2. SendEmailByTemplateIdAsync
```csharp
Task<EmailSendResult> SendEmailByTemplateIdAsync(
    int siteId,
    int templateId,
    string toEmail,
    Dictionary<string, string> variables,
    string toDisplayName = null
)
```

#### 3. GetAvailableTemplatesAsync
```csharp
Task<List<TemplateInfo>> GetAvailableTemplatesAsync(int siteId)
```

#### 4. GetTemplatesByCategoryAsync
```csharp
Task<List<TemplateInfo>> GetTemplatesByCategoryAsync(
    int siteId,
    string category
)
```

**See [Integration Guide](INTEGRATION_GUIDE.md) for detailed API documentation.**

---

## Testing

### Unit Testing
```csharp
[Fact]
public async Task SendEmail_WithValidTemplate_ReturnsSuccess()
{
    var mockService = new Mock<IEmailSendingService>();
    mockService
        .Setup(s => s.SendEmailByTemplateNameAsync(
            It.IsAny<int>(),
            "Welcome Email",
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<string>()
        ))
        .ReturnsAsync(new EmailSendResult 
        { 
            Success = true, 
            NotificationId = 123 
        });

    var service = new YourService(mockService.Object);
    await service.RegisterUser(testUser);

    mockService.Verify(/* ... */, Times.Once);
}
```

### Integration Testing
1. Create test template in UI
2. Use "Send Test Email" feature
3. Verify email delivery
4. Test variable replacement
5. Test error scenarios

---

## Troubleshooting

### Common Issues

**Q: Test Email section not visible?**  
A: Restart the application (Shift+F5, then F5). Hot reload cannot apply interface changes.

**Q: Template not found error?**  
A: Check template name spelling (case-sensitive) and ensure IsActive = true.

**Q: Email not arriving?**  
A: Verify SMTP configuration, check NotificationJob is enabled, wait 1-2 minutes for delivery.

**Q: Variables not replacing?**  
A: Ensure JSON keys match variable names exactly (case-sensitive): `{{FirstName}}` requires `{"FirstName":"..."}`.

**See [Troubleshooting Guide](INTEGRATION_GUIDE.md#troubleshooting) for more details.**

---

## Dependencies

### Runtime Dependencies
- Oqtane Framework 10.0.4+
- .NET 10
- Entity Framework Core 10.0
- Oqtane.Shared (included with framework)

### For Integration
- Reference `Amazing.Module.EmailTemplate.Shared.dll` in your module

**No additional NuGet packages required!**

---

## Roadmap

### Current Version (1.0.0)
- [x] Email template CRUD
- [x] Rich HTML editor
- [x] Template variables
- [x] Test email feature
- [x] Public email service API
- [x] Category management
- [x] Active/inactive status
- [x] Module portability

### Future Enhancements
- [ ] Variable picker dropdown
- [ ] Email preview panel
- [ ] Template versioning
- [ ] Bulk email sending
- [ ] Scheduled sending
- [ ] Email analytics (open/click tracking)
- [ ] A/B testing
- [ ] Attachment support
- [ ] CC/BCC support
- [ ] Multi-language templates

---

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development Setup
1. Clone this repository
2. Open `Amazing.Module.EmailTemplate.sln` in Visual Studio 2022+
3. Ensure Oqtane Framework 10.0.4+ is referenced
4. Build solution
5. Run and test

### Coding Standards
- Follow Oqtane coding conventions
- Add XML documentation for public APIs
- Include unit tests for new features
- Update documentation for API changes

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- **Oqtane Framework** - The amazing CMS/Application Framework this module is built for
- **QuillJS** - Rich text editor integration
- **MailKit** - Email delivery (via Oqtane)
- **Oqtane Community** - Support and guidance

---

## Support

### Documentation
- [Integration Guide](INTEGRATION_GUIDE.md) - Complete API reference
- [Example Integration](EXAMPLE_INTEGRATION.md) - Working code samples
- [Quick Start Guide](QUICKSTART_GUIDE.md) - Getting started

### Community
- [Oqtane GitHub](https://github.com/oqtane/oqtane.framework)
- [Oqtane Discussions](https://github.com/oqtane/oqtane.framework/discussions)
- [Oqtane Docs](https://docs.oqtane.org)

### Issues
Found a bug or have a feature request? Please [open an issue](../../issues).

---

## Stats

- **Lines of Code**: ~1,800
- **Public API Methods**: 4
- **Internal Methods**: 6
- **Models**: 5
- **Controllers**: 1 (6 endpoints)
- **UI Components**: 3
- **Migrations**: 2
- **Documentation Files**: 6

---

## Learn More

### About Oqtane
[Oqtane](https://www.oqtane.org) is an open-source modular application framework for Blazor. It provides a robust foundation for building modern web applications with .NET.

### About This Module
This module demonstrates:
- Service-mediated architecture
- Public API design for inter-module communication
- Rich text editing in Blazor
- Template variable replacement
- Integration with Oqtane's notification system
- Governance-compliant development

---

## Star This Repository

If you find this module useful, please consider giving it a star! It helps others discover the project.

---

## Quick Links

| Documentation | Link |
|---------------|------|
| **For Administrators** | [Quick Start Guide](QUICKSTART_GUIDE.md) |
| **For Developers** | [Integration Guide](INTEGRATION_GUIDE.md) |
| **Code Examples** | [Example Integration](EXAMPLE_INTEGRATION.md) |
| **Complete Overview** | [Module Summary](MODULE_COMPLETE_SUMMARY.md) |
| **Database Details** | [Data Model](DATAMODEL_ENHANCEMENT.md) |
| **Editor Details** | [Rich Text Editor](RICHTEXTEDITOR_INTEGRATION.md) |

---

<div align="center">

**Built with love for the Oqtane Community**

[Report Bug](../../issues) · [Request Feature](../../issues) · [Documentation](INTEGRATION_GUIDE.md)

</div>
