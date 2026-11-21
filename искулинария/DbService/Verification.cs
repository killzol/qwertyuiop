using System.Security.Cryptography;
using System.Text;

namespace искулинария.DbService;

public static class Verification
{
    public static string GetSHA512Hash(string input)
    {
        byte[] dannie;
        using (SHA512 sha512 = SHA512.Create())
        {
            dannie = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        StringBuilder stroka = new StringBuilder();
        foreach (byte chislo in dannie)
            stroka.Append(chislo.ToString("x2"));

        return stroka.ToString();
    }

    public static bool VerifySHA512Hash(string input, string hash)
    {
        string hashVvoda = GetSHA512Hash(input);
        return StringComparer.OrdinalIgnoreCase.Equals(hashVvoda, hash);
    }
}
