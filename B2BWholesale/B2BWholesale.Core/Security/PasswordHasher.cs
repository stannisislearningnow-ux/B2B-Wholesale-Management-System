using System;
using System.Security.Cryptography;
using System.Text;

namespace B2BWholesale.Core.Security
{
    /// <summary>PBKDF2-HMAC-SHA256 password hashing shared by the desktop and web applications.</summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;
        private const string Prefix = "PBKDF2$SHA256$";

        public static string HashPassword(string password)
        {
            if (String.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            byte[] hash = Pbkdf2Sha256(password, salt, Iterations, HashSize);
            return Prefix + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (String.IsNullOrEmpty(password) || String.IsNullOrEmpty(storedHash)) return false;

            try
            {
                string[] parts = storedHash.Split('$');
                if (parts.Length != 5 || parts[0] != "PBKDF2" || parts[1] != "SHA256") return false;

                int iterations;
                if (!Int32.TryParse(parts[2], out iterations) || iterations < 10000 || iterations > 2000000)
                    return false;

                byte[] salt = Convert.FromBase64String(parts[3]);
                byte[] expected = Convert.FromBase64String(parts[4]);
                if (salt.Length < SaltSize || expected.Length != HashSize) return false;

                byte[] actual = Pbkdf2Sha256(password, salt, iterations, expected.Length);
                int difference = 0;
                for (int i = 0; i < actual.Length; i++) difference |= actual[i] ^ expected[i];
                return difference == 0;
            }
            catch (FormatException) { return false; }
            catch (CryptographicException) { return false; }
        }

        // Small RFC 2898 implementation so the same PBKDF2-SHA256 format can be
        // consumed by .NET Framework 4.7.2 (which cannot reference netstandard2.1).
        private static byte[] Pbkdf2Sha256(string password, byte[] salt, int iterations, int outputLength)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            const int hashLength = 32;
            int blockCount = (outputLength + hashLength - 1) / hashLength;
            byte[] output = new byte[outputLength];

            using (HMACSHA256 hmac = new HMACSHA256(passwordBytes))
            {
                for (int block = 1; block <= blockCount; block++)
                {
                    byte[] saltAndBlock = new byte[salt.Length + 4];
                    Buffer.BlockCopy(salt, 0, saltAndBlock, 0, salt.Length);
                    saltAndBlock[salt.Length] = (byte)(block >> 24);
                    saltAndBlock[salt.Length + 1] = (byte)(block >> 16);
                    saltAndBlock[salt.Length + 2] = (byte)(block >> 8);
                    saltAndBlock[salt.Length + 3] = (byte)block;

                    byte[] u = hmac.ComputeHash(saltAndBlock);
                    byte[] accumulator = (byte[])u.Clone();
                    for (int iteration = 1; iteration < iterations; iteration++)
                    {
                        u = hmac.ComputeHash(u);
                        for (int i = 0; i < accumulator.Length; i++)
                            accumulator[i] ^= u[i];
                    }

                    int offset = (block - 1) * hashLength;
                    int bytesToCopy = Math.Min(hashLength, outputLength - offset);
                    Buffer.BlockCopy(accumulator, 0, output, offset, bytesToCopy);
                }
            }
            return output;
        }
    }
}
