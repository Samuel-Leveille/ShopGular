using System.Security.Cryptography;
using System.Text;

namespace ShopGular.Backend;
public class PasswordHashing
{
    public static string HashPassword(string str)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        byte[] hash = sha256.ComputeHash(bytes);

        return Convert.ToHexString(hash);
    }

    public static bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        string hashedInput = HashPassword(plainPassword);
        return string.Equals(hashedInput, hashedPassword, StringComparison.OrdinalIgnoreCase);
    }
}