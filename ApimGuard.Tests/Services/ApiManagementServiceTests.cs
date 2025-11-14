using ApimGuard.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using System.Reflection;
using ApimGuard.Models;

namespace ApimGuard.Tests.Services;

public class ApiManagementServiceTests
{
    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldParseApplicationIds()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <application-id>app-id-1</application-id>
            <application-id>app-id-2</application-id>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Equal(2, applicationIds.Count);
        Assert.Contains("app-id-1", applicationIds);
        Assert.Contains("app-id-2", applicationIds);
        Assert.Empty(audiences);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldParseAudiences()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <audiences>
                <audience>api://audience-1</audience>
                <audience>api://audience-2</audience>
            </audiences>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Empty(applicationIds);
        Assert.Equal(2, audiences.Count);
        Assert.Contains("api://audience-1", audiences);
        Assert.Contains("api://audience-2", audiences);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldParseBothApplicationIdsAndAudiences()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <application-id>app-id-1</application-id>
            <application-id>app-id-2</application-id>
            <audiences>
                <audience>api://audience-1</audience>
                <audience>api://audience-2</audience>
                <audience>api://audience-3</audience>
            </audiences>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Equal(2, applicationIds.Count);
        Assert.Contains("app-id-1", applicationIds);
        Assert.Contains("app-id-2", applicationIds);
        Assert.Equal(3, audiences.Count);
        Assert.Contains("api://audience-1", audiences);
        Assert.Contains("api://audience-2", audiences);
        Assert.Contains("api://audience-3", audiences);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldHandleMultipleValidateTokenElements()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <application-id>app-id-1</application-id>
            <audiences>
                <audience>api://audience-1</audience>
            </audiences>
        </validate-azure-ad-token>
        <validate-azure-ad-token tenant-id=""tenant-456"">
            <application-id>app-id-2</application-id>
            <audiences>
                <audience>api://audience-2</audience>
            </audiences>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Equal(2, applicationIds.Count);
        Assert.Contains("app-id-1", applicationIds);
        Assert.Contains("app-id-2", applicationIds);
        Assert.Equal(2, audiences.Count);
        Assert.Contains("api://audience-1", audiences);
        Assert.Contains("api://audience-2", audiences);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldHandleEmptyXml()
    {
        // Arrange
        var policyXml = @"<policies></policies>";

        // Act
        var (applicationIds, audiences) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Empty(applicationIds);
        Assert.Empty(audiences);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldHandleXmlWithNoValidateToken()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <base />
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Empty(applicationIds);
        Assert.Empty(audiences);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldTrimWhitespace()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <application-id>  app-id-1  </application-id>
            <audiences>
                <audience>
                    api://audience-1
                </audience>
            </audiences>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Single(applicationIds);
        Assert.Contains("app-id-1", applicationIds);
        Assert.Single(audiences);
        Assert.Contains("api://audience-1", audiences);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldIgnoreEmptyValues()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <application-id></application-id>
            <application-id>app-id-1</application-id>
            <audiences>
                <audience></audience>
                <audience>api://audience-1</audience>
            </audiences>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Single(applicationIds);
        Assert.Contains("app-id-1", applicationIds);
        Assert.Single(audiences);
        Assert.Contains("api://audience-1", audiences);
    }

    // Helper method to invoke the private ParseAzureAdSecurityDetails method using reflection
    private (List<string> applicationIds, List<string> audiences) InvokeParseMethod(string policyXml)
    {
        // Create a mock configuration
        var mockConfig = new Mock<IOptions<AzureConfiguration>>();
        mockConfig.Setup(x => x.Value).Returns(new AzureConfiguration
        {
            TenantId = "test-tenant",
            SubscriptionId = "test-subscription",
            ResourceGroupName = "test-rg",
            ApiManagementServiceName = "test-apim",
            ClientId = "test-client",
            ClientSecret = "test-secret"
        });

        var mockLogger = new Mock<ILogger<ApiManagementService>>();
        var service = new ApiManagementService(mockConfig.Object, mockLogger.Object);

        // Use reflection to call the private method
        var method = typeof(ApiManagementService).GetMethod(
            "ParseAzureAdSecurityDetails",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (method == null)
        {
            throw new InvalidOperationException("ParseAzureAdSecurityDetails method not found");
        }

        var result = method.Invoke(service, new object[] { policyXml });
        
        // The method returns a tuple (List<string>, List<string>)
        dynamic tuple = result!;
        return (tuple.Item1, tuple.Item2);
    }
}
