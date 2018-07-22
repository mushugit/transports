using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class SaveHandler
{
    public static readonly string Extention = ".json";
    public static readonly string Folder = "saves";

    public static readonly string MinCompatibleVersion = "0.0.5.4";

    private static readonly string savePath = Application.persistentDataPath + Path.DirectorySeparatorChar + Folder + Path.DirectorySeparatorChar;

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

    public static bool Save(out string errorMessage)
    {
        var now = DateTime.Now;
        var stringDate = now.ToString("yyyyMMdd_HHmmss");

        var saveName = $"save_{stringDate}";
        var fullFilePath = savePath + saveName + Extention;
        var serializer = GetSerializer();
        StreamWriter stream = null;
        try
        {
            stream = File.CreateText(fullFilePath);

            var saveData = WorldSave.GetSave();
            serializer.Serialize(stream, saveData);
        }
        catch (JsonSerializationException jsonException)
        {
            stream?.Close();
            stream = null;

            var sb = new StringBuilder();
            sb.Append("<b>Erreur de création de la sauvegarde</b>");
            sb.Append($"\n{saveName} n'a pas pu être créé.\n");

            try
            {
                File.Delete(fullFilePath);
            }
            catch (Exception)
            {
                sb.Append($"\nFichier corrompu toujours présent !\n");
            }

            sb.Append($"\nDétail : {jsonException.Message}");
            errorMessage = sb.ToString();
            return false;
        }

        stream?.Close();

        PauseMenu.Display($"Jeu sauvegardé sous {saveName}");
        errorMessage = "";
        return true;
    }

    public static bool Load(string fileName, out string errorMessage)
    {
        var serializer = GetSerializer();
        var fullFilePath = savePath + fileName;
        var stream = File.OpenText(fullFilePath);
        var jsonStream = new JsonTextReader(stream);

        World.ClearInstance();
        WorldSave saveData = null;
        try
        {
            saveData = serializer.Deserialize<WorldSave>(jsonStream);
        }
        catch (JsonSerializationException jsonException)
        {
            jsonStream.Close();

            var sb = new StringBuilder();
            sb.Append("<b>Erreur de lecture de la sauvegarde</b>");
            sb.Append($"\n{fileName} n'a pas pu être chargée.\n");
            sb.Append($"\nDétail : {jsonException.Message}");
            errorMessage = sb.ToString();
            return false;
        }

        jsonStream.Close();

        if (CheckVersionCompatibility(saveData.Version))
        {
            World.loadData = saveData;
            World.ReloadLevel(World.worldLoadSceneIndex);
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

