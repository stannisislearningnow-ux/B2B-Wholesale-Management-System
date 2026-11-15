using System;
using System.Security.Cryptography;

namespace B2BWholesale.Security
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string HashPassword(string password)
        {
            if (String.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.");

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);
                return "PBKDF2$SHA256$" + Iterations + "$" +
                    Convert.ToBase64String(salt) + "$" +
                    Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (String.IsNullOrEmpty(password) || String.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                string[] parts = storedHash.Split('$');
                if (parts.Length != 5 || parts[0] != "PBKDF2" ||
                    parts[1] != "SHA256") return false;

                int iterations;
                if (!Int32.TryParse(parts[2], out iterations) ||
                    iterations < 10000 || iterations > 2000000) return false;

                byte[] salt = Convert.FromBase64String(parts[3]);
                byte[] expected = Convert.FromBase64String(parts[4]);
                if (salt.Length < 16 || expected.Length != 32) return false;

                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    byte[] actual = pbkdf2.GetBytes(expected.Length);
                    int difference = 0;
                    for (int i = 0; i < actual.Length; i++)
                        difference |= actual[i] ^ expected[i];
                    return difference == 0;
                }
            }
            catch { return false; }
        }
    }
}
