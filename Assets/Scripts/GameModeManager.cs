public class GameModeManager
{
    public static GameModeManager instance = new();

    public GameMode selectedMode { get; private set; } = GameMode.PvE;
    public bool playerColor { get; private set; } = true;
    public bool clientColor { get; private set; } = false;

    public void SetPlayerColor(bool isWhite) => playerColor = isWhite;
    public void SetClientColor(bool isWhite) => clientColor = isWhite;
    public void SetGameMode(GameMode gameMode) => selectedMode = gameMode;
}