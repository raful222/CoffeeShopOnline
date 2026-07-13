using Microsoft.AspNet.Identity;
using System;
using System.Text.RegularExpressions;

namespace CoffeeShopOnline
{
    public class CustomPasswordHasher : IPasswordHasher
    {
        private static readonly Regex LegacyMd5Hash = new Regex("^[a-fA-F0-9]{32}$", RegexOptions.Compiled);
        private readonly PasswordHasher _secureHasher = new PasswordHasher();

        public string HashPassword(string password)
        {
            return _secureHasher.HashPassword(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword) || providedPassword == null)
            {
                return PasswordVerificationResult.Failed;
            }

            // Accounts created by the original application used unsalted MD5. Accept
            // those hashes once and let ASP.NET Identity transparently replace them
            // with its salted PBKDF2 format after a successful sign-in.
            if (LegacyMd5Hash.IsMatch(hashedPassword))
            {
                var legacyHash = Encrypt.GetMd5Hash(providedPassword);
                return string.Equals(hashedPassword, legacyHash, StringComparison.OrdinalIgnoreCase)
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : PasswordVerificationResult.Failed;
            }

            return _secureHasher.VerifyHashedPassword(hashedPassword, providedPassword);
        }
    }
}
