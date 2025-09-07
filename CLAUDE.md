# CLAUDE.md - Development Guidelines
## C# Coding Standards

### Enforcement Levels

These guidelines include three enforcement levels:
- **WARNING**: Must be followed - these rules are enforced and violations will cause warnings
- **SUGGESTION**: Should be followed - these are recommended practices
- **SILENT/NONE**: Optional - developer preference is acceptable

### File Formatting

**Indentation and Spacing:**
- **C# files (.cs)**: 4 spaces (no tabs)
- **Config files (.csproj, .config)**: 2 spaces
- **Web files (JS, TS, JSON, XML, HTML, CSS, SCSS)**: 2 spaces

**File Structure:**
- Line endings: **CRLF** (Windows style)
- Encoding: **UTF-8**
- Always insert a **final newline**
- File-scoped namespaces preferred: `namespace FlexyBox.Core.Administration;`

### Naming Conventions (WARNING Level - Must Follow)

| Symbol Type | Style | Example |
|------------|-------|---------|
| **Namespaces** | PascalCase | `FlexyBox.Core.Administration` |
| **Types** (classes, structs, enums) | PascalCase | `NewsService`, `NewsModel` |
| **Interfaces** | IPascalCase | `INewsService`, `IBackendApi` |
| **Methods** | PascalCase | `GetNews()`, `ProcessData()` |
| **Public/Internal Properties/Fields** | PascalCase | `public string Title { get; set; }` |
| **Private/Protected Fields** | _PascalCase | `private readonly IService _service;` |
| **Public/Internal Constants** | UPPER_CASE | `public const string API_BASE_URL = "";` |
| **Private/Protected Constants** | _UPPER_CASE | `private const int _MAX_RETRY_COUNT = 3;` |
| **Parameters** | camelCase | `public void Method(string userName)` |
| **Local Variables** | camelCase | `var localVariable = "";` |

### C# Language Preferences

**Variable Declarations (`var` usage):**
- **SUGGESTION**: Use `var` when type is apparent:
  ```csharp
  var user = new User();  // Type is obvious
  var count = 10;         // Built-in type
  ```
- Use explicit types when not obvious:
  ```csharp
  IEnumerable<string> items = GetItems();  // Return type not immediately clear
  ```

**Expression-Bodied Members:**
- Use for single-line implementations:
  ```csharp
  public string FullName => $"{FirstName} {LastName}";
  public void Log(string msg) => _logger.LogInformation(msg);
  ```
- **WARNING**: Do NOT use for constructors

**Modern C# Features (Preferred):**
- Pattern matching over traditional checks:
  ```csharp
  // ✅ Preferred
  if (obj is User user) { }
  
  // ❌ Avoid
  if (obj is User) { var user = (User)obj; }
  ```
- Simple using statements: `using var stream = new FileStream(...);`
- Auto properties: `public string Name { get; set; }`
- Readonly fields: `private readonly IService _service;`
- Null propagation: `user?.Name?.Length`
- String interpolation: `$"Hello {name}"`

### Code Organization

**Using Directives (WARNING Level):**
- Place **outside** namespace declarations
- Do NOT separate into groups
- Do NOT sort System directives first

```csharp
using Microsoft.Extensions.DependencyInjection;
using System;
using FlexyBox.Core.Administration;

namespace FlexyBox.Core.Administration.FlexyLoyalty;
```

**Type Keywords (WARNING Level):**
- Use language keywords over BCL types:
  ```csharp
  // ✅ Correct
  string text = "";
  int count = 0;
  bool isValid = true;
  
  // ❌ Avoid
  String text = "";
  Int32 count = 0;
  Boolean isValid = true;
  ```

**Modifier Order:**
```
public, private, protected, internal, file, static, extern, new, virtual, abstract, sealed, override, readonly, unsafe, required, volatile, async
```

**Code Blocks:**
- Braces required for multiline statements (WARNING)
- No unnecessary `this.` qualification

### Formatting Rules

**Brace Placement:**
- New line before **all** opening braces
- New line before `catch`, `else`, `finally`

```csharp
public class NewsService
{
    public void ProcessNews()
    {
        if (condition)
        {
            // code
        }
        else
        {
            // code
        }
    }
}
```

**Spacing:**
- Space after keywords: `if (condition)`, `for (int i = 0; ...)`
- Space around binary operators: `x + y`, `a == b`
- Space after comma: `Method(a, b, c)`
- NO space after cast: `(int)value`
- NO space before/after dots: `obj.Method()`
- NO space before comma, semicolon, or brackets

**Indentation:**
- Indent block contents
- Indent `case` contents in switch statements
- Do NOT indent braces themselves

### Blazor Component Guidelines (WARNING Level - Must Follow)

**Logic Separation:**
- **All logic MUST be in .razor.cs files** - Keep .razor files for markup only
- Conditional logic, calculations, and complex expressions belong in code-behind
- Use computed properties in .razor.cs instead of inline expressions in .razor

**❌ WRONG - Logic in .razor file:**
```razor
<!-- NewsPublishedTag.razor - BAD EXAMPLE -->
<FbxTag 
    Type="@(IsPublished == true ? FbxTagType.Success : FbxTagType.Ghost)"
    Corners="@FbxTagCorners.Regular">
    <FbxCaption>
        <b>@(IsPublished == true ? 
            _Localizer[AdministrationResources.FlexyLoyaltyNews_PublishedTag] : 
            _Localizer[AdministrationResources.FlexyLoyaltyNews_UnpublishedTag])</b>
    </FbxCaption>
</FbxTag>
```

**✅ CORRECT - Logic in .razor.cs file:**
```razor
<!-- NewsPublishedTag.razor - GOOD EXAMPLE -->
<FbxTag Type=_FbxTag
        Corners=@FbxTagCorners.Regular>
    <FbxCaption>
        <b>@_CaptionText</b>
    </FbxCaption>
</FbxTag>
```

```csharp
// NewsPublishedTag.razor.cs - GOOD EXAMPLE
public partial class NewsPublishedTag : ComponentBase
{
    [Parameter] public bool IsPublished { get; set; }
    
    private FbxTagType _FbxTag => IsPublished ? FbxTagType.Success : FbxTagType.Ghost;
    
    private string _CaptionText => IsPublished 
        ? _Localizer[AdministrationResources.FlexyLoyaltyNews_PublishedTag] 
        : _Localizer[AdministrationResources.FlexyLoyaltyNews_UnpublishedTag];
}
```

**Benefits:**
- Improved readability and maintainability
- Better testability (logic can be unit tested)
- Clear separation of concerns
- Easier debugging and refactoring

### Relaxed Rules (Not Enforced)

The following patterns are **optional** - use based on readability and context:

- **Conditional expressions**: Traditional if/else is acceptable instead of ternary operators
- **Switch statements**: Traditional switch statements are acceptable instead of switch expressions
- **Namespace structure**: Namespaces don't need to match folder hierarchy
- **Null checks**: Traditional null checks are acceptable alongside null-conditional operators
- **Top-level statements**: Not preferred but not enforced
- **Primary constructors**: Not preferred but allowed

### Suppressed Diagnostics

The following code analysis rules are intentionally disabled:

- **IDE0045/IDE0046**: Conditional expressions for assignment/return (use if/else when clearer)
- **IDE0058**: Unnecessary expression value
- **IDE0066**: Convert switch statement to expression
- **IDE0130**: Namespace doesn't match folder structure
- **IDE0270**: Simplified null checks
- **CA1016**: AssemblyVersionAttribute not required
- **CA1826**: FirstOrDefault/LastOrDefault are acceptable