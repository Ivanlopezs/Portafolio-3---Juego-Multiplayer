using System;

[Serializable]
public class PlayerData
{
    public int playerID;
    public string playerName;
    public PlayerHand hand;

    public PlayerData(int id, string name)
    {
        playerID = id;
        playerName = name;
        hand = new PlayerHand();
    }
}
