public class Knight : MonoBehaviorPiece
{
    void Awake()
    {
        helperClass = new KnightHelper(
                isWhite,
                BoardPos.VectorToBoardPosObject(transform.position),
                this.gameObject);
    }
}
