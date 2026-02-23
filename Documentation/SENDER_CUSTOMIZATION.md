# Sender Customization Feature

## Version: 1.0.3
## Date: 2025-01-XX

---

## Overview

The EmailTemplate module now supports **sender customization**, allowing module developers to specify the sender's display name and email address when sending templated emails.

---

## What Changed

### API Enhancement

Both public methods in `IEmailSendingService` now accept optional sender parameters:

```csharp
Task<EmailSendResult> SendEmailByTemplateNameAsync(
    int siteId, 
    string templateName, 
    string toEmail, 
    Dictionary<string, string> variables, 
    string toDisplayName = null,
    string fromDisplayName = null,      // NEW: Optional sender display name
    string fromEmail = null             // NEW: Optional sender email address
)

Task<EmailSendResult> SendEmailByTemplateIdAsync(
    int siteId, 
    int templateId, 
    string toEmail, 
    Dictionary<string, string> variables, 
    string toDisplayName = null,
    string fromDisplayName = null,      // NEW: Optional sender display name
    string fromEmail = null             // NEW: Optional sender email address
)
```

### Default Behavior

**If not specified**:
- `fromDisplayName` defaults to the site name (`_alias.Name`)
- `fromEmail` defaults to empty string (uses SMTP configuration default)

**Backward Compatibility**: :white_check_mark:
- Existing code continues to work without changes
- Optional parameters maintain backward compatibility

---

## Use Cases

### 1. Department-Specific Emails

```csharp
// Support emails
await _emailService.SendEmailByTemplateNameAsync(
    siteId,
    "Support Response",
    customer.Email,
    variables,
    customer.Name,
    "Technical Support",
    "support@company.com"
);

// Sales emails
await _emailService.SendEmailByTemplateNameAsync(
    siteId,
    "Quote Sent",
    prospect.Email,
    variables,
    prospect.Name,
    "Sales Team",
    "sales@company.com"
);
```

### 2. No-Reply System Emails

```csharp
await _emailService.SendEmailByTemplateNameAsync(
    siteId,
    "Account Verification",
    user.Email,
    variables,
    user.DisplayName,
    "System",
    "noreply@company.com"
);
```

### 3. Personal Sender

```csharp
await _emailService.SendEmailByTemplateNameAsync(
    siteId,
    "Direct Message",
    recipient.Email,
    variables,
    recipient.Name,
    $"{sender.FirstName} {sender.LastName}",
    sender.Email
);
```

---

## Benefits

:white_check_mark: **Better Email Organization**
- Recipients can filter emails by sender address
- Clear identification of email source

:white_check_mark: **Professional Branding**
- Department-specific sender names
- Consistent email identity

:white_check_mark: **Improved Deliverability**
- Proper sender addresses reduce spam classification
- Better email reputation management

:white_check_mark: **Enhanced User Experience**
- Users know who to reply to
- Clear context for each email type

:white_check_mark: **Backward Compatible**
- Existing integrations continue to work
- No breaking changes

---

## Implementation Details

### File Changes

1. **Shared/Services/IEmailSendingService.cs**
   - Added `fromDisplayName` parameter to both methods
   - Added `fromEmail` parameter to both methods
   - Updated XML documentation

2. **Server/Services/EmailSendingService.cs**
   - Updated method signatures
   - Added logic to use provided values or defaults
   - Enhanced logging to include sender email

### Code Changes

**Before**:
```csharp
var notification = new Notification(
    siteId,
    _alias.Name,           // Always site name
    string.Empty,          // Always empty
    toDisplayName ?? string.Empty,
    toEmail,
    subject,
    body
);
```

**After**:
```csharp
// Use provided fromDisplayName or default to site name
string senderDisplayName = fromDisplayName ?? _alias.Name;
// Use provided fromEmail or default to empty string
string senderEmail = fromEmail ?? string.Empty;

var notification = new Notification(
    siteId,
    senderDisplayName,     // Customizable
    senderEmail,           // Customizable
    toDisplayName ?? string.Empty,
    toEmail,
    subject,
    body
);
```

---

## Best Practices

### DO

:white_check_mark: Use descriptive sender names
```csharp
"Technical Support", "Sales Team", "Billing Department"
```

:white_check_mark: Use dedicated email addresses
```csharp
"support@company.com", "sales@company.com"
```

:white_check_mark: Use "noreply@" for automated emails
```csharp
"noreply@company.com", "no-reply@company.com"
```

:white_check_mark: Create helper classes for consistency
```csharp
public static class EmailSenders
{
    public static (string name, string email) Support 
        => ("Technical Support", "support@company.com");
    
    public static (string name, string email) Sales 
        => ("Sales Team", "sales@company.com");
}
```

