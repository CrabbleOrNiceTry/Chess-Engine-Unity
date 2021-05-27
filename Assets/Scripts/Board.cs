using System.Collections;

using System.Collections.Generic;

using System;

using UnityEngine;

public class Board : MonoBehaviour
{
    // {'P': 10, 'N': 35, 'B': 35, 'R': 52.5, 'Q': 100, 'K': 1000, 'p': -10, 'n': -35, 'b': -35, 'r': -52.5, 'q': -100, 'k': -1000}
    public Square[] squares;    
    public List<Square> squaresList;
    [HideInInspector]
    public string fen;
    // Whites turn if true
    public bool white;
    private int kingIndex;
    private int whiteKingIndex;
    private int blackKingIndex;
    private List<Move> legalMoves;

    private List<int> rookIndex;
    public bool stalemate;

    public bool checkmate;

    private Dictionary<char, float> pieceVal;
    // private HashSet<Square> lockedSquares;
    public HashSet<int> pieceIndex;



    public float sum;
    void Awake()
    {
        squares     = new Square[64];
        squaresList = new List<Square>();
        pieceVal    = new Dictionary<char, float>();
        pieceIndex  = new HashSet<int>();
        legalMoves  = new List<Move>();
        rookIndex   = new List<int>();
        white       = true;
        stalemate   = false;
        checkmate   = false;
        #region Piece Values
            pieceVal.Add('P', 10.0f);
            pieceVal.Add('N', 35.0f);
            pieceVal.Add('B', 35.0f);
            pieceVal.Add('R', 52.5f);
            pieceVal.Add('Q', 100.0f);
            pieceVal.Add('K', 1000.0f);
            pieceVal.Add('p', -10.0f);
            pieceVal.Add('n', -35.0f);
            pieceVal.Add('b', -35.0f);
            pieceVal.Add('r', -52.5f);
            pieceVal.Add('q', -100.0f);
            pieceVal.Add('k', -1000.0f);
        #endregion
    }

    void Start()
    {
        GetKingIndex();
    }


    public void AddSquare(Square square)
    {
        squaresList.Add(square);
        
    }

    public void ConvertSquareListToArray()
    {
        squares = squaresList.ToArray();
    }

    public int GetIndex(string position)
    {
        return ((( 8 - ((int)(position[1] - '0'))) * 8) + ((int)position[0] - 97));
    }

    private void GetKingIndex()
    {
        for (int i = 0; i < squares.Length; i++)
        {
            if (squares[i].piece.Equals("K"))
            {
                // 6
                whiteKingIndex = i;
            }
            else if (squares[i].piece.Equals("k"))
            {
                blackKingIndex = i;
            }
        }
    }
    /*
     * Returns a set of psuedo legal moves in string form such as e2e4
     * Returns a set of possibly illegal moves. These are then parsed
     * through and the illegal moves will be thrown out in 
     * the GetLegalMoves() function.
    */
    public void GetPsuedoLegalMoves()
    {
        pieceIndex.Clear();
        white = GameManager.instance.white;
        byte numericColor = (byte)(white ? 1 : 2);

        legalMoves = new List<Move>();
        
        string pawnPieceToLookFor = (white ? "P" : "p");
        string rookPieceToLookFor = (white ? "R" : "r");
        string knightPieceToLookFor = (white ? "N" : "n");
        string bishopPieceToLookFor = (white ? "B" : "b");
        string kingPieceToLookFor = (white ? "K" : "k");
        string queenPieceToLookFor = (white ? "Q" : "q");
        

        kingIndex = -1;

        for (int i = 0; i < squares.Length; i++)
        {
            if (squares[i].piece.Equals("")) continue;
            pieceIndex.Add(i);
            if (squares[i].piece.Equals(pawnPieceToLookFor))
            {
                // 8
                GetPawnMoves(squares[i], i);
            }
            else if (squares[i].piece.Equals(knightPieceToLookFor))
            {  
                // 4
                GetKnightMoves(squares[i], i);
            } 
            else if (squares[i].piece.Equals(queenPieceToLookFor))
            {
                // 3
                GetQueenMoves(squares[i], i);
            }
            else if (squares[i].piece.Equals(bishopPieceToLookFor))
            {
                // 8
                GetBishopMoves(squares[i], i);
            }
            else if (squares[i].piece.Equals(rookPieceToLookFor))
            {
                // 10
                GetRookMoves(squares[i], i);
                rookIndex.Add(i);
            }
            else if (squares[i].piece.Equals(kingPieceToLookFor))
            {
                // 6
                kingIndex = i;
                GetKingMoves(squares[i], i);
            }

        }

        // GetCastleMove(squares[kingIndex], kingIndex);


        if (kingIndex == -1)
            Debug.Log(("No King Piece Found {0}", white.ToString()));
    }

