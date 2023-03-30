using System;
using System.Diagnostics.CodeAnalysis;

namespace P1SmartMeter
{
    /** 
     * CRC-16-IBM / CRC-16-ANSI with a reverse polynomial representation (polynomial 0xA001)
     * https://en.wikipedia.org/wiki/Cyclic_redundancy_check
     */
    [SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
    public sealed class CRC16
    {
        private const ushort polynomial = 0xA001;
        private static readonly ushort[] _lookupTable = new ushort[256];

        static CRC16()
        {
            for (ushort i = 0; i < _lookupTable.Length; ++i)
            {
                ushort value = 0;
                ushort temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                _lookupTable[i] = value;
            }
        }

        public static byte[] ComputeChecksumBytes(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        public static ushort ComputeChecksum(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            return ComputeChecksum(bytes, bytes.Length);
        }

        public static ushort ComputeChecksum(byte[] bytes, int length)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), length, "Should not be a negative value");

            ushort crc16 = 0;
            for (int i = 0; i < length; ++i)
            {
                byte index = (byte)(crc16 ^ bytes[i]);
                crc16 = (ushort)((crc16 >> 8) ^ _lookupTable[index]);
            }
            return crc16;
        }

        public static string ComputeChecksumAsString(byte[] bytes, int length)
        {
            var computedchecksum = ComputeChecksum(bytes, length);
            string hexValue = string.Format("{0:X4}", computedchecksum);
            return hexValue;
        }

        public static bool ValidateChecksum(byte[] bytes, string checksum)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ArgumentNullException.ThrowIfNullOrEmpty(checksum);
            return ValidateChecksum(bytes, bytes.Length, checksum);
        }

        public static bool ValidateChecksum(byte[] bytes, ushort checksum)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            return ValidateChecksum(bytes, bytes.Length, checksum);
        }

        public static bool ValidateChecksum(byte[] bytes, int length, string checksum)
        {
            string hexValue = ComputeChecksumAsString(bytes, length);
            return hexValue.Equals(checksum, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ValidateChecksum(byte[] bytes, int length, ushort checksum)
        {
            var computedchecksum = ComputeChecksum(bytes, length);
            return computedchecksum == checksum;
        }
    }
}
