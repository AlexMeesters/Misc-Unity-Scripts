
// Created by Alex Meesters
// www.low-scope.com, www.alexmeesters.nl
// Licenced under the MIT Licence. https://en.wikipedia.org/wiki/MIT_License

using System;
using System.IO;
using System.Text;

public static class BackupABExample
{
	private const string BackupExtension = ".b";

	public static string SaveBackupPath(string path)
	{
		string altPath = GetAlternativeFilePath(path, BackupExtension);
		return GetOldestFilePath(path, altPath);
	}

	public static string LoadBackupPath(string path)
	{
		string altPath = GetAlternativeFilePath(path, BackupExtension);
		return GetNewestFilePath(path, altPath);
	}

	private static string GetOldestFilePath(string pathOne, string pathTwo)
	{
		bool pathOneExists = File.Exists(pathOne);
		bool pathTwoExists = File.Exists(pathTwo);

		if (!pathOneExists)
			return pathOne;

		if (!pathTwoExists)
			return pathTwo;

		DateTime pathOneWriteTime = File.GetLastWriteTime(pathOne);
		DateTime pathTwoWriteTime = File.GetLastWriteTime(pathTwo);

		return pathOneWriteTime < pathTwoWriteTime ? pathOne : pathTwo;
	}

	private static string GetNewestFilePath(string pathOne, string pathTwo)
	{
		bool pathOneExists = File.Exists(pathOne);
		bool pathTwoExists = File.Exists(pathTwo);

		if (!pathOneExists && !pathTwoExists)
			return "";

		if (pathOneExists && !pathTwoExists)
			return pathOne;

		if (!pathOneExists)
			return pathTwo;

		DateTime pathOneWriteTime = File.GetLastWriteTime(pathOne);
		DateTime pathTwoWriteTime = File.GetLastWriteTime(pathTwo);

		return pathOneWriteTime > pathTwoWriteTime ? pathOne : pathTwo;
	}

	private static string GetAlternativeFilePath(string path, string extensionName)
	{
		return new StringBuilder()
			.Append(path)
			.Append(extensionName)
			.ToString();
	}
}
