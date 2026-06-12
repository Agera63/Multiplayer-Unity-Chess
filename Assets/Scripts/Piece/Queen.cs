public class Queen : MonoBehaviorPiece
{
    void Awake()
    {
        helperClass = new QueenHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            this.gameObject);
    }
}
