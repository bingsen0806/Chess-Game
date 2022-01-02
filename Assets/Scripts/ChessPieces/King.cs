using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        SpecialMove r = SpecialMove.None;
        //First see if King and Rooks has moved since the start of the game
        var kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == 0) ? 0 : 7));
        var leftRook = moveList.Find(m => m[0].x == 0 && m[0].y == ((team == 0) ? 0 : 7));
        var rightRook = moveList.Find(m => m[0].x == 7 && m[0].y == ((team == 0) ? 0 : 7));

        if (kingMove == null & currentX == 4)
        {
            int yCoord = (team == 0) ? 0 : 7;
            //Left rook
            if (leftRook == null
            && board[0, yCoord].type == ChessPieceType.Rook
            && board[0, yCoord].team == team
            && board[3, yCoord] == null && board[2, yCoord] == null && board[1, yCoord] == null)
            {
                availableMoves.Add(new Vector2Int(2, yCoord));
                r = SpecialMove.Castling;
            }
            //Right rook
            if (rightRook == null
            && board[7, yCoord].type == ChessPieceType.Rook
            && board[7, yCoord].team == team
            && board[5, yCoord] == null && board[6, yCoord] == null)
            {
                availableMoves.Add(new Vector2Int(6, yCoord));
                r = SpecialMove.Castling;
            }
        }
        return r;
    }



    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Right
        if (currentX + 1 < tileCountX)
        {
            if (board[currentX + 1, currentY] == null)
            {
                r.Add(new Vector2Int(currentX + 1, currentY));
            }
            else if (board[currentX + 1, currentY].team != team)
            {
                r.Add(new Vector2Int(currentX + 1, currentY));
            }
        }

        //Top Right
        if (currentX + 1 < tileCountX && currentY + 1 < tileCountY)
        {
            if (board[currentX + 1, currentY + 1] == null)
            {
                r.Add(new Vector2Int(currentX + 1, currentY + 1));
            }
            else if (board[currentX + 1, currentY + 1].team != team)
            {
                r.Add(new Vector2Int(currentX + 1, currentY + 1));
            }
        }

        //Bottom Right
        if (currentX + 1 < tileCountX && currentY - 1 >= 0)
        {
            if (board[currentX + 1, currentY - 1] == null)
            {
                r.Add(new Vector2Int(currentX + 1, currentY - 1));
            }
            else if (board[currentX + 1, currentY - 1].team != team)
            {
                r.Add(new Vector2Int(currentX + 1, currentY - 1));
            }
        }

        //Left
        if (currentX - 1 >= 0)
        {
            if (board[currentX - 1, currentY] == null)
            {
                r.Add(new Vector2Int(currentX - 1, currentY));
            }
            else if (board[currentX - 1, currentY].team != team)
            {
                r.Add(new Vector2Int(currentX - 1, currentY));
            }
        }

        //Top Left
        if (currentX - 1 >= 0 && currentY + 1 < tileCountY)
        {
            if (board[currentX - 1, currentY + 1] == null)
            {
                r.Add(new Vector2Int(currentX - 1, currentY + 1));
            }
            else if (board[currentX - 1, currentY + 1].team != team)
            {
                r.Add(new Vector2Int(currentX - 1, currentY + 1));
            }
        }

        //Bottom Left
        if (currentX - 1 >= 0 && currentY - 1 >= 0)
        {
            if (board[currentX - 1, currentY - 1] == null)
            {
                r.Add(new Vector2Int(currentX - 1, currentY - 1));
            }
            else if (board[currentX - 1, currentY - 1].team != team)
            {
                r.Add(new Vector2Int(currentX - 1, currentY - 1));
            }
        }

        //Up
        if (currentY + 1 < tileCountY)
        {
            if (board[currentX, currentY + 1] == null || board[currentX, currentY + 1].team != team)
            {
                r.Add(new Vector2Int(currentX, currentY + 1));
            }
        }

        //Down
        if (currentY - 1 >= 0)
        {
            if (board[currentX, currentY - 1] == null || board[currentX, currentY - 1].team != team)
            {
                r.Add(new Vector2Int(currentX, currentY - 1));
            }
        }

        return r;
    }
}
