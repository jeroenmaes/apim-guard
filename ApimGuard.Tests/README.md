# ApimGuard Tests

This directory contains unit tests for the APIM Guard application.

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run tests with verbose output
```bash
dotnet test --verbosity normal
```

### Run tests with code coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run specific test class
```bash
dotnet test --filter FullyQualifiedName~ApimGuard.Tests.Controllers.HomeControllerTests
```

### Run specific test method
```bash
dotnet test --filter FullyQualifiedName~ApimGuard.Tests.Controllers.HomeControllerTests.Index_ReturnsViewResult
```

## Test Structure

The test project is organized to mirror the main project structure:

```
ApimGuard.Tests/
├── Models/
│   ├── ApiInfoTests.cs              # Tests for ApiInfo model
│   ├── SubscriptionInfoTests.cs     # Tests for SubscriptionInfo model
│   └── ...
├── Controllers/
│   ├── HomeControllerTests.cs       # Tests for HomeController
│   ├── ApiManagementControllerTests.cs  # Tests for ApiManagementController
│   ├── SubscriptionsControllerTests.cs  # Tests for SubscriptionsController
│   └── ...
└── Services/
    ├── AzureConfigurationTests.cs   # Tests for AzureConfiguration
    └── ...
```

## Test Coverage

Current test coverage includes:

- **Models**: Unit tests for data models (ApiInfo, SubscriptionInfo, AzureConfiguration)
- **Controllers**: Unit tests for all MVC controllers with mocked dependencies
- **Services**: Configuration validation tests

## Testing Framework

- **xUnit**: Testing framework
- **Moq**: Mocking framework for creating test doubles
- **Microsoft.AspNetCore.Mvc.Testing**: ASP.NET Core testing utilities

## Writing New Tests

When adding new tests:

1. Follow the existing naming convention: `[ClassName]Tests.cs`
2. Use the Arrange-Act-Assert (AAA) pattern
3. Test one behavior per test method
4. Use descriptive test method names: `[MethodName]_[Scenario]_[ExpectedBehavior]`
5. Mock external dependencies using Moq

Example:
```csharp
[Fact]
public async Task GetApi_ReturnsApi_WhenApiExists()
{
    // Arrange
    var expectedApi = new ApiInfo { Id = "test", Name = "Test API" };
    _mockService.Setup(s => s.GetApiAsync("test")).ReturnsAsync(expectedApi);

    // Act
    var result = await _controller.Details("test");

    // Assert
    var viewResult = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<ApiInfo>(viewResult.Model);
    Assert.Equal("test", model.Id);
}
```
