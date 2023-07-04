using System;
using FluentAssertions;
using EMS.Library.Passwords;

namespace PasswordQualityCheckerUnitTest
{
    public class PasswordQualityCheckerTests
    {
        [Theory]
        [InlineData("", PasswordStrengthIndicator.Blank)]
        [InlineData(" ", PasswordStrengthIndicator.Blank)]
        [InlineData("\t", PasswordStrengthIndicator.Blank)]
        [InlineData("a", PasswordStrengthIndicator.Blank)]
        [InlineData("aB", PasswordStrengthIndicator.VeryWeak)]
        [InlineData("aB1", PasswordStrengthIndicator.Weak)]
        [InlineData("aB1!", PasswordStrengthIndicator.Medium)]
        [InlineData("aB1!1234901", PasswordStrengthIndicator.Strong)]
        [InlineData("aB1!5678901234", PasswordStrengthIndicator.VeryStrong)]
        public void ValidatesBasicStrength(string password, PasswordStrengthIndicator expectedStrength)
        {
            PasswordQualityChecker.GetPasswordStrength(password).Should().Be(expectedStrength);
        }


        [Theory]
        [InlineData("PjY{ezb)*pl5")]
        [InlineData("SterlingGmail20.15")]
        public void PassesExampleStrongandVeryPasswords(string password)
        {
            PasswordQualityChecker.GetPasswordStrength(password).Should().BeOneOf(PasswordStrengthIndicator.Strong, PasswordStrengthIndicator.VeryStrong);
        }

        [Theory]
        [InlineData("12345678901!aQ0", true)]
        [InlineData("", false)]
        [InlineData("fietsen!", false)]
        [InlineData("00000000000", false)]
        [InlineData("12345678901!AQQ", false)]
        [InlineData("12345678901!aqq", false)]
        [InlineData("ABCDEFGHIJK!aqq", false)]
        public void Isvalid(string password, bool expected)
        {
            PasswordQualityChecker.IsValidPassword(password).Should().Be(expected);
        }
    }
}
