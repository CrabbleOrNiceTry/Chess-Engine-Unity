using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public Square original; 
    public Square newSquare;
    public string pieceOriginal;
    public string pieceNew;
    public string castle;
    public bool isCastle;
    public bool pawnPromote;
    public int index;

    public Move(Square original, Square newSquare, int index, bool isCastle=false, string castle="")
    {
        // Don't want an else cause branching = bad. Though Im one to talk lmao GetPawnMoves() :(
        this.isCastle = false;
        if (isCastle)
        {
            this.castle = castle;
            this.isCastle = true;
        }
        this.original = original;
        this.newSquare = newSquare;
        this.index = index;
        pieceOriginal = "" + original.piece;
        pieceNew = "" + newSquare.piece;
        pawnPromote = false;
    }

    public string ToString()
    {
        if (isCastle) return castle;
        return original.position + newSquare.position;
    }

    public bool Equals(Move other)
    {
        return this.ToString().Equals(other.ToString());
    }
}
