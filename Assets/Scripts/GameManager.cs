using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static GameManager instance;
    public Board board;    
    public bool white;
    public AI computer;
    // Squares will be stored in an array for faster traversing than a list.

    

    void Awake()
    {
        instance = this;
        white = true;
        board = FindObjectOfType<Board>();
        computer = new AI();
    }
}
