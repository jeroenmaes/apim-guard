# apim-guard

APIM Guard is an ASP.NET Core MVC application designed to simplify the management of OAuth2-secured APIs in Azure API Management by providing a unified interface for managing APIs, subscriptions, and Entra ID App Registrations.

## Features

- **API Management**: Manage APIs registered in Azure API Management
  - Create, view, and delete APIs
  - Import APIs from OpenAPI/Swagger specification files
  - Configure API endpoints and protocols
  
- **Product Management**: Manage API Products in Azure API Management
  - Create, view, and delete products
  - Configure product visibility and subscription settings
  - Set approval requirements and subscription limits
  - Define product terms and conditions
  
- **Subscription Management**: Control access to your APIs
  - Create and manage API subscriptions
  - Generate and regenerate subscription keys
  - Monitor subscription status
  
- **App Registration Management**: Manage Entra ID App Registrations
  - Create and delete app registrations
  - Manage client secrets with expiration tracking
  - Upload and manage client certificates
  - Support for OAuth2 authentication flows

- **Audit Logging**: Comprehensive auditing of user interactions
  - Automatic capture of all HTTP requests
  - Track user actions, IP addresses, and timestamps
  - In-memory storage for audit entries
  - Query audit logs by date range or user

## Prerequisites

- .NET 9.0 SDK
- Azure Subscription
- Azure API Management instance
- Entra ID (Azure Active Directory) access

## Configuration

Update the `appsettings.json` file with your Azure configuration:

```json
{
  "Azure": {
    "TenantId": "your-tenant-id",
    "SubscriptionId": "your-subscription-id",
    "ResourceGroupName": "your-resource-group",
    "ApiManagementServiceName": "your-apim-instance-name",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/jeroenmaes/apim-guard.git
   cd apim-guard
   ```

2. Navigate to the application directory:
   ```bash
   cd ApimGuard
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Update the configuration in `appsettings.json`

5. Run the application:
   ```bash
   dotnet run
   ```

6. Open your browser and navigate to `https://localhost:5001`

## Using the API Import Feature

APIM Guard supports importing APIs from OpenAPI/Swagger specification files. This allows you to quickly create APIs with their operations and schemas already defined.

### Supported Formats

- OpenAPI 3.0 (JSON or YAML)
- OpenAPI 2.0 / Swagger (JSON)
- WADL (XML)
- WSDL (XML)

### How to Import an API

1. Navigate to the "Create API" page
2. In the "Import from Specification File" section, click "Choose File"
3. Select your OpenAPI/Swagger specification file (`.json`, `.yaml`, `.yml`, or `.xml`)
4. Fill in the required API details:
   - **Name**: Unique identifier (lowercase, no spaces)
   - **Display Name**: Human-readable name
   - **Path**: URL path for the API (e.g., `/api/v1/products`)
   - **Service URL**: (Optional) Backend service URL if not specified in the file
   - **Description**: (Optional) API description
5. Click "Create"

The API will be created with all operations, parameters, and schemas defined in the specification file.

### Example OpenAPI Specification

Here's a minimal OpenAPI 3.0 specification example:

```json
{
  "openapi": "3.0.0",
  "info": {
    "title": "Sample API",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "https://api.example.com/v1"
    }
  ],
  "paths": {
    "/users": {
      "get": {
        "summary": "Get all users",
        "responses": {
          "200": {
            "description": "Successful response"
          }
        }
      }
    }
  }
}
```

## Running Tests

The project includes comprehensive unit tests to ensure code quality and reliability.

```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

For more information about testing, see [ApimGuard.Tests/README.md](ApimGuard.Tests/README.md).

## Project Structure

```
ApimGuard/
├── Controllers/
│   ├── ApiManagementController.cs     # API management operations
│   ├── ProductsController.cs          # Product management operations
│   ├── SubscriptionsController.cs     # Subscription management
│   ├── AppRegistrationsController.cs  # App registration management
│   └── HomeController.cs              # Home page
├── Models/
│   ├── ApiInfo.cs                     # API model
│   ├── ProductInfo.cs                 # Product model
│   ├── SubscriptionInfo.cs            # Subscription model
│   ├── AppRegistrationInfo.cs         # App registration model
│   ├── AppSecretInfo.cs              # Secret model
│   ├── AppCertificateInfo.cs         # Certificate model
│   └── AzureConfiguration.cs          # Azure config model
└── Views/
    ├── ApiManagement/                 # API management views
    ├── Products/                      # Product management views
    ├── Subscriptions/                 # Subscription views
    ├── AppRegistrations/              # App registration views
    └── Home/                          # Home page views

ApimGuard.Tests/
├── Controllers/                       # Controller unit tests
├── Models/                            # Model unit tests
└── Services/                          # Service unit tests
```

## Technologies Used

- ASP.NET Core 9.0 MVC
- Azure SDK for .NET
- Azure Resource Manager API Management
- Microsoft Graph SDK
- Azure Identity
- Entity Framework Core (In-Memory)
- Bootstrap 5
- xUnit (Testing Framework)
- Moq (Mocking Framework)

## Future Enhancements

- Integration with Azure API Management REST API
- Integration with Microsoft Graph API for Entra ID operations
- Authentication and authorization
- Role-based access control
- Persistent storage for audit logs (database integration)
- Audit log viewer UI
- API versioning support
- Advanced certificate management
- Export/Import configurations

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.