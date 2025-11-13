# apim-guard

APIM Guard is an ASP.NET Core MVC application designed to simplify the management of OAuth2-secured APIs in Azure API Management by providing a unified interface for managing APIs, subscriptions, and Entra ID App Registrations.

## Features

- **API Management**: Manage APIs registered in Azure API Management
  - Create, view, and delete APIs
  - Configure API endpoints and protocols
  
- **Subscription Management**: Control access to your APIs
  - Create and manage API subscriptions
  - Generate and regenerate subscription keys
  - Monitor subscription status
  
- **App Registration Management**: Manage Entra ID App Registrations
  - Create and delete app registrations
  - Manage client secrets with expiration tracking
  - Upload and manage client certificates
  - Support for OAuth2 authentication flows

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
│   ├── SubscriptionsController.cs     # Subscription management
│   ├── AppRegistrationsController.cs  # App registration management
│   └── HomeController.cs              # Home page
├── Models/
│   ├── ApiInfo.cs                     # API model
│   ├── SubscriptionInfo.cs            # Subscription model
│   ├── AppRegistrationInfo.cs         # App registration model
│   ├── AppSecretInfo.cs              # Secret model
│   ├── AppCertificateInfo.cs         # Certificate model
│   └── AzureConfiguration.cs          # Azure config model
└── Views/
    ├── ApiManagement/                 # API management views
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
- Audit logging
- API versioning support
- Advanced certificate management
- Export/Import configurations

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.