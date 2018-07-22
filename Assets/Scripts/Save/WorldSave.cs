using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using static EconomyTemplate;

public class WorldSave
{
    [JsonProperty]
    public string Version;
    [JsonProperty]
    public string Build;
    [JsonProperty]
    public float Width;
    [JsonProperty]
    public float Height;
    [JsonProperty]
    public int? Balance;
    [JsonProperty]
    public Difficulty? DifficultyLevel;

    public List<Construction> Constructions = new List<Construction>();

    public List<Flux> AllFlux = new List<Flux>();

    private WorldSave() { }

    public static WorldSave GetSave()
    {
        var w = World.Instance;
        var save = new WorldSave
        {
            Width = World.width,
            Height = World.height,

            Version = Application.version,
            Build = Application.buildGUID,
            AllFlux = Flux.AllFlux,
            Balance = World.LocalEconomy.Balance,
            DifficultyLevel = World.LocalEconomy.DifficultyLevel
        };

        var c = w.Constructions;
        var width = c.GetLength(0);
        var height = c.GetLength(1);
        save.Constructions = new List<Construction>(width * height);
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                if (c[i, j] != null)
                    save.Constructions.Add(c[i, j]);

        return save;
    }
}

