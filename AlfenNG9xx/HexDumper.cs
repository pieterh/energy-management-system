using System;
using System.Linq;
using System.Text;
namespace AlfenNG9xx
{
public static class HexDumper
	{
        public static string ConvertToHexDump(ushort[] shorts)
        {
            var bytes = new byte[(shorts.Length * 2)];
            Buffer.BlockCopy(shorts, 0, bytes, 0, bytes.Length);
            return ConvertToHexDump(bytes);
        }
		public static string ConvertToHexDump(long l)
        {
			long[] larray = {l};
            var bytes = new byte[sizeof(long)];
            Buffer.BlockCopy(larray, 0, bytes, 0, bytes.Length);
            return ConvertToHexDump(bytes);
        }
		public static string ConvertToHexDump(double d)
        {
			double[] darray = {d};
            var bytes = new byte[sizeof(double)];
            Buffer.BlockCopy(darray, 0, bytes, 0, bytes.Length);
            return ConvertToHexDump(bytes);
        }		
		public static string ConvertToHexDump(UInt64 d)
        {
			UInt64[] darray = {d};
            var bytes = new byte[sizeof(double)];
            Buffer.BlockCopy(darray, 0, bytes, 0, bytes.Length);
            return ConvertToHexDump(bytes);
        }			
		public static string ConvertToHexDump(float l)
        {
			float[] larray = {l};
            var bytes = new byte[sizeof(float)];
            Buffer.BlockCopy(larray, 0, bytes, 0, bytes.Length);
            return ConvertToHexDump(bytes);
        }
		public static string ConvertToHexDump(byte[] bytes)
		{
			string result = string.Empty;
			for(int i = 0; i < bytes.Length; i += 16)
			{
				if (i > 0) result += '\n';
				int padding = bytes.Length.Log16().ExtremeValuesToZero() - i.Log16().ExtremeValuesToZero() + 1;
				byte[] bytesChunk = bytes.Skip(i).Take(16).ToArray();
				result += $"{i:X}".PadLeft(padding, '0').PadRight(5, ' ');
				result += ConvertBytesToHex(bytesChunk);
				result += string.Empty.PadRight((17 - bytesChunk.Length) * 3, ' ');
				result += ConvertBytesToString(bytesChunk);
			}

			return result;
		}

		private static string ConvertBytesToHex(byte[] bytes)
		{
			string hexDump = BitConverter.ToString(bytes);
			hexDump = hexDump.Replace("-", " ");

			return hexDump.ToLower();
		}

		private static string ConvertBytesToString(byte[] bytes)
		{
			string result = string.Empty;
			foreach(var hex in bytes) {
				char mark = Encoding.ASCII.GetChars(new byte[] { hex })[0];
				if (char.IsControl(mark) || char.IsSurrogate(mark) || mark.Equals('?'))
					result += '.';
				else
					result += mark;
			}

			return result;
		}

        internal static int Log16(this int input)
		{
			return (int)System.Math.Floor(System.Math.Log(input, 16));
		}

		internal static int ExtremeValuesToZero(this int input)
		{
			if (input == int.MaxValue || input == int.MinValue)
				return 0;

			return input;
		}
	}
}