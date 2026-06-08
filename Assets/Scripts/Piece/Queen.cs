public class Queen : MonoBehaviorPiece
{
   void Start()
    {
        helperClass = new QueenHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            this.gameObject);
    }
}
