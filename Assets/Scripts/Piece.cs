using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Piece : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool controllingPiece;
    private bool controllingThisPiece;
    private Square originalSquare;
    public bool wasControlled;
    public string piece;

    void Start()
    {
        controllingPiece = false;
        wasControlled = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0) && controllingThisPiece)
        {
            controllingThisPiece = false;
            controllingPiece = false;
            wasControlled = true;
            GameObject closestObjectToMouse = GetNearestSquare();
            // List<string> moves = new List<string>();
            float time = Time.realtimeSinceStartup;
            Move[] moves = GameManager.instance.board.GetLegalMoves();
            Debug.Log("Outside Function: " + (Time.realtimeSinceStartup - time));

            Move move = new Move(originalSquare, closestObjectToMouse.GetComponent<Square>());
            bool moveFound = false;
            foreach (Move i in moves)
            {
                if (i.ToString().Equals(move.ToString()))
                {
                    transform.position = closestObjectToMouse.transform.position;
                    if (!closestObjectToMouse.GetComponent<Square>().piece.Equals(""))
                    {
                        Destroy(closestObjectToMouse.GetComponent<Square>().pieceObj);
                    }
                    closestObjectToMouse.GetComponent<Square>().piece = originalSquare.piece;
                    closestObjectToMouse.GetComponent<Square>().pieceObj = originalSquare.pieceObj;
                    originalSquare.piece = "";
                    
                    moveFound = true;
                    GameManager.instance.white = !GameManager.instance.white;
                    break;
                }
            }
            if (!moveFound)
            {
                transform.position = originalSquare.gameObject.transform.position;
            }
        }
        else if (controllingThisPiece)
        {
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        }
    }

    private GameObject GetNearestSquare()
    {
        GameObject closestObjectToMouse = GameManager.instance.board.squares[0].gameObject;
        float closestDistance = 10000f;
        for (int i = 0; i < GameManager.instance.board.squares.Length; i++)
        {
            float distance = Vector3.Distance(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)), 
                                                                             new Vector3(GameManager.instance.board.squares[i].gameObject.transform.position.x, 
                                                                             GameManager.instance.board.squares[i].gameObject.transform.position.y, 0f));
            if (distance <  closestDistance)
            {
                closestDistance = distance;
                closestObjectToMouse = GameManager.instance.board.squares[i].gameObject;
            }
        }
        return closestObjectToMouse;
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButton(0))
        {
            if (!controllingPiece)
            {
                controllingThisPiece = true;
                controllingPiece = true;
                originalSquare = GetNearestSquare().GetComponent<Square>();
            }
            
        }
    }

    
}
