using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1; //go up if white, down if black

        //Move one step in front
        if (board[currentX, currentY + direction] == null)
        {
            r.Add(new Vector2Int(currentX, currentY + direction));
        }

        //Two steps in front
        //First check if nobody one step in front to prevent jumping over
        if (board[currentX, currentY + direction] == null)
        {
            //White team
            if (team == 0 && currentY == 1 && board[currentX, currentY + direction * 2] == null)
            {
                r.Add(new Vector2Int(currentX, currentY + direction * 2));
            }
            if (team == 1 && currentY == 6 && board[currentX, currentY + direction * 2] == null)
            {
                r.Add(new Vector2Int(currentX, currentY + direction * 2));
            }
        }

        //Take pieces in diagonal
        if (currentX != tileCountX - 1
            && board[currentX + 1, currentY + direction] != null
            && board[currentX + 1, currentY + direction].team != team)
        {
            r.Add(new Vector2Int(currentX + 1, currentY + direction));
        }
        if (currentX != 0
            && board[currentX - 1, currentY + direction] != null
            && board[currentX - 1, currentY + direction].team != team)
        {
            r.Add(new Vector2Int(currentX - 1, currentY + direction));
        }
        return r;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == 0) ? 1 : -1;
        if ((team == 0 && currentY == 6) || (team == 1 && currentY == 1))
        {
            return SpecialMove.Promotion;
        }
        //En Passant
        if (moveList.Count > 0)
        {
            Vector2Int[] lastmove = moveList[moveList.Count - 1];
            //Check if last move piece is a pawn
            if (board[lastmove[1].x, lastmove[1].y].type == ChessPieceType.Pawn
                //Check if last pawn moved 2 steps
                && Mathf.Abs(lastmove[0].y - lastmove[1].y) == 2
                && board[lastmove[1].x, lastmove[1].y].team != team
                && lastmove[1].y == currentY)
            {
                if (lastmove[1].x == currentX - 1)
                { //opponent landed on left
                    availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                    return SpecialMove.EnPassant;
                }
                if (lastmove[1].x == currentX + 1)
                { //opponent landed on right
                    availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                    return SpecialMove.EnPassant;
                }
            }
        }
        return SpecialMove.None;
    }
}
