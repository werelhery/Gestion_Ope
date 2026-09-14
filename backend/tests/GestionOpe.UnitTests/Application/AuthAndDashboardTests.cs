using FluentAssertions;
using GestionOpe.Application.Features.Auth;
using GestionOpe.Application.Features.Dashboard;
using GestionOpe.Application.Interfaces;
using GestionOpe.Domain.Entities;
using GestionOpe.Domain.Enums;
using GestionOpe.Domain.Exceptions;
using GestionOpe.Domain.Interfaces;
using GestionOpe.Domain.ValueObjects;
using Moq;
using Xunit;

namespace GestionOpe.UnitTests.Application;

public class AuthCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenService> _jwtServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task LoginCommandHandler_Should_Return_Token_When_Credentials_Valid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@gestionope.fr",
            PasswordHash = "hashed_pw",
            FirstName = "Jean",
            LastName = "Dupont",
            Role = UserRole.LogisticsManager,
            IsActive = true
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@gestionope.fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.VerifyPassword("ValidSecret123!", "hashed_pw"))
            .Returns(true);

        _jwtServiceMock.Setup(j => j.GenerateToken(user)).Returns("mocked_jwt_token");
        _jwtServiceMock.Setup(j => j.ExpirationMinutes).Returns(480);

        var handler = new LoginCommandHandler(
            _userRepoMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(new LoginCommand("test@gestionope.fr", "ValidSecret123!"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("mocked_jwt_token");
        result.User.Email.Should().Be("test@gestionope.fr");
        result.User.FullName.Should().Be("Jean Dupont");
    }

    [Fact]
    public async Task LoginCommandHandler_Should_Throw_DomainException_When_Password_Invalid()
    {
        // Arrange
        var user = new User
        {
            Email = "test@gestionope.fr",
            PasswordHash = "hashed_pw",
            IsActive = true
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@gestionope.fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.VerifyPassword("WrongPassword!", "hashed_pw"))
            .Returns(false);

        var handler = new LoginCommandHandler(
            _userRepoMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new LoginCommand("test@gestionope.fr", "WrongPassword!"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Identifiants invalides.");
    }
}

public class DashboardQueryHandlerTests
{
    private readonly Mock<IShipmentRepository> _shipmentRepoMock = new();
    private readonly Mock<IWarehouseRepository> _warehouseRepoMock = new();
    private readonly Mock<IIncidentRepository> _incidentRepoMock = new();

    [Fact]
    public async Task GetDashboardMetricsQueryHandler_Should_Compute_Correct_KPIs()
    {
        // Arrange
        _shipmentRepoMock.Setup(r => r.CountAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(100);
        _shipmentRepoMock.Setup(r => r.CountAsync(ShipmentStatus.InTransit, It.IsAny<CancellationToken>())).ReturnsAsync(30);
        _shipmentRepoMock.Setup(r => r.CountAsync(ShipmentStatus.Delivered, It.IsAny<CancellationToken>())).ReturnsAsync(60);
        _shipmentRepoMock.Setup(r => r.CountDelayedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(5);

        var warehouses = new List<Warehouse>
        {
            new() { TotalCapacityM3 = 1000, UsedCapacityM3 = 800 }, // 80%
            new() { TotalCapacityM3 = 1000, UsedCapacityM3 = 600 }  // 60%
        };
        _warehouseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(warehouses);

        _incidentRepoMock.Setup(r => r.GetOpenIncidentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Incident>());

        _shipmentRepoMock.Setup(r => r.GetAllAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Shipment>());

        var handler = new GetDashboardMetricsQueryHandler(
            _shipmentRepoMock.Object,
            _warehouseRepoMock.Object,
            _incidentRepoMock.Object);

        // Act
        var result = await handler.Handle(new GetDashboardMetricsQuery(), CancellationToken.None);

        // Assert
        result.TotalShipments.Should().Be(100);
        result.InTransitShipments.Should().Be(30);
        result.DeliveredShipments.Should().Be(60);
        result.DelayedShipments.Should().Be(5);
        result.OnTimeDeliveryRatePercentage.Should().Be(95.0); // (100 - 5) / 100 = 95%
        result.TotalWarehouses.Should().Be(2);
        result.AverageWarehouseUtilizationPercentage.Should().Be(70.0); // (80 + 60) / 2 = 70%
    }
}
