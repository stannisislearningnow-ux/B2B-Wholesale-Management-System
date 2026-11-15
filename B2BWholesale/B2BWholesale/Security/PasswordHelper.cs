using B2BWholesale.Core.Security;

namespace B2BWholesale.Security
{
    // Kept as a compatibility wrapper for the existing WinForms forms.
    public static class PasswordHelper
    {
        public static string HashPassword(string password) => PasswordHasher.HashPassword(password);
        public static bool VerifyPassword(string password, string storedHash) =>
            PasswordHasher.VerifyPassword(password, storedHash);
    }
}
