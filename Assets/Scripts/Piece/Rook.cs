public class Rook : MonoBehaviorPiece
{    
    void Start()
    {
        helperClass = new RookHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            this.gameObject);
    }
}
