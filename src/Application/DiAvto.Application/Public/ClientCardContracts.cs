namespace DiAvto.Application.Public;

public sealed record ClientCardSpecDto(string Label, string Value);

/// <summary>
/// Публичная карточка клиента: итоговая стоимость автомобиля под ключ.
/// Отдаётся только по неугадываемому <see cref="PublicId"/>.
/// </summary>
public sealed record ClientCardPublicDto(
    string PublicId,
    bool IsActive,
    DateTime CreatedAt,
    string? CustomerName,
    string CountryName,
    string CityName,
    string DestinationCity,
    string Title,
    int? Year,
    string Mileage,
    string Displacement,
    string Gearbox,
    string FuelType,
    string BodyType,
    string Color,
    string EmissionStandard,
    string FirstRegistration,
    int? TransferCount,
    bool IsNewEnergy,
    string? Note,
    decimal TotalRub,
    decimal ExchangeRate,
    decimal MarkupPercent,
    decimal EffectiveRate,
    bool HasCityDelivery,
    IReadOnlyList<string> PhotoUrls,
    IReadOnlyList<ClientCardSpecDto> Specs,
    string ContactText,
    string SiteName)
{
    public string MainPhotoUrl => PhotoUrls.Count > 0 ? PhotoUrls[0] : "/images/car-placeholder.svg";

    public bool HasContactText => !string.IsNullOrWhiteSpace(ContactText);
}

public interface IClientCardPublicService
{
    Task<ClientCardPublicDto?> GetAsync(string publicId, CancellationToken cancellationToken = default);
}
