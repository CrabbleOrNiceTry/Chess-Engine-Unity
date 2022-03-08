using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AI : MonoBehaviour
{
    private Board board;
    private int depth;

    private Move throwAwayMove;

    private Move bestMove;

    private Dictionary<char, float> pieceVal;
    private Dictionary<char, float[]> positionalVal;
    private float infinity;


    public AI()
    {
        infinity = 999999f;
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
        positionalVal.Add('P', new float[] {0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,
                                            5.0f,  5.0f,  5.0f,  5.0f,  5.0f,  5.0f,  5.0f,  5.0f,
                                            1.0f,  1.0f,  2.0f,  3.0f,  3.0f,  2.0f,  1.0f,  1.0f,
                                            0.5f,  0.5f,  1.0f,  2.5f,  2.5f,  1.0f,  0.5f,  0.5f,
                                            0.0f,  0.0f,  0.0f,  2.0f,  2.0f,  0.0f,  0.0f,  0.0f,
                                            0.5f, -0.5f, -1.0f,  0.0f,  0.0f, -1.0f, -0.5f,  0.5f,
                                            0.5f,  1.0f, 1.0f,  -2.0f, -2.0f,  1.0f,  1.0f,  0.5f,
                                            0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f});
        positionalVal.Add('N', new float[] {-5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f,
                                            -4.0f, -2.0f,  0.0f,  0.0f,  0.0f,  0.0f, -2.0f, -4.0f,
                                            -3.0f,  0.0f,  1.0f,  1.5f,  1.5f,  1.0f,  0.0f, -3.0f,
                                            -3.0f,  0.5f,  1.5f,  2.0f,  2.0f,  1.5f,  0.5f, -3.0f,
                                            -3.0f,  0.0f,  1.5f,  2.0f,  2.0f,  1.5f,  0.0f, -3.0f,
                                            -3.0f,  0.5f,  1.0f,  1.5f,  1.5f,  1.0f,  0.5f, -3.0f,
                                            -4.0f, -2.0f,  0.0f,  0.5f,  0.5f,  0.0f, -2.0f, -4.0f,
                                            -5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f });
        positionalVal.Add('B', new float[] {-2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f,
                                        -1.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f, -1.0f,
                                            -1.0f,  0.0f,  0.5f,  1.0f,  1.0f,  0.5f,  0.0f, -1.0f,
                                            -1.0f,  0.5f,  0.5f,  1.0f,  1.0f,  0.5f,  0.5f, -1.0f,
                                            -1.0f,  0.0f,  1.0f,  1.0f,  1.0f,  1.0f,  0.0f, -1.0f,
                                            -1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.0f, -1.0f,
                                            -1.0f,  0.5f,  0.0f,  0.0f,  0.0f,  0.0f,  0.5f, -1.0f,
                                            -2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f });
        positionalVal.Add('R', new float[] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,  0.0f,
                                             0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,  0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                             0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.0f,  0.0f});
        positionalVal.Add('Q', new float[] {-2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f,
                                            -1.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f, -1.0f,
                                            -1.0f,  0.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -1.0f,
                                            -0.5f,  0.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -0.5f,
                                            -0.5f,  0.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -0.5f,
                                            -1.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -1.0f,
                                            -1.0f,  0.0f,  0.5f,  0.0f,  0.0f,  0.0f,  0.0f, -1.0f,
                                            -2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f});
        positionalVal.Add('K', new float[] {-3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -2.0f, -3.0f, -3.0f, -4.0f, -4.0f, -3.0f, -3.0f, -2.0f,
                                             -1.0f, -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, -1.0f,
                                              2.0f,  2.0f,  0.0f,  0.0f,  0.0f,  0.0f,  2.0f,  2.0f,
                                              2.0f,  3.0f,  1.0f,  0.0f,  0.0f,  1.0f,  3.0f,  2.0f });
        positionalVal.Add('p', new float[] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                                            0.5f, 1.0f, 1.0f, -2.0f, -2.0f, 1.0f, 1.0f, 0.5f,
                                            0.5f, -0.5f, -1.0f, 0.0f, 0.0f, -1.0f, -0.5f, 0.5f,
                                            0.0f, 0.0f, 0.0f, 2.0f, 2.0f, 0.0f, 0.0f, 0.0f,
                                            0.5f, 0.5f, 1.0f, 2.5f, 2.5f, 1.0f, 0.5f, 0.5f,
                                            1.0f, 1.0f, 2.0f, 3.0f, 3.0f, 2.0f, 1.0f, 1.0f,
                                            5.0f, 5.0f, 5.0f, 5.0f, 5.0f, 5.0f, 5.0f, 5.0f,
                                            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f});
        positionalVal.Add('n', new float[] {-5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f,
                                            -4.0f, -2.0f, 0.0f, 0.5f, 0.5f, 0.0f, -2.0f, -4.0f,
                                            -3.0f, 0.5f, 1.0f, 1.5f, 1.5f, 1.0f, 0.5f, -3.0f,
                                            -3.0f, 0.0f, 1.5f, 2.0f, 2.0f, 1.5f, 0.0f, -3.0f,
                                            -3.0f, 0.5f, 1.5f, 2.0f, 2.0f, 1.5f, 0.5f, -3.0f,
                                            -3.0f, 0.0f, 1.0f, 1.5f, 1.5f, 1.0f, 0.0f, -3.0f,
                                            -4.0f, -2.0f, 0.0f, 0.0f, 0.0f, 0.0f, -2.0f, -4.0f,
                                            -5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f});
        positionalVal.Add('b', new float[] {-2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f,
                                            -1.0f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, -1.0f,
                                            -1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f,
                                            -1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, -1.0f,
                                            -1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 0.5f, 0.5f, -1.0f,
                                            -1.0f, 0.0f, 0.5f, 1.0f, 1.0f, 0.5f, 0.0f, -1.0f,
                                            -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f,
                                            -2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f});
        positionalVal.Add('r', new float[] {0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.0f, 0.0f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f,
                                            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f});
        positionalVal.Add('q', new float[] {-2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f,
                                            -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f, -1.0f,
                                            -1.0f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, -1.0f,
                                            -0.5f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, -0.5f,
                                            -1.0f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, -1.0f,
                                            -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f,
                                            -2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f});
        positionalVal.Add('k', new float[] {2.0f, 3.0f, 1.0f, 0.0f, 0.0f, 1.0f, 3.0f, 2.0f,
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
        bestMove = new Move(this.board.squares[1], this.board.squares[26], 0);
        throwAwayMove = new Move(this.board.squares[1], this.board.squares[26], 0);
    }

    public float Search(int depth, int originalDepth, bool player, string color, float alpha, float beta)
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
                        return infinity;
                    else
                        return infinity;
                }
                else if (board.stalemate)
                    return 0;
                HashSet<int> pieceIndex = new HashSet<int>();
                pieceIndex = board.pieceIndex;
                return CalculatePos(legalMoves, pieceIndex);
            }
            else if (color == "BLACK")
            {
                if (board.checkmate)
                {
                    if (player)
                        return -infinity;
                    else
                        return infinity;
                }
                else if (board.stalemate)
                    return 0;
                HashSet<int> pieceIndex = new HashSet<int>();
                pieceIndex = board.pieceIndex;
                return -CalculatePos(legalMoves, pieceIndex);
            }
        }

        Move currentBestMove = new Move(this.board.squares[1], this.board.squares[26], 0);
        if (player)
        {
            float max = -infinity;
            float bestValueMove = -infinity;
            foreach (Move move in legalMoves)
            {
                // Make the move
                board.MakeMove(move);

                // Recursively check next moves in line
                float currentEval = Search(depth - 1, originalDepth, false, color, alpha, beta); // infinity
                board.UnmakeMove(move);
                max = Mathf.Max(max, currentEval); // 

                if (max != infinity)
                {
                    alpha = Mathf.Max(alpha, max);

                    if (alpha >= beta)
                        break;
                }
                if (max > bestValueMove)
                {
                    bestValueMove = max;
                    currentBestMove = move;
                }
            }
            if (depth < originalDepth) return max;
        }
        else
        {
            float min = infinity;
            float bestValueMove = infinity;
            foreach (Move move in legalMoves)
            {
                // Make the move -- not the problem
                board.MakeMove(move);

                // Recursively check next moves in line
                float currentEval = Search(depth - 1, originalDepth, true, color, alpha, beta);
                board.UnmakeMove(move);

                min = Mathf.Min(min, currentEval);
                if (min != -infinity)
                {
                    // Alpha Beta pruning -- not the problem.
                    beta = Mathf.Min(beta, min);

                    if (beta <= alpha)
                        break;
                }
                // Can't be this
                if (min < bestValueMove)
                {
                    bestValueMove = min;
                    currentBestMove = move;
                }
            }
            if (depth < originalDepth) return min;
        }
        // if (bestMove.Equals(throwAwayMove))
        // {
        //     Debug.Log("Something went Wrong");
        //     return (legalMoves[0], 0f);
        // }
        bestMove = currentBestMove;
        board.GetLegalMoves();
        return 0f;
    }

    public Move GetBestMove(int depth, int originalDepth, bool player, string color, float alpha, float beta)
    {
        Search(depth, originalDepth, player, color, alpha, beta);
        return bestMove;
    }

    public float CalculatePos(Move[] legalMoves, HashSet<int> pieceIndex)
    {
        float sum = 0;
        foreach (int i in pieceIndex)
        {
            try
            {
                if (Char.IsLower(board.squares[i].piece))
                    sum += pieceVal[board.squares[i].piece] - positionalVal[board.squares[i].piece][board.GetIndex(board.squares[i].position)];
                else
                    sum += pieceVal[board.squares[i].piece] + positionalVal[board.squares[i].piece][board.GetIndex(board.squares[i].position)];
            }
            catch (Exception e)
            {
                throw new Exception("The given key was not found in the dictionary: " + i + ". Error: " + e.Message);
            }
        }
        return sum;
    }


}
