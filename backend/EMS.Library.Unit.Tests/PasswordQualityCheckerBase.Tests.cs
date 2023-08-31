using System;
using FluentAssertions;
using EMS.Library.Passwords;

namespace PasswordQualityCheckerBaseArgumentValidation
{
    public class PasswordQualityCheckerBaseArgumentValidationTests
    {
        [Fact]
        public void HasMinimumLengthThrowsNullException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action a = () => PasswordQualityCheckerBase.HasMinimumLength(null, 1);
#pragma warning restore
            a.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HasMinimumLengthThrowsArgumentOutOfRangeException()
        {
            Action a = () => PasswordQualityCheckerBase.HasMinimumLength(string.Empty, -1);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void HasMinimumUniqueCharsThrowsNullException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action a = () => PasswordQualityCheckerBase.HasMinimumUniqueChars(null, 1);
#pragma warning restore
            a.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HasMinimumUniqueCharsThrowsArgumentOutOfRangeException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action a = () => PasswordQualityCheckerBase.HasMinimumUniqueChars(string.Empty, -1);
#pragma warning restore
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void HasDigitThrowsNullException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action a = () => PasswordQualityCheckerBase.HasDigit(null);
#pragma warning restore
            a.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HasNonAlphanumericCharThrowsNullException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action a = () => PasswordQualityCheckerBase.HasNonAlphanumericChar(null);
#pragma warning restore
            a.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HasUpperCaseLetterCharThrowsNullException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action a = () => PasswordQualityCheckerBase.HasUpperCaseLetter(null);
#pragma warning restore
            a.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HasLowerCaseLetterThrowsNullException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action a = () => PasswordQualityCheckerBase.HasLowerCaseLetter(null);
#pragma warning restore
            a.Should().Throw<ArgumentNullException>();
        }
    }

    public class PasswordQualityCheckerBaseTests
    {
        [Fact]
        public void HasMinimumLengthValidatesProperly()
        {
            PasswordQualityCheckerBase.HasMinimumLength("", 0).Should().Be(true);
            PasswordQualityCheckerBase.HasMinimumLength("a", 1).Should().Be(true);
            PasswordQualityCheckerBase.HasMinimumLength("", 1).Should().Be(false);
            PasswordQualityCheckerBase.HasMinimumLength("ab", 1).Should().Be(true);
            PasswordQualityCheckerBase.HasMinimumLength("1234567890", 10).Should().Be(true);
            PasswordQualityCheckerBase.HasMinimumLength("123456789", 10).Should().Be(false);
            PasswordQualityCheckerBase.HasMinimumLength("12345678901", 10).Should().Be(true);
        }

        [Fact]
        public void HasMinimumUniqueCharsValidatesProperly()
        {
            PasswordQualityCheckerBase.HasMinimumUniqueChars("", 0).Should().Be(true);
            PasswordQualityCheckerBase.HasMinimumUniqueChars("a", 1).Should().Be(true);
            PasswordQualityCheckerBase.HasMinimumUniqueChars("", 1).Should().Be(false);
            PasswordQualityCheckerBase.HasMinimumUniqueChars("ab", 1).Should().Be(true);
            PasswordQualityCheckerBase.HasMinimumUniqueChars("1234567890", 10).Should().Be(true);
            PasswordQualityCheckerBase.HasMinimumUniqueChars("123456789", 10).Should().Be(false);
            PasswordQualityCheckerBase.HasMinimumUniqueChars("12345678901", 10).Should().Be(true);
            PasswordQualityCheckerBase.HasMinimumUniqueChars("!@#$%^&*?_~-£().,", 10).Should().Be(true);
        }

        [Fact]
        public void HasDigitValidatesProperly()
        {
            PasswordQualityCheckerBase.HasDigit("0").Should().Be(true);
            PasswordQualityCheckerBase.HasDigit("1").Should().Be(true);
            PasswordQualityCheckerBase.HasDigit("9").Should().Be(true);

            PasswordQualityCheckerBase.HasDigit("1234567890").Should().Be(true);
            PasswordQualityCheckerBase.HasDigit("!@#$%^&*?_~-£().,1").Should().Be(true);
            PasswordQualityCheckerBase.HasDigit("1!@#$%^&*?_~-£().,").Should().Be(true);

            PasswordQualityCheckerBase.HasDigit("").Should().Be(false);
            PasswordQualityCheckerBase.HasDigit("a").Should().Be(false);
            PasswordQualityCheckerBase.HasDigit("ab").Should().Be(false);

        }

