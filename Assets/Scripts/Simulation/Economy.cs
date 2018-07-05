using static EconomyTemplate;

public class Economy
{
	public int Balance { get; private set; }
	private EconomyTemplate template;

	public Economy(Difficulty difficulty)
	{
		template = Obtain(difficulty);
		Reset();
	}

	public void Reset()
	{
		Balance = template.Gain("start");
	}

	public void ChangeDifficulty(Difficulty newDifficulty)
	{
		template = Obtain(newDifficulty);
	}

	public void Credit(int sum)
	{
		Balance += sum;
	}

	public bool Debit(int sum)
	{
		if (Balance - sum <= 0)
			return false;
		Balance -= sum;
		return true;
	}

	public void ForcedDebit(int sum)
	{
		Balance -= sum;
	}

	public void ForcedCost(string operationName, out int cost)
	{
		cost = template.Cost(operationName);
		ForcedDebit(cost);
	}

	public bool DoCost(string operationName, out int cost)
	{
		cost = template.Cost(operationName);
		return Debit(cost);
	}

	public void DoGain(string operationName, out int gain)
	{
		gain = template.Gain(operationName);
		Credit(gain);
	}

	public int GetCost(string operationName)
	{
		return template.Cost(operationName);
	}

	public int GetGain(string operationName)
	{
		return template.Gain(operationName);
	}

}

