using DiscountServer.Storage;

namespace DiscountServer;

public class DiscountService(DiscountStorage storage)
{
    private readonly DiscountStorage _storage = storage;
    private readonly object _lock = new();

    public List<string> GenerateCodes(ushort count, byte length)
    {
        if (length < 7 || length > 8 || count > 2000)
            return [];

        lock (_lock)
        {
            var newCodes = new HashSet<string>();
            var existing = _storage.GetAll();

            while (newCodes.Count < count)
            {
                string code = GenerateRandomCode(length);
                if (!existing.Contains(code) && !newCodes.Contains(code))
                    newCodes.Add(code);
            }

            _storage.SaveCodes(newCodes);
            return [.. newCodes];
        }
    }


    public byte UseCode(string code)
    {
        lock (_lock)
        {
            if (_storage.MarkCodeAsUsed(code))
                return 1;
            return 0;
        }
    }

    private static string GenerateRandomCode(int length)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var rand = new Random();
        var code = new char[length];

        for (int i = 0; i < length; i++)
            code[i] = chars[rand.Next(chars.Length)];

        return new string(code);
    }

    public Dictionary<string, bool> GetAllCodes()
    {
        lock (_lock)
        {
            return _storage.GetAllWithStatus();
        }
    }
}

