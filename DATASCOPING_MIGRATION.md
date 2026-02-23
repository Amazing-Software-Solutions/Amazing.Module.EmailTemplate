# Data Scoping Migration - ModuleId to SiteId

## Date: 2025-01-XX
## Version: 1.0.2
## Migration: 01000002_ChangeScopeToSiteId

---

## Why This Change?

### Oqtane Multi-Tenant Data Scoping

Based on [Oqtane Data Scoping Blog](https://www.oqtane.org/blog/!/72/data-scoping), Oqtane has 7 levels of data scope:

1. **Installation Scope** - Global (all tenants)
2. **Tenant Scope** - Database-level (all sites in tenant)
3. **Site Scope** - Site-level (most common for shared resources) :arrow_left: **We use this**
4. **Page Scope** - Page-level (page-specific settings)
5. **Module Scope** - Module instance-level (instance-specific data) :arrow_left: **Was using this**
6. **Page/Module Scope** - Combination (module on specific page)
7. **User Scope** - User-level (user personalization)

### EmailTemplate Module Requirements

Email templates are **shared resources** that should be available to:

- All modules across the entire site
- All pages within the site
- Any module that needs to send emails

**Correct Scope**: **Site Scope (Level 3)** :white_check_mark:

- Templates managed once per site
- Available to all modules in the site
- Isolated by site (multi-tenant safe)

**Incorrect Scope**: ~~Module Scope (Level 5)~~ :x:

- Would create separate templates per module instance
- Would require duplicating templates across modules
- Would break the reusable service pattern

---

## Changes Made

### 1. Database Schema

**File**: `Server/Migrations/01000002_ChangeScopeToSiteId.cs`

- Renamed column: `ModuleId` :arrow_right: `SiteId`
- Changed foreign key: `FK_AmazingEmailTemplate_Module` :arrow_right: `FK_AmazingEmailTemplate_Site`
- References: `Module.ModuleId` :arrow_right: `Site.SiteId`
- Down() method: Fully reversible

### 2. Entity Model

**File**: `Shared/Models/EmailTemplate.cs`

```csharp
// Before
public int ModuleId { get; set; }

// After
public int SiteId { get; set; }
```

### 3. EntityBuilder

**File**: `Server/Migrations/EntityBuilders/EmailTemplateEntityBuilder.cs`

```csharp
// Before
private readonly ForeignKey<EmailTemplateEntityBuilder> _moduleForeignKey = 
    new("FK_AmazingEmailTemplate_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

// After
private readonly ForeignKey<EmailTemplateEntityBuilder> _siteForeignKey = 
    new("FK_AmazingEmailTemplate_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);
```

### 4. Repository

**File**: `Server/Repository/EmailTemplateRepository.cs`

**Interface Changes**:

```csharp
// Before
IEnumerable<Models.EmailTemplate> GetEmailTemplates(int ModuleId);
Models.EmailTemplate GetEmailTemplateByName(string templateName, int? moduleId = null);
IEnumerable<Models.EmailTemplate> GetActiveEmailTemplates();
IEnumerable<Models.EmailTemplate> GetEmailTemplatesByCategory(string category);

// After
IEnumerable<Models.EmailTemplate> GetEmailTemplates(int SiteId);
Models.EmailTemplate GetEmailTemplateByName(string templateName, int siteId);
IEnumerable<Models.EmailTemplate> GetActiveEmailTemplates(int siteId);
IEnumerable<Models.EmailTemplate> GetEmailTemplatesByCategory(string category, int siteId);
```

**Query Changes**: All queries now filter by `SiteId` instead of `ModuleId`

### 5. Services

**File**: `Server/Services/EmailTemplateService.cs`

- Uses `_alias.SiteId` from ITenantManager
- Validates site isolation on all operations
- Authorization still uses ModuleId (for UI permissions)
- Data queries use SiteId (for site-wide access)

**File**: `Server/Services/EmailSendingService.cs`

- Updated to pass `siteId` to repository methods
- Enhanced validation to check `template.SiteId == siteId`

### 6. Controllers

**File**: `Server/Controllers/EmailTemplateController.cs`

- API signature updated (moduleid as query parameter for POST/PUT)
- Authorization validates module permissions (UI access)
- Service layer handles site-scoped data access

### 7. Client UI

**File**: `Client/Modules/Amazing.Module.EmailTemplate/Edit.razor`

```csharp
// Before
EmailTemplate.ModuleId = ModuleState.ModuleId;

// After
EmailTemplate.SiteId = PageState.Site.SiteId;
```

### 8. Manager (IPortable/ISearchable)

**File**: `Server/Manager/EmailTemplateManager.cs`

- `ExportModule`: Exports all site templates (not just module)
- `ImportModule`: Checks for duplicates by name before import
- `GetSearchContentsAsync`: Indexes all site templates

### 9. Version Update

**File**: `Client/Modules/Amazing.Module.EmailTemplate/ModuleInfo.cs`

```csharp
Version = "1.0.2",
ReleaseVersions = "1.0.0,1.0.1,1.0.2"
```

---

## Impact Analysis

### What Changed

:white_check_mark: Data is now site-scoped instead of module-scoped  
:white_check_mark: Templates shared across all modules in a site  
:white_check_mark: Multi-tenant isolation maintained (by SiteId)  
:white_check_mark: Migration handles schema change automatically  

### What Stayed the Same

:white_check_mark: UI authorization (still module-based permissions)  
:white_check_mark: API endpoints (still use moduleid parameter for auth)  
:white_check_mark: User experience (no visible changes)  
:white_check_mark: IEmailSendingService API (already used siteId)

### Why Authorization Still Uses ModuleId

**Important**: Authorization and data scoping are different concerns:

- **Authorization** (Module-based): Controls who can VIEW/EDIT templates via UI
  - Uses module permissions: `IsAuthorized(EntityNames.Module, ModuleId, PermissionNames.Edit)`
  - Only users with Edit permission on the EmailTemplate module can manage templates
  
- **Data Scoping** (Site-based): Controls which templates are available
  - Queries filter by: `WHERE SiteId = @siteId`
  - All modules in the site can use templates via IEmailSendingService
  - Templates isolated by site (multi-tenant safe)

**Analogy**: 

- Site Settings are site-scoped data
- But you need module permissions to access the Site Settings UI
- Same pattern here!

---

## Migration Safety

### Automatic Execution

- Migration runs on next application startup
- Oqtane detects version 1.0.2 > current version
- Executes `Up()` method automatically
- Renames column and updates foreign key

### Data Preservation

- Existing `ModuleId` values become `SiteId` values
- **Important**: Existing data will reference Module IDs
- On first run after migration, may need data cleanup
- Templates should be reassigned to proper SiteIds

### Rollback Support

- `Down()` method fully reverses changes
- Can downgrade to previous version if needed
- Renames `SiteId` back to `ModuleId`

---

## Testing Checklist

### After Migration

- [ ] Verify table schema: `SiteId` column exists
- [ ] Verify foreign key: `FK_AmazingEmailTemplate_Site`
- [ ] Check existing templates load in UI
- [ ] Create new template (should use current SiteId)
- [ ] Test email sending via IEmailSendingService
- [ ] Verify site isolation (no cross-site access)
- [ ] Test with multiple sites in tenant (if applicable)
- [ ] Verify export/import works

### Multi-Site Testing

```sql
-- Verify site isolation
SELECT EmailTemplateId, SiteId, Name, IsActive 
FROM AmazingEmailTemplate 
ORDER BY SiteId, Name;

-- Should show templates grouped by SiteId
-- Each site should only see its own templates
```

---

## Integration Impact

### For Consuming Modules

**No changes required!** 

The `IEmailSendingService` API already used `siteId` parameter:

```csharp
await _emailService.SendEmailByTemplateNameAsync(
    siteId,        // Already required
    templateName,
    toEmail,
    variables
);
```

### For Template Creation

Templates created via UI now automatically use current site's SiteId:

```csharp
// Before
EmailTemplate.ModuleId = ModuleState.ModuleId;  // Wrong scope

// After
EmailTemplate.SiteId = PageState.Site.SiteId;   // Correct scope
```

---

## Benefits of Site Scoping

### 1. Template Reusability

- Create template once
- All modules in site can use it
- No duplication needed

### 2. Centralized Management

- Site admin manages templates in one place
- Changes affect all modules immediately
- Consistent branding across site

### 3. Multi-Tenant Isolation

- Each site has its own templates
- No cross-site contamination
- Secure by design

### 4. Scalability

- Single source of truth per site
- Easier to maintain
- Better performance (fewer templates)

---

## Example: Before vs After

### Before (Module Scoped)

```
Site A:
  |-- Module Instance 1 (Registration Form)
  |   +-- EmailTemplate: "Welcome Email" (ID 1, ModuleId=10)
  |
  +-- Module Instance 2 (Contact Form)
      +-- EmailTemplate: "Welcome Email" (ID 2, ModuleId=11)  :x: Duplicate!

:x: Problem: Duplicated templates, harder to maintain
```

### After (Site Scoped)

```
Site A:
  |-- EmailTemplate: "Welcome Email" (ID 1, SiteId=1) :white_check_mark:
  |
  +-- All modules in Site A can use it:
      |-- Registration module
      |-- Contact module
      |-- E-commerce module
      +-- Any other module via IEmailSendingService

:white_check_mark: Benefit: Single source of truth, shared across site
```

---

## Data Migration Strategy

### For Existing Installations

If you have existing templates with ModuleId values:

**Option 1: Manual Reassignment (Recommended)**

1. Before upgrading, export existing templates
2. After migration, templates will have incorrect SiteId (old ModuleId value)
3. Delete invalid templates
4. Import templates (will use correct SiteId)

**Option 2: SQL Update Script (Advanced)**

```sql
-- Map ModuleId to SiteId after migration
UPDATE AmazingEmailTemplate 
SET SiteId = (SELECT SiteId FROM [Module] WHERE ModuleId = AmazingEmailTemplate.SiteId)
WHERE SiteId IN (SELECT ModuleId FROM [Module]);

-- Note: This assumes ModuleId values are still in the renamed column
```

**Option 3: Fresh Start**

1. Delete all existing templates before upgrade
2. Migrate (creates clean schema)
3. Create templates with proper SiteId

---

## Governance Compliance

### Migration Rules (027x-migrations.md)

:white_check_mark: **Rule 1**: Migration executes automatically on startup  
:white_check_mark: **Rule 2**: Version `01000002` follows 8-digit format  
:white_check_mark: **Rule 3**: Inherits `MultiDatabaseMigration`, proper attributes  
:white_check_mark: **Rule 4**: EntityBuilder updated appropriately  
:white_check_mark: **Rule 5**: `Up()` uses database-agnostic operations  
:white_check_mark: **Rule 6**: `Down()` fully reverses `Up()`  
:white_check_mark: **Rule 7**: Uses standard EF Core operations  
:white_check_mark: **Rule 8**: Model synchronized with schema  
:white_check_mark: **Rule 9**: `ReleaseVersion` updated to 1.0.2

---

## Future Considerations

### Template Sharing Across Sites (Tenant Scope)

If you need templates shared across multiple sites in a tenant:

- Would require Tenant Scope (Level 2)
- Add `TenantId` column instead of `SiteId`
- Reference: `Tenant.TenantId`

**Current Design Decision**: Site Scope is correct for most use cases

- Most installations want site isolation
- Templates often site-specific (branding, content)
- Can export/import between sites if needed

---

## Module File Structure

```
Amazing.Module.EmailTemplate/
|-- Shared/                           # Models and interfaces
|   |-- Models/
|   |   |-- EmailTemplate.cs         # SiteId (not ModuleId)
|   |   |-- EmailSendResult.cs
|   |   |-- TemplateInfo.cs
|   |   |-- TestEmailRequest.cs
|   |   +-- TestEmailResult.cs
|   +-- Services/
|       +-- IEmailSendingService.cs   # Public API
|
|-- Server/                           # Business logic and API
|   |-- Controllers/
|   |   +-- EmailTemplateController.cs
|   |-- Services/
|   |   |-- EmailTemplateService.cs  # Internal
|   |   +-- EmailSendingService.cs   # Public
|   |-- Repository/
|   |   |-- EmailTemplateContext.cs
|   |   +-- EmailTemplateRepository.cs  # Site-scoped queries
|   |-- Manager/
|   |   +-- EmailTemplateManager.cs
|   |-- Migrations/
|   |   |-- 01000000_InitializeModule.cs
|   |   |-- 01000001_AddEmailTemplateFields.cs
|   |   |-- 01000002_ChangeScopeToSiteId.cs  # This migration
|   |   +-- EntityBuilders/
|   |       +-- EmailTemplateEntityBuilder.cs
|   +-- Startup/
|       +-- ServerStartup.cs
|
+-- Client/                           # Blazor UI components
    |-- Modules/Amazing.Module.EmailTemplate/
    |   |-- Index.razor
    |   +-- Edit.razor
    +-- Services/
        +-- EmailTemplateService.cs
```

---

## Summary

**Change**: ModuleId :arrow_right: SiteId  
**Reason**: Email templates are site-wide shared resources  
**Scope**: Site Scope (Level 3) in Oqtane architecture  
**Impact**: Templates now available to all modules in site  
**Migration**: Version 1.0.2, automatic execution  
**Testing**: Verify site isolation and multi-tenant support

---

**This change aligns the module with Oqtane's architectural best practices for multi-tenant, shared resources.**
