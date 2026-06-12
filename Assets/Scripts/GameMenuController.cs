using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuController : MonoBehaviour
{
    public static event Action<char> sendPromotionValue;

    public void BishopPromotion() => sendPromotionValue?.Invoke('b');

    public void QueenPromotion() => sendPromotionValue?.Invoke('q');

    public void KnightPromotion() => sendPromotionValue?.Invoke('n');

    public void RookPromotion() => sendPromotionValue?.Invoke('r');

    public void ExitGame() => SceneManager.LoadScene("MainMenu");
}
