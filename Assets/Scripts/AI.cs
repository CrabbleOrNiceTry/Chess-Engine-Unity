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

    private Dictionary<PieceE, float> pieceVal;
    private Dictionary<PieceE, float[]> positionalVal;
    private System.Diagnostics.Stopwatch stopWatch;
    private int nodesSearched;
    private float infinity;


    public AI()
    {
        infinity = 999999f;
        pieceVal = new Dictionary<PieceE, float>();
        positionalVal = new Dictionary<PieceE, float[]>();
        broken = false;
        stopWatch = new System.Diagnostics.Stopwatch();

        #region Piece Values
        pieceVal.Add((PieceE.Pawn | PieceE.White), 10.0f);
        pieceVal.Add((PieceE.Knight | PieceE.White), 35.0f);
        pieceVal.Add((PieceE.Bishop | PieceE.White), 35.0f);
        pieceVal.Add((PieceE.Rook | PieceE.White), 52.5f);
        pieceVal.Add((PieceE.Queen | PieceE.White), 100.0f);
        pieceVal.Add((PieceE.King | PieceE.White), 100000.0f);
        pieceVal.Add((PieceE.Pawn | PieceE.Black), -10.0f);
        pieceVal.Add((PieceE.Knight | PieceE.Black), -35.0f);
        pieceVal.Add((PieceE.Bishop | PieceE.Black), -35.0f);
        pieceVal.Add((PieceE.Rook | PieceE.Black), -52.5f);
        pieceVal.Add((PieceE.Queen | PieceE.Black), -100.0f);
        pieceVal.Add((PieceE.King | PieceE.Black), -100000.0f);
        #endregion

        #region Positional Piece Values
        positionalVal.Add((PieceE.Pawn | PieceE.White), new float[] {0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,
                                            5.0f,  5.0f,  5.0f,  5.0f,  5.0f,  5.0f,  5.0f,  5.0f,
                                            1.0f,  1.0f,  2.0f,  3.0f,  3.0f,  2.0f,  1.0f,  1.0f,
                                            0.5f,  0.5f,  1.0f,  2.5f,  2.5f,  1.0f,  0.5f,  0.5f,
                                            0.0f,  0.0f,  0.0f,  2.0f,  2.0f,  0.0f,  0.0f,  0.0f,
                                            0.5f, -0.5f, -1.0f,  0.0f,  0.0f, -1.0f, -0.5f,  0.5f,
                                            0.5f,  1.0f, 1.0f,  -2.0f, -2.0f,  1.0f,  1.0f,  0.5f,
                                            0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f});
        positionalVal.Add((PieceE.Knight | PieceE.White), new float[] {-5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f,
                                            -4.0f, -2.0f,  0.0f,  0.0f,  0.0f,  0.0f, -2.0f, -4.0f,
                                            -3.0f,  0.0f,  1.0f,  1.5f,  1.5f,  1.0f,  0.0f, -3.0f,
                                            -3.0f,  0.5f,  1.5f,  2.0f,  2.0f,  1.5f,  0.5f, -3.0f,
                                            -3.0f,  0.0f,  1.5f,  2.0f,  2.0f,  1.5f,  0.0f, -3.0f,
                                            -3.0f,  0.5f,  1.0f,  1.5f,  1.5f,  1.0f,  0.5f, -3.0f,
                                            -4.0f, -2.0f,  0.0f,  0.5f,  0.5f,  0.0f, -2.0f, -4.0f,
                                            -5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f });
        positionalVal.Add((PieceE.Bishop | PieceE.White), new float[] {-2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f,
                                        -1.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f, -1.0f,
                                            -1.0f,  0.0f,  0.5f,  1.0f,  1.0f,  0.5f,  0.0f, -1.0f,
                                            -1.0f,  0.5f,  0.5f,  1.0f,  1.0f,  0.5f,  0.5f, -1.0f,
                                            -1.0f,  0.0f,  1.0f,  1.0f,  1.0f,  1.0f,  0.0f, -1.0f,
                                            -1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.0f, -1.0f,
                                            -1.0f,  0.5f,  0.0f,  0.0f,  0.0f,  0.0f,  0.5f, -1.0f,
                                            -2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f });
        positionalVal.Add((PieceE.Rook | PieceE.White), new float[] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,  0.0f,
                                             0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,  0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                             0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.0f,  0.0f});
        positionalVal.Add((PieceE.Queen | PieceE.White), new float[] {-2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f,
                                            -1.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f,  0.0f, -1.0f,
                                            -1.0f,  0.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -1.0f,
                                            -0.5f,  0.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -0.5f,
                                            -0.5f,  0.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -0.5f,
                                            -1.0f,  0.5f,  0.5f,  0.5f,  0.5f,  0.5f,  0.0f, -1.0f,
                                            -1.0f,  0.0f,  0.5f,  0.0f,  0.0f,  0.0f,  0.0f, -1.0f,
                                            -2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f});
        positionalVal.Add((PieceE.King | PieceE.White), new float[] {-3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -3.0f, -4.0f, -4.0f, -5.0f, -5.0f, -4.0f, -4.0f, -3.0f,
                                             -2.0f, -3.0f, -3.0f, -4.0f, -4.0f, -3.0f, -3.0f, -2.0f,
                                             -1.0f, -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, -1.0f,
                                              2.0f,  2.0f,  0.0f,  0.0f,  0.0f,  0.0f,  2.0f,  2.0f,
                                              2.0f,  3.0f,  1.0f,  0.0f,  0.0f,  1.0f,  3.0f,  2.0f });
        positionalVal.Add((PieceE.Pawn | PieceE.Black), new float[] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                                            0.5f, 1.0f, 1.0f, -2.0f, -2.0f, 1.0f, 1.0f, 0.5f,
                                            0.5f, -0.5f, -1.0f, 0.0f, 0.0f, -1.0f, -0.5f, 0.5f,
                                            0.0f, 0.0f, 0.0f, 2.0f, 2.0f, 0.0f, 0.0f, 0.0f,
                                            0.5f, 0.5f, 1.0f, 2.5f, 2.5f, 1.0f, 0.5f, 0.5f,
                                            1.0f, 1.0f, 2.0f, 3.0f, 3.0f, 2.0f, 1.0f, 1.0f,
                                            5.0f, 5.0f, 5.0f, 5.0f, 5.0f, 5.0f, 5.0f, 5.0f,
                                            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f});
        positionalVal.Add((PieceE.Knight | PieceE.Black), new float[] {-5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f,
                                            -4.0f, -2.0f, 0.0f, 0.5f, 0.5f, 0.0f, -2.0f, -4.0f,
                                            -3.0f, 0.5f, 1.0f, 1.5f, 1.5f, 1.0f, 0.5f, -3.0f,
                                            -3.0f, 0.0f, 1.5f, 2.0f, 2.0f, 1.5f, 0.0f, -3.0f,
                                            -3.0f, 0.5f, 1.5f, 2.0f, 2.0f, 1.5f, 0.5f, -3.0f,
                                            -3.0f, 0.0f, 1.0f, 1.5f, 1.5f, 1.0f, 0.0f, -3.0f,
                                            -4.0f, -2.0f, 0.0f, 0.0f, 0.0f, 0.0f, -2.0f, -4.0f,
                                            -5.0f, -4.0f, -3.0f, -3.0f, -3.0f, -3.0f, -4.0f, -5.0f});
        positionalVal.Add((PieceE.Bishop | PieceE.Black), new float[] {-2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f,
                                            -1.0f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, -1.0f,
                                            -1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f,
                                            -1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, -1.0f,
                                            -1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 0.5f, 0.5f, -1.0f,
                                            -1.0f, 0.0f, 0.5f, 1.0f, 1.0f, 0.5f, 0.0f, -1.0f,
                                            -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f,
                                            -2.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -2.0f});
        positionalVal.Add((PieceE.Rook | PieceE.Black), new float[] {0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.0f, 0.0f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -0.5f,
                                            0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f,
                                            0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f});
        positionalVal.Add((PieceE.Queen | PieceE.Black), new float[] {-2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f,
                                            -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f, -1.0f,
                                            -1.0f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, -1.0f,
                                            -0.5f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, -0.5f,
                                            -0.5f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, -0.5f,
                                            -1.0f, 0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, -1.0f,
                                            -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f,
                                            -2.0f, -1.0f, -1.0f, -0.5f, -0.5f, -1.0f, -1.0f, -2.0f});
        positionalVal.Add((PieceE.King | PieceE.Black), new float[] {2.0f, 3.0f, 1.0f, 0.0f, 0.0f, 1.0f, 3.0f, 2.0f,
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
        nodesSearched = 0;
        stopWatch.Reset();
    }

    private float BaseCase(string color, bool player)
    {
        if (color == "WHITE")
        {
            if (GameManager.instance.board.checkmate)
            {
                Debug.Log("Found Mate");
                if (player)
                    return infinity;
                else
                    return -infinity;
            }
            else if (GameManager.instance.board.stalemate)
                return 0;
            return CalculatePos();
        }
        else
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

    public float Search(int depth, int originalDepth, bool player, string color, float alpha, float beta)
    {
        // stopWatch.Start();
        Move[] legalMoves = GameManager.instance.board.GetLegalMoves();
        // stopWatch.Stop();
        if (depth == 0 || legalMoves.Length == 0)
        {
            return BaseCase(color, player);
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
                nodesSearched++;
                GameManager.instance.board.UnmakeMove(move);
                // if (broken)
                // {
                //     GameManager.instance.board.PrintBoard("Assets/Resources/BrokenBoardDepth" + depth + ".txt");
                //     return 1;
                // }

                value = Mathf.Max(value, currentEval); // 
                if (value != infinity)
                {
                    if (value >= beta)
                        break;

                    alpha = Mathf.Max(alpha, value);
                }


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
                nodesSearched++;
                GameManager.instance.board.UnmakeMove(move);

                // if (broken)
                // {
                //     GameManager.instance.board.PrintBoard("Assets/Resources/BrokenBoardDepth" + depth + ".txt");
                //     return 1;
                // }

                value = Mathf.Min(value, currentEval);
                // Alpha Beta pruning -- not the problem.

                if (value != -infinity)
                {
                    if (value <= alpha)
                        break;

                    beta = Mathf.Min(beta, value);
                }


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
        return (float)nodesSearched;
    }

    public Move GetBestMove(int depth, int originalDepth, bool player, string color, float alpha, float beta)
    {
        System.Diagnostics.Stopwatch nodesSearchedTime = new System.Diagnostics.Stopwatch();
        nodesSearchedTime.Start();
        float nodesSearched = Search(depth, originalDepth, player, color, alpha, beta);
        nodesSearchedTime.Stop();
        Debug.Log("Currently timed functions took " + GameManager.instance.board.stopWatch.ElapsedMilliseconds +
                  "ms of a total search time of " + nodesSearchedTime.ElapsedMilliseconds + "ms. Approximately " + Convert.ToString((float)((float)GameManager.instance.board.stopWatch.ElapsedMilliseconds / (float)nodesSearchedTime.ElapsedMilliseconds) * 100) + "% of the total search time.");
        GameManager.instance.board.stopWatch.Reset();
        // Debug.Log(nodesSearched + " nodes searched in " + nodesSearchedTime.ElapsedMilliseconds + "ms.");
        return bestMove;
    }

    public float CalculatePos()
    {
        float sum = 0;
        foreach (int i in GameManager.instance.board.pieceIndex)
        {
            try
            {
                if (!PieceEnum.IsWhite(GameManager.instance.board.squares[i].piece))
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
