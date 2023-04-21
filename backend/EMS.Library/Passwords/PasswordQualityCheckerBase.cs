using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EMS.Library.Passwords
{
    public enum PasswordStrengthIndicator
    {
        Blank = 0,
        VeryWeak = 1,
        Weak = 2,
        Medium = 3,
        Strong = 4,
        VeryStrong = 5
    }

    public abstract class PasswordQualityCheckerBase
    {
        protected PasswordQualityCheckerBase() { }

        public static bool HasMinimumLength(string password, int minLength)
        {
            ArgumentNullException.ThrowIfNull(password);
            if (minLength < 0) throw new ArgumentOutOfRangeException(nameof(minLength));
            return password.Length >= minLength;
        }

        public static bool HasMinimumUniqueChars(string password, int minUniqueChars)
        {
            ArgumentNullException.ThrowIfNull(password);
            if (minUniqueChars < 0) throw new ArgumentOutOfRangeException(nameof(minUniqueChars));
            return password.Distinct().Count() >= minUniqueChars;
        }

        /// <summary>
        /// Returns TRUE if the password has at least one digit
        /// </summary>
        public static bool HasDigit([NotNull] string password)
        {
            ArgumentNullException.ThrowIfNull(password);
            return password.Any(c => char.IsDigit(c));
        }

        /// <summary>
        /// Returns TRUE if the password has at least one special character
        /// </summary>
        public static bool HasNonAlphanumericChar([NotNull] string password)
        {
            ArgumentNullException.ThrowIfNull(password);
            return password.IndexOfAny("!@#$%^&*?_~-£().,".ToCharArray()) != -1;
        }

        /// <summary>
        /// Returns TRUE if the password has at least one uppercase letter
        /// </summary>
        public static bool HasUpperCaseLetter([NotNull] string password)
        {
            ArgumentNullException.ThrowIfNull(password);
            return password.Any(c => char.IsUpper(c));
        }

        /// <summary>
        /// Returns TRUE if the password has at least one lowercase letter
        /// </summary>
        public static bool HasLowerCaseLetter([NotNull] string password)
        {
            ArgumentNullException.ThrowIfNull(password);
            return password.Any(c => char.IsLower(c));
        }
    }
}