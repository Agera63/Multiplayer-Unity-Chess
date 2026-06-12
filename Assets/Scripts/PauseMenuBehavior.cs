using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuBehavior : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    private bool isActive = false;
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isActive = !isActive;
            pauseMenu.gameObject.SetActive(isActive);
        }       
    }
}
