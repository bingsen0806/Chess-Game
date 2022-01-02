using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
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
