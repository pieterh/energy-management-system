using System.Security.Cryptography;
using EMS.Library.TestableDateTime;

namespace Enphase
{
    internal static class Authentication
    {
        private const string defaultRealm = "enphaseenergy.com";

        [SuppressMessage("", "CA5351", Justification = "Unable to change due compatibility with existing hardware")]
        [SuppressMessage("", "S4790", Justification = "Unable to change due compatibility with existing hardware")]
        private static readonly MD5 hashlib = MD5.Create();

        internal static string GetPublicPasswd(string serialNumber, string userName, string realm = "", DateTimeOffset expiryTimestamp = default)
        {
            if (expiryTimestamp == default)
            {
                expiryTimestamp = DateTimeOffsetProvider.Now;
            }
            var s = "[e]" + userName + '@' + realm + '#' + serialNumber + expiryTimestamp.ToUnixTimeSeconds();
            var bytes = Encoding.UTF8.GetBytes(s);
            var hash = hashlib.ComputeHash(bytes);
            string hexDump = BitConverter.ToString(hash).Replace("-", null, StringComparison.OrdinalIgnoreCase);
#pragma warning disable CA1308
            return hexDump.ToLowerInvariant();
#pragma warning restore
        }

        internal static string GetPasswdForSerial(string serialNumber, string userName, string realm = defaultRealm)
        {
            if (string.IsNullOrWhiteSpace(serialNumber) || string.IsNullOrWhiteSpace(userName))
                return string.Empty;
            if (string.IsNullOrWhiteSpace(realm))
                realm = defaultRealm;
            var s = "[e]" + userName + '@' + realm + '#' + serialNumber + " EnPhAsE eNeRgY ";
            var bytes = Encoding.UTF8.GetBytes(s);
            var hash = hashlib.ComputeHash(bytes);
            string hexDump = BitConverter.ToString(hash).Replace("-", null, StringComparison.OrdinalIgnoreCase);
#pragma warning disable CA1308
            return hexDump.ToLowerInvariant();
#pragma warning restore
        }

        internal static string GetMobilePasswdForSerial(string serialNumber, string userName, string realm = "")
        {
            var digest = GetPasswdForSerial(serialNumber, userName, realm);
            if (string.IsNullOrWhiteSpace(digest) || digest.Length > 32) return string.Empty;

            int countZero = digest.Count(b => b == '0');
            int countOne = digest.Count(b => b == '1');

            Span<char> reversedDigestAsSpan = stackalloc char[32];
            digest.AsSpan().CopyTo(reversedDigestAsSpan);
            reversedDigestAsSpan.Reverse();

            Span<char> password = stackalloc char[8];

            for (int idx = 0; idx < 8; idx++)
            {
                char cc = reversedDigestAsSpan[idx];

                AdjustCounts(ref countZero, '0');
                AdjustCounts(ref countOne, '1');

                if (cc == '0')
                {
                    password[idx] = (char)('f' + countZero);
                    countZero--;
                }
                else if (cc == '1')
                {
                    password[idx] = (char)('@' + countOne);
                    countOne--;
                }
                else
                {
                    password[idx] = cc;
                }
            }

            return password.ToString();
        }

        private static void AdjustCounts(ref int count, char character)
        {
            if (character == '0' && (count == 3 || count == 6 || count == 9) || character == '1' && (count == 9 || count == 15))
            {
                count--;
            }

            int max = character == '0' ? 20 : 26;
            if (count > max)
            {
                count = max;
            }
            else if (count < 0)
            {
                count = 0;
            }
        }
    }
}