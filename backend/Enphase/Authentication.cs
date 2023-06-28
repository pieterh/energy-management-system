using System.Security.Cryptography;
using EMS.Library.TestableDateTime;

namespace Enphase
{
    internal static class Authentication
    {
        private const string defaultRealm = "enphaseenergy.com";

        [SuppressMessage("", "CA5351", Justification = "Unable to change due compatibility with existing hardware")]
        private static MD5 hashlib = MD5.Create();

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

            var countZero = digest.Count((b) => b.Equals('0'));
            var countOne = digest.Count((b) => b.Equals('1'));
          
            Span<char> reversedDigestAsSpan = stackalloc char[32];

            digest.AsSpan().CopyTo(reversedDigestAsSpan);
            reversedDigestAsSpan.Reverse();          

            Span<char> password = stackalloc char[8];
            int idx = 0;

            foreach (char cc in reversedDigestAsSpan.Slice(0, 8))
            {
                if (countZero == 3 || countZero == 6 || countZero == 9)
                    countZero = countZero - 1;
                if (countZero > 20)
                    countZero = 20;
                if (countZero < 0)
                    countZero = 0;

                if (countOne == 9 || countOne == 15)
                    countOne = countOne - 1;
                if (countOne > 26)
                    countOne = 26;
                if (countOne < 0)
                    countOne = 0;
                if (cc == '0') {
                    password[idx] = Convert.ToChar((byte)'f' + countZero);
                    idx++;
                    countZero = countZero - 1;
                } else
                    if (cc == '1') {
                    password[idx] = Convert.ToChar((byte)'@' + countOne);
                    idx++;
                    countOne = countOne - 1;
                }
                else {
                    password[idx] = cc;
                    idx++;
                }
            }
            var passwordStr = password.ToString();
            return passwordStr;
        }
    }
}
