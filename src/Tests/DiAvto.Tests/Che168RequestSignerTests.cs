using DiAvto.Infrastructure.Marketplaces.Che168;

namespace DiAvto.Tests;

public class Che168RequestSignerTests
{
    [Fact]
    public void EmptyParameters_ProduceKnownSignature()
    {
        var signed = Che168RequestSigner.BuildSignedParameters(
            [],
            "2sc.m",
            "11.41.5",
            "2sc.pc");

        Assert.Equal("6d9199a82e3feaec0da69e9fbaad0b33", signed["_sign"]);
        Assert.Equal("2sc.m", signed["_appid"]);
        Assert.Equal("11.41.5", signed["v"]);
        Assert.Equal("2sc.pc", signed["_subappid"]);
    }

    [Fact]
    public void SearchParameters_ProduceKnownSignature()
    {
        var values = new Dictionary<string, string?>
        {
            ["cityid"] = "110100",
            ["pageindex"] = "1",
            ["pagesize"] = "20",
            ["ssnew"] = "1"
        };

        var signed = Che168RequestSigner.BuildSignedParameters(
            values,
            "2sc.m",
            "11.41.5",
            "2sc.pc");

        Assert.Equal("ebcfa9a6b6dd46bd3b4026e6760157ab", signed["_sign"]);
    }

    [Fact]
    public void QueryString_UrlEncodesValues()
    {
        var signed = Che168RequestSigner.BuildSignedParameters(
            [new KeyValuePair<string, string?>("keywords", "宝马 3系")],
            "2sc.m",
            "11.41.5",
            "");

        var query = Che168RequestSigner.BuildQueryString(signed);

        Assert.Contains("keywords=%E5%AE%9D%E9%A9%AC%203%E7%B3%BB", query);
        Assert.Contains("_sign=", query);
    }

    [Fact]
    public void WanFormatting_UsesInvariantCulture()
    {
        Assert.Equal("12.98", Che168RequestSigner.FormatWan(129_800m));
        Assert.Equal("0.01", Che168RequestSigner.FormatWan(100m));
    }
}
