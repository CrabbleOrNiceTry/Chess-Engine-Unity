using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AI : MonoBehaviour
{
    private Board board;
    private int depth;
    private bool broken;

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
        broken = false;

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
        // this.GameManager.instance.board = GameManager.instance.board;
        // This exists only to satisfy the tuple return
        bestMove = new Move(GameManager.instance.board.squares[1], GameManager.instance.board.squares[26], 0);
        throwAwayMove = new Move(GameManager.instance.board.squares[1], GameManager.instance.board.squares[26], 0);
    }

    public float Search(int depth, int originalDepth, bool player, string color, float alpha, float beta)
    {
        Move[] legalMoves = GameManager.instance.board.GetLegalMoves();

        if (depth == 0 || legalMoves.Length == 0)
        {

            if (color == "WHITE")
            {
                if (GameManager.instance.board.checkmate)
                {
                    Debug.Log("Found Mate");
                    if (player)
                        return -infinity;
                    else
                        return infinity;
                }
                else if (GameManager.instance.board.stalemate)
                    return 0;
                HashSet<int> pieceIndex = GameManager.instance.board.pieceIndex;
                return CalculatePos();
            }
            else if (color == "BLACK")
            {
                if (GameManager.instance.board.checkmate)
                {
                    if (player)
                    {
                        return -infinity;
                    }
                    else
                    {
                        return infinity;
                    }
                }
                else if (GameManager.instance.board.stalemate)
                    return 0;
                return -CalculatePos();
            }
        }

        Move currentBestMove = new Move(GameManager.instance.board.squares[1], GameManager.instance.board.squares[26], 0);
        if (player)
        {
            float value = -infinity;
            float bestValueMove = -infinity;
            foreach (Move move in legalMoves)
            {
                // Make the move
                GameManager.instance.board.MakeMove(move);

                // Recursively check next moves in line
                float currentEval = Search(depth - 1, originalDepth, false, color, alpha, beta); // infinity
                GameManager.instance.board.UnmakeMove(move);
                if (broken)
                {
                    GameManager.instance.board.PrintBoard("Assets/Resources/BrokenBoardDepth" + depth + ".txt");
                    return 1;
                }
                value = Mathf.Max(value, currentEval); // 

                if (value >= beta)
                    break;

                alpha = Mathf.Max(alpha, value);

                if (value > bestValueMove)
                {
                    bestValueMove = value;
                    currentBestMove = move;
                }
            }
            if (depth < originalDepth) return value;
        }
        else
        {
            float value = infinity;
            float bestValueMove = infinity;
            foreach (Move move in legalMoves)
            {
                // Make the move -- not the problem
                GameManager.instance.board.MakeMove(move);

                // Recursively check next moves in line
                float currentEval = Search(depth - 1, originalDepth, true, color, alpha, beta);

                GameManager.instance.board.UnmakeMove(move);
                if (broken)
                {
                    GameManager.instance.board.PrintBoard("Assets/Resources/BrokenBoardDepth" + depth + ".txt");
                    return 1;
                }
                value = Mathf.Min(value, currentEval);
                // Alpha Beta pruning -- not the problem.

                if (value <= alpha)
                    break;

                beta = Mathf.Min(beta, value);

                // Can't be this
                if (value < bestValueMove)
                {
                    bestValueMove = value;
                    currentBestMove = move;
                }
            }
            if (depth < originalDepth) return value;
        }
        // if (bestMove.Equals(throwAwayMove))
        // {
        //     Debug.Log("Something went Wrong");
        //     return (legalMoves[0], 0f);
        // }
        bestMove = currentBestMove;
        GameManager.instance.board.GetLegalMoves();
        return 0f;
    }

    public Move GetBestMove(int depth, int originalDepth, bool player, string color, float alpha, float beta)
    {
        Search(depth, originalDepth, player, color, alpha, beta);
        return bestMove;
    }

    public float CalculatePos()
    {
        float sum = 0;
        foreach (int i in GameManager.instance.board.pieceIndex)
        {
            try
            {
                if (Char.IsLower(GameManager.instance.board.squares[i].piece))
                    sum += pieceVal[GameManager.instance.board.squares[i].piece] - positionalVal[GameManager.instance.board.squares[i].piece][i];
                else
                    sum += pieceVal[GameManager.instance.board.squares[i].piece] + positionalVal[GameManager.instance.board.squares[i].piece][i];
            }
            catch (Exception e)
            {
                var f = GameManager.instance.board.squares;
                var n = GameManager.instance.board.whiteKingIndex;
                GameManager.instance.board.PrintBoard("Assets/Resources/CalcPosErrorBoard.txt");
                Debug.Log("The given key was not found in the dictionary: " + i + ". Piece: " + GameManager.instance.board.squares[i].piece + ". Error: " + e.Message);
                broken = true;
                return 1;
            }
        }
        return sum;
    }


}
