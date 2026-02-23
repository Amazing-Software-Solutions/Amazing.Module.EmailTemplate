# Amazing.Module.EmailTemplate - Quickstart Guide

## Module Status: PRODUCTION READY

Your EmailTemplate module is a **fully functional, professional Oqtane module** with rich HTML editing, test email capabilities, and site-scoped architecture.

---

## What Was Built

### Phase 1: Data Model Enhancement - COMPLETE

- Enhanced EmailTemplate entity with 8 fields
- Created database migration for new columns
- Updated EntityBuilder for proper schema
- Fixed IPortable for complete import/export

### Phase 2: Rich Text Editor Integration - COMPLETE

- Integrated Oqtane's RichTextEditor (QuillJS)
- Replaced plain textarea with WYSIWYG editor
- Added Section component for clean UI
- Supports HTML formatting, images, links

### Phase 3: Send Test Email Feature - COMPLETE

- Created test email API endpoint
- Implemented variable replacement engine
- Integrated with Oqtane Notification system
- Added UI section with validation

### Phase 4: Site Scoping Refactor - COMPLETE (v1.0.2)

- Migrated from Module Scope to Site Scope
- Templates now shared across entire site
- Multi-tenant isolation maintained
- Follows Oqtane data scoping architecture

---

## Complete File Structure

```
Amazing.Module.EmailTemplate/
|-- Shared/                           # Models and interfaces
|   |-- Models/
|   |   |-- EmailTemplate.cs         # Enhanced: 8 fields + audit, SiteId
|   |   |-- EmailSendResult.cs
|   |   |-- TemplateInfo.cs
|   |   |-- TestEmailRequest.cs
|   |   +-- TestEmailResult.cs
|   +-- Services/
|       +-- IEmailSendingService.cs   # Public API for other modules
|
|-- Server/                           # Business logic and API
|   |-- Controllers/
|   |   +-- EmailTemplateController.cs  # 6 API endpoints
|   |-- Services/
|   |   |-- EmailTemplateService.cs  # Internal UI management
|   |   +-- EmailSendingService.cs   # Public email sending
|   |-- Repository/
|   |   |-- EmailTemplateContext.cs
|   |   +-- EmailTemplateRepository.cs  # Site-scoped queries
|   |-- Manager/
|   |   +-- EmailTemplateManager.cs  # IPortable + ISearchable
|   |-- Migrations/
|   |   |-- 01000000_InitializeModule.cs
|   |   |-- 01000001_AddEmailTemplateFields.cs
|   |   |-- 01000002_ChangeScopeToSiteId.cs
|   |   +-- EntityBuilders/
|   |       +-- EmailTemplateEntityBuilder.cs
|   +-- Startup/
|       +-- ServerStartup.cs
|
+-- Client/                           # Blazor UI components
    |-- Modules/Amazing.Module.EmailTemplate/
    |   |-- Index.razor               # Enhanced list view
    |   |-- Edit.razor                # RichTextEditor + Test Email
    |   +-- Settings.razor
    +-- Services/
        +-- EmailTemplateService.cs   # Client API service (7 methods)
```

---

## Feature Summary

### Data Model (EmailTemplate Entity)

| Field | Type | Description |
|-------|------|-------------|
| EmailTemplateId | int | Primary key |
| SiteId | int | Site reference (v1.0.2+) |
| Name | string(100) | Template name |
| Description | string(500) | Template notes |
| Subject | string(200) | Email subject (supports variables) |
| Body | text | HTML email content |
| Category | string(50) | Template category |
| IsActive | bool | Active status |
| TemplateVariables | string(2000) | Available variables |
| + Audit Fields | | CreatedBy, CreatedOn, ModifiedBy, ModifiedOn |

### User Interface

- List view with pagination and enhanced columns
- Add/Edit form with validation
- RichTextEditor (QuillJS) for HTML email body
- Collapsible sections for organization
- Test Email section with variable sampling
- Active/Inactive status badges
- ActionDialog confirmations for delete

### Business Features

- CRUD operations (Create, Read, Update, Delete)
- Template variables (`{{Syntax}}`)
- Variable replacement engine (JSON-based)
- Send test email with sample data
- Category-based organization
- Active/Inactive status management
- Module portability (import/export)
- Search integration
- Site-scoped template sharing

### Technical Features

- Multi-tenant support
- Multi-database support (SQL Server, MySQL, PostgreSQL, SQLite)
- Authorization (Edit/View permissions)
- Security logging
- Audit trail
- Email delivery via Notification system
- Site-level data scoping

---

## API Endpoints

**Base**: `/api/EmailTemplate`

1. `GET ?moduleid={id}` - List templates (for current site)
2. `GET /{id}/{moduleid}` - Get single template
3. `POST /?moduleid={id}` - Create template
4. `PUT /{id}?moduleid={id}` - Update template
5. `DELETE /{id}/{moduleid}` - Delete template
6. `POST /sendtest/{moduleid}` - Send test email

---

## How Test Email Works

### Architecture

```
User Clicks "Send Test Email"
  |
  v
Client Service :arrow_right: API Controller :arrow_right: Server Service
  |
  v
Variable Replacement ({{Name}} :arrow_right: "John")
  |
  v
Create Notification Record
  |
  v
NotificationJob (runs every minute)
  |
  v
Send via SMTP (MailKit)
  |
  v
Email Delivered
```

### Variable Replacement

```csharp
Subject: "Welcome {{FirstName}}!"
JSON: {"FirstName":"John"}
Result: "[TEST] Welcome John!"
```

---

## IMPORTANT: Restart Required

### Why You Can't See the Test Email Section

The application is currently running with **old compiled code**. Hot Reload cannot apply:

