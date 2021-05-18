using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Board : MonoBehaviour
{
    // Start is called before the first frame update
    public Square[] squares;    
    public List<Square> squaresList;
    [HideInInspector]
    public string fen;
    // Whites turn if true
    public bool white;
    private int kingIndex;
    private List<Move> legalMoves;

    void Awake()
    {
        squares = new Square[64];
        squaresList = new List<Square>();
        white = true;
        legalMoves = new List<Move>();
    }

    public void AddSquare(Square square)
    {
        squaresList.Add(square);
    }

    public void ConvertSquareListToArray()
    {
        squares = squaresList.ToArray();
    }


    /*
     * Returns a set of psuedo legal moves in string form such as e2e4
     * Returns a set of possibly illegal moves. These are then parsed
     * through and the illegal moves will be thrown out in 
     * the GetLegalMoves() function.
    */
    public void GetPsuedoLegalMoves()
    {
        
        white = GameManager.instance.white;

        legalMoves = new List<Move>();

        string pawnPieceToLookFor = (white ? "p" : "P");
        string rookPieceToLookFor = (white ? "r" : "R");
        string knightPieceToLookFor = (white ? "n" : "N");
        string bishopPieceToLookFor = (white ? "b" : "B");
        string kingPieceToLookFor = (white ? "k" : "K");
        string queenPieceToLookFor = (white ? "q" : "Q");

        kingIndex = -1;

        for (int i = 0; i < squares.Length; i++)
        {
            if (squares[i].piece.Equals("")) continue;
            if (squares[i].piece.Equals(pawnPieceToLookFor))
            {
                GetPawnMoves(squares[i], i);
            }
            else if (squares[i].piece.Equals(knightPieceToLookFor))
            {  
                GetKnightMoves(squares[i], i);
            } 
            else if (squares[i].piece.Equals(queenPieceToLookFor))
            {
                GetQueenMoves(squares[i], i);
            }
            else if (squares[i].piece.Equals(bishopPieceToLookFor))
            {
                GetBishopMoves(squares[i], i);
            }
            else if (squares[i].piece.Equals(rookPieceToLookFor))
            {
                GetRookMoves(squares[i], i);
            }
            else if (squares[i].piece.Equals(kingPieceToLookFor))
            {
                kingIndex = i;
                GetKingMoves(squares[i], i);
            }
        }
    }

    /*
     * Parses through the psuedo legal moves and 
     * throws out the illegal moves
     * Returns a set of proper legal moves.
    */ 
    public Move[] GetLegalMoves()
    {
        GetPsuedoLegalMoves();
        for (int i = 0; i < legalMoves.Count; i++)
        {
            string newOriginalSquare = MakeMove(legalMoves[i]);
            if (IsInCheck(squares[kingIndex], kingIndex))
            {
                UnmakeMove(legalMoves[i], newOriginalSquare);
                Debug.Log("Removing: " + legalMoves[i].ToString());
                legalMoves.RemoveAt(i);
                i--;
                continue;
            }
            UnmakeMove(legalMoves[i], newOriginalSquare);
        }
        Debug.Log(legalMoves.Count);
        return legalMoves.ToArray();
    }

    private bool IsInCheck(Square square, int i)
    {
        if (KingDiagnolCheck(square, i) || KingVerticalCheck(square, i) || KingHorizontalCheck(square, i)) return true;
        return false;

    }

    private string MakeMove(Move move)
    {
        Square original = move.original;
        Square newSquare = move.newSquare;
        string temp = newSquare.piece;
        newSquare.piece = original.piece;
        return temp;
    }

    private void UnmakeMove(Move move, string originalStr)
    {
        Square original = move.original;
        Square newSquare = move.newSquare;
        original.piece = newSquare.piece;
        newSquare.piece = originalStr;
    }

    private void GetKingMoves(Square square, int i)
    {
        int[] offset = King.GetOffset(square);

        foreach (int j in offset)
        {
            if (i + j > 63 || i + j < 0) continue;
            if (squares[i + j].piece.Equals("") || Char.IsUpper(squares[i + j].piece[0]) == white) legalMoves.Add(new Move(square, squares[i + j]));
        }
    }

    private void GetPawnMoves(Square square, int i)
    {
        // Worst code I've ever written. kill me now. literally just a bunch of fucking if statements. IDEFC. Not sure how I would do pawn moves in a clean way.
        // mult is 1 if searching for black pawn moves, reason:
        // black pawns move up 8 on 1D chess array whereas white move down 8
        
        int mult = (white ? -1 : 1);
        if (white
            && square.position[1] == '2'
            && squares[i + (16 * mult)].piece.Equals(""))
        {
            legalMoves.Add(new Move(square, squares[i + (16 * mult)]));
        }
        else if (!white
            && square.position[1] == '7'
            && squares[i + (16 * mult)].piece.Equals(""))
        {
            legalMoves.Add(new Move(square, squares[i + (16 * mult)]));
        }
        if (squares[i + (8 * mult)].piece.Equals(""))
        {
            legalMoves.Add(new Move(square, squares[i + (8 * mult)]));
        }
        if (square.position[0] == 'a' && !squares[i + (7 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (7 * mult)].piece[0]) == white)
        {
            legalMoves.Add(new Move(square, squares[i + (7 * mult)]));
            return;
        }
        else if (square.position[0] == 'h' && !squares[i + (9 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (9 * mult)].piece[0]) == white)
        {
            legalMoves.Add(new Move(square, squares[i + (9 * mult)]));
            return;
        }
        if (square.position[0] == 'h' || square.position[0] == 'a') return;
        if (!squares[i + (9 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (9 * mult)].piece[0]) == white)
        {
            legalMoves.Add(new Move(square, squares[i + (9 * mult)]));
        }
        if (!squares[i + (7 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (7 * mult)].piece[0]) == white)
        {
            legalMoves.Add(new Move(square , squares[i + (7 * mult)]));
        }
    }

    private void GetKnightMoves(Square square, int i)
    {
        int[] offset = Knight.GetOffset(square.position);
       
        for (int j = 0; j < offset.Length; j++)
        {
            if (i + offset[j] > 63 || i + offset[j] < 0) continue;
            if (squares[i + offset[j]].piece.Equals("")
                || Char.IsUpper(squares[i + offset[j]].piece[0]) == white)
            {
                legalMoves.Add(new Move(square, squares[i + offset[j]]));
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
                if (i + ((j + 1) * offset[k]) < 0 || i + ((j + 1) * offset[k]) > 63) break;
                
                if (squares[i + ((j + 1) * offset[k])].position[0] == 'h')
                {
                    if (squares[i + ((j + 1) * offset[k])].piece.Equals("") || Char.IsUpper(squares[i + ((j + 1) * offset[k])].piece[0]) == white)
                    {
                        legalMoves.Add(new Move(square, squares[i + ((j + 1) * offset[k])]));
                    }
                    break;
                }
                if (squares[i + ((j + 1) * offset[k])].piece.Equals("")) // square is empty add to possible moves
                {
                    legalMoves.Add(new Move(square, squares[i + ((j + 1) * offset[k])]));
                }
                else if (Char.IsUpper(squares[i + ((j + 1) * offset[k])].piece[0]) == white) // Square contains enemy piece, piece can be taken then break 
                {
                    legalMoves.Add(new Move(square, squares[i + ((j + 1) * offset[k])]));
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
                if (i + (j * offset[k]) < 0 || i + (j * offset[k]) > 63) break;
                if (squares[i + (j * offset[k])].piece.Equals(""))
                {
                    legalMoves.Add(new Move(square, squares[i + (j * offset[k])]));
                }
                else if (Char.IsUpper(squares[i + (j * offset[k])].piece[0]) == white)
                {
                    legalMoves.Add(new Move(square, squares[i + (j * offset[k])]));
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
        if (square.position[0] == 'a')
        {
            offset = new int[]{9, -7};
        }
        else if (square.position[0] == 'h')
        {
            offset = new int[]{7, -9};  
        }
        else
        {
            offset = new int[]{9, -9, 7, -7};
        }
        
        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 1; j < 9; j++)
            {   
                if (i + (j * offset[k]) < 0 || i + (j * offset[k]) > 63) break;
                if (squares[i + (j * offset[k])].piece.Equals(""))
                {
                    legalMoves.Add(new Move(square, squares[i + (j * offset[k])]));
                }
                else if (Char.IsUpper(squares[i + (j * offset[k])].piece[0]) == white)
                {
                    legalMoves.Add(new Move(square, squares[i + (j * offset[k])]));
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

    private bool KingHorizontalCheck(Square square, int i)
    {
        int[] offset;
        
        offset = Rook.GetHorizontalOffset(square);
        
        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 0; j < 8; j++)
            {   
                if (i + ((j + 1) * offset[k]) < 0 || i + ((j + 1) * offset[k]) > 63) break;
                if (squares[i + ((j + 1) * offset[k])].piece.Equals("")) continue;
                string piece = ("" + squares[i + ((j + 1) * offset[k])].piece[0] + "").ToUpper();
                if (squares[i + ((j + 1) * offset[k])].position[0] == 'h')
                {
                    if (Char.IsUpper(squares[i + ((j + 1) * offset[k])].piece[0]) == white && piece.Equals("Q") || piece.Equals("R"))
                    {
                        Debug.Log("Found Check at " + squares[i + ((j + 1) * offset[k])].position);
                        return true;
                    }
                }
                else if (!squares[i + ((j + 1) * offset[k])].piece.Equals("") && Char.IsUpper(squares[i + ((j + 1) * offset[k])].piece[0]) == white) // Square contains enemy piece, piece can be taken then break 
                {
                    if (!piece.Equals("R") || !piece.Equals("Q")) break; // Irrelevant piece blocking diagnol meaning another bishop can't check. no need to search ray.
                    Debug.Log("Found Check at " + squares[i + ((j + 1) * offset[k])].position);
                    return true;
                }
                else if (!squares[i + ((j + 1) * offset[k])].piece.Equals("") && Char.IsUpper(squares[i + ((j + 1) * offset[k])].piece[0]) != white) // Freindly Piece
                {
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
                if (i + (j * offset[k]) < 0 || i + (j * offset[k]) > 63) break;
                if (squares[i + (j * offset[k])].piece.Equals("")) continue;
                string piece = ("" + squares[i + (j * offset[k])].piece[0]).ToUpper();
                if (!squares[i + (j * offset[k])].piece.Equals("") && Char.IsUpper(squares[i + (j * offset[k])].piece[0]) == white) // Enemy Piece
                {
                    if (!piece.Equals("R") || !piece.Equals("Q")) break; // Irrelevant piece blocking diagnol meaning another bishop can't check. no need to search ray.
                    Debug.Log("Found Check at " + squares[i + (j * offset[k]) ].position);
                    return true;
                }
                else if (!squares[i + (j * offset[k])].piece.Equals("") && Char.IsUpper(squares[i + (j * offset[k])].piece[0]) != white) // Friendly Piece
                {
                    break;
                }
            }
        }
        return false;
    }

    private bool KingDiagnolCheck(Square square, int i)
    {
        int[] offset;
        if (square.position[0] == 'a')
        {
            offset = new int[]{9, -7};
        }
        else if (square.position[0] == 'h')
        {
            offset = new int[]{7, -9};  
        }
        else
        {
            offset = new int[]{9, -9, 7, -7};
        }
        
        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 1; j < 9; j++)
            {   
                if (i + (j * offset[k]) < 0 || i + (j * offset[k]) > 63) break;
                if (squares[i + (j * offset[k])].piece.Equals("")) continue;
                string piece = ("" + squares[i + (j * offset[k])].piece[0]).ToUpper();
                if (!squares[i + (j * offset[k])].piece.Equals("") && Char.IsUpper(squares[i + (j * offset[k])].piece[0]) == white) // Enemy Piece
                {
                    if (!piece.Equals("B") || !piece.Equals("Q")) break; // Irrelevant piece blocking diagnol meaning another bishop can't check. no need to search ray.
                    Debug.Log("Found Check at " + squares[i + (j * offset[k])].position + " containing " + piece);
                    return true;
                }
                else if (!squares[i + (j * offset[k])].piece.Equals("") && Char.IsUpper(squares[i + (j * offset[k])].piece[0]) != white) // Freindly Piece
                {
                    break;
                }
            }
        }
        return false;
    }

}
