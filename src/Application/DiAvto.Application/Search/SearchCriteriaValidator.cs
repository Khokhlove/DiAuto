namespace DiAvto.Application.Search;

/// <summary>Проверки поисковых критериев, общие для формы и сервиса выполнения.</summary>
public static class SearchCriteriaValidator
{
    public static void Validate(SearchCriteria criteria)
    {
        if (criteria.BrandId <= 0)
        {
            throw new ArgumentException("Выберите марку автомобиля.");
        }

        if (criteria.YearFrom is not null && criteria.YearTo is not null && criteria.YearFrom > criteria.YearTo)
        {
            throw new ArgumentException("Год «от» не может быть больше года «до».");
        }

        if (criteria.PriceFrom is not null && criteria.PriceTo is not null && criteria.PriceFrom > criteria.PriceTo)
        {
            throw new ArgumentException("Цена «от» не может быть больше цены «до».");
        }

        if (criteria.PriceFrom is < 0 || criteria.PriceTo is < 0 || criteria.MileageMaxKm is < 0)
        {
            throw new ArgumentException("Числовые фильтры не могут быть отрицательными.");
        }

        if (criteria.DisplacementFromL is < 0 || criteria.DisplacementToL is < 0)
        {
            throw new ArgumentException("Объём двигателя не может быть отрицательным.");
        }

        if (criteria.DisplacementFromL is not null && criteria.DisplacementToL is not null
            && criteria.DisplacementFromL > criteria.DisplacementToL)
        {
            throw new ArgumentException("Объём «от» не может быть больше объёма «до».");
        }

        if (criteria.DeliveryFromVladivostokTariffId is < 0)
        {
            throw new ArgumentException("Идентификатор тарифа не может быть отрицательным.");
        }

        if (criteria.EngineCc < 0 || criteria.PowerHp < 0)
        {
            throw new ArgumentException("Объём и мощность двигателя не могут быть отрицательными.");
        }
    }
}


