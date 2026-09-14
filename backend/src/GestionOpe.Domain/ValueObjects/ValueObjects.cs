namespace GestionOpe.Domain.ValueObjects;

public record Address(
    string Street,
    string City,
    string PostalCode,
    string Country,
    double Latitude = 0.0,
    double Longitude = 0.0
);

public record Dimensions(
    double LengthCm,
    double WidthCm,
    double HeightCm,
    double WeightKg
)
{
    public double VolumeM3 => Math.Round((LengthCm * WidthCm * HeightCm) / 1_000_000.0, 4);
}
