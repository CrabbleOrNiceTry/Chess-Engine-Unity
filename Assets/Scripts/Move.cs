using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public Square original; 
    public Square newSquare;
    public Move(Square original, Square newSquare)
    {
        this.original = original;
        this.newSquare = newSquare;
    }

    public string ToString()
    {
        return original.position + newSquare.position;
    }
}
