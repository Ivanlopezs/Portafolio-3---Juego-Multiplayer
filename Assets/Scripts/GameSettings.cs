public static class GameSettings
{
    public static int PlayerCount = 2;

    public static GameMode SelectedMode = GameMode.Local;
}

public enum GameMode
{
    Local,
    Online
}
