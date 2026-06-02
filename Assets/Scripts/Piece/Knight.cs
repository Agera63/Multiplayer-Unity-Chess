using System.Collections;
using UnityEngine;

public class Knight : MonoBehaviour
{
    [SerializeField] public Collider trigger;
    [SerializeField] public bool isWhite;
    public KnightHelper helperClass;
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
