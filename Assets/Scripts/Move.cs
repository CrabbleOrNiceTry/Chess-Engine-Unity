using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public Square original; 
    public Square newSquare;
    public int index;
    public Move(Square original, Square newSquare, int index)
    {
        this.original = original;
        this.newSquare = newSquare;
        this.index = index;
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
