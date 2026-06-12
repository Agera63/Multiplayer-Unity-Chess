public class Rook : MonoBehaviorPiece
{    
    void Awake()
    {
        helperClass = new RookHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            this.gameObject);
    }
}
