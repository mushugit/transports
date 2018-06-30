using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveHandler
{
	public static readonly string Extention = ".json";
	public static readonly string Folder = "saves";

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

	public static void Load(string fileName)
	{
		var serializer = GetSerializer();
		var fullFilePath = savePath + fileName;
		var stream = File.OpenText(fullFilePath);
		var jsonStream = new JsonTextReader(stream);

		var saveData = serializer.Deserialize<WorldSave>(jsonStream);
		jsonStream.Close();

		World.loadData = saveData;
		World.ReloadLevel();
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

}

