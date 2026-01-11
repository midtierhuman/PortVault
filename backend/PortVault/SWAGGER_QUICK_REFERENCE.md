# ?? Swagger + Copilot Quick Reference

## Access Swagger UI
```
https://localhost:5001/swagger
```

## Export API Spec for Copilot
```bash
curl https://localhost:5001/swagger/v1/swagger.json -o portvault-api.json
```

## Best Copilot Prompts

### Generate Types
```
@workspace Using portvault-api.json, generate TypeScript 
interfaces for CreateTransactionRequest and TransactionResponse
```

### Create Service
```
@workspace Using portvault-api.json, create an Axios-based 
TypeScript service for all transaction endpoints
```

### Create Hook
```
@workspace Using portvault-api.json, create a React Query hook 
for managing transactions with full CRUD operations
```

### Create Component
```
@workspace Using portvault-api.json, create a transaction list 
component with pagination, add, and delete functionality
```

### Generate Validation
```
@workspace Using portvault-api.json, create Zod schemas 
for all transaction request models
```

## Quick XML Documentation Template

```csharp
/// <summary>
/// Brief description
/// </summary>
/// <param name="paramName">Parameter description</param>
/// <returns>Return value description</returns>
/// <remarks>
/// Detailed explanation with examples:
/// 
///     POST /api/endpoint
///     {
///       "field": "value"
///     }
/// </remarks>
/// <response code="200">Success case</response>
/// <response code="400">Error case</response>
[HttpPost]
[SwaggerOperation(
    Summary = "Short summary",
    Description = "Detailed description",
    OperationId = "UniqueOperationId",
    Tags = new[] { "TagName" }
)]
[ProducesResponseType(typeof(ApiResponse<T>), 200)]
[ProducesResponseType(typeof(ApiResponse<object>), 400)]
public async Task<IActionResult> MethodName([FromBody] Request request)
{
    // Implementation
}
```

## Swagger Annotations

```csharp
[SwaggerTag("Description of this controller")]
public class MyController : ControllerBase

[SwaggerOperation(Summary = "...", Description = "...", Tags = new[] { "..." })]
public async Task<IActionResult> Method()

[SwaggerParameter("Description", Required = true)]
string paramName

[SwaggerRequestBody("Description", Required = true)]
RequestModel request

[SwaggerResponse(200, "Description", typeof(ResponseModel))]
```

## Testing Workflow

1. Start API: `dotnet run`
2. Open Swagger: `https://localhost:5001/swagger`
3. Click **Authorize**
4. Enter: `Bearer {your-token}`
5. Click any endpoint ? **Try it out**
6. Fill parameters ? **Execute**
7. View response

## Export and Use

```bash
# Export spec
curl https://localhost:5001/swagger/v1/swagger.json -o spec.json

# Generate TypeScript client
npx @openapitools/openapi-generator-cli generate \
  -i spec.json \
  -g typescript-axios \
  -o ./src/api

# Use with Copilot
# Place spec.json in your workspace, then:
# @workspace Using spec.json, create a [whatever you need]
```

## Enum Documentation

```csharp
/// <summary>
/// Trade type
/// </summary>
public enum TradeType
{
    /// <summary>Buy transaction</summary>
    Buy,
    
    /// <summary>Sell transaction</summary>
    Sell
}
```

## Response Type Annotations

```csharp
[ProducesResponseType(typeof(ApiResponse<T>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
```

## Common Issues

| Issue | Solution |
|-------|----------|
| Swagger not loading | Check `UseSwagger()` and `UseSwaggerUI()` order |
| XML comments missing | Build project, check bin folder for .xml file |
| Enum showing numbers | Verify EnumSchemaFilter is registered |
| Types not generated | Ensure spec.json is in workspace |
| Copilot not using spec | Reference explicitly: "Using spec.json" |

## Files Reference

- **SWAGGER_DOCUMENTATION_GUIDE.md** - Complete feature guide
- **COPILOT_WITH_SWAGGER_GUIDE.md** - Copilot usage examples
- **SWAGGER_IMPLEMENTATION_SUMMARY.md** - What changed

---

**Quick Test:**
```bash
# 1. Run API
dotnet run --project PortVault.Api

# 2. In browser
https://localhost:5001/swagger

# 3. Export spec
curl https://localhost:5001/swagger/v1/swagger.json -o api.json

# 4. Use with Copilot
# "@workspace Using api.json, create a TypeScript service"
```

?? **Done!** Your API is now fully documented and Copilot-ready!
