public class Knight : MonoBehaviorPiece
{
    void Start()
    {
        helperClass = new KnightHelper(
                isWhite,
                BoardPos.VectorToBoardPosObject(transform.position),
                this.gameObject);
    }
}
