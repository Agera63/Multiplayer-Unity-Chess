using System.Collections;
using UnityEngine;

public class Rook : MonoBehaviour
{
    [SerializeField] public Collider trigger;
    [SerializeField] public bool isWhite;
    public RookHelper helperClass;
    public bool isClicked;

    void Start()
    {

    }

    void Update()
    {

    }

    public void ShowLegalMoves()
    {

    }

    IEnumerator MoveAnimation(Vector3 pos)
    {
        yield return null;
    }

    private void ActivateTrigger()
    {
        trigger.gameObject.SetActive(true);
    }

    private void DeactivateTrigger()
    {
        trigger.gameObject.SetActive(false);
    }
}
