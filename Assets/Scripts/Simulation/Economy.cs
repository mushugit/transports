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

	public bool DoCost(string operationName, out int cost, int quantity)
	{
		cost = template.Cost(operationName);
		return Debit(cost*quantity);
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

    public static bool CheckCost(Economy economy, string operationName, string messageOperation, out int cost, int quantity = 1)
    {
        if (!economy.DoCost(operationName, out cost, quantity))
        {
            Message.ShowError("Pas assez d'argent",
                $"Vous ne pouvez pas {messageOperation} celà coûte {cost} et vous avez {economy.Balance}");
            return false;
        }
        else
            return true;
    }

}