        [Fact]
        public void HasNonAlphanumericCharValidatesProperly()
        {
            PasswordQualityCheckerBase.HasNonAlphanumericChar("!@#$%^&*?_~-£().,").Should().Be(true);
            PasswordQualityCheckerBase.HasNonAlphanumericChar("1!@#$%^&*?_~-£().,").Should().Be(true);
            PasswordQualityCheckerBase.HasNonAlphanumericChar("!@#$%^&*?_~-£().,1").Should().Be(true);
            PasswordQualityCheckerBase.HasNonAlphanumericChar("").Should().Be(false);
            PasswordQualityCheckerBase.HasNonAlphanumericChar("A").Should().Be(false);
            PasswordQualityCheckerBase.HasNonAlphanumericChar("0").Should().Be(false);
        }

        [Fact]
        public void HasUpperCaseLetterValidatesProperly()
        {
            PasswordQualityCheckerBase.HasUpperCaseLetter("A").Should().Be(true);
            PasswordQualityCheckerBase.HasUpperCaseLetter("Ab").Should().Be(true);
            PasswordQualityCheckerBase.HasUpperCaseLetter("aB").Should().Be(true);
            PasswordQualityCheckerBase.HasUpperCaseLetter("aBc").Should().Be(true);
            PasswordQualityCheckerBase.HasUpperCaseLetter("a1B1c").Should().Be(true);
            PasswordQualityCheckerBase.HasUpperCaseLetter("A!@#$%^&*?_~-£().,").Should().Be(true);
            PasswordQualityCheckerBase.HasUpperCaseLetter("!@#$%^&*?_~-£().,A").Should().Be(true);

            PasswordQualityCheckerBase.HasUpperCaseLetter("!@#$%^&*?_~-£().,").Should().Be(false);
            PasswordQualityCheckerBase.HasUpperCaseLetter("a!@#$%^&*?_~-£().,").Should().Be(false);
            PasswordQualityCheckerBase.HasUpperCaseLetter("!@#$%^&*?_~-£().,a").Should().Be(false);
            PasswordQualityCheckerBase.HasUpperCaseLetter("").Should().Be(false);
            PasswordQualityCheckerBase.HasUpperCaseLetter("0").Should().Be(false);
        }

        [Fact]
        public void HasLowerCaseLetterValidatesProperly()
        {
            PasswordQualityCheckerBase.HasLowerCaseLetter("a").Should().Be(true);
            PasswordQualityCheckerBase.HasLowerCaseLetter("aB").Should().Be(true);
            PasswordQualityCheckerBase.HasLowerCaseLetter("Ab").Should().Be(true);
            PasswordQualityCheckerBase.HasLowerCaseLetter("AbC").Should().Be(true);
            PasswordQualityCheckerBase.HasLowerCaseLetter("A1b1C").Should().Be(true);
            PasswordQualityCheckerBase.HasLowerCaseLetter("a!@#$%^&*?_~-£().,").Should().Be(true);
            PasswordQualityCheckerBase.HasLowerCaseLetter("!@#$%^&*?_~-£().,a").Should().Be(true);

            PasswordQualityCheckerBase.HasLowerCaseLetter("!@#$%^&*?_~-£().,").Should().Be(false);
            PasswordQualityCheckerBase.HasLowerCaseLetter("A!@#$%^&*?_~-£().,").Should().Be(false);
            PasswordQualityCheckerBase.HasLowerCaseLetter("!@#$%^&*?_~-£().,A").Should().Be(false);
            PasswordQualityCheckerBase.HasLowerCaseLetter("").Should().Be(false);
            PasswordQualityCheckerBase.HasLowerCaseLetter("0").Should().Be(false);
        }
    }
}
