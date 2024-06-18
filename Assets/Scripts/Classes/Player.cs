public class Player
{
    public string name;
    //public int coins;
    public float computerHistory;
    public float computerElements;
    public float numberSystem;
    public float introProgramming;

    public Player(string name)
    {
        this.name = name;
        //coins = 0;
    }

    //public void AddCoins(int amount)
    //{
    //    coins += amount;
    //}

    public void IncreaseStat(string statName, float amount)
    {
        switch (statName)
        {
            case "computerHistory":
                computerHistory += amount;
                break;
            case "computerElements":
                computerElements += amount;
                break;
            case "numberSystem":
                numberSystem += amount;
                break;
            case "introProgramming":
                introProgramming += amount;
                break;
            default:
                break;
        }
    }
}
