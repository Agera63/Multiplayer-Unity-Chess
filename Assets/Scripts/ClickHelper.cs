using UnityEngine;
using UnityEngine.InputSystem;

public class ClickHelper : MonoBehaviour
{
    /// <summary>
    /// Allows for a user click to be inputed and returns a the clicked gameobject.
    /// </summary>
    /// <returns>The game object that was clicked.</returns>
    public static GameObject FindClickedObject()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        GameObject selectedObject = null;

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            selectedObject = hit.collider.gameObject;
        }

        return selectedObject;
    }
}
