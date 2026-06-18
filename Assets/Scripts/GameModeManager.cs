using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager instance { get; private set; }

    public GameMode selectedMode { get; private set; } = GameMode.PvE;
    public bool playerColor { get; private set; } = true;
    public bool clientColor { get; private set; } = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerColor(bool isWhite) => playerColor = isWhite;
    public void SetClientColor(bool isWhite) => clientColor = isWhite;
    public void SetGameMode(GameMode gameMode) => selectedMode = gameMode;
}