using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AI : MonoBehaviour
{
    private Board board;
    private int depth;

    private Move throwAwayMove;

    private Dictionary<char, float> pieceVal;
    private Dictionary<char, float[]> positionalVal;

    public AI()
    {
        pieceVal = new Dictionary<char, float>();
        positionalVal = new Dictionary<char, float[]>();
        #region Piece Values
            pieceVal.Add('P', 10.0f);
            pieceVal.Add('N', 35.0f);
            pieceVal.Add('B', 35.0f);
            pieceVal.Add('R', 52.5f);
            pieceVal.Add('Q', 100.0f);
            pieceVal.Add('K', 100000.0f);
            pieceVal.Add('p', -10.0f);
            pieceVal.Add('n', -35.0f);
            pieceVal.Add('b', -35.0f);
            pieceVal.Add('r', -52.5f);
            pieceVal.Add('q', -100.0f);
            pieceVal.Add('k', -100000.0f);
        #endregion

        #region Positional Piece Values
            positionalVal.Add('p', new float[] {0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,
                                            5.0f,  5.0f,  5.0f,  5.0f,  5.0f,  5.0f,  5.0f,  5.0f,
                                            1.0f,  1.0f,  2.0f,  3.0f,  3.0f,  2.0f,  1.0f,  1.0f,
                                            0.5f,  0.5f,  1.0f,  2.5f,  2.5f,  1.0f,  0.5f,  0.5f,
                                            0.0f,  0.0f,  0.0f,  2.0f,  2.0f,  0.0f,  0.0f,  0.0f,
                                            0.5f, -0.5f, -1.0f,  0.0f,  0.0f, -1.0f, -0.5f,  0.5f,
                                            0.5f,  1.0f, 1.0f,  -2.0f, -2.0f,  1.0f,  1.0f,  0.5f,
                                            0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f});
            positionalVal.Add('n', new float[] {-5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f,
                                            -4.0f, -2.0f,  0.0f,  0.0f,  0.0f,  0.0f, -2.0f, -4.0f,
                                            -3.0f,  0.0f,  1.0f,  1.5f,  1.5f,  1.0f,  0.0f, -3.0f,
                                            -3.0f,  0.5f,  1.5f,  2.0f,  2.0f,  1.5f,  0.5f, -3.0f,
                                            -3.0f,  0.0f,  1.5f,  2.0f,  2.0f,  1.5f,  0.0f, -3.0f,
                                            -3.0f,  0.5f,  1.0f,  1.5f,  1.5f,  1.0f,  0.5f, -3.0f,
                                            -4.0f, -2.0f,  0.0f,  0.5f,  0.5f,  0.0f, -2.0f, -4.0f,
                                            -5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f });
            positionalVal.Add('b', new float[] {-2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f,
                                        -1.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f, -1.0f,
                                            -1.0f,  0.0f,  0.5f,  1.0f,  1.0f,  0.5f,  0.0f, -1.0f,
                                            -1.0f,  0.5f,  0.5f,  1.0f,  1.0f,  0.5f,  0.5f, -1.0f,
                                            -1.0f,  0.0f,  1.0f,  1.0f,  1.0f,  1.0f,  0.0f, -1.0f,
                                            -1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.0f, -1.0f,
                                            -1.0f,  0.5f,  0.0f,  0.0f,  0.0f,  0.0f,  0.5f, -1.0f,
                                            -2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f });
            positionalVal.Add('r', new float[] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,  0.0f,
                                             0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,  0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                             0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.0f,  0.0f});
            positionalVal.Add('q', new float[] {-2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f,
                                            -1.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f, -1.0f,
                                            -1.0f,  0.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -1.0f,
                                            -0.5f,  0.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -0.5f,
                                            -0.5f,  0.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -0.5f,
                                            -1.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -1.0f,
                                            -1.0f,  0.0f,  0.5f,  0.0f,  0.0f,  0.0f,  0.0f, -1.0f,
                                            -2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f});
            positionalVal.Add('k', new float[] {-3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -2.0f, -3.0f, -3.0f, -4.0f, -4.0f, -3.0f, -3.0f, -2.0f,
                                             -1.0f, -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, -1.0f,
                                              2.0f,  2.0f,  0.0f,  0.0f,  0.0f,  0.0f,  2.0f,  2.0f,
                                              2.0f,  3.0f,  1.0f,  0.0f,  0.0f,  1.0f,  3.0f,  2.0f });
            positionalVal.Add('P', new float[] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 
                                            0.5f, 1.0f, 1.0f, -2.0f, -2.0f, 1.0f, 1.0f, 0.5f, 
                                            0.5f, -0.5f, -1.0f, 0.0f, 0.0f, -1.0f, -0.5f, 0.5f, 
                                            0.0f, 0.0f, 0.0f, 2.0f, 2.0f, 0.0f, 0.0f, 0.0f, 
                                            0.5f, 0.5f, 1.0f, 2.5f, 2.5f, 1.0f, 0.5f, 0.5f, 
                                            1.0f, 1.0f, 2.0f, 3.0f, 3.0f, 2.0f, 1.0f, 1.0f, 
                                            5.0f, 5.0f, 5.0f, 5.0f, 5.0f, 5.0f, 5.0f, 5.0f, 
                                            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f});
            positionalVal.Add('N', new float[] {-5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f, 
                                            -4.0f, -2.0f, 0.0f, 0.5f, 0.5f, 0.0f, -2.0f, -4.0f, 
                                            -3.0f, 0.5f, 1.0f, 1.5f, 1.5f, 1.0f, 0.5f, -3.0f, 
                                            -3.0f, 0.0f, 1.5f, 2.0f, 2.0f, 1.5f, 0.0f, -3.0f, 
                                            -3.0f, 0.5f, 1.5f, 2.0f, 2.0f, 1.5f, 0.5f, -3.0f, 
                                            -3.0f, 0.0f, 1.0f, 1.5f, 1.5f, 1.0f, 0.0f, -3.0f, 
                                            -4.0f, -2.0f, 0.0f, 0.0f, 0.0f, 0.0f, -2.0f, -4.0f, 
                                            -5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f});
            positionalVal.Add('B', new float[] {-2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f, 
                                            -1.0f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, -1.0f, 
                                            -1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f, 
                                            -1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, -1.0f, 
                                            -1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 0.5f, 0.5f, -1.0f, 
                                            -1.0f, 0.0f, 0.5f, 1.0f, 1.0f, 0.5f, 0.0f, -1.0f, 
                                            -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 
                                            -2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f});
            positionalVal.Add('R', new float[] {0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.0f, 0.0f, 
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f, 
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f, 
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f, 
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f, 
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f, 
                                            0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 
                                            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f});
            positionalVal.Add('Q', new float[] {-2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f, 
                                            -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f, -1.0f, 
                                            -1.0f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, -1.0f, 
                                            -0.5f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, -0.5f, 
                                            -0.5f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, -0.5f, 
                                            -1.0f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, -1.0f, 
                                            -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 
                                            -2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f});
            positionalVal.Add('K', new float[] {2.0f, 3.0f, 1.0f, 0.0f, 0.0f, 1.0f, 3.0f, 2.0f, 
                                            2.0f, 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, 2.0f, 2.0f, 
                                            -1.0f, -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, -1.0f, 
                                            -2.0f, -3.0f, -3.0f, -4.0f, -4.0f, -3.0f, -3.0f, -2.0f, 
                                            -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f, 
                                            -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f, 
                                            -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f, 
                                            -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f});
        #endregion
    }
    public void PrepareSearch(Board board)
    {
        this.board = board;
        // This exists only to satisfy the tuple return
        throwAwayMove = new Move(this.board.squares[1], this.board.squares[26], 0);
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
                    Debug.Log("Found Mate");
                    if (player)
                        return (throwAwayMove, Mathf.Infinity);
                    else
                        return (throwAwayMove, -Mathf.Infinity);
                }
                else if (board.stalemate)
                    return (throwAwayMove, 0);
                return (throwAwayMove, CalculatePos(legalMoves));
            }
            else if (color == "BLACK")
            {
                if (board.checkmate)
                {
                    Debug.Log("Found Mate");
                    if (player)
                        return (throwAwayMove, -Mathf.Infinity);
                    else 
                        return (throwAwayMove, Mathf.Infinity);
                }
                else if (board.stalemate)
                    return (throwAwayMove, 0);
                return (throwAwayMove, CalculatePos(legalMoves));
            }
        }

        Move bestMove = new Move(this.board.squares[1], this.board.squares[26], 0);
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

                alpha = Mathf.Max(alpha, max);

                if (alpha >= beta && max != Mathf.Infinity)
                    break;

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

                beta = Mathf.Min(beta, min);

                if (beta <= alpha && min != Mathf.Infinity)
                    break;
                
                if (min < bestValueMove)
                {
                    bestValueMove = min;
                    bestMove = move;
                }
            }
            if (depth < originalDepth) return (throwAwayMove, min);
        }
        if (bestMove.Equals(throwAwayMove))
        {
            Debug.Log("Something went Wrong");
            return (legalMoves[0], 0f);
        }
        Debug.Log(CalculatePos(legalMoves));
        return (bestMove, 0f);
    }

    public float CalculatePos(Move[] legalMoves)
    {
        float sum = 0;
        for (int i = 0; i < board.squares.Length; i++)
        {
            if (board.squares[i].piece.Equals("")) continue;
            sum += pieceVal[board.squares[i].piece[0]];
            sum += positionalVal[board.squares[i].piece[0]][board.GetIndex(board.squares[i].position)];
        }
        return sum;
    }

    
}
