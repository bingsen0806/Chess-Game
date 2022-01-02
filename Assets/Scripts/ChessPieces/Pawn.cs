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
}