    /*
     * Parses through the psuedo legal moves and 
     * throws out the illegal moves
     * Returns a set of proper legal moves.
    */ 
    public Move[] GetLegalMoves()
    {
        GetPsuedoLegalMoves();
        
        stalemate = false;
        checkmate = false;
        // Check if statemate
        stalemate = (legalMoves.Count == 0);
        if (stalemate) return legalMoves.ToArray();

        int tempKingIndex = kingIndex;

        // float time = Time.realtimeSinceStartup;
        for (int i = legalMoves.Count - 1; i > -1; i--)
        {
            if (legalMoves[i].isCastle) continue;
            // Check if the move being checked is a king move, if so
            // change king index to new square.
            if (legalMoves[i].original.piece.ToUpper().Equals("K"))
            {
                tempKingIndex = legalMoves[i].index;
            }
            else
            {
                tempKingIndex = kingIndex;
            }

            // Check if the move is in a checked position. If so
            // remove the move from the legal moves list.
            if (!legalMoves[i].original.piece.ToUpper().Equals("K") && legalMoves[i].newSquare.piece.ToUpper().Equals("K"))
            {
                legalMoves.RemoveAt(i);
                continue;
            }
            MakeMove(legalMoves[i]);
            if (IsInCheck(squares[tempKingIndex], tempKingIndex))   
            {
                // Debug.Log("Found check at " + legalMoves[i].ToString());
                UnmakeMove(legalMoves[i]);
                legalMoves.RemoveAt(i);
                continue;
            }
            UnmakeMove(legalMoves[i]);
        }
        // time = Time.realtimeSinceStartup - time;
        // Debug.Log(time);
        
        // After parsing all illegal moves there were no possible moves for player
        // meaning he is in check and that means mate.
        if (legalMoves.Count == 0) 
        {
            checkmate = true;
        }
        
        GetCastleMove();

        return legalMoves.ToArray();
    }

    private void AddToSum(string piece)
    {
        sum += pieceVal[piece[0]];
    }

    private bool IsInCheck(Square square, int i)
    {
        return (KingDiagnolCheck(square, i) || KingVerticalCheck(square, i) || KingHorizontalCheck(square, i) || KingKnightCheck(square, i) || KingPawnCheck(square, i));
    }

    public void MakeMove(Move move)
    {
        if (move.isCastle)
        {
            if (move.castle.Equals("BLACK-QUEEN"))
            {
                squares[0].piece = "";
                squares[3].piece = "r";
                squares[2].piece = "k";
                squares[4].piece = "";
            }
            else if (move.castle.Equals("BLACK-KING"))
            {
                squares[7].piece = "";
                squares[5].piece = "r";
                squares[6].piece = "k";
                squares[4].piece = "";
            }
            else if (move.castle.Equals("WHITE-QUEEN"))
            {
                squares[56].piece = "";
                squares[59].piece = "R";
                squares[58].piece = "K";
                squares[60].piece = "";
            }
            else if (move.castle.Equals("WHITE-KING"))
            {
                squares[63].piece = "";
                squares[61].piece = "R";
                squares[62].piece = "K";
                squares[60].piece = "";
            }
            GameManager.instance.white = !GameManager.instance.white;
            return;
        }

        if (move.newSquare.piece.ToUpper().Equals("K") && !move.original.piece.ToUpper().Equals("K"))
            Debug.Log("king taken");
        if (move.original.piece.ToUpper().Equals("P"))
        {
            if (move.newSquare.position[1] == '8' || move.newSquare.position[1] == '1')
            {
                if (Char.IsLower(move.original.piece, 0))
                    move.newSquare.piece = "q";
                else
                    move.newSquare.piece = "Q";
                move.original.piece = "";
                // temp += "+";
                move.pawnPromote = true;
                GameManager.instance.white = !GameManager.instance.white;
                return;
            }
        }
        move.newSquare.piece = move.original.piece;   
        move.original.piece = "";
        GameManager.instance.white = !GameManager.instance.white;
        // return temp;
    }

