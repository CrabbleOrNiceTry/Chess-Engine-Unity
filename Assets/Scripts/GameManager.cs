using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static GameManager instance;
    public Board board;    
    // Squares will be stored in an array for faster traversing than a list.

    

    void Awake()
    {
        instance = this;
        board = FindObjectOfType<Board>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