### DON'T

:x: Use generic sender names
```csharp
"Admin", "System", "Website"
```

:x: Use personal emails for system notifications
```csharp
"john.doe@company.com" // Not appropriate for automated emails
```

:x: Leave sender information inconsistent
```csharp
// Bad: Sometimes "Support", sometimes "Technical Support", sometimes "Help Desk"
// Good: Always "Technical Support"
```

---

## Testing

### Manual Testing

1. **Test with default sender** (omit parameters):
```csharp
await _emailService.SendEmailByTemplateNameAsync(
    siteId, "Test", email, variables
);
// Should use site name as sender
```

2. **Test with custom sender**:
```csharp
await _emailService.SendEmailByTemplateNameAsync(
    siteId, "Test", email, variables, null, "Custom Sender", "custom@test.com"
);
// Should use "Custom Sender" and "custom@test.com"
```

3. **Verify in database**:
```sql
SELECT FromName, FromEmail, Subject, Body 
FROM Notification 
ORDER BY CreatedOn DESC;
```

4. **Check delivered emails**:
- Verify sender name displays correctly in email client
- Verify sender email address is correct
- Test reply functionality (if applicable)

---

## Migration Notes

### Version Update

- **Previous Version**: 1.0.2
- **New Version**: 1.0.3
- **Migration Required**: No
- **Breaking Changes**: None

### Backward Compatibility

:white_check_mark: **100% Backward Compatible**

Existing code like this will continue to work:
```csharp
await _emailService.SendEmailByTemplateNameAsync(
    siteId,
    templateName,
    email,
    variables,
    displayName
);
// Uses default sender (site name, empty email)
```

New code can leverage the feature:
```csharp
await _emailService.SendEmailByTemplateNameAsync(
    siteId,
    templateName,
    email,
    variables,
    displayName,
    "Custom Sender",
    "custom@company.com"
);
```

---

## Documentation Updates

Updated files:
- :white_check_mark: `INTEGRATION_GUIDE.md` - Added sender customization section with examples
- :white_check_mark: `EXAMPLE_INTEGRATION.md` - Updated examples to show new parameters
- :white_check_mark: `Documentation/SENDER_CUSTOMIZATION.md` - This file

---

## Future Enhancements

Potential future improvements:

1. **Sender Templates**
   - Store sender configurations in database
   - Manage sender identities via UI
   - Sender categories for different use cases

2. **Reply-To Support**
   - Add separate `replyToEmail` parameter
   - Allow different reply address from sender

3. **CC/BCC Support**
   - Add CC/BCC recipient parameters
   - Multi-recipient support

4. **Sender Validation**
   - Validate sender email addresses
   - Check against allowed sender list
   - Domain verification

---

## Support

### Common Questions

**Q: What if I don't specify fromEmail?**  
A: It defaults to empty string, and the SMTP configuration will use its default sender address.

**Q: Can I use any email address as fromEmail?**  
A: The email must be properly configured in your email system. Using unauthorized addresses may cause delivery failures.

**Q: Does this affect existing emails?**  
A: No, this only affects new emails sent after the update.

**Q: Can I specify fromDisplayName without fromEmail?**  
A: Yes! Each parameter is optional and independent.

**Q: What happens if I specify an invalid email address?**  
A: The notification will be queued, but delivery may fail at the SMTP level. Check Oqtane logs for delivery issues.

---

## Complete Example

```csharp
using Amazing.Module.EmailTemplate.Services;

public class OrderService
{
    private readonly IEmailSendingService _emailService;

    public async Task SendOrderConfirmation(Order order)
    {
        var variables = new Dictionary<string, string>
        {
            { "OrderNumber", order.Id.ToString() },
            { "CustomerName", order.CustomerName },
            { "OrderTotal", order.Total.ToString("C") },
            { "OrderDate", order.CreatedOn.ToString("d") }
        };

        var result = await _emailService.SendEmailByTemplateNameAsync(
            siteId: order.SiteId,
            templateName: "Order Confirmation",
            toEmail: order.CustomerEmail,
            variables: variables,
            toDisplayName: order.CustomerName,
            fromDisplayName: "E-commerce Team",       // Customized sender
            fromEmail: "orders@yourstore.com"         // Dedicated order email
        );

        if (!result.Success)
        {
            _logger.LogError("Failed to send order confirmation: {Error}", result.Message);
        }
    }
}
```

---

**Status**: Complete

**Build Status**: :white_check_mark: Successful

**Breaking Changes**: :x: None

**Backward Compatibility**: :white_check_mark: 100%
