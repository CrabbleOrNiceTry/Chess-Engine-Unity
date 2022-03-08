using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{

    // position as in e8 or d8 or f4
    public string position;
    public char piece;
    public GameObject pieceObj;
    public bool isPinned;
    public bool pieceMoved;
    public int index;
    // public int piecePinning;

    void Awake()
    {
        piece = '\0';
        isPinned = false;
    }

    public void SetPosition(string position)
    {
        this.position = position;
    }

    public bool IsEmpty()
    {
        return piece == '\0';
    }
}
