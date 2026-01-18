# Copilot Instructions for OAuthDemo

## Program entry
- Don't use Toplevel programs
- Use Startup.cs for service registration

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
```

## Dependency Patterns
- Services used in multiple methods are to be injected using primary constructors
- Services used in a single methods are to be injected using method injection
```csharp
// Controller pattern with primary constructor if the service is used in multiple methods(preferred)
// Use method injection if there's only a single method using the service
[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger) : ControllerBase
{
	[ HttpGet( "Get1")]
	[ ProducesResponseType(StatusCodes.Status200OK)]
	[ SwaggerOperation(Tags = new[] { "Get1" }, Summary = "Get1")]
	public async Task<IActionResult> Get1( )
	{
	    logger.LogInfo( "Get1 running");
	}
	
	[ HttpGet( "Get2")]
	[ ProducesResponseType(StatusCodes.Status200OK)]
	[ SwaggerOperation(Tags = new[] { "Get2" }, Summary = "Get2")]
	public async Task<IActionResult> Get2( )
	{
	    logger.LogInfo( "Get1 running");
	}
	
	// Method injection
	[ HttpGet( "Get3")]
	[ ProducesResponseType(StatusCodes.Status200OK)]
	[ SwaggerOperation(Tags = new[] { "Get3" }, Summary = "Get3")]
	public async Task<IActionResult> Get3( ISomeService someService)
	{
	    logger.LogInfo( "Get2 running");
	    var result = someService.DoSomething
	    return Ok($"it worked:[{ result}]");
	}	
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
- When pushing, always increment the version
- When pushing, include a tag specifying the version like "v3.4.3.2".
