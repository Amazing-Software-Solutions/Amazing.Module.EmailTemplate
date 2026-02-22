# Module-Specific AI Instructions

These instructions extend the canonical Copilot instructions.

> ☝️ This should be copied to the Solution as it specific to the module.

Additional constraints:
- Deletions must always be confirmed using ActionDialog
- System fields are immutable except for visibility
- All user-facing strings must be localized (027x-localization is opted-in)


Module Architecture & Development Guidelines

## Core Principles

### Current Status

_Module-Specific AI Instructions._

The absence of entries indicates full canonical compliance.

---

## Data Scoping Memory (Added 2025-01-XX)

### Oqtane Multi-Tenant Data Scoping

Reference: https://www.oqtane.org/blog/!/72/data-scoping

Oqtane has 7 data scope levels:
1. **Installation Scope** - Global across all tenants
2. **Tenant Scope** - Shared across all sites in tenant/database
3. **Site Scope** - Site-level isolation (most common for shared resources)
4. **Page Scope** - Page-specific settings
5. **Module Scope** - Module instance-level (instance-specific data)
6. **Page/Module Scope** - Module on specific page combination
7. **User Scope** - User personalization

### When to Use Site Scope vs Module Scope

**Use Site Scope** when data is:
- Shared resource across modules (✓ Email templates, categories, lookups)
- Centrally managed by site admin
- Needs multi-tenant isolation but site-wide availability
- Not tied to specific module instance

**Use Module Scope** when data is:
- Module instance-specific (blog posts, comments, forms)
- User-generated content per module
- Tied to module configuration
- Needs module-level isolation

### EmailTemplate Module Pattern

This module uses **Site Scope (Level 3)**:
- **Data Storage**: `SiteId` column, FK to Site table
- **Authorization**: Module-based (need module permissions for UI)
- **Queries**: Filter by `WHERE SiteId = @siteId`
- **Result**: Templates shared across all modules in site

### Two-Layer Architecture

**Layer 1: Authorization (Module-Based)**
```csharp
// Controls WHO can manage via UI
if (_userPermissions.IsAuthorized(user, siteId, EntityNames.Module, moduleId, PermissionNames.Edit))
```

**Layer 2: Data Access (Site-Based)**
```csharp
// Controls WHICH data is available
db.EmailTemplate.Where(t => t.SiteId == siteId)
```

### Key Pattern to Remember

**Similar to Site Settings**:
- Site Settings are site-scoped data
- But require module permissions to access Settings UI
- EmailTemplate follows same proven pattern

**Code Example**:
```csharp
// Model
public class EmailTemplate : ModelBase
{
    public int SiteId { get; set; }  // Data scope
}

// Repository
IEnumerable<EmailTemplate> GetEmailTemplates(int siteId);  // Site-scoped query

// Service
public Task<List<EmailTemplate>> GetEmailTemplatesAsync(int moduleId)
{
    // moduleId for authorization check
    if (IsAuthorized(moduleId))
        return _repo.GetEmailTemplates(_alias.SiteId);  // siteId for data
}

// Controller
[HttpPost]
public async Task<EmailTemplate> Post([FromBody] EmailTemplate template, [FromQuery] int moduleid)
{
    // moduleid for authorization
    if (IsAuthorized(moduleid))
    {
        template.SiteId = _alias.SiteId;  // Force current site
        return await _service.AddEmailTemplateAsync(template, moduleid);
    }
}

// UI
EmailTemplate.SiteId = PageState.Site.SiteId;  // Data uses SiteId
await Service.AddEmailTemplateAsync(template, ModuleState.ModuleId);  // Auth uses ModuleId
```

### Migration Pattern

When changing scope, create migration to:
1. Rename column: `ModuleId` → `SiteId`
2. Drop old foreign key
3. Add new foreign key to correct table
4. Update EntityBuilder
5. Increment module version

**Example**: See `01000002_ChangeScopeToSiteId.cs`

---

**Remember**: Data scoping and authorization are separate concerns. Choose scope based on data sharing needs, not authorization requirements.

