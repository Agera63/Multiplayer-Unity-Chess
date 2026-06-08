using UnityEngine;

public class GameModeManager
{
    public static GameModeManager instance = new(); //Singleton

    public GameMode selectedMode { get; private set; } = GameMode.PvE;
    public bool playerColor { get; private set; } = true; //True = white | False = black

    public void SetPlayerColor(bool isWhite) => playerColor = isWhite;
    public void SetGameMode(GameMode gameMode) => selectedMode = gameMode;
}
