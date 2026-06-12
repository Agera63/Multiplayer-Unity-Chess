public class Bishop : MonoBehaviorPiece
{
    void Awake()
    {
        helperClass = new BishopHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            this.gameObject);
    }
}
