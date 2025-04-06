namespace DiscountServer.Storage
{
    public class DiscountStorage
    {
        private readonly string _filePath;
        private readonly Dictionary<string, bool> _codes;

        public DiscountStorage(string filePath)
        {
            _filePath = filePath;
            _codes = [];

            if (File.Exists(_filePath))
            {
                foreach (var line in File.ReadAllLines(_filePath))
                {
                    var parts = line.Split('|');
                    _codes[parts[0]] = parts[1] == "1";
                }
            }
        }

        public HashSet<string> GetAll() => [.. _codes.Keys];

        public void SaveCodes(HashSet<string> newCodes)
        {
            using var writer = File.AppendText(_filePath);
            foreach (var code in newCodes)
            {
                _codes[code] = false;
                writer.WriteLine($"{code}|0");
            }
        }

        public bool MarkCodeAsUsed(string code)
        {
            if (_codes.TryGetValue(code, out bool used) && !used)
            {
                _codes[code] = true;
                File.WriteAllLines(_filePath, GetAllLines());
                return true;
            }
            return false;
        }

        private IEnumerable<string> GetAllLines()
        {
            foreach (var kvp in _codes)
                yield return $"{kvp.Key}|{(kvp.Value ? "1" : "0")}";
        }

        public Dictionary<string, bool> GetAllWithStatus()
        {
            return new Dictionary<string, bool>(_codes);
        }
    }
}

