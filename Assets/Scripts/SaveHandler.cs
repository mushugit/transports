using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class SaveHandler
{
	public static readonly string Extention = ".json";
	public static readonly string Folder = "saves";

	public static readonly string MinCompatibleVersion = "0.0.4";

	private static string savePath = Application.persistentDataPath + Path.DirectorySeparatorChar + Folder + Path.DirectorySeparatorChar;

	static SaveHandler()
	{
		Directory.CreateDirectory(savePath);
	}

	private static JsonSerializer GetSerializer()
	{
		var serializer = new JsonSerializer
		{
			TypeNameHandling = TypeNameHandling.All
		};

		return serializer;
	}

	public static void Save()
	{
		var now = DateTime.Now;
		var stringDate = now.ToString("yyyyMMdd_HHmmss");

		var saveName = $"save_{stringDate}";
		var fullFilePath = savePath + saveName + Extention;

		var serializer = GetSerializer();
		var stream = File.CreateText(fullFilePath);
		
		var saveData = WorldSave.GetSave();
		serializer.Serialize(stream, saveData);
		stream.Close();

		InfoText.Display($"Jeu sauvegardé sous {saveName}");
	}

	public static bool Load(string fileName, out string errorMessage)
	{
		var serializer = GetSerializer();
		var fullFilePath = savePath + fileName;
		var stream = File.OpenText(fullFilePath);
		var jsonStream = new JsonTextReader(stream);

		var saveData = serializer.Deserialize<WorldSave>(jsonStream);
		jsonStream.Close();

		if (CheckVersionCompatibility(saveData.Version))
		{
			World.loadData = saveData;
			World.ReloadLevel();
			errorMessage = "";
			return true;
		}
		else
		{
			var sb = new StringBuilder();
			sb.Append("<b>Version incompatible</b>");
			sb.Append($"\n{fileName} n'a pas pu être chargée.\n");
			sb.Append($"\nDernière version compatible : {MinCompatibleVersion}");
			sb.Append($"\nVersion de la sauvegarde : {saveData.Version}");
			errorMessage = sb.ToString();
			return false;
		}
	}

	public static string[] ListSaveGames()
	{
		var fullItems = Directory.GetFiles(savePath);
		for (int i = 0; i < fullItems.Length; i++)
		{
			fullItems[i] = Path.GetFileNameWithoutExtension(fullItems[i]);
		}
		return fullItems;
	}

	public static bool CheckVersionCompatibility(string version)
	{
		Version minVersion = Version.Parse(MinCompatibleVersion);
		Version actualVersion = Version.Parse(Application.version);
		Version saveVersion = Version.Parse(version);

		return saveVersion.CompareTo(actualVersion) <= 0 && saveVersion.CompareTo(minVersion) >= 0;
	}

}

