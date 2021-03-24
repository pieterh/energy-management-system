using System;

namespace P1SmartMeter
{
    /**
     * CRC-16-IBM with a reverse polynomial representation (polynomial 0xA001)
     */
    public class CRC16
    {
        private const ushort polynomial = 0xA001;
        private readonly ushort[] _lookupTable = new ushort[256];

        public CRC16()
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

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        public ushort ComputeChecksum(byte[] bytes)
        {
            return ComputeChecksum(bytes, bytes.Length);
        }

        public ushort ComputeChecksum(byte[] bytes, int length)
        {
            ushort crc16 = 0;
            for (int i = 0; i < length; ++i)
            {
                byte index = (byte)(crc16 ^ bytes[i]);
                crc16 = (ushort)((crc16 >> 8) ^ _lookupTable[index]);
            }
            return crc16;
        }

        public string ComputeChecksumAsString(byte[] bytes, int length)
        {
            var computedchecksum = ComputeChecksum(bytes, length);
            string hexValue = string.Format("{0:X4}", computedchecksum);
            return hexValue;
        }

        public bool ValidateChecksum(byte[] bytes, string checksum)
        {
            return ValidateChecksum(bytes, bytes.Length, checksum);
        }

        public bool ValidateChecksum(byte[] bytes, ushort checksum)
        {
            return ValidateChecksum(bytes, bytes.Length, checksum);
        }

        public bool ValidateChecksum(byte[] bytes, int length, string checksum)
        {
            string hexValue = ComputeChecksumAsString(bytes, length);
            return hexValue.Equals(checksum, StringComparison.OrdinalIgnoreCase);
        }

        public bool ValidateChecksum(byte[] bytes, int length, ushort checksum)
        {
            var computedchecksum = ComputeChecksum(bytes, length);
            return computedchecksum == checksum;
        }
    }
}
