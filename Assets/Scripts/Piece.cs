using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Piece : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool controllingPiece;
    private bool controllingThisPiece;
    private Square originalSquare;
    public bool wasControlled;
    public string piece;

    // Current index on 64 integer array board.
    public int currentIndex;

    void Start()
    {
        controllingPiece = false;
        wasControlled = false;
        piece = "";
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0) && controllingThisPiece)
        {
            controllingThisPiece = false;
            controllingPiece = false;
            wasControlled = true;
            GameObject closestObjectToMouse = GetNearestSquare();
            Move[] moves = GameManager.instance.board.GetLegalMoves();

            Move move = new Move(originalSquare, closestObjectToMouse.GetComponent<Square>(), -1);
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
            else
            {
                int depth = 3;
                // Initialize some values
                GameManager.instance.computer.PrepareSearch(GameManager.instance.board);

                // Search for the best move 
                Move computerMove = GameManager.instance.computer.GetBestMove(depth, depth, true, "BLACK", -Mathf.Infinity, Mathf.Infinity);
                Debug.Log(computerMove.ToString());


                // Change the position of the original object to where it should be after the move
                computerMove.original.pieceObj.transform.position = new Vector3(computerMove.newSquare.transform.position.x, computerMove.newSquare.transform.position.y, computerMove.newSquare.transform.position.z);
                
                // Destroy the piece that is being taken if present.
                if (!computerMove.newSquare.piece.Equals(""))
                    Destroy(computerMove.newSquare.pieceObj);
                
                computerMove.newSquare.pieceObj = computerMove.original.pieceObj;

                // Make the move on the board array
                GameManager.instance.board.MakeMove(computerMove);
                
                // Check pawn promotion
                if (computerMove.pawnPromote)
                    PromotePawn(computerMove.newSquare.pieceObj, computerMove.newSquare.gameObject, GameManager.instance.white);

                // Change the next player to move
            }
        }
        else if (controllingThisPiece)
        {
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        }
    }

    // Only promotes to queen. :( no way Im doing anything else lmao. Maybe in the futre.
    private void PromotePawn(GameObject square, GameObject square2, bool white)
    {
        byte[] bytes = File.ReadAllBytes("./Assets/Resources/Pieces/" + ((!white) ? "Black Pieces/" : "White Pieces/") + ((!white) ? "Q" : "q") + ".png");
        float size = square2.transform.localScale.x / square2.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        Texture2D texture = new Texture2D((int)size, (int)size, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);
        texture.Apply();

        square.GetComponent<UnityEngine.UI.RawImage>().texture = texture;

    }

    private GameObject GetNearestSquare()
    {
        GameObject closestObjectToMouse = (GameObject)Instantiate(GameManager.instance.board.squares[0].gameObject);
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
