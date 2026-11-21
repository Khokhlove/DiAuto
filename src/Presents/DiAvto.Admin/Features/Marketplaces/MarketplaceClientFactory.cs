namespace DiAvto.Admin.Features.Marketplaces;

public sealed class MarketplaceClientFactory : IMarketplaceClientFactory
{
    private readonly Dictionary<string, IMarketplaceClient> _clients;

    public MarketplaceClientFactory(IEnumerable<IMarketplaceClient> clients)
    {
        _clients = clients.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<IMarketplaceClient> All => _clients.Values.ToList();

    public IMarketplaceClient Get(string key)
    {
        if (_clients.TryGetValue(key, out var client))
        {
            return client;
        }

        throw new InvalidOperationException($"Источник «{key}» не подключён.");
    }
}

