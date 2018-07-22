using static EconomyTemplate;

public class Economy
{
    public int Balance { get; private set; }
    public Difficulty DifficultyLevel { get; private set; }

    private EconomyTemplate _template;

    public Economy(Difficulty difficulty, int? balance = null)
    {
        DifficultyLevel = difficulty;
        _template = Obtain(difficulty);
        Reset();

        if (balance.HasValue)
            Balance = balance.Value;
    }

    private void Reset()
    {
        Balance = _template.Gain("start");
    }

    public void ChangeDifficulty(Difficulty newDifficulty)
    {
        _template = Obtain(newDifficulty);
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
        cost = _template.Cost(operationName);
        ForcedDebit(cost);
    }

    public bool DoCost(string operationName, out int cost, int quantity)
    {
        cost = _template.Cost(operationName);
        return Debit(cost * quantity);
    }

    public void DoGain(string operationName, out int gain)
    {
        gain = _template.Gain(operationName);
        Credit(gain);
    }

    public int GetCost(string operationName)
    {
        return _template.Cost(operationName);
    }

    public int GetGain(string operationName)
    {
        return _template.Gain(operationName);
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

