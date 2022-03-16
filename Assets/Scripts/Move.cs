using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public Square original;
    public Square newSquare;
    public PieceE pieceOriginal;
    public PieceE pieceNew;
    public string castle;
    public bool isCastle;
    public bool pawnPromote;
    public int index;
    public Vector3 newRookPosition;
    public GameObject castleRook;
    public bool tookOppositeColor;


    public Move(Square original, Square newSquare, int index, bool isCastle = false, string castle = "")
    {
        // Don't want an else cause branching = bad. Though Im one to talk lmao GetPawnMoves() :(
        this.isCastle = isCastle;
        this.castle = castle;
        if (isCastle)
            SetCastle();
        this.original = original;
        this.newSquare = newSquare;
        this.index = index;
        pieceOriginal = original.piece;
        pieceNew = newSquare.piece;
        pawnPromote = false;
        tookOppositeColor = false;
    }

    public string ToString()
    {
        return original.position + newSquare.position;
    }

    public bool Equals(Move other)
    {
        return this.ToString().Equals(other.ToString());
    }

    private void SetCastle()
    {
        if (castle.Equals("WHITE-KING"))
        {
            castleRook = GameManager.instance.board.squares[63].pieceObj;
            newRookPosition = GameManager.instance.board.squares[61].transform.position;
        }
        else if (castle.Equals("WHITE-QUEEN"))
        {
            castleRook = GameManager.instance.board.squares[56].pieceObj;
            newRookPosition = GameManager.instance.board.squares[59].transform.position;
        }
        else if (castle.Equals("BLACK-KING"))
        {
            castleRook = GameManager.instance.board.squares[7].pieceObj;
            newRookPosition = GameManager.instance.board.squares[5].transform.position;
        }
        else if (castle.Equals("BLACK-QUEEN"))
        {
            castleRook = GameManager.instance.board.squares[0].pieceObj;
            newRookPosition = GameManager.instance.board.squares[3].transform.position;
        }
    }
}
