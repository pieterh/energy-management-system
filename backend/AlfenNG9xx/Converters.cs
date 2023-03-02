using System;
using System.Text;

namespace AlfenNG9xx
{
    public static class Converters
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        static Converters()
        {
            Logger.Info($"Is little endian: {BitConverter.IsLittleEndian}");
        }

        /// <summary>
        /// Converts 16 - Bit Register values to String
        /// </summary>
        /// <param name="registers">Register array received via Modbus</param>
        /// <param name="offset">First Register containing the String to convert</param>
        /// <param name="stringLength">number of characters in String (must be even)</param>
        /// <returns>Converted String</returns>
        public static string ConvertRegistersToString(ushort[] registers)
        {
            return ConvertRegistersToString(registers, 0, registers.Length);
        }

        /// <summary>
        /// Converts 16 - Bit Register values to String
        /// </summary>
        /// <param name="registers">Register array received via Modbus</param>
        /// <param name="offset">First Register containing the String to convert</param>
        /// <param name="stringLength">number of characters in String (must be even)</param>
        /// <returns>Converted String</returns>
        public static string ConvertRegistersToString(ushort[] registers, int offset, int nrOfRegisters)
        {
            byte[] result = new byte[nrOfRegisters * 2];

            for (int i = 0; i < nrOfRegisters; i++)
            {
                byte[] registerResult = BitConverter.GetBytes(registers[offset + i]);
                result[i * 2] = registerResult[1];
                result[i * 2 + 1] = registerResult[0];
            }

            // Do not use GetString to convert. 
            // Due to trailing 0 character the string length will be incorrect
            var resultStr = new StringBuilder();
            foreach (var b in result)
            {
                if (b == 0) break;
                resultStr.Append((char)b);
            }
            return resultStr.ToString();
        }

        public static UInt16 ConvertRegistersShort(ushort[] registers)
        {
            return ConvertRegistersShort(registers, 0);
        }

        public static UInt16 ConvertRegistersShort(ushort[] registers, int offset)
        {            
            if  (registers.Length - offset <= 0) throw new ArgumentOutOfRangeException(nameof(registers));

            byte[] registerBytes = BitConverter.GetBytes(registers[offset]);
            byte[] bytes = {
                                registerBytes[0],
                                registerBytes[1]
                            };
            return BitConverter.ToUInt16(bytes, 0);
        }

        public static UInt32 ConvertRegistersUInt32(ushort[] registers)
        {
            return ConvertRegistersUInt32(registers, 0);
        }

        public static UInt32 ConvertRegistersUInt32(ushort[] registers, int offset)
        {
            if (registers.Length - offset - 1 <= 0) throw new ArgumentOutOfRangeException(nameof(registers));

            byte[] bytes = ConvertBigToLittleEndian32bit(registers[0 + offset], registers[1 + offset]);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static UInt64 ConvertRegistersLong(ushort[] registers)
        {
            return ConvertRegistersLong(registers, 0);
        }

        public static UInt64 ConvertRegistersLong(ushort[] registers, int offset)
        {
            if (registers.Length - offset - 2 <= 0) throw new ArgumentOutOfRangeException(nameof(registers));

            byte[] bytes = ConvertBigToLittleEndian64bit(registers[0 + offset], registers[1 + offset], registers[2 + offset], registers[3 + offset]);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static float ConvertRegistersFloat(ushort[] registers)
        {
            return ConvertRegistersFloat(registers, 0);
        }

        public static float ConvertRegistersFloat(ushort[] registers, int offset)
        {
            if (registers.Length - offset - 1 <= 0) throw new ArgumentOutOfRangeException(nameof(registers));

            byte[] bytes = ConvertBigToLittleEndian32bit(registers[0 + offset], registers[1 + offset]);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static ushort[] ConvertFloatToRegisters(float f)
        {
            float[] floats = new float[] {f};
            byte[] bytes = new byte[4];
            Buffer.BlockCopy(floats, 0, bytes, 0, bytes.Length);
            ushort[] result = ConvertLittleToBigEndian32bit(bytes);
            return result;
        }

        public static double ConvertRegistersDouble(ushort[] registers)
        {
            return ConvertRegistersDouble(registers, 0);
        }

        public static double ConvertRegistersDouble(ushort[] registers, int offset)
        {
            if (registers.Length - offset - 2 <= 0) throw new ArgumentOutOfRangeException(nameof(registers));
            byte[] bytes = ConvertBigToLittleEndian64bit(registers[0 + offset], registers[1 + offset], registers[2 + offset], registers[3 + offset]);
            return BitConverter.ToDouble(bytes, 0);
        }

        private static byte[] ConvertBigToLittleEndian32bit(int abRegister, int cdRegister)
        {
            byte[] abBytes = BitConverter.GetBytes(abRegister);
            byte[] cdBytes = BitConverter.GetBytes(cdRegister);
            byte[] bytes = {
                                    cdBytes[0],     // C
                                    cdBytes[1],     // D
                                    abBytes[0],     // A
                                    abBytes[1]      // B
                                };
            return bytes;
        }
        private static ushort[] ConvertLittleToBigEndian32bit(byte[] bytes)
        {
            var result = new ushort[2];
            byte[] abBytes = new byte[] {bytes[2], bytes[3]};
            byte[] cdBytes = new byte[] {bytes[0], bytes[1]};
            Buffer.BlockCopy(abBytes, 0, result, 0, abBytes.Length);
            Buffer.BlockCopy(cdBytes, 0, result, 2, cdBytes.Length);
            return result;
        }
        private static byte[] ConvertBigToLittleEndian64bit(int abRegister, int cdRegister, int efRegister, int ghRegister)
        {
            byte[] abBytes = BitConverter.GetBytes(abRegister);
            byte[] cdBytes = BitConverter.GetBytes(cdRegister);
            byte[] efBytes = BitConverter.GetBytes(efRegister);
            byte[] ghBytes = BitConverter.GetBytes(ghRegister);
            byte[] bytes = {
                                    ghBytes[0],                     // G
                                    ghBytes[1],                     // H
                                    efBytes[0],                     // E
                                    efBytes[1],                     // F
                                    cdBytes[0],                     // C
                                    cdBytes[1],                     // D
                                    abBytes[0],                     // A
                                    abBytes[1]                      // B
                                };
            return bytes;
        }
    }
}
