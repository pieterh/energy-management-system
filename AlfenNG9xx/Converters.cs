using System;

namespace AlfenNG9xx
{
    public static class Converters
    {
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
            byte[] registerResult = new byte[2];

            for (int i = 0; i < nrOfRegisters; i++)
            {
                registerResult = BitConverter.GetBytes(registers[offset + i]);
                result[i * 2] = registerResult[1];
                result[i * 2 + 1] = registerResult[0];
            }
            return System.Text.Encoding.Default.GetString(result);
        }

        public static UInt16 ConvertRegistersShort(ushort[] registers)
        {
            return ConvertRegistersShort(registers, 0);
        }
        public static UInt16 ConvertRegistersShort(ushort[] registers, int offset)
        {
            byte[] registerBytes = BitConverter.GetBytes(registers[offset]);
            byte[] bytes = {
                                registerBytes[0],
                                registerBytes[1]
                            };
            return BitConverter.ToUInt16(bytes, 0);
        }

        public static UInt64 ConvertRegistersLong(ushort[] registers, int offset)
        {
            int highRegister = registers[0 + offset];
            int highLowRegister = registers[1 + offset];
            int lowHighRegister = registers[2 + offset];
            int lowRegister = registers[3 + offset];

            byte[] highRegisterBytes = BitConverter.GetBytes(highRegister);
            byte[] highLowRegisterBytes = BitConverter.GetBytes(highLowRegister);
            byte[] lowHighRegisterBytes = BitConverter.GetBytes(lowHighRegister);
            byte[] lowRegisterBytes = BitConverter.GetBytes(lowRegister);
            byte[] longBytes = {
                                    lowRegisterBytes[0],
                                    lowRegisterBytes[1],
                                    lowHighRegisterBytes[0],
                                    lowHighRegisterBytes[1],
                                    highLowRegisterBytes[0],
                                    highLowRegisterBytes[1],
                                    highRegisterBytes[0],
                                    highRegisterBytes[1]
                                };
            return BitConverter.ToUInt64(longBytes, 0);
        }
    }
}
