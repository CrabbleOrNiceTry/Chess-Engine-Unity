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
    public bool hasMoved;

    // Current index on 64 integer array board.
    public int currentIndex;

    void Start()
    {
        controllingPiece = false;
        wasControlled = false;
        hasMoved = false;
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
            string moveStr = move.ToString();
            bool moveFound = false;
            if (piece.ToUpper().Equals("K"))
            {   
                if (closestObjectToMouse.GetComponent<Square>().position.Equals("g1"))
                {
                    moveStr = "O-O";
                }
                else if (closestObjectToMouse.GetComponent<Square>().position.Equals("c1"))
                {
                    moveStr = "O-O-O";
                }
            }

            foreach (Move i in moves)
            {
                if (i.ToString().Equals(moveStr))
                {
                    transform.position = new Vector3(closestObjectToMouse.transform.position.x, closestObjectToMouse.transform.position.y ,closestObjectToMouse.transform.position.z - 0.01f);
                    if (!closestObjectToMouse.GetComponent<Square>().piece.Equals(""))
                    {
                        Destroy(closestObjectToMouse.GetComponent<Square>().pieceObj);
                    }
                    
                    closestObjectToMouse.GetComponent<Square>().pieceObj = (GameObject)Instantiate(originalSquare.pieceObj, originalSquare.pieceObj.transform.position, Quaternion.identity);
                    // Destroy(originalSquare.pieceObj);
                    
                    GameManager.instance.board.MakeMove(i);
                    this.hasMoved = true;

                    
                    if (i.pawnPromote)
                        PromotePawn(i.newSquare.pieceObj, move.newSquare.gameObject, GameManager.instance.white);

                    moveFound = true;
                    FindObjectOfType<Sound>().PlayMoveSound();

                    break;
                }
            }
            if (!moveFound)
            {
                transform.position = originalSquare.gameObject.transform.position;
            }
            else
            {
                int depth = 4;
                // Initialize some values
                GameManager.instance.computer.PrepareSearch(GameManager.instance.board);

                // Search for the best move 
                float time = Time.realtimeSinceStartup;
                Move computerMove = GameManager.instance.computer.GetBestMove(depth, depth, true, "BLACK", -999999f, 999999f);
                time = Time.realtimeSinceStartup - time;
                Debug.Log(time);
                Debug.Log(computerMove.ToString());

                computerMove.original.pieceObj.GetComponent<Piece>().hasMoved = true;

                
                computerMove.original.pieceObj.transform.position = new Vector3(computerMove.newSquare.transform.position.x, computerMove.newSquare.transform.position.y, computerMove.newSquare.transform.position.z);
                
                // Change the position of the original object to where it should be after the move
                
                // Destroy the piece that is being taken if present.
                if (!computerMove.newSquare.piece.Equals(""))
                    Destroy(computerMove.newSquare.pieceObj);
                
                computerMove.newSquare.pieceObj = computerMove.original.pieceObj;

                // Make the move on the board array
                GameManager.instance.board.MakeMove(computerMove);
                
                // Check pawn promotion
                if (computerMove.pawnPromote)
                    PromotePawn(computerMove.newSquare.pieceObj, computerMove.newSquare.gameObject, GameManager.instance.white);

                FindObjectOfType<Sound>().PlayMoveSound();
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
        byte[] bytes = File.ReadAllBytes("./Assets/Resources/Pieces/" + ((!white) ? "White Pieces/" : "Black Pieces/") + ((!white) ? "Q" : "q") + ".png");
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
