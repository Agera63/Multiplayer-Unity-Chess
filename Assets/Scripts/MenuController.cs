using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject botColorPick;
    [SerializeField] private GameObject onlineCode;
    [SerializeField] private GameObject joinSection;

    public void BotHanlder()
    {
        GameModeManager.instance.SetGameMode(GameMode.PvE);
        botColorPick.gameObject.SetActive(true);
    }

    public void OnlineHanlder()
    {
        GameModeManager.instance.SetGameMode(GameMode.PvP_Online);
        onlineCode.gameObject.SetActive(true);
    }

    public void Quitter()
    {
        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    //For botColorPick menu
    public void WhitePick()
    {
        GameModeManager.instance.SetPlayerColor(true);
        SceneManager.LoadScene("GameScene");
    }

    public void BlackPick()
    {
        GameModeManager.instance.SetPlayerColor(false);
        SceneManager.LoadScene("GameScene");
    }

    public void RandomPick()
    {
        int random = Random.Range(0, 2);
        if(random == 0)
            GameModeManager.instance.SetPlayerColor(true);
        else
            GameModeManager.instance.SetPlayerColor(false);

        SceneManager.LoadScene("GameScene");
    }

    //For Join Code menu
    public void CloseJoiningMenu()
    {
        joinSection.SetActive(true);
        onlineCode.SetActive(false);
    }
}
