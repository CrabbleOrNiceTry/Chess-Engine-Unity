using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{

    // position as in e8 or d8 or f4
    public string position;
    public string piece;
    public GameObject pieceObj;

    void Awake()
    {
        piece = "";
    }

    public void SetPosition(string position)
    {
        this.position = position;
    }
}
