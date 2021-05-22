using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AI : MonoBehaviour
{
    private Board board;
    private int depth;

    private Move throwAwayMove;

    public void PrepareSearch(Board board)
    {
        this.board = board;
        // This exists only to satisfy the tuple return
        throwAwayMove = new Move(this.board.squares[0], this.board.squares[1], 0);
    }

    public (Move, float) Search(int depth, int originalDepth, bool player, string color, float alpha, float beta)
    {
        Move[] legalMoves = board.GetLegalMoves();
        if (depth == 0 || legalMoves.Length == 0)
        {
            if (color == "WHITE")
            {
                if (board.checkmate)
                {
                    if (player)
                        return (throwAwayMove, Mathf.Infinity);
                    else
                        return (throwAwayMove, -Mathf.Infinity);
                }
                else if (board.stalemate)
                    return (throwAwayMove, 0);
                return (throwAwayMove, CalculatePos());
            }
            else if (color == "BLACK")
            {
                if (board.checkmate)
                {
                    if (player)
                        return (throwAwayMove, -Mathf.Infinity);
                    else 
                        return (throwAwayMove, Mathf.Infinity);
                }
                else if (board.stalemate)
                    return (throwAwayMove, 0);
                return (throwAwayMove, -CalculatePos());
            }
        }

        Move bestMove = null;
        if (player)
        {
            float max = -Mathf.Infinity;
            float bestValueMove = -Mathf.Infinity;
            foreach (Move move in legalMoves)
            {
                // Make the move
                string temp = board.MakeMove(move);

                // Recursively check next moves in line
                float currentEval = Search(depth -1, originalDepth, false, color, alpha, beta).Item2;
                board.UnmakeMove(move, temp);
                max = Mathf.Max(max, currentEval);

                if (max != Mathf.Infinity)
                {
                    alpha = Mathf.Max(alpha, max);

                    if (alpha > beta)
                        break;
                }
                if (max > bestValueMove)
                {
                    bestValueMove = max;
                    bestMove = move;
                }
            }
            if (depth < originalDepth) return (throwAwayMove, max);
        }
        else
        {
            float min = Mathf.Infinity;
            float bestValueMove = Mathf.Infinity;
            foreach (Move move in legalMoves)
            {
                // Make the move
                string temp = board.MakeMove(move);

                // Recursively check next moves in line
                float currentEval = Search(depth -1, originalDepth, true, color, alpha, beta).Item2;
                board.UnmakeMove(move, temp);
                min = Mathf.Min(min, currentEval);

                if (min != Mathf.Infinity)
                {
                    alpha = Mathf.Min(alpha, min);

                    if (alpha > beta)
                        break;
                }
                if (min > bestValueMove)
                {
                    bestValueMove = min;
                    bestMove = move;
                }
            }
            if (depth < originalDepth) return (throwAwayMove, min);
        }
        return (bestMove, 0f);
    }

    public float CalculatePos()
    {
        return board.sum;
    }
}
