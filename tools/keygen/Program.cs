// Arcadia Controls — License Key Generator
// Usage: dotnet run -- [pro|enterprise] [count]
// Example: dotnet run -- pro 5

var tier = args.Length > 0 ? args[0].ToLower() : "pro";
var count = args.Length > 1 && int.TryParse(args[1], out var c) ? c : 1;

var prefix = tier switch
{
    "enterprise" or "ent" or "e" => "E",
    _ => "P"
};

var tierName = prefix == "E" ? "Enterprise" : "Pro";

Console.WriteLine($"Generating {count} {tierName} key(s):\n");

var rng = new Random();
const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

for (var i = 0; i < count; i++)
{
    // Generate two random 4-char groups (first char is the tier prefix)
    var g1 = prefix + RandomChars(3);
    var g2 = RandomChars(4);

    // Compute checksum from the two groups
    var payload = g1 + g2;
    var checksum = ComputeChecksum(payload);

    var key = $"ARC-{g1}-{g2}-{checksum}";
    Console.WriteLine(key);
}

string RandomChars(int len)
{
    var result = new char[len];
    for (var i = 0; i < len; i++)
        result[i] = chars[rng.Next(chars.Length)];
    return new string(result);
}

string ComputeChecksum(string payload)
{
    long hash = 0;
    for (var i = 0; i < payload.Length; i++)
        hash = (hash * 31) + payload[i];

    hash = Math.Abs(hash);

    const string c = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var result = new char[4];
    for (var i = 0; i < 4; i++)
    {
        result[i] = c[(int)(hash % c.Length)];
        hash /= c.Length;
    }
    return new string(result);
}
