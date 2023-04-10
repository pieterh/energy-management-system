using System;
namespace EMS.Library.Files
{
	public static class FileTools
	{
		public static bool FileExistsAndReadable(string? path)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(path)) return false;
				using var f = File.OpenRead(path);
				return true;
			}
			catch (FileNotFoundException) { /* the file was not found */ }
			catch (FieldAccessException) { /* no access to the file */ }
			return false;
		}
	}
}

