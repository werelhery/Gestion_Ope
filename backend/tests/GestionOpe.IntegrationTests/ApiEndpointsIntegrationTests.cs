using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GestionOpe.Application.DTOs;
using GestionOpe.Application.Features.Auth;
using GestionOpe.Application.Features.Shipments.Commands;
using GestionOpe.Domain.Enums;
using Xunit;

namespace GestionOpe.IntegrationTests;

public class ApiEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Endpoint_Should_Return_Ok()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task GetShipments_Should_Return_Ok_With_List()
    {
        // Act
        var response = await _client.GetAsync("/api/shipments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var shipments = await response.Content.ReadFromJsonAsync<IReadOnlyList<ShipmentDto>>();
        shipments.Should().NotBeNull();
        shipments!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDashboardMetrics_Should_Return_Ok_With_Accurate_Data()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var metrics = await response.Content.ReadFromJsonAsync<DashboardMetricsDto>();
        metrics.Should().NotBeNull();
        metrics!.TotalShipments.Should().BeGreaterThan(0);
        metrics.TotalWarehouses.Should().Be(4);
    }

    [Fact]
    public async Task GetWarehouses_Should_Return_4_Seeded_Hubs()
    {
        // Act
        var response = await _client.GetAsync("/api/warehouses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var warehouses = await response.Content.ReadFromJsonAsync<IReadOnlyList<WarehouseDto>>();
        warehouses.Should().NotBeNull();
        warehouses!.Should().HaveCount(4);
        warehouses.Select(w => w.Code).Should().Contain("WH-PARIS-NORD");
    }

    [Fact]
    public async Task Login_Should_Return_Jwt_Token_For_Valid_Admin()
    {
        // Arrange
        var loginPayload = new LoginCommand("admin@gestionope.fr", "Admin123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.User.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task CreateShipment_With_Auth_Should_Return_201_Created()
    {
        // 1. Authenticate to obtain token
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginCommand("admin@gestionope.fr", "Admin123!"));
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        auth.Should().NotBeNull();

        // 2. Fetch warehouses to get a valid warehouse ID
        var whResponse = await _client.GetFromJsonAsync<IReadOnlyList<WarehouseDto>>("/api/warehouses");
        var originWhId = whResponse!.First().Id;

        // 3. Prepare authenticated request
        var createCommand = new CreateShipmentCommand(
            Description: "Équipements médicaux d'urgence",
            SenderName: "Labo Pasteur",
            RecipientName: "Clinique du Parc",
            OriginAddress: new AddressDto("15 Rue Pasteur", "Lille", "59000", "France", 50.6, 3.0),
            DestinationAddress: new AddressDto("20 Avenue Foch", "Paris", "75016", "France", 48.87, 2.28),
            OriginWarehouseId: originWhId,
            DestinationWarehouseId: null,
            CarrierId: null,
            Priority: ShipmentPriority.Critical,
            Dimensions: new DimensionsDto(50, 40, 30, 12, 0.06),
            IsTemperatureControlled: true,
            RequiredTemperatureCelsius: 5.0,
            ScheduledPickupDateUtc: DateTime.UtcNow.AddHours(2),
            EstimatedDeliveryDateUtc: DateTime.UtcNow.AddHours(8)
        );

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/shipments")
        {
            Content = JsonContent.Create(createCommand)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ShipmentDto>();
        created.Should().NotBeNull();
        created!.TrackingNumber.Should().StartWith("FR-EXP-");
        created.Status.Should().Be(ShipmentStatus.Scheduled);
    }
}
