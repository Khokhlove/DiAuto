using System.Security.Claims;
using DiAvto.Application.Access;
using DiAvto.Application.Catalog;
using DiAvto.Application.Formatting;
using DiAvto.Application.Pricing;
using DiAvto.Application.Search;
using DiAvto.Domain;
using DiAvto.UI.Formatting;

namespace DiAvto.Tests;

public class CurrencyAndSearchTests
{
    [Fact]
    public void CalculateRub_AppliesRateAndMarkup()
    {
        Assert.Equal(1_214_600m, PricingCalculator.CnyToRub(100_000m, 12.146m, 0m));
        Assert.Equal(1_251_038m, PricingCalculator.CnyToRub(100_000m, 12.146m, 3m));
    }

    [Fact]
    public void SearchCriteria_UsesExactCnyAndKm()
    {
        var criteria = new SearchCriteria
        {
            PriceFrom = 55_000m,
            PriceTo = 200_000m,
            MileageMaxKm = 85_000
        };

        Assert.Equal(55_000m, criteria.PriceFromCny);
        Assert.Equal(200_000m, criteria.PriceToCny);
        Assert.Equal(85_000, criteria.MileageMaxKm);
    }

    [Fact]
    public void SearchCriteria_ConvertsRubToCny()
    {
        var criteria = new SearchCriteria
        {
            PriceCurrency = PriceCurrency.Rub,
            PriceFrom = 1_214_600m,
            PriceTo = 2_429_200m,
            ExchangeRate = 12.146m,
            MarkupPercent = 0m
        };

        Assert.Equal(100_000m, criteria.PriceFromCny);
        Assert.Equal(200_000m, criteria.PriceToCny);
    }

    [Fact]
    public void PriceRange_FormatsBounds()
    {
        var text = DisplayFormat.PriceRange(50_000m, 200_000m);
        Assert.Contains("5", text);
        Assert.Contains("20", text);
        Assert.Contains("¥", text);
    }

    [Fact]
    public void AdminRole_HasAccessToEveryPage()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, "Admin")], "test"));

        Assert.True(AdminPageAccess.HasPage(principal, "catalog"));
        Assert.True(AdminPageAccess.HasPage(principal, "users"));
        Assert.True(AdminPageAccess.HasAnyPage(principal));
    }

    [Fact]
    public void Manager_NeedsExplicitPageClaim()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(AdminPageAccess.ClaimType, "catalog")], "test"));

        Assert.True(AdminPageAccess.HasPage(principal, "catalog"));
        Assert.False(AdminPageAccess.HasPage(principal, "users"));
        Assert.True(AdminPageAccess.HasAnyPage(principal));
    }

    [Fact]
    public void UserWithoutPageClaims_HasNoAdminAccess()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([], "test"));

        Assert.False(AdminPageAccess.HasPage(principal, "catalog"));
        Assert.False(AdminPageAccess.HasAnyPage(principal));
    }

    [Theory]
    [InlineData("汽油", "Бензин")]
    [InlineData("自动", "Автомат")]
    [InlineData("紧凑型车", "Компактный класс")]
    [InlineData("欧IV", "Евро-4")]
    [InlineData("温州", "Вэньчжоу")]
    [InlineData("香槟/棕色", "Шампань/Коричневый")]
    public void CarSpecTranslator_TranslatesKnownValues(string input, string expected)
    {
        Assert.Equal(expected, CarSpecTranslator.Translate(input));
    }

    [Fact]
    public void CarSpecTranslator_KeepsUnknownValues()
    {
        Assert.Equal("Неизвестное значение", CarSpecTranslator.Translate("Неизвестное значение"));
        Assert.Equal("", CarSpecTranslator.Translate(null));
    }

    [Fact]
    public void CarSpecTranslator_BuildsReadableTitle()
    {
        var title = CarSpecTranslator.BuildTitle(
            "沃尔沃", "沃尔沃V40", 2013, "2.0T", "自动", "沃尔沃V40 2013款 2.0T 智雅版");

        Assert.Equal("Volvo V40 2013 2.0T Автомат", title);
    }

    [Theory]
    [InlineData("雅阁", "Accord")]
    [InlineData("雅阁新能源", "Accord PHEV")]
    [InlineData("雅阁(海外)", "Accord (за рубежом)")]
    [InlineData("本田CR-V新能源(海外)", "CR-V PHEV (за рубежом)")]
    [InlineData("飞度新能源(进口)", "Fit PHEV (импорт)")]
    public void ModelLocalization_TranslatesSourceNames(string input, string expected)
    {
        Assert.Equal(expected, ModelLocalization.GetName(input));
    }

    [Fact]
    public void ModelLocalization_LeavesUnknownModelsUntranslated()
    {
        Assert.Null(ModelLocalization.GetName("未知车型"));
    }
}
