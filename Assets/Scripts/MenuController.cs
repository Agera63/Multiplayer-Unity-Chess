using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject botColorPick;
    [SerializeField] private GameObject onlineCode;
    [SerializeField] private GameObject joinSection;

    /// <summary>
    /// Once the bot gamemode has been selected, it will open a menu
    /// where the color picking menu will show up.
    /// </summary>
    public void BotHanlder()
    {
        GameModeManager.instance.SetGameMode(GameMode.PvE);
        botColorPick.gameObject.SetActive(true);
    }

    /// <summary>
    /// Opens the online menu so it can host or enter a code.
    /// </summary>
    public void OnlineHanlder()
    {
        GameModeManager.instance.SetGameMode(GameMode.PvP_Online);
        onlineCode.gameObject.SetActive(true);
    }

    /// <summary>
    /// Attachted to the quit button.
    /// Allows the user to quit the application.
    /// </summary>
    public void Quitter()
    {
        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    /// <summary>
    /// This is attached to the white color pick.
    /// Once this is selected, the user color vs Stockfish is white.
    /// </summary>
    public void WhitePick()
    {
        GameModeManager.instance.SetPlayerColor(true);
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// This is attached to the black color pick.
    /// Once this is selected, the user color vs Stockfish is black.
    /// </summary>
    public void BlackPick()
    {
        GameModeManager.instance.SetPlayerColor(false);
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// This is attached to the random color pick.
    /// Once this is selected, the user color vs Stockfish is random.
    /// </summary>
    public void RandomPick()
    {
        int random = Random.Range(0, 2);
        if(random == 0)
            GameModeManager.instance.SetPlayerColor(true);
        else
            GameModeManager.instance.SetPlayerColor(false);

        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Only for online menu, this allows the menu to close.
    /// </summary>
    public void CloseJoiningMenu()
    {
        joinSection.SetActive(true);
        onlineCode.SetActive(false);
    }
}
