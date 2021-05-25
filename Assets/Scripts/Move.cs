using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public Square original; 
    public Square newSquare;
    public string pieceOriginal;
    public string pieceNew;
    public int index;
    public bool pawnPromote;
    public Move(Square original, Square newSquare, int index)
    {
        this.original = original;
        this.newSquare = newSquare;
        this.index = index;
        pieceOriginal = original.piece;
        pieceNew = newSquare.piece;
        pawnPromote = false;
    }

    public string ToString()
    {
        return original.position + newSquare.position;
    }

    public bool Equals(Move other)
    {
        return this.ToString().Equals(other.ToString());
    }
}
