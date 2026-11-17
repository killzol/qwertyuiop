using System.Security.Cryptography;
using System.Text;

namespace искулинария.DbService;

public static class Verification
{
    public static string GetSHA512Hash(string input)
    {
        var hash = SHA512.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static bool VerifySHA512Hash(string input, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            var expected = Convert.FromHexString(hash);
            var actual = SHA512.HashData(Encoding.UTF8.GetBytes(input));
            return expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
