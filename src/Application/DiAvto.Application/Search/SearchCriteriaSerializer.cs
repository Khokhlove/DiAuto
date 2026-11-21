using System.Text.Json;

namespace DiAvto.Application.Search;

/// <summary>Сериализация критериев поиска для истории и снимка бюджета.</summary>
public static class SearchCriteriaSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize(SearchCriteria criteria)
    {
        return JsonSerializer.Serialize(criteria, Options);
    }

    public static SearchCriteria? TryDeserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<SearchCriteria>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}


