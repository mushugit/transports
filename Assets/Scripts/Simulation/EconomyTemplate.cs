using System;
using System.Collections.Generic;

public class EconomyTemplate
{
	public enum Difficulty
	{
		Free,
		Easy,
		Normal,
		Hard
	};

	private static readonly Dictionary<Difficulty, double> difficultyMultiplier = new Dictionary<Difficulty, double>()
	{
		{
			Difficulty.Free,
			0
		},
		{
			Difficulty.Easy,
			0.5
		},
		{
			Difficulty.Normal,
			1
		},
		{
			Difficulty.Hard,
			2
		}
	};

	private static readonly Dictionary<Difficulty, EconomyTemplate> templates = new Dictionary<Difficulty, EconomyTemplate>()
	{
		{
			Difficulty.Free,
			new EconomyTemplate(Difficulty.Free)
		},
		{
			Difficulty.Easy,
			new EconomyTemplate(Difficulty.Easy)
		},
		{
			Difficulty.Normal,
			new EconomyTemplate(Difficulty.Normal)
		},
		{
			Difficulty.Hard,
			new EconomyTemplate(Difficulty.Hard)
		}
	};

	private readonly Dictionary<string, int> operationsCosts;
	private readonly Dictionary<string, int> operationGains;
	public readonly Difficulty SelectedDifficulty;


	private EconomyTemplate(Difficulty selectedDifficulty)
	{
		SelectedDifficulty = selectedDifficulty;
		var mCost = difficultyMultiplier[SelectedDifficulty];
		var mGain = 1d;
		if (mCost != 0)
			mGain = 1 / mCost;

		operationGains = new Dictionary<string, int>
		{
			{ "start",                              (int)Math.Round(10000 * mGain) },
			{ "loose",								(int)Math.Round(-10000 * mGain) },

			{ "flux_deliver_percell",				(int)Math.Round(42 * mGain) }
		};

		operationsCosts = new Dictionary<string, int>
		{
			{ "build_city",							(int)Math.Round(20000 * mCost) },
			{ "destroy_city",                       (int)Math.Round(10000 * mCost) },

			{ "build_road",                         (int)Math.Round(50 * mCost) },
			{ "destroy_road",                       (int)Math.Round(25 * mCost) },

			{ "build_depot",                        (int)Math.Round(250 * mCost) },
			{ "destroy_depot",                      (int)Math.Round(125 * mCost) },

			{ "flux_create",                        (int)Math.Round(150 * mCost) },
			{ "flux_running",                        (int)Math.Round(1 * mCost) }
		};

	}

	public static EconomyTemplate Obtain(Difficulty difficulty)
	{
		return templates[difficulty];
	}

	public int Cost(string operationName)
	{
		return operationsCosts[operationName];
	}

	public int Gain(string operationName)
	{
		return operationGains[operationName];
	}
}

