using System;
using System.Diagnostics.CodeAnalysis;

namespace P1SmartMeter
{
    /** 
     * CRC-16 / CRC-16-IBM / CRC-16-ANSI / Modbus / USB with a reverse polynomial representation (polynomial 0xA001)
     * https://en.wikipedia.org/wiki/Cyclic_redundancy_check
     */
    [SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
    public sealed class CRC16
    {
        public const ushort Polynomial = 0xA001;              // 0x8005 with the bits reversed
        public const ushort InitialValue = 0x0000;
        public const ushort FinalXOR = 0x0000;

        private static readonly ushort[] _lookupTable = InitializeLookupTable();

        /// <summary>
        /// Used to create the lookup table on the fly. This decreases the size of the assembly a bit 
        /// </summary>
        /// <returns>the lookup table</returns>
        private static ushort[] InitializeLookupTable()
        {
            ushort[] lookupTable = new ushort[256];
            for (ushort i = 0; i < lookupTable.Length; ++i)
            {
                ushort value = 0;
                ushort temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ Polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                lookupTable[i] = value;
            }
            return lookupTable;
        }

        private static ushort ComputeChecksum(ushort crc16, byte[] bytes, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                byte index = (byte)(crc16 ^ bytes[i]);
                crc16 = (ushort)((crc16 >> 8) ^ _lookupTable[index]);
            }
            return (ushort)(crc16 ^ FinalXOR);
        }

        public static ushort ComputeChecksum(byte[] bytes, int length)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), length, "Should not be a negative value");

            return ComputeChecksum(InitialValue, bytes, length);
        }

        public static ushort ComputeChecksum(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            return ComputeChecksum(InitialValue, bytes, bytes.Length);
        }

        public static byte[] ComputeChecksumBytes(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ushort crc = ComputeChecksum(bytes, bytes.Length);
            return BitConverter.GetBytes(crc);
        }

        public static string ComputeChecksumAsString(byte[] bytes, int length)
        {
            var computedchecksum = ComputeChecksum(bytes, length);
            string hexValue = string.Format("{0:X4}", computedchecksum);
            return hexValue;
        }

        public static bool ValidateChecksum(byte[] bytes, int length, ushort checksum)
        {
            var computedchecksum = ComputeChecksum(bytes, length);
            return computedchecksum == checksum;
        }

        public static bool ValidateChecksum(byte[] bytes, int length, string checksum)
        {
            string hexValue = ComputeChecksumAsString(bytes, length);
            return hexValue.Equals(checksum, StringComparison.OrdinalIgnoreCase);
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

        /*
         * With Restart / Continue, it is possible to continue calculating the crc16 with additional data.
         * This is needed in the case that you would not like to have all data in memory for wich the crc16 needs to be calculated
         */
        public ushort CurrentValue { get; private set; }
        public void Restart() { CurrentValue = InitialValue; }
        public ushort Continue(byte[] bytes, int length)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), length, "Should not be a negative value");
            CurrentValue = ComputeChecksum(CurrentValue, bytes, length);
            return CurrentValue;
        }

        public ushort Continue(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            return Continue(bytes, bytes.Length);
        }
    }
}
