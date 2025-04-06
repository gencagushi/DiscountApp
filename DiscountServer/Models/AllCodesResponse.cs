namespace DiscountServer.Models;

public class AllCodesResponse
{
    public List<CodeEntry> AllCodes { get; set; } = new();
}

public class CodeEntry
{
    public string Code { get; set; }
    public bool Used { get; set; }
}

