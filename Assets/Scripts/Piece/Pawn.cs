public class Pawn : MonoBehaviorPiece
{   void Start()
    {
        helperClass = new PawnHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            this.gameObject);
    }
}
