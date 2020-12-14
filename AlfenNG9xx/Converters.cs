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

            for (int i = 0; i < nrOfRegisters; i++)
            {
                byte[] registerResult = BitConverter.GetBytes(registers[offset + i]);
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
        public static UInt32 ConvertRegistersUInt32(ushort[] registers, int offset)
        {
             int lowRegister = registers[0 + offset];   // AB
            int highRegister = registers[1 + offset];  // CD

            byte[] abBytes = BitConverter.GetBytes(lowRegister);
            byte[] cdBytes = BitConverter.GetBytes(highRegister);
            byte[] bytes = {
                                    cdBytes[0],     // C
                                    cdBytes[1],     // D
                                    abBytes[0],     // A
                                    abBytes[1]      // B
                                };
            return BitConverter.ToUInt32(bytes, 0);
        }
        public static UInt64 ConvertRegistersLong(ushort[] registers, int offset)
        {
            int abRegister = registers[0 + offset];                // AB
            int cdRegister = registers[1 + offset];                // CD
            int efRegister = registers[2 + offset];                // EF
            int ghRegister = registers[3 + offset];                // GH

            byte[] abBytes = BitConverter.GetBytes(abRegister);
            byte[] cdBytes = BitConverter.GetBytes(cdRegister);
            byte[] efBytes = BitConverter.GetBytes(efRegister);
            byte[] ghBytes = BitConverter.GetBytes(ghRegister);
            byte[] bytes = {
                                    ghBytes[0],
                                    ghBytes[1],
                                    efBytes[0],
                                    efBytes[1],
                                    cdBytes[0],
                                    cdBytes[1],
                                    abBytes[0],
                                    abBytes[1]
                                };
            return BitConverter.ToUInt64(bytes, 0);
        }
        public static float ConvertRegistersFloat(ushort[] registers, int offset)
        {
            int lowRegister = registers[0 + offset];   // AB
            int highRegister = registers[1 + offset];  // CD

            byte[] abBytes = BitConverter.GetBytes(lowRegister);
            byte[] cdBytes = BitConverter.GetBytes(highRegister);
            byte[] bytes = {
                                    cdBytes[0],     // C
                                    cdBytes[1],     // D
                                    abBytes[0],     // A
                                    abBytes[1]      // B
                                };
            return BitConverter.ToSingle(bytes, 0);
        }
        public static double ConvertRegistersDouble(ushort[] registers, int offset)
        {
            int abRegister = registers[0 + offset];                // AB
            int cdRegister = registers[1 + offset];                // CD
            int efRegister = registers[2 + offset];                // EF
            int ghRegister = registers[3 + offset];                // GH

            byte[] abBytes = BitConverter.GetBytes(abRegister);
            byte[] cdBytes = BitConverter.GetBytes(cdRegister);
            byte[] efBytes = BitConverter.GetBytes(efRegister);
            byte[] ghBytes = BitConverter.GetBytes(ghRegister);
            byte[] bytes = {
                                    ghBytes[0],
                                    ghBytes[1],
                                    efBytes[0],
                                    efBytes[1],
                                    cdBytes[0],
                                    cdBytes[1],
                                    abBytes[0],
                                    abBytes[1]
                                };
            return BitConverter.ToDouble(bytes, 0);
        }        
    }
}