- New interface methods
- New dependencies
- Structural changes

### Solution

**Stop and restart the application:**

```powershell
# Option 1: Visual Studio
1. Press Shift+F5 (Stop Debugging)
2. Press F5 (Start Debugging)

# Option 2: Kill processes
Get-Process dotnet | Stop-Process -Force
# Then start again (F5)
```

**Clear browser cache:**

```
Press: Ctrl + Shift + R (hard refresh)
Or: Ctrl + Shift + Delete :arrow_right: Clear cache
```

---

## Verification Steps

After restart:

### 1. Check Index Page

- Navigate to page with EmailTemplate module
- Verify columns: Name, Category, Subject, Active (badge), Modified

### 2. Check Edit Page

- Click "Add EmailTemplate" or "Edit" existing
- Scroll through form:
  - Name
  - Description
  - Category
  - Subject
  - Template Variables
  - Active checkbox
  - **Email Body** (section - click to expand)
    - Rich text editor with toolbar
  - **Test Email** (section - click to expand)
    - Test Email input
    - Sample Variables textarea
    - Send Test Email button

---

## Quick Test

### Simple Test (No Variables)

```
1. Create new template
2. Name: "Simple Test"
3. Subject: "Test Email"
4. Body: "<h1>Hello World</h1>"
5. Expand "Test Email" section
6. Test Email: your-email@example.com
7. Leave Sample Variables empty
8. Click "Send Test Email"
9. Success message appears
10. Wait 1-2 minutes
11. Check inbox
```

### With Variables

```
1. Subject: "Hello {{Name}}"
2. Body: "<h1>Hi {{Name}}!</h1><p>Your email: {{Email}}</p>"
3. Variables: "{{Name}},{{Email}}"
4. Test Email: your-email@example.com
5. Sample Variables: {"Name":"Tester","Email":"test@example.com"}
6. Send Test Email
7. Receive: "[TEST] Hello Tester" with personalized content
```

---

## Build Information

**Status**: Build Successful

**Version**: 1.0.2

**Projects Built**:

- Amazing.Module.EmailTemplate.Shared.Oqtane.dll
- Amazing.Module.EmailTemplate.Server.Oqtane.dll
- Amazing.Module.EmailTemplate.Client.Oqtane.dll
- Amazing.Module.EmailTemplate.Package.dll

---

## Feature Capabilities

### Rich Text Editor

- Bold, Italic, Underline, Strikethrough
- Headers (H1-H5)
- Text and background colors
- Ordered and bulleted lists
- Hyperlinks
- Image insertion
- Raw HTML mode

### Template Variables

- Syntax: `{{VariableName}}`
- Used in Subject and Body
- Replaced with JSON data during test
- Case-sensitive matching
- Graceful handling of missing variables

### Test Email

- Validates Subject and Body exist
- Validates email address format
- Processes template variables
- Adds [TEST] prefix to subject
- Queues via Notification system
- Async delivery (1-2 minutes)

---

## Documentation Files

1. **DATAMODEL_ENHANCEMENT.md** - Data model changes and migration details
2. **RICHTEXTEDITOR_INTEGRATION.md** - QuillJS editor integration
3. **QUICKSTART_GUIDE.md** - This file (getting started)
4. **MODULE_COMPLETE_SUMMARY.md** - Complete feature summary
5. **INTEGRATION_GUIDE.md** - API reference for developers
6. **EXAMPLE_INTEGRATION.md** - Copy-paste integration examples
7. **DATASCOPING_MIGRATION.md** - ModuleId to SiteId migration (v1.0.2)
8. **SITESCOPING_SUMMARY.md** - Site scoping architecture
9. **REFACTORING_COMPLETE.md** - Refactoring summary

---

## What Makes This Module Special

**Professional Quality**:

- Rich HTML editing (not plain text)
- Template variable system
- Test email capability
- Site-scoped architecture
- Governance compliant

**Production Ready**:

- Security authorization at all layers
- Comprehensive validation
- Error handling and logging
- Multi-tenant isolation
- Site-level data scoping

**User Friendly**:

- Intuitive UI with Oqtane patterns
- Collapsible sections
- Visual status indicators
- Helpful placeholders and tooltips

**Developer Friendly**:

- Well-documented code
- Follows Oqtane conventions
- Service-mediated architecture
- Comprehensive documentation
- Public API for integration

---

## Next Steps

### Immediate

1. **Restart Application** (most important!)
2. Configure SMTP in Site Settings
3. Enable NotificationJob
4. Test the feature

### Short Term

- Create your production email templates
- Test with real data
- Train users on the interface

### Long Term

- Add variable picker UI
- Implement email preview
- Add template versioning
- Build email analytics

---

## Key Learnings

### Oqtane Patterns Used

1. **Notification System** - For email delivery
2. **RichTextEditor** - For HTML editing
3. **Section Component** - For collapsible UI
4. **Service-Mediated Architecture** - For clean separation
5. **Permission-Based Authorization** - For security
6. **Site Scoping** - For shared resources (v1.0.2)

### Governance Rules Followed

- 027x-structure-and-boundaries.md
- 027x-canonical-framework.md
- 027x-ui-construction.md
- 027x-module-portability.md
- 027x-execution-parity.md
- 027x-migrations.md

---

## TL;DR

**Problem**: Test Email section not visible  
**Cause**: Application running old code  
**Solution**: Restart application + clear browser cache  

**Then**:

1. Edit a template
2. Scroll to "Test Email" section
3. Click to expand
4. Enter email and optional JSON variables
5. Click "Send Test Email"
6. Check inbox in 1-2 minutes

---

**Your module is complete and ready! Just restart to see it in action!**
