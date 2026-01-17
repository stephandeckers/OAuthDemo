# Copilot Instructions for eFns.LocalPreAssignation

## Code Style & Conventions
- Use the existing debug logging pattern with `g.WriteLine()` and `d.WriteLine()`
- Follow the namespace organization: Controllers, Data, Services, and main API namespace
- Use the existing Global.CallCount pattern for tracking method calls
- Include thread ID in debug output: `({Environment.CurrentManagedThreadId}.{Global.CallCount++})`
- Use `#region -- Using directives --` for organizing imports
- Follow the existing file header format with @Name, @Purpose, @Date, @Author, @Description
- Private methods should follow camel case naming convention

## Global Using Directives Pattern
Always include at the top of files:
```csharp
global using d = System.Diagnostics.Debug;
global using g = Whoua.Core.Api.Global;
```

## Global documentation Pattern
- Use XML documentation comments for public APIs
- Add the author and date/time of a change in the comments using the current moment when editing
- Use the format: DD-MM-YYYY, HH:MM:SS (24-hour format)
- Example format:
```csharp
/// <summary date="03-08-2025, 17:46:00" author="Copilot">
/// Description of the method or class
/// </summary>
```

## API Conventions
- Use ApiController and Route attributes
- Follow RESTful naming conventions
- Include proper HTTP method attributes (HttpGet, HttpPost, etc.)
- Use IEnumerable for collections in API responses
- Controllers should be in format: `[Route("[controller]")]`

## Service Registration Pattern
Always register services in Program.cs:
```csharp
builder.Services.AddScoped<IBasicService, BasicService>();
```

## Constructor Patterns
```csharp
// Controller pattern with primary constructor (preferred)
public SomeController(ILogger<SomeController> logger) : ControllerBase
{
    // Use logger parameter directly, no need for private fields
}

// Traditional constructor pattern (legacy)
public SomeController(ILogger<SomeController> logger)
{
    g.WriteLine(GetType().Name);
    _logger = logger;
}

// Service pattern
public SomeService()
{
    g.WriteLine(GetType().Name);
}
```

## Primary Constructor Guidelines
- When using primary constructors, do NOT create redundant private readonly fields
- Use the constructor parameters directly throughout the class
- Primary constructor parameters are automatically available in all class methods
- Example: `public class MyController(IService service) : ControllerBase` - use `service` directly, not `_service`

## Method Implementation Patterns
- Use Global.CallCount for method tracking
- Include debug output in all public methods
- Return meaningful values, not hardcoded defaults

## File Header Template
```csharp
/**
 * @Name FileName.cs
 * @Purpose [Brief description]
 * @Date [DD Month YYYY, HH:MM:SS]
 * @Author Copilot
 * @Description [Detailed description]
 */
```

## git conventions
- When pushing, include a tag specifying the version like "v3.4.3.2".
- The version to be used can be found in file eFns.LocalPreAssignation\LPA.Common\Global.cs