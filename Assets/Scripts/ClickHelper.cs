using UnityEngine;
using UnityEngine.InputSystem;

public class ClickHelper : MonoBehaviour
{
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
