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
            <client-application-ids>
                <application-id>app-id-1</application-id>
                <application-id>app-id-2</application-id>
            </client-application-ids>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Equal(2, applicationIds.Count);
        Assert.Contains("app-id-1", applicationIds);
        Assert.Contains("app-id-2", applicationIds);
        Assert.Empty(audiences);
        Assert.Empty(requiredClaims);
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
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Empty(applicationIds);
        Assert.Equal(2, audiences.Count);
        Assert.Contains("api://audience-1", audiences);
        Assert.Contains("api://audience-2", audiences);
        Assert.Empty(requiredClaims);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldParseBothApplicationIdsAndAudiences()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <audiences>
                <audience>api://audience-1</audience>
                <audience>api://audience-2</audience>
                <audience>api://audience-3</audience>
            </audiences>
            <client-application-ids>
                <application-id>app-id-1</application-id>
                <application-id>app-id-2</application-id>
            </client-application-ids>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Equal(2, applicationIds.Count);
        Assert.Contains("app-id-1", applicationIds);
        Assert.Contains("app-id-2", applicationIds);
        Assert.Equal(3, audiences.Count);
        Assert.Contains("api://audience-1", audiences);
        Assert.Contains("api://audience-2", audiences);
        Assert.Contains("api://audience-3", audiences);
        Assert.Empty(requiredClaims);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldHandleMultipleValidateTokenElements()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <audiences>
                <audience>api://audience-1</audience>
            </audiences>
            <client-application-ids>
                <application-id>app-id-1</application-id>
            </client-application-ids>
        </validate-azure-ad-token>
        <validate-azure-ad-token tenant-id=""tenant-456"">
            <audiences>
                <audience>api://audience-2</audience>
            </audiences>
            <client-application-ids>
                <application-id>app-id-2</application-id>
            </client-application-ids>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Equal(2, applicationIds.Count);
        Assert.Contains("app-id-1", applicationIds);
        Assert.Contains("app-id-2", applicationIds);
        Assert.Equal(2, audiences.Count);
        Assert.Contains("api://audience-1", audiences);
        Assert.Contains("api://audience-2", audiences);
        Assert.Empty(requiredClaims);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldHandleEmptyXml()
    {
        // Arrange
        var policyXml = @"<policies></policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Empty(applicationIds);
        Assert.Empty(audiences);
        Assert.Empty(requiredClaims);
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
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Empty(applicationIds);
        Assert.Empty(audiences);
        Assert.Empty(requiredClaims);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldTrimWhitespace()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <audiences>
                <audience>
                    api://audience-1
                </audience>
            </audiences>
            <client-application-ids>
                <application-id>  app-id-1  </application-id>
            </client-application-ids>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Single(applicationIds);
        Assert.Contains("app-id-1", applicationIds);
        Assert.Single(audiences);
        Assert.Contains("api://audience-1", audiences);
        Assert.Empty(requiredClaims);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldIgnoreEmptyValues()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <audiences>
                <audience></audience>
                <audience>api://audience-1</audience>
            </audiences>
            <client-application-ids>
                <application-id></application-id>
                <application-id>app-id-1</application-id>
            </client-application-ids>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Single(applicationIds);
        Assert.Contains("app-id-1", applicationIds);
        Assert.Single(audiences);
        Assert.Contains("api://audience-1", audiences);
        Assert.Empty(requiredClaims);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldParseRequiredClaims()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <required-claims>
                <claim name=""roles"" match=""any"">
                    <value>admin</value>
                    <value>user</value>
                </claim>
            </required-claims>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Empty(applicationIds);
        Assert.Empty(audiences);
        Assert.Single(requiredClaims);
        var claim = requiredClaims[0];
        Assert.Equal("roles", claim.Name);
        Assert.Equal("any", claim.Match);
        Assert.Equal(2, claim.Values.Count);
        Assert.Contains("admin", claim.Values);
        Assert.Contains("user", claim.Values);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldParseRequiredClaimsWithDefaultMatch()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <required-claims>
                <claim name=""sub"">
                    <value>specific-user-id</value>
                </claim>
            </required-claims>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Single(requiredClaims);
        var claim = requiredClaims[0];
        Assert.Equal("sub", claim.Name);
        Assert.Equal("all", claim.Match); // Default match type
        Assert.Single(claim.Values);
        Assert.Contains("specific-user-id", claim.Values);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldParseMultipleRequiredClaims()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <required-claims>
                <claim name=""roles"" match=""any"">
                    <value>admin</value>
                </claim>
                <claim name=""scope"" match=""all"">
                    <value>read</value>
                    <value>write</value>
                </claim>
            </required-claims>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Equal(2, requiredClaims.Count);
        
        var rolesClaim = requiredClaims.FirstOrDefault(c => c.Name == "roles");
        Assert.NotNull(rolesClaim);
        Assert.Equal("any", rolesClaim.Match);
        Assert.Single(rolesClaim.Values);
        Assert.Contains("admin", rolesClaim.Values);
        
        var scopeClaim = requiredClaims.FirstOrDefault(c => c.Name == "scope");
        Assert.NotNull(scopeClaim);
        Assert.Equal("all", scopeClaim.Match);
        Assert.Equal(2, scopeClaim.Values.Count);
        Assert.Contains("read", scopeClaim.Values);
        Assert.Contains("write", scopeClaim.Values);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldParseCompletePolicy()
    {
        // Arrange - This is the example from the issue
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""xxx"">
            <client-application-ids>
                <application-id>tttt</application-id>
            </client-application-ids>
            <audiences>
                <audience>api://ccccc</audience>
            </audiences>
            <required-claims>
                <claim name=""roles"" match=""any"">
                    <value>xyc</value>
                </claim>
            </required-claims>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Single(applicationIds);
        Assert.Contains("tttt", applicationIds);
        Assert.Single(audiences);
        Assert.Contains("api://ccccc", audiences);
        Assert.Single(requiredClaims);
        var claim = requiredClaims[0];
        Assert.Equal("roles", claim.Name);
        Assert.Equal("any", claim.Match);
        Assert.Single(claim.Values);
        Assert.Contains("xyc", claim.Values);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldIgnoreClaimsWithoutName()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <required-claims>
                <claim match=""any"">
                    <value>some-value</value>
                </claim>
                <claim name=""roles"" match=""any"">
                    <value>admin</value>
                </claim>
            </required-claims>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Single(requiredClaims);
        Assert.Equal("roles", requiredClaims[0].Name);
    }

    [Fact]
    public void ParseAzureAdSecurityDetails_ShouldIgnoreEmptyClaimValues()
    {
        // Arrange
        var policyXml = @"
<policies>
    <inbound>
        <validate-azure-ad-token tenant-id=""tenant-123"">
            <required-claims>
                <claim name=""roles"" match=""any"">
                    <value></value>
                    <value>admin</value>
                    <value>  </value>
                </claim>
            </required-claims>
        </validate-azure-ad-token>
    </inbound>
</policies>";

        // Act
        var (applicationIds, audiences, requiredClaims) = InvokeParseMethod(policyXml);

        // Assert
        Assert.Single(requiredClaims);
        var claim = requiredClaims[0];
        Assert.Single(claim.Values);
        Assert.Contains("admin", claim.Values);
    }

    // Helper method to invoke the private ParseAzureAdSecurityDetails method using reflection
    private (List<string> applicationIds, List<string> audiences, List<RequiredClaim> requiredClaims) InvokeParseMethod(string policyXml)
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
        
        // The method returns a tuple (List<string>, List<string>, List<RequiredClaim>)
        dynamic tuple = result!;
        return (tuple.Item1, tuple.Item2, tuple.Item3);
    }
}
