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

    private List<string> legalMoves;

    void Awake()
    {
        squares = new Square[64];
        squaresList = new List<Square>();
        white = true;
        legalMoves = new List<string>();
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
     * Returns a set of legal moves in string form such as e2e4
     * I do it in this fashion because this has to be a very fast funciton and the only alternative that was 
     * cleaner code what about 10x larger big O notation, best case. So I just shove it all into one for loop.
    */
    public List<string> GetLegalMoves()
    {


        legalMoves = new List<string>();
        string pawnPieceToLookFor = (white ? "p" : "P");
        string rookPieceToLookFor = (white ? "r" : "R");
        string knightPieceToLookFor = (white ? "n" : "N");
        string bishopPieceToLookFor = (white ? "b" : "B");
        string kingPieceToLookFor = (white ? "k" : "K");
        string queenPieceToLookFor = (white ? "q" : "Q");

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
        }
        return legalMoves;
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
            legalMoves.Add(square.position + squares[i + (16 * mult)].position);
        }
        else if (!white
            && square.position[1] == '7'
            && squares[i + (16 * mult)].piece.Equals(""))
        {
            legalMoves.Add(square.position + squares[i + (16 * mult)].position);
        }
        if (squares[i + (8 * mult)].piece.Equals(""))
        {
            legalMoves.Add(square.position + squares[i + (8 * mult)].position);
        }
        if (square.position[0] == 'a' && !squares[i + (7 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (7 * mult)].piece[0]) == white)
        {
            legalMoves.Add(square.position + squares[i + (7 * mult)].position);
            return;
        }
        else if (square.position[0] == 'h' && !squares[i + (9 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (9 * mult)].piece[0]) == white)
        {
            legalMoves.Add(square.position + squares[i + (9 * mult)].position);
            return;
        }
        if (square.position[0] == 'h' || square.position[0] == 'a') return;
        if (!squares[i + (9 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (9 * mult)].piece[0]) == white)
        {
            legalMoves.Add(square.position + squares[i + (9 * mult)].position);
        }
        if (!squares[i + (7 * mult)].piece.Equals("") && Char.IsUpper(squares[i + (7 * mult)].piece[0]) == white)
        {
            legalMoves.Add(square.position + squares[i + (7 * mult)].position);
        }
    }

    private void GetKnightMoves(Square square, int i)
    {
        int[] offset;
        if (square.position[0] == 'b')
        {
            offset = new int[]{-15, 15, -17, 17, -6, 10};
        }
        else if (square.position[0] == 'g')
        {
            offset = new int[]{-15, 15, -17, 17, 6, -10};
        }
        else if (square.position[0] == 'h')
        {
            offset = new int[]{-15, 15, 6, -6, 10, -10};
        }
        else if (square.position[0] == 'a')
        {
            offset = new int[]{-17, 17, 6, -6, 10, -10};
        }
        else
        {
            offset = new int[]{-15, 15, -17, 17, 6, -6, 10, -10};
        }
        // Check if piece is pinned here. We can have a list of pinned pieces or something.
        for (int j = 0; j < offset.Length; j++)
        {
            if (i + offset[j] > 63 || i + offset[j] < 0) continue;
            if (squares[i + offset[j]].piece.Equals("")
                || Char.IsUpper(squares[i + offset[j]].piece[0]) == white)
            {
                legalMoves.Add(squares[i].position + squares[i + offset[j]].position);
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

    private void GetKingMoves()
    {
        
    }

    private void GetHorizontalMoves(Square square, int i)
    {
        int[] offset;
        if (square.position[0] == 'a')
        {
            offset = new int[]{1};
        }
        else if (square.position[0] == 'h')
        {
            offset = new int[]{-1};
        }
        else
        {
            offset = new int[]{1, -1};
        }
        
        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 0; j < 8; j++)
            {   
                if (i + ((j + 1) * offset[k]) < 0 || i + ((j + 1) * offset[k]) > 63) break;
                if (squares[i + ((j + 1) * offset[k])].position[0] == 'h'
                    && squares[i + ((j + 1) * offset[k])].position[0] == 'h')
                {
                    if (squares[i + ((j + 1) * offset[k])].piece.Equals("") || Char.IsUpper(squares[i + ((j + 1) * offset[k])].piece[0]) == white)
                    {
                        legalMoves.Add(square.position + squares[i + ((j + 1) * offset[k])].position);
                    }
                    break;
                }
                if (squares[i + ((j + 1) * offset[k])].piece.Equals(""))
                {
                    legalMoves.Add(square.position + squares[i + ((j + 1) * offset[k])].position);
                }
                else if (Char.IsUpper(squares[i + ((j + 1) * offset[k])].piece[0]) == white)
                {
                    legalMoves.Add(square.position + squares[i + ((j + 1) * offset[k])].position);
                    break;
                }
                else
                {
                    break;
                }
            }
        }
    }
    
    private void GetVerticalMoves(Square square, int i)
    {
        int[] offset = new int[]{8, -8};
        
        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 1; j < 9; j++)
            {   
                if (i + (j * offset[k]) < 0 || i + (j * offset[k]) > 63) break;
                if (squares[i + (j * offset[k])].piece.Equals(""))
                {
                    legalMoves.Add(square.position + squares[i + (j * offset[k])].position);
                }
                else if (Char.IsUpper(squares[i + (j * offset[k])].piece[0]) == white)
                {
                    legalMoves.Add(square.position + squares[i + (j * offset[k])].position);
                    break;
                }
                else
                {
                    break;
                }
            }
        }
    }

    private void GetDiagnolMoves(Square square, int i)
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
                    legalMoves.Add(square.position + squares[i + (j * offset[k])].position);
                }
                else if (Char.IsUpper(squares[i + (j * offset[k])].piece[0]) == white)
                {
                    legalMoves.Add(square.position + squares[i + (j * offset[k])].position);
                    break;
                }
                else
                {
                    break;
                }
            }
        }
    }

}
