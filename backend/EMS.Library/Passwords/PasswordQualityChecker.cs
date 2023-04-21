using System;
using System.Diagnostics.CodeAnalysis;

namespace EMS.Library.Passwords
{
    public class PasswordQualityChecker : PasswordQualityCheckerBase
    {
        protected PasswordQualityChecker() { }

        /// <summary>
        /// Generic method to retrieve password strength: use this for general purpose scenarios, 
        /// i.e. when you don't have a strict policy to follow.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static PasswordStrengthIndicator GetPasswordStrength([NotNull] string password)
        {
            ArgumentNullException.ThrowIfNull(password);
            int score = 0;
            if (String.IsNullOrEmpty(password) || String.IsNullOrWhiteSpace(password)) return PasswordStrengthIndicator.Blank;
            if (HasMinimumLength(password, 11)) score++;
            if (HasMinimumLength(password, 14)) score++;
            if (HasUpperCaseLetter(password) && HasLowerCaseLetter(password)) score++;
            if (HasDigit(password)) score++;
            if (HasNonAlphanumericChar(password)) score++;
            return (PasswordStrengthIndicator)score;
        }

        /// <summary>
        /// Sample password policy implementation following the Microsoft.AspNetCore.Identity.PasswordOptions standard.
        /// </summary>
        public static bool IsValidPassword([NotNull] string password)
        {
            ArgumentNullException.ThrowIfNull(password);
            return IsValidPassword(
                password: password,
                requiredLength: 11,
                requiredUniqueChars: 5,
                requireNonAlphanumeric: true,
                requireLowercase: true,
                requireUppercase: true,
                requireDigit: true
            );
        }

        /// <summary>
        /// Sample password policy implementation following the Microsoft.AspNetCore.Identity.PasswordOptions standard.
        /// </summary>
        protected static bool IsValidPassword(
            [NotNull] string password,
            int requiredLength,
            int requiredUniqueChars,
            bool requireNonAlphanumeric,
            bool requireLowercase,
            bool requireUppercase,
            bool requireDigit)
        {
            ArgumentNullException.ThrowIfNull(password);
            if (String.IsNullOrWhiteSpace(password)) return false;
            if (!HasMinimumLength(password, requiredLength)) return false;
            if (!HasMinimumUniqueChars(password, requiredUniqueChars)) return false;
            if (requireNonAlphanumeric && !HasNonAlphanumericChar(password)) return false;
            if (requireLowercase && !HasLowerCaseLetter(password)) return false;
            if (requireUppercase && !HasUpperCaseLetter(password)) return false;
            if (requireDigit && !HasDigit(password)) return false;
            return true;
        }
    }
}

