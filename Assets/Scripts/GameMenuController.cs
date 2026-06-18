using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuController : MonoBehaviour
{
    public static event Action<char> sendPromotionValue;

    public void BishopPromotion() => sendPromotionValue?.Invoke('b');

    public void QueenPromotion() => sendPromotionValue?.Invoke('q');

    public void KnightPromotion() => sendPromotionValue?.Invoke('n');

    public void RookPromotion() => sendPromotionValue?.Invoke('r');

    public void ExitGame()
    {
        if (NetworkManager.Singleton != null &&
            (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene("MainMenu");
    }
}