    public void UnmakeMove(Move move)
    {
        if (move.isCastle)
        {
            if (move.castle.Equals("BLACK-QUEEN"))
            {
                squares[0].piece = "r";
                squares[3].piece = "";
                squares[2].piece = "";
                squares[4].piece = "k";
            }
            else if (move.castle.Equals("BLACK-KING"))
            {
                squares[7].piece = "r";
                squares[5].piece = "";
                squares[6].piece = "";
                squares[4].piece = "k";
            }
            else if (move.castle.Equals("WHITE-QUEEN"))
            {
                squares[56].piece = "R";
                squares[59].piece = "";
                squares[58].piece = "";
                squares[60].piece = "K";
            }
            else if (move.castle.Equals("WHITE-KING"))
            {
                squares[63].piece = "R";
                squares[61].piece = "";
                squares[62].piece = "";
                squares[60].piece = "K";
            }
            GameManager.instance.white = !GameManager.instance.white;
            return;
        }


        if (move.pawnPromote)
        {
            if (Char.IsLower(move.newSquare.piece, 0))
            {
                move.original.piece = "p";
            }
            else
            {
                move.original.piece = "P";     
            }
            move.newSquare.piece = move.pieceNew;
            move.pawnPromote = false;
        }
        else
        {
            move.original.piece = move.pieceOriginal;
            move.newSquare.piece = move.pieceNew;
        }
        GameManager.instance.white = !GameManager.instance.white;
    }
    #region Castle Stuff

    /* 
     * Checks black queen side castle availability
     * Adds legal move is available
     */
    private void CheckBlackQueenSideCastle()
    {
        if (squares[0].pieceMoved == false && squares[4].pieceMoved == false)
        {
            // Now check if their are any pieces in the way.
            for (int i = 1; i < 4; i++)
            {
                if (!squares[i].piece.Equals(""))
                {
                    return;
                }
            }
            if(!IsInCheck(squares[4], 4))
                legalMoves.Add(new Move(squares[4], squares[2], -1, isCastle: true, castle: "BLACK-QUEEN"));
        }
    }   

    /* 
     * Checks black king side castle availability
     * Adds legal move is available
     */
    private void CheckBlackKingSideCastle()
    {
        if (squares[7].pieceMoved == false && squares[4].pieceMoved == false)
        {
            // Now check if their are any pieces in the way.
            for (int i = 6; i > 4; i++)
            {
                if (!squares[i].piece.Equals(""))
                {
                    return;
                }
            }
            if (!IsInCheck(squares[4], 4))
                legalMoves.Add(new Move(squares[4], squares[6], -1, isCastle: true, castle: "BLACK-KING"));
        }
    }     

    /* 
     * Checks white queen side castle availability
     * Adds legal move is available
     */
    private void CheckWhiteQueenSideCastle()
    {
        if (squares[56].pieceMoved == false && squares[60].pieceMoved == false)
        {
            // Now check if their are any pieces in the way.
            for (int i = 57; i < 60; i++)
            {
                if (!squares[i].piece.Equals(""))
                {
                    return;
                }
            }
            if (!IsInCheck(squares[60], 60))
                legalMoves.Add(new Move(squares[60], squares[58], -1, isCastle: true, castle: "WHITE-QUEEN"));
        }
    }   

    /* 
     * Checks white king side castle availability
     * Adds legal move is available
     */
    private void CheckWhiteKingSideCastle()
    {
        if (squares[63].pieceMoved == false && squares[60].pieceMoved == false)
        {
            // Now check if their are any pieces in the way.
            for (int i = 61; i < 63; i++)
            {
                if (!squares[i].piece.Equals(""))
                {
                    return;
                }
            }
            // We do this last because this is the longest operation.
            if (!IsInCheck(squares[60], 60))
            {
                legalMoves.Add(new Move(squares[60], squares[62], -1, isCastle: true, castle: "WHITE-KING"));
            }
        }
    }     

