using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

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

	public List<Construction> Constructions;

	public List<Flux> AllFlux;

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
			AllFlux = Flux.AllFlux
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