    /* 
     * Uppercase is white pieces. Lowercase is black pieces.
     * If the pieces are on the correct spots and the pieces both have not moved and the king is not in check.
     * Add the move as valid. 
    */ 
    private void GetCastleMove()
    {
        CheckWhiteKingSideCastle();
        CheckBlackKingSideCastle();
        CheckWhiteQueenSideCastle();
        CheckBlackQueenSideCastle();

    }

    #endregion

    private void GetKingMoves(Square square, int i)
    {
        int[] offset = King.GetOffset(square);

        foreach (int j in offset)
        {
            if (i + j > 63 || i + j < 0) continue;
            if (squares[i + j].piece.Equals("") || Char.IsUpper(squares[i + j].piece[0]) != white) legalMoves.Add(new Move(square, squares[i + j], i + j));
        }
    }

    private void GetPawnMoves(Square square, int i)
    {
        
        int mult = (white ? -1 : 1);
        if (square.position[1] == '8' || square.position[1] == '1')
        {  
            if (Char.IsLower(square.piece, 0))
                square.piece = "q";
            else
                square.piece = "Q";
            GetQueenMoves(square, i);
            return;
        }
        if (white
            && square.position[1] == '2'
            && squares[i + (16 * mult)].piece.Equals("")
            && squares[i + (8 * mult)].piece.Equals(""))
        {
            legalMoves.Add(new Move(square, squares[i + (16 * mult)], i + (16 * mult)));
        }
        else if (!white
            && square.position[1] == '7'
            && squares[i + (16 * mult)].piece.Equals("")
            && squares[i + (8 * mult)].piece.Equals(""))
        {
            legalMoves.Add(new Move(square, squares[i + (16 * mult)], i + (16 * mult)));
        }
        if (squares[i + (8 * mult)].piece.Equals(""))
        {
            legalMoves.Add(new Move(square, squares[i + (8 * mult)], i + (16 * mult)));
        }
        if (square.position[0] == 'a' && !squares[i + (7 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (7 * mult)].piece[0]) != white)
        {
            legalMoves.Add(new Move(square, squares[i + (7 * mult)], i + (16 * mult)));
            return;
        }
        else if (square.position[0] == 'h' && !squares[i + (7 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (7 * mult)].piece[0]) != white)
        {
            legalMoves.Add(new Move(square, squares[i + (9 * mult)], i + (16 * mult)));
            return;
        }
        if (square.position[0] == 'h' || square.position[0] == 'a') return;
        if (!squares[i + (9 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (9 * mult)].piece[0]) != white)
        {
            legalMoves.Add(new Move(square, squares[i + (9 * mult)], i + (16 * mult)));
        }
        if (!squares[i + (7 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (7 * mult)].piece[0]) != white)
        {
            legalMoves.Add(new Move(square , squares[i + (7 * mult)], i + (16 * mult)));
        }
    }

    private void GetKnightMoves(Square square, int i)
    {
        int[] offset = Knight.GetOffset(square.position);
       
        for (int j = 0; j < offset.Length; j++)
        {
            int index = i + offset[j];

            if (index > 63 || index < 0) continue;

            if (squares[index].piece.Equals("")
                || Char.IsUpper(squares[index].piece[0]) != white)
            {
                legalMoves.Add(new Move(square, squares[index], index));
            }
        }
    }

    private void GetQueenMoves(Square square, int i)
    {
        GetHorizontalMoves(square, i);
        GetVerticalMoves(square, i);
        GetDiagnolMoves(square, i);
    }
    
    private void GetBishopMoves(Square square, int i)
    {
        GetDiagnolMoves(square, i);
    }

    private void GetRookMoves(Square square, int i)
    {
        GetHorizontalMoves(square, i);
        GetVerticalMoves(square, i);
    }

    private bool GetHorizontalMoves(Square square, int i)
    {
        int[] offset;
        
        offset = Rook.GetHorizontalOffset(square);
        
        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 0; j < 8; j++)
            {   
                int index = i + ((j + 1) * offset[k]);

                if (index < 0 || index > 63) break;

                if (squares[index].position[0] == 'h' || squares[index].position[0] == 'a')
                {
                    if (squares[index].piece.Equals("") || Char.IsUpper(squares[index].piece[0]) != white)
                    {
                        legalMoves.Add(new Move(square, squares[index], index));
                    }
                    break;
                }
                if (squares[index].piece.Equals("")) // square is empty add to possible moves
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                }
                else if (Char.IsUpper(squares[index].piece[0]) != white) // Square ContainsKey enemy piece, piece can be taken then break 
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                    break;
                }
                else // Friendly piece is obstructing path break
                {
                    break;
                }
            }
        }
        return true;
    }
    
    private bool GetVerticalMoves(Square square, int i)
    {
        int[] offset = new int[]{8, -8};
        
        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 1; j < 9; j++)
            {   
                int index = i + (j * offset[k]);
                if (index < 0 || index > 63) break;
                if (squares[index].piece.Equals(""))
                {
                    legalMoves.Add(new Move(square, squares[index], i));
                }
                else if (Char.IsUpper(squares[index].piece[0]) != white)
                {
                    legalMoves.Add(new Move(square, squares[index], i));
                    break;
                }
                else
                {
                    break;
                }
            }
        }
        return true;
    }

    private bool GetDiagnolMoves(Square square, int i)
    {
        int[] offset;
        bool stopAtH = false;
        bool stopAtA = false; 
        if (square.position[0] == 'a')
        {
            stopAtH = true;
            offset = new int[]{9, -7};
        }
        else if (square.position[0] == 'h')
        {
            stopAtA = true;
            offset = new int[]{7, -9};  
        }
        else
        {
            stopAtA = true;
            stopAtH = true;
            offset = new int[]{9, -9, 7, -7};
        }


        
        for (int k = 0; k < offset.Length; k++)
        {
            // bool isPossibleCheck = IsPossibleCheck(i, offset[k]);

            // bool pathBlocked = false;
            for (int j = 1; j < 9; j++)
            {   
                int index = i + (j * offset[k]);

                if (index < 0 || index > 63) break;
                
                // if (isPossibleCheck)
                // {
                //     if (squares[index].piece.Equals("") && !pathBlocked)
                //     {   
                //         legalMoves.Add(new Move(square, squares[index], index));
                //     }
                //     else if (Char.IsUpper(squares[index].piece[0]) != white && !pathBlocked)
                //     {
                //         legalMoves.Add(new Move(square, squares[index], index));
                //         pathBlocked = true;
                //     }
                //     else
                //     {
                //         break;
                //     }
                //     continue;
                // }

                if (squares[index].piece.Equals(""))
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                }
                else if (Char.IsUpper(squares[index].piece[0]) != white)
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                    break;
                }
                else
                {
                    break;
                }

                if (stopAtA && squares[index].position[0] == 'a')
                    break;
                else if (stopAtH && squares[index].position[0] == 'h')
                    break;
            }
        }
        return true;
    }

    // returns if a king and the piece are in the same path
    // meaning king may be in check 
    // Will not work for checking horizontal because offset is 1, -1
    private bool IsPossibleCheck(int i, int offset)
    {   
        int enemyKingIndex = (white ? blackKingIndex : whiteKingIndex);
        
        if (enemyKingIndex % offset == i % offset)
            return true;
        return false;
    }

    private bool KingHorizontalCheck(Square square, int i)
    {
        int[] offset;
        
        offset = Rook.GetHorizontalOffset(square);
        
        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 0; j < 8; j++)
            {   
                int index = i + ((j + 1) * offset[k]);
                
                if (index < 0 || index > 63) break;
                if (squares[index].piece.Equals("")) continue;

                string piece = ("" + squares[index].piece[0] + "").ToUpper();
                if (!squares[index].piece.Equals("") && Char.IsUpper(squares[index].piece[0]) == white) // Freindly Piece
                {
                    break;
                }
                else if (squares[index].position[0] == 'h')
                {
                    if (Char.IsUpper(squares[index].piece[0]) != white && piece.Equals("Q") || piece.Equals("R"))
                    {
                        return true;
                    }
                    if (offset[k] == 1) break;
                }
                else if (squares[index].position[0] == 'a')
                {
                    if (Char.IsUpper(squares[index].piece[0]) != white && piece.Equals("Q") || piece.Equals("R"))
                    {
                        return true;
                    }
                    if (offset[k] == -1) break;
                }
                else if (!squares[index].piece.Equals("") && Char.IsUpper(squares[index].piece[0]) != white) // Square ContainsKey enemy piece, piece can be taken then break 
                {
                    if (piece.Equals("R") || piece.Equals("Q"))
                    {
                        return true;
                    }
                    break;
                }
            }
        }
        return false;
    }
    
    private bool KingVerticalCheck(Square square, int i)
    {
        int[] offset = new int[]{8, -8};
        
        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 1; j < 9; j++)
            {   
                int index = i + (j * offset[k]);

                if (index < 0 || i + (j * offset[k]) > 63) break;
                if (squares[i + (j * offset[k])].piece.Equals("")) continue;

                string piece = ("" + squares[i + (j * offset[k])].piece[0]).ToUpper();

                if (!squares[i + (j * offset[k])].piece.Equals("") && Char.IsUpper(squares[i + (j * offset[k])].piece[0]) == white) // Friendly Piece
                {
                    break;
                }
                else if (!squares[i + (j * offset[k])].piece.Equals("") && Char.IsUpper(squares[i + (j * offset[k])].piece[0]) != white) // Enemy Piece
                {
                    if (piece.Equals("R") || piece.Equals("Q"))
                    {
                        return true;
                    }
                    break;
                }
            }
        }
        return false;
    }

    private bool KingDiagnolCheck(Square square, int i)
    {
        int[] offset;

        bool stopAtH = false;
        bool stopAtA = false; 
        if (square.position[0] == 'a')
        {
            stopAtH = true;
            offset = new int[]{9, -7};
        }
        else if (square.position[0] == 'h')
        {
            stopAtA = true;
            offset = new int[]{7, -9};  
        }
        else
        {
            stopAtA = true;
            stopAtH = true;
            offset = new int[]{9, -9, 7, -7};
        }

        
        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 1; j < 9; j++)
            {   
                if (i + (j * offset[k]) < 0 || i + (j * offset[k]) > 63) { break;}
                if (squares[i + (j * offset[k])].piece.Equals("")) 
                {
                    if (stopAtA && squares[i + (j * offset[k])].position[0] == 'a')
                        break;
                    else if (stopAtH && squares[i + (j * offset[k])].position[0] == 'h')
                        break;
                    continue;
                }
                string piece = ("" + squares[i + (j * offset[k])].piece).ToUpper();

                if (!squares[i + (j * offset[k])].piece.Equals("") && Char.IsUpper(squares[i + (j * offset[k])].piece[0]) == white) // Freindly Piece
                {
                    // Debug.Log((("broke off from {0}", offset[k]) ));
                    break;
                }
                else if (!squares[i + (j * offset[k])].piece.Equals("") && Char.IsUpper(squares[i + (j * offset[k])].piece[0]) != white) // Enemy Piece
                {
                    if (piece.Equals("B") || piece.Equals("Q")) 
                    {
                        // Debug.Log(String.Format("Found diagnol check with offset {0}, from {1}, kingIndex: {2}", offset[k], (i + (j * offset[k])), i));
                        return true;
                    }
                    break;
                }
            }
        }
        return false;
    }

    private bool KingPawnCheck(Square square, int i )
    {
        int[] offset;
        if (white)
            offset = new int[] {-7, -9};
        else
            offset = new int[] {7, 9};

        foreach (int j in offset)
        {
            if (i + j > 63 || i + j < 0) continue;
            if (squares[i + j].piece.ToUpper().Equals("P") && Char.IsUpper(squares[i + j].piece[0]) != white)
            {
                // Debug.Log("Found pawn check at " + square.position + squares[i + j].position);
                return true;
            }
        }
        return false;
    }

    private bool KingKnightCheck(Square square, int i)
    {
        int[] offset = Knight.GetOffset(square.position);
        foreach (int j in offset)
        {
            if (i + j > 63 || i + j < 0) continue;
            if (squares[i + j].piece.ToUpper().Equals("N") && Char.IsUpper(squares[i + j].piece[0]) != white)
            {
                return true;
            }
        }
        return false;
    }

    
}
