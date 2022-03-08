using System.Collections;

using System.Collections.Generic;

using System;

using UnityEngine;

// rnbqkbnrpppppppp8888PPPPPPPPRNBQKBNR
// 83k42b1r35B24N35K288 -- Bishop pin
// 83k42b584N32r1BK288 -- Rook pin

public class Board : MonoBehaviour
{
    // {'P': 10, 'N': 35, 'B': 35, 'R': 52.5, 'Q': 100, 'K': 1000, 'p': -10, 'n': -35, 'b': -35, 'r': -52.5, 'q': -100, 'k': -1000}

    private int whiteKingIndex;
    private int blackKingIndex;
    private int checkCount;

    private Dictionary<char, float> pieceVal;
    // private Dictionary<char, List<int>> pieceDictionary;

    public HashSet<int> whitePieces;
    public HashSet<int> blackPieces;


    public Square[] squares;
    public List<Square> squaresList;
    public List<Move> legalMoves;
    public HashSet<int> pieceIndex;
    private HashSet<int> pinnedPieces; // These pieces cannot move. 


    private Dictionary<int, List<List<int>>> offsetVerticalDictionary;
    private Dictionary<int, List<List<int>>> offsetHorizontalDictionary;
    private Dictionary<int, List<List<int>>> diagnolOffsetDictionary;
    private Dictionary<int, int[]> kingOffsetDictionary;
    private Dictionary<int, int[]> knightOffsetDictionary;

    private HashSet<int> attackedSquares;

    [HideInInspector]
    public string fen;

    // Whites turn if true

    public bool stalemate;

    public bool checkmate;

    public float sum;
    void Awake()
    {
        squares = new Square[64];
        squaresList = new List<Square>();
        pieceVal = new Dictionary<char, float>();
        pieceIndex = new HashSet<int>();
        legalMoves = new List<Move>();
        whitePieces = new HashSet<int>();
        blackPieces = new HashSet<int>();
        pinnedPieces = new HashSet<int>();
        attackedSquares = new HashSet<int>();
        offsetVerticalDictionary = new Dictionary<int, List<List<int>>>();
        offsetHorizontalDictionary = new Dictionary<int, List<List<int>>>();
        diagnolOffsetDictionary = new Dictionary<int, List<List<int>>>();
        kingOffsetDictionary = new Dictionary<int, int[]>();
        knightOffsetDictionary = new Dictionary<int, int[]>();
        // pieceDictionary = new Dictionary<char, List<int>>();
        stalemate = false;
        checkmate = false;
        #region Piece Values
        pieceVal.Add('P', 10.0f);
        pieceVal.Add('N', 35.0f);
        pieceVal.Add('B', 35.0f);
        pieceVal.Add('R', 52.5f);
        pieceVal.Add('Q', 100.0f);
        pieceVal.Add('K', 1000.0f);
        pieceVal.Add('p', -10.0f);
        pieceVal.Add('n', -35.0f);
        pieceVal.Add('b', -35.0f);
        pieceVal.Add('r', -52.5f);
        pieceVal.Add('q', -100.0f);
        pieceVal.Add('k', -1000.0f);
        #endregion
    }

    void Start()
    {
        GetKingIndex();
        InitializePieceDictionary();
    }

    private void InitializePieceDictionary()
    {
        for (int i = 0; i < squares.Length; i++)
        {
            if (squares[i].piece != '\0')
            {
                if (Char.IsUpper(squares[i].piece))
                {
                    whitePieces.Add(i);
                }
                else
                {
                    blackPieces.Add(i);
                }

            }
            CreateDiagnolDictionary(i);
            CreateHorizontalDictionary(i);
            CreateVerticalDictionary(i);
            CreateKnightDictionary(i);
            CreateKingDictionary(i, squares[i]);
        }
    }

    #region Piece Offset Dictionary Creation

    private void CreateDiagnolDictionary(int i)
    {
        int[] offset;
        bool stopAtH = false;
        bool stopAtA = false;
        if (squares[i].position[0] == 'a')
        {
            stopAtH = true;
            offset = new int[] { 9, -7 };
        }
        else if (squares[i].position[0] == 'h')
        {
            stopAtA = true;
            offset = new int[] { 7, -9 };
        }
        else
        {
            stopAtA = true;
            stopAtH = true;
            offset = new int[] { 9, -9, 7, -7 };
        }

        List<List<int>> allSquaresToCheck = new List<List<int>>();

        for (int k = 0; k < offset.Length; k++)
        {
            List<int> tempSquares = new List<int>();
            for (int j = 1; j < 9; j++)
            {
                int index = i + (j * offset[k]);

                if (index < 0 || index > 63) break;

                tempSquares.Add(index);

                if (stopAtA && squares[index].position[0] == 'a')
                    break;
                else if (stopAtH && squares[index].position[0] == 'h')
                    break;
            }
            allSquaresToCheck.Add(tempSquares);
        }
        diagnolOffsetDictionary.Add(i, allSquaresToCheck);
    }

    /* 
        Functions creates dictionary that returns each square needed to check 
        for a rook given the an index.
    */
    private void CreateVerticalDictionary(int index)
    {
        int squaresAbove = index / 8;
        int squaresBelow = 8 - (squaresAbove + 1); // The 1 is to account for the square the piece is on

        List<int> upSquaresToCheckList = new List<int>();
        List<int> downSquaresToCheckList = new List<int>();

        for (int j = 1; j < (int)(index / 8) + 1; j++)
        {
            downSquaresToCheckList.Add(index - (j * 8));
        }
        for (int j = 1; j < 8 - (int)(((index) / 8)); j++)
        {
            upSquaresToCheckList.Add(index + (j * 8));
        }

        List<List<int>> allSquaresToCheck = new List<List<int>>();

        allSquaresToCheck.Add(upSquaresToCheckList);
        allSquaresToCheck.Add(downSquaresToCheckList);

        offsetVerticalDictionary.Add(index, allSquaresToCheck);
    }

    private void CreateHorizontalDictionary(int index)
    {
        int squaresLeft = index % 8;
        int squaresRight = 8 - (squaresLeft + 1); // The 1 is to account for the square the piece is on

        List<int> leftSquaresToCheckList = new List<int>();
        List<int> rightSquaresToCheckList = new List<int>();

        for (int j = 1; j < (int)(index % 8) + 1; j++)
        {
            rightSquaresToCheckList.Add(index - j);
        }
        for (int j = 1; j < 8 - (int)(((index) % 8)); j++)
        {
            leftSquaresToCheckList.Add(index + j);
        }

        List<List<int>> allSquaresToCheck = new List<List<int>>();

        allSquaresToCheck.Add(leftSquaresToCheckList);
        allSquaresToCheck.Add(rightSquaresToCheckList);

        offsetHorizontalDictionary.Add(index, allSquaresToCheck);
    }

    private void CreateKnightDictionary(int index)
    {
        int[] offset;

        string pos = squares[index].position;

        if (pos[0] == 'b')
        {
            offset = new int[] { -15, 15, -17, 17, -6, 10 };
        }
        else if (pos[0] == 'g')
        {
            offset = new int[] { -15, 15, -17, 17, 6, -10 };
        }
        else if (pos[0] == 'h')
        {
            offset = new int[] { -17, 6, 15, -10 };
        }
        else if (pos[0] == 'a')
        {
            offset = new int[] { -15, 17, 10, -6 };
        }
        else
        {
            offset = new int[] { -15, 15, -17, 17, 6, -6, 10, -10 };
        }
        knightOffsetDictionary.Add(index, offset);
    }

    private void CreateKingDictionary(int index, Square square)
    {
        int[] offset;
        if (square.position[0] == 'a' && square.position[1] == '1')
        {
            offset = new int[] { 1, -7, -8 };
        }
        // Bottom right Corner
        else if (square.position[0] == 'h' && square.position[1] == '1')
        {
            offset = new int[] { -1, -9, -8 };

        }
        // Upper left Corner
        else if (square.position[0] == 'a' && square.position[1] == '8')
        {
            offset = new int[] { -1, 8, 9 };
        }
        // Upper right Corner
        else if (square.position[0] == 'h' && square.position[1] == '8')
        {
            offset = new int[] { -1, 8, 7 };
        }
        //Bottom 
        else if (square.position[1] == '1')
        {
            offset = new int[] { 1, -1, -8, -9, -7 };
        }
        // Upper 
        else if (square.position[1] == '8')
        {
            offset = new int[] { 1, -1, 8, 9, 7 };
        }
        // On left side
        else if (square.position[0] == 'a')
        {
            offset = new int[] { 1, 8, -8, -7, 7 };
        }
        // On right side
        else if (square.position[0] == 'h')
        {
            offset = new int[] { -1, 8, -8, -9, 9 };
        }
        else
        {
            offset = new int[] { 1, -1, 8, -8, 7, -7, 9, -9 };
        }
        kingOffsetDictionary.Add(index, offset);
    }

    #endregion


    public void AddSquare(Square square)
    {
        squaresList.Add(square);

    }

    public void ConvertSquareListToArray()
    {
        squares = squaresList.ToArray();
    }

    public int GetIndex(string position)
    {
        return (((8 - ((int)(position[1] - '0'))) * 8) + ((int)position[0] - 97));
    }

    private void GetKingIndex()
    {
        for (int i = 0; i < squares.Length; i++)
        {
            if (squares[i].piece == 'K')
            {
                // 6
                whiteKingIndex = i;
            }
            else if (squares[i].piece == 'k')
            {
                blackKingIndex = i;
            }
        }
    }

    /*
     * Returns a set of psuedo legal moves in string form such as e2e4
     * Returns a set of possibly illegal moves. These are then parsed
     * through and the illegal moves will be thrown out in 
     * the GetLegalMoves() function.
    */
    public async void GetPsuedoLegalMoves()
    {
        pieceIndex = new HashSet<int>();
        // Make pieceIndex a set of all the pieces from whitePieces and blackPieces
        pieceIndex = new HashSet<int>();
        pieceIndex.UnionWith(whitePieces);
        pieceIndex.UnionWith(blackPieces);
        // IMPORTANT: pieceIndex is CRUCIAL to the function of the CalculatePos() function in AI.cs
        // DO NOT DELTE IT unless being replaced. 


        legalMoves = new List<Move>();

        char pawnPieceToLookFor = (GameManager.instance.white ? 'P' : 'p');
        char rookPieceToLookFor = (GameManager.instance.white ? 'R' : 'r');
        char knightPieceToLookFor = (GameManager.instance.white ? 'N' : 'n');
        char bishopPieceToLookFor = (GameManager.instance.white ? 'B' : 'b');
        char kingPieceToLookFor = (GameManager.instance.white ? 'K' : 'k');
        char queenPieceToLookFor = (GameManager.instance.white ? 'Q' : 'q');

        checkCount = 0;

        // Get pinned pieces and attacked squares
        foreach (int i in (GameManager.instance.white ? blackPieces : whitePieces))
        {
            if (squares[i].piece.Equals(Char.ToLower(bishopPieceToLookFor)))
            {
                CheckDiagnolPins(i, squares[i], (GameManager.instance.white ? whiteKingIndex : blackKingIndex));
            }
            else if (squares[i].piece.Equals(Char.ToLower(rookPieceToLookFor)))
            {
                CheckHorizontalPins(i, (GameManager.instance.white ? whiteKingIndex : blackKingIndex), squares[i]);
                CheckVerticalPins(i, (GameManager.instance.white ? whiteKingIndex : blackKingIndex), squares[i]);
            }
        }

        if (checkCount >= 2) { GetKingMoves(squares[GameManager.instance.white ? whiteKingIndex : blackKingIndex], GameManager.instance.white ? whiteKingIndex : blackKingIndex); return; }

        // kingIndex = -1;
        foreach (int i in (GameManager.instance.white ? whitePieces : blackPieces))
        // for (int i = 0; i < squares.Length; i++)
        {
            if (squares[i].piece.Equals(pawnPieceToLookFor))
            {
                // 8
                GetPawnMoves(squares[i], i);
            }
            else if (squares[i].piece.Equals(knightPieceToLookFor))
            {
                // 4
                GetKnightMoves(squares[i], i);
            }
            else if (squares[i].piece.Equals(queenPieceToLookFor))
            {
                // 3
                GetQueenMoves(squares[i], i);
            }
            else if (squares[i].piece.Equals(bishopPieceToLookFor))
            {
                // 8
                GetBishopMoves(squares[i], i);
            }
            else if (squares[i].piece.Equals(rookPieceToLookFor))
            {
                // 10
                GetRookMoves(squares[i], i);
            }
            // else if (squares[i].piece.Equals(kingPieceToLookFor))
            // {
            //     // 6
            //     kingIndex = i;
            //     GetKingMoves(squares[i], i);
            // }
        }
        GetKingMoves(squares[GameManager.instance.white ? whiteKingIndex : blackKingIndex], GameManager.instance.white ? whiteKingIndex : blackKingIndex);
    }

    /*
     * Parses through the psuedo legal moves and 
     * throws out the illegal moves
     * Returns a set of proper legal moves.
    */
    public Move[] GetLegalMoves()
    {
        GetPsuedoLegalMoves();
        stalemate = false;
        checkmate = false;
        // Check if statemate
        stalemate = (legalMoves.Count == 0);
        if (stalemate)
        {

            return legalMoves.ToArray();
        }

        int tempKingIndex = (GameManager.instance.white ? whiteKingIndex : blackKingIndex);

        // float time = Time.realtimeSinceStartup;
        for (int i = legalMoves.Count - 1; i > -1; i--)
        {
            // Check if the move being checked is a king move, if so
            // change king index to new square.
            if (Char.ToUpper(legalMoves[i].original.piece) == 'K')
            {
                tempKingIndex = legalMoves[i].index;
            }
            else
            {
                tempKingIndex = (GameManager.instance.white ? whiteKingIndex : blackKingIndex);
            }

            // Check if the move is in a checked position. If so
            // remove the move from the legal moves list.
            if (Char.ToUpper(legalMoves[i].original.piece) != 'K' && Char.ToUpper(legalMoves[i].newSquare.piece) == 'K')
            {
                legalMoves.RemoveAt(i);
                continue;
            }
            MakeMove(legalMoves[i]);
            if (IsInCheck(squares[tempKingIndex], tempKingIndex))
            {
                // Debug.Log("Found check at " + legalMoves[i].ToString());
                UnmakeMove(legalMoves[i]);
                legalMoves.RemoveAt(i);
                continue;
            }
            UnmakeMove(legalMoves[i]);
        }
        // time = Time.realtimeSinceStartup - time;
        // Debug.Log(time);
        // After parsing all illegal moves there were no possible moves for player
        // meaning he is in check and that means mate.
        if (legalMoves.Count == 0)
        {
            checkmate = true;
        }

        GetCastleMove();
        return legalMoves.ToArray();
    }

    private bool IsInCheck(Square square, int i)
    {
        return (KingDiagnolCheck(square, i) || KingVerticalCheck(square, i) || KingHorizontalCheck(square, i) || KingKnightCheck(square, i) || KingPawnCheck(square, i));
    }

    public void MakeMove(Move move)
    {
        if (move.isCastle)
        {
            if (move.castle.Equals("BLACK-QUEEN"))
            {
                squares[0].piece = '\0';
                squares[3].piece = 'r';
                squares[2].piece = 'k';
                squares[4].piece = '\0';
                blackPieces.Remove(0);
                blackPieces.Remove(4);
                blackPieces.Add(2);
                blackPieces.Add(3);
                blackKingIndex = 2;
            }
            else if (move.castle.Equals("BLACK-KING"))
            {
                squares[7].piece = '\0';
                squares[5].piece = 'r';
                squares[6].piece = 'k';
                squares[4].piece = '\0';
                blackPieces.Remove(7);
                blackPieces.Remove(4);
                blackPieces.Add(6);
                blackPieces.Add(5);
                blackKingIndex = 6;
            }
            else if (move.castle.Equals("WHITE-QUEEN"))
            {
                squares[56].piece = '\0';
                squares[59].piece = 'R';
                squares[58].piece = 'K';
                squares[60].piece = '\0';
                whitePieces.Remove(56);
                whitePieces.Remove(60);
                whitePieces.Add(58);
                whitePieces.Add(59);
                whiteKingIndex = 58;

            }
            else if (move.castle.Equals("WHITE-KING"))
            {
                squares[63].piece = '\0';
                squares[61].piece = 'R';
                squares[62].piece = 'K';
                squares[60].piece = '\0';
                whitePieces.Remove(63);
                whitePieces.Remove(60);
                whitePieces.Add(62);
                whitePieces.Add(61);
                whiteKingIndex = 62;

            }
            GameManager.instance.white = !GameManager.instance.white;
            return;
        }

        // For debugging purposes, in the likely scenario something breaks and this happens. 
        if (Char.ToUpper(move.newSquare.piece) == 'K' && Char.ToUpper(move.original.piece) != 'K')
            Debug.Log("King Taken.");

        if (Char.ToUpper(move.original.piece) == 'P')
        {
            if (move.newSquare.position[1] == '8' || move.newSquare.position[1] == '1')
            {
                if (Char.IsLower(move.original.piece))
                    move.newSquare.piece = 'q';
                else
                    move.newSquare.piece = 'Q';
                move.original.piece = '\0';
                // temp += "+";
                move.pawnPromote = true;
                if (GameManager.instance.white)
                {
                    whitePieces.Remove(move.original.index);
                    whitePieces.Add(move.newSquare.index);
                    move.tookOppositeColor = blackPieces.Remove(move.newSquare.index);
                }
                else
                {
                    blackPieces.Remove(move.original.index);
                    blackPieces.Add(move.newSquare.index);
                    move.tookOppositeColor = whitePieces.Remove(move.newSquare.index);
                }
                GameManager.instance.white = !GameManager.instance.white;
                return;
            }
        }
        move.newSquare.piece = move.original.piece;
        move.original.piece = '\0';
        if (GameManager.instance.white)
        {
            if (move.newSquare.piece == 'K')
                whiteKingIndex = move.newSquare.index;
            whitePieces.Remove(move.original.index);
            whitePieces.Add(move.newSquare.index);
            move.tookOppositeColor = blackPieces.Remove(move.newSquare.index);
        }
        else
        {
            if (move.newSquare.piece == 'k')
                blackKingIndex = move.newSquare.index;
            var remove = blackPieces.Remove(move.original.index);
            var add = blackPieces.Add(move.newSquare.index);
            move.tookOppositeColor = whitePieces.Remove(move.newSquare.index);
        }
        GameManager.instance.white = !GameManager.instance.white;
        // return temp;
    }

    public void UnmakeMove(Move move)
    {
        GameManager.instance.white = !GameManager.instance.white;
        if (move.isCastle)
        {
            if (move.castle.Equals("BLACK-QUEEN"))
            {
                squares[0].piece = 'r';
                squares[3].piece = '\0';
                squares[2].piece = '\0';
                squares[4].piece = 'k';
                blackPieces.Add(4);
                blackPieces.Remove(3);
                blackPieces.Remove(2);
                blackPieces.Add(0);
                blackKingIndex = 4;
            }
            else if (move.castle.Equals("BLACK-KING"))
            {
                squares[7].piece = 'r';
                squares[5].piece = '\0';
                squares[6].piece = '\0';
                squares[4].piece = 'k';
                blackPieces.Add(4);
                blackPieces.Remove(5);
                blackPieces.Remove(6);
                blackPieces.Add(7);
                blackKingIndex = 4;
            }
            else if (move.castle.Equals("WHITE-QUEEN"))
            {
                squares[56].piece = 'R';
                squares[59].piece = '\0';
                squares[58].piece = '\0';
                squares[60].piece = 'K';
                whitePieces.Add(60);
                whitePieces.Remove(59);
                whitePieces.Remove(58);
                whitePieces.Add(56);
                whiteKingIndex = 60;
            }
            else if (move.castle.Equals("WHITE-KING"))
            {
                squares[63].piece = 'R';
                squares[61].piece = '\0';
                squares[62].piece = '\0';
                squares[60].piece = 'K';
                whitePieces.Add(60);
                whitePieces.Remove(61);
                whitePieces.Remove(62);
                whitePieces.Add(63);
                whiteKingIndex = 60;
            }
            return;
        }


        if (move.pawnPromote)
        {
            if (Char.IsLower(move.newSquare.piece))
            {
                move.original.piece = 'p';
            }
            else
            {
                move.original.piece = 'P';
            }
            move.newSquare.piece = move.pieceNew;
            move.pawnPromote = false;
        }
        else
        {
            move.original.piece = move.pieceOriginal;
            move.newSquare.piece = move.pieceNew;
        }
        if (GameManager.instance.white)
        {
            if (move.original.piece == 'K')
                whiteKingIndex = move.original.index;
            whitePieces.Add(move.original.index);
            whitePieces.Remove(move.newSquare.index);
            if (move.tookOppositeColor)
            {
                blackPieces.Add(move.newSquare.index);
            }
        }
        else
        {
            if (move.original.piece == 'k')
                blackKingIndex = move.original.index;
            blackPieces.Add(move.original.index);
            var remove = blackPieces.Remove(move.newSquare.index);
            if (move.tookOppositeColor)
            {
                whitePieces.Add(move.newSquare.index);
            }
        }
    }
    #region Castle Stuff

    /* 
     * Checks black queen side castle availability
     * Adds legal move is available
     */
    private void CheckBlackQueenSideCastle()
    {
        if (squares[0].pieceMoved == false && squares[4].pieceMoved == false)
        {
            // Now check if their are any pieces in the way.
            for (int i = 1; i < 4; i++)
            {
                if (!squares[i].IsEmpty())
                {
                    return;
                }
            }
            if (!IsInCheck(squares[4], 4))
                legalMoves.Add(new Move(squares[4], squares[2], -1, isCastle: true, castle: "BLACK-QUEEN"));
        }
    }

    /* 
     * Checks black king side castle availability
     * Adds legal move is available
     */
    private void CheckBlackKingSideCastle()
    {
        if (squares[7].pieceMoved == false && squares[4].pieceMoved == false)
        {
            // Now check if their are any pieces in the way.
            for (int i = 6; i > 4; i++)
            {
                if (!squares[i].IsEmpty())
                {
                    return;
                }
            }
            if (!IsInCheck(squares[4], 4))
                legalMoves.Add(new Move(squares[4], squares[6], -1, isCastle: true, castle: "BLACK-KING"));
        }
    }

    /* 
     * Checks white queen side castle availability
     * Adds legal move is available
     */
    private void CheckWhiteQueenSideCastle()
    {
        if (squares[56].pieceMoved == false && squares[60].pieceMoved == false)
        {
            // Now check if their are any pieces in the way.
            for (int i = 57; i < 60; i++)
            {
                if (!squares[i].IsEmpty())
                {
                    return;
                }
            }
            if (!IsInCheck(squares[60], 60))
                legalMoves.Add(new Move(squares[60], squares[58], -1, isCastle: true, castle: "WHITE-QUEEN"));
        }
    }

    /* 
     * Checks white king side castle availability
     * Adds legal move is available
     */
    private void CheckWhiteKingSideCastle()
    {
        if (squares[63].pieceMoved == false && squares[60].pieceMoved == false)
        {
            // Now check if their are any pieces in the way.
            for (int i = 61; i < 63; i++)
            {
                if (!squares[i].IsEmpty())
                {
                    return;
                }
            }
            // We do this last because this is the longest operation.
            if (!IsInCheck(squares[60], 60))
            {
                legalMoves.Add(new Move(squares[60], squares[62], -1, isCastle: true, castle: "WHITE-KING"));
            }
        }
    }

    /* 
     * Uppercase is white pieces. Lowercase is black pieces.
     * If the pieces are on the correct spots and the pieces both have not moved and the king is not in check.
     * Add the move as valid. 
    */
    private void GetCastleMove()
    {
        CheckWhiteKingSideCastle();
        CheckBlackKingSideCastle();
        CheckWhiteQueenSideCastle();
        CheckBlackQueenSideCastle();

    }

    #endregion

    private async void GetKingMoves(Square square, int i)
    {
        int[] offset = King.GetOffset(square);

        foreach (int j in offset)
        {
            if (i + j > 63 || i + j < 0) continue;

            if ((squares[i + j].IsEmpty() || Char.IsUpper(squares[i + j].piece) != GameManager.instance.white) && !attackedSquares.Contains(i + j))
            {
                legalMoves.Add(new Move(squares[i], squares[i + j], i + j));
            }
        }
    }

    private void GetPawnMoves(Square square, int i)
    {
        if (square.position[1] == '8' || square.position[1] == '1')
        {
            if (Char.IsLower(square.piece))
                square.piece = 'q';
            else
                square.piece = 'Q';
            GetQueenMoves(square, i);
            return;
        }

        int mult = (GameManager.instance.white ? -1 : 1);

        List<int> offset = new List<int>();
        List<int> attackOffset = new List<int>();

        if (square.position[0] == 'a' && GameManager.instance.white) attackOffset.Add(-7);
        else if (square.position[0] == 'h' && GameManager.instance.white) attackOffset.Add(-9);
        else if (square.position[0] == 'a' && !GameManager.instance.white) attackOffset.Add(9);
        else if (square.position[0] == 'h' && !GameManager.instance.white) attackOffset.Add(7);
        else
        {
            attackOffset.Add(9 * mult);
            attackOffset.Add(7 * mult);
        }
        if (square.position[1] == '2' && GameManager.instance.white) offset.Add(-16);
        else if (square.position[1] == '7' && !GameManager.instance.white) offset.Add(16);
        offset.Add(8 * mult);

        foreach (int index in attackOffset)
        {
            if (!squares[i + index].IsEmpty() && Char.IsUpper(squares[i + index].piece) != GameManager.instance.white)
            {
                legalMoves.Add(new Move(squares[i], squares[i + index], index));
            }
        }
        foreach (int index in offset)
        {
            if (squares[i + index].IsEmpty())
            {
                legalMoves.Add(new Move(squares[i], squares[i + index], index));
            }
        }
    }

    private void GetKnightMoves(Square square, int i)
    {
        int[] offset = Knight.GetOffset(square.position);

        for (int j = 0; j < offset.Length; j++)
        {
            int index = i + offset[j];

            if (index > 63 || index < 0) continue;

            if (squares[index].IsEmpty()
                || Char.IsUpper(squares[index].piece) != GameManager.instance.white)
            {
                legalMoves.Add(new Move(square, squares[index], index));
            }
        }
    }

    private void GetQueenMoves(Square square, int i)
    {
        GetHorizontalMoves(square, i);
        GetVerticalMoves(square, i);
        GetDiagnolMoves(square, i);
    }

    private void GetBishopMoves(Square square, int i)
    {
        GetDiagnolMoves(square, i);
    }

    private void GetRookMoves(Square square, int i)
    {
        GetHorizontalMoves(square, i);
        GetVerticalMoves(square, i);
    }

    private void GetHorizontalMoves(Square square, int i)
    {

        List<List<int>> allSquaresToCheck = offsetHorizontalDictionary[i];

        foreach (List<int> dir in allSquaresToCheck)
        {
            foreach (int index in dir)
            {
                if (squares[index].IsEmpty()) // square is empty add to possible moves
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                }
                else if (Char.IsUpper(squares[index].piece) != GameManager.instance.white) // Square ContainsKey enemy piece, piece can be taken then break 
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                    break;
                }
                else // Friendly piece is obstructing path break
                {
                    break;
                }
            }
        }

        // List<int> leftSquaresToCheckList = new List<int>();  
        // List<int> rightSquaresToCheckList = new List<int>();  
        // int size = (((i + 1) % 8) == 0 ? 7 : ((i + 1) % 8) - 1);
        // int size_right = (((i + 1) % 8) == 0 ? 0 : 8 - ((i + 1) % 8));
        // int[] leftSquaresToCheck = new int[size];
        // int[] rightSquaresToCheck = new int[size_right];

        // if ((i + 1) % 8 == 0)
        // {
        //     int count = 0;
        //     for (int j = 1; j < 8; j++)
        //     {
        //         leftSquaresToCheck[count] = (i - j);
        //         count++;
        //     }
        // }
        // else
        // {
        //     int count = 0;
        //     for (int j = 1; j < (i + 1) % 8; j++)
        //     {
        //         leftSquaresToCheck[count] = (i - j);
        //         count++;
        //     }
        //     count = 0;
        //     for (int j = 1; j < 8 - ((i + 1) % 8) + 1; j++)
        //     {
        //         rightSquaresToCheck[count] = (i + j);
        //         count++;
        //     }
        // }

        // // int[] leftSquaresToCheck = leftSquaresToCheckList.ToArray();
        // // int[] rightSquaresToCheck = rightSquaresToCheckList.ToArray();

        // foreach (int j in leftSquaresToCheck)
        // {

        //     if (squares[j].IsEmpty()) // square is empty add to possible moves
        //     {
        //         legalMoves.Add(new Move(square, squares[j], j));
        //     }
        //     else if (Char.IsUpper(squares[j].piece) != GameManager.instance.white) // Square ContainsKey enemy piece, piece can be taken then break 
        //     {
        //         legalMoves.Add(new Move(square, squares[j], j));
        //         break;
        //     }
        //     else // Friendly piece is obstructing path break
        //     {
        //         break;
        //     }
        // }
        // foreach (int j in rightSquaresToCheck)
        // {
        //     if (squares[j].IsEmpty()) // square is empty add to possible moves
        //     {
        //         legalMoves.Add(new Move(square, squares[j], j));
        //     }
        //     else if (Char.IsUpper(squares[j].piece) != GameManager.instance.white) // Square ContainsKey enemy piece, piece can be taken then break 
        //     {
        //         legalMoves.Add(new Move(square, squares[j], j));
        //         break;
        //     }
        //     else // Friendly piece is obstructing path break
        //     {
        //         break;
        //     }
        // }
    }

    private void GetVerticalMoves(Square square, int i)
    {

        List<List<int>> allSquaresToCheck = offsetVerticalDictionary[i];

        foreach (List<int> dir in allSquaresToCheck)
        {
            foreach (int index in dir)
            {
                if (squares[index].IsEmpty()) // square is empty add to possible moves
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                }
                else if (Char.IsUpper(squares[index].piece) != GameManager.instance.white) // Square ContainsKey enemy piece, piece can be taken then break 
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                    break;
                }
                else // Friendly piece is obstructing path break
                {
                    break;
                }
            }
        }

        // List<int> upSquaresToCheckList = new List<int>();
        // List<int> downSquaresToCheckList = new List<int>();

        // for (int j = 1; j < (int)(i / 8) + 1; j++)
        // {

        //     downSquaresToCheckList.Add(i - (j * 8));

        // }
        // for (int j = 1; j < 8 - (int)(((i) / 8)); j++)
        // {
        //     upSquaresToCheckList.Add(i + (j * 8));
        //     if (i == 63)
        //         Debug.Log(i - (j * 8));
        // }

        // int[] upSquaresToCheck = upSquaresToCheckList.ToArray();
        // int[] downSquaresToCheck = downSquaresToCheckList.ToArray();

        // foreach (int j in downSquaresToCheck)
        // {
        //     try
        //     {
        //         if (squares[j].IsEmpty()) // square is empty add to possible moves
        //         {
        //             legalMoves.Add(new Move(square, squares[j], j));
        //         }
        //         else if (Char.IsUpper(squares[j].piece) != GameManager.instance.white) // Square ContainsKey enemy piece, piece can be taken then break 
        //         {
        //             legalMoves.Add(new Move(square, squares[j], j));
        //             break;
        //         }
        //         else // Friendly piece is obstructing path break
        //         {
        //             break;
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.Log(e.Message);
        //         Debug.Log(j);
        //         throw e;
        //     }
        // }
        // foreach (int j in upSquaresToCheck)
        // {
        //     if (squares[j].IsEmpty()) // square is empty add to possible moves
        //     {
        //         legalMoves.Add(new Move(square, squares[j], j));
        //     }
        //     else if (Char.IsUpper(squares[j].piece) != GameManager.instance.white) // Square ContainsKey enemy piece, piece can be taken then break 
        //     {
        //         legalMoves.Add(new Move(square, squares[j], j));
        //         break;
        //     }
        //     else // Friendly piece is obstructing path break
        //     {
        //         break;
        //     }
        // }
    }

    private bool GetDiagnolMoves(Square square, int i)
    {
        List<List<int>> allSquaresToCheck = new List<List<int>>();
        try
        {
            allSquaresToCheck = diagnolOffsetDictionary[i];
        }
        catch
        {
            Debug.Log(i);
            throw new Exception("Diagnol Offset Dictionary does not contain key: " + i);
        }
        foreach (var dir in allSquaresToCheck)
        {
            foreach (int index in dir)
            {
                if (squares[index].IsEmpty()) // square is empty add to possible moves
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                }
                else if (Char.IsUpper(squares[index].piece) != GameManager.instance.white) // Square ContainsKey enemy piece, piece can be taken then break 
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                    break;
                }
                else // Friendly piece is obstructing path break
                {
                    break;
                }
            }
        }








        // int[] offset;
        // bool stopAtH = false;
        // bool stopAtA = false;
        // if (square.position[0] == 'a')
        // {
        //     stopAtH = true;
        //     offset = new int[] { 9, -7 };
        // }
        // else if (square.position[0] == 'h')
        // {
        //     stopAtA = true;
        //     offset = new int[] { 7, -9 };
        // }
        // else
        // {
        //     stopAtA = true;
        //     stopAtH = true;
        //     offset = new int[] { 9, -9, 7, -7 };
        // }



        // for (int k = 0; k < offset.Length; k++)
        // {
        //     // bool isPossibleCheck = IsPossibleCheck(i, offset[k]);

        //     for (int j = 1; j < 9; j++)
        //     {
        //         int index = i + (j * offset[k]);

        //         if (index < 0 || index > 63) break;

        //         if (squares[index].IsEmpty()) // Square is empty
        //         {
        //             legalMoves.Add(new Move(square, squares[index], index));
        //         }
        //         else if (Char.IsUpper(squares[index].piece) != GameManager.instance.white) // Contains an enemy piece
        //         {
        //             legalMoves.Add(new Move(square, squares[index], index));
        //             break;
        //         }
        //         else // Contains a friendly piece
        //         {
        //             break;
        //         }

        //         if (stopAtA && squares[index].position[0] == 'a')
        //             break;
        //         else if (stopAtH && squares[index].position[0] == 'h')
        //             break;
        //     }
        // }
        return true;
    }

    // returns if a king and the piece are in the same path
    // meaning king may be in check 
    // Will not work for checking horizontal because offset is 1, -1
    private bool IsPossibleCheck(int i, int offset)
    {
        int enemyKingIndex = (GameManager.instance.white ? blackKingIndex : whiteKingIndex);

        if (enemyKingIndex % offset == i % offset)
            return true;
        return false;
    }

    private bool KingHorizontalCheck(Square square, int i)
    {
        int[] offset;

        offset = Rook.GetHorizontalOffset(square);

        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 0; j < 8; j++)
            {
                int index = i + ((j + 1) * offset[k]);

                if (index < 0 || index > 63) break;
                if (squares[index].IsEmpty()) continue;

                char piece = Char.ToUpper(squares[index].piece);
                if (!squares[index].IsEmpty() && Char.IsUpper(squares[index].piece) != GameManager.instance.white) // Freindly Piece
                {
                    break;
                }
                else if (squares[index].position[0] == 'h')
                {
                    if (Char.IsUpper(squares[index].piece) == GameManager.instance.white && piece == 'Q' || piece == 'R')
                    {
                        return true;
                    }
                    if (offset[k] == 1) break;
                }
                else if (squares[index].position[0] == 'a')
                {
                    if (Char.IsUpper(squares[index].piece) == GameManager.instance.white && piece == 'Q' || piece == 'R')
                    {
                        return true;
                    }
                    if (offset[k] == -1) break;
                }
                else if (squares[index].position[0] == 'a')
                {
                    if (Char.IsUpper(squares[index].piece) == GameManager.instance.white && piece == 'Q' || piece == 'R')
                    {
                        return true;
                    }
                    if (offset[k] == -1) break;
                }
                else if (!squares[index].IsEmpty() && Char.IsUpper(squares[index].piece) == GameManager.instance.white) // Square ContainsKey enemy piece, piece can be taken then break 
                {
                    if (piece == 'R' || piece == 'Q')
                    {
                        return true;
                    }
                    break;
                }
            }
        }
        return false;
    }

    private bool KingVerticalCheck(Square square, int i)
    {
        int[] offset = new int[] { 8, -8 };

        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 1; j < 9; j++)
            {
                int index = i + (j * offset[k]);

                if (index < 0 || i + (j * offset[k]) > 63) break;
                if (squares[i + (j * offset[k])].IsEmpty()) continue;

                char piece = Char.ToUpper(squares[i + (j * offset[k])].piece);

                if (!squares[i + (j * offset[k])].IsEmpty() && Char.IsUpper(squares[i + (j * offset[k])].piece) != GameManager.instance.white) // Friendly Piece
                {
                    break;
                }
                else if (!squares[i + (j * offset[k])].IsEmpty() && Char.IsUpper(squares[i + (j * offset[k])].piece) == GameManager.instance.white) // Enemy Piece
                {
                    if (piece == 'R' || piece == 'Q')
                    {
                        return true;
                    }
                    break;
                }
            }
        }
        return false;
    }

    private bool KingDiagnolCheck(Square square, int i)
    {
        int[] offset;

        bool stopAtH = false;
        bool stopAtA = false;
        if (square.position[0] == 'a')
        {
            stopAtH = true;
            offset = new int[] { 9, -7 };
        }
        else if (square.position[0] == 'h')
        {
            stopAtA = true;
            offset = new int[] { 7, -9 };
        }
        else
        {
            stopAtA = true;
            stopAtH = true;
            offset = new int[] { 9, -9, 7, -7 };
        }


        for (int k = 0; k < offset.Length; k++)
        {
            for (int j = 1; j < 9; j++)
            {
                if (i + (j * offset[k]) < 0 || i + (j * offset[k]) > 63) { break; }
                if (squares[i + (j * offset[k])].IsEmpty())
                {
                    if (stopAtA && squares[i + (j * offset[k])].position[0] == 'a')
                        break;
                    else if (stopAtH && squares[i + (j * offset[k])].position[0] == 'h')
                        break;
                    continue;
                }
                char piece = (squares[i + (j * offset[k])].piece);

                if (!squares[i + (j * offset[k])].IsEmpty() && Char.IsUpper(squares[i + (j * offset[k])].piece) != GameManager.instance.white) // Freindly Piece
                {
                    break;
                }
                else if (!squares[i + (j * offset[k])].IsEmpty() && Char.IsUpper(squares[i + (j * offset[k])].piece) == GameManager.instance.white) // Enemy Piece
                {
                    if (piece == 'B' || piece == 'Q')
                    {
                        return true;
                    }
                    break;
                }
            }
        }
        return false;
    }

    private bool KingPawnCheck(Square square, int i)
    {
        int[] offset;
        if (GameManager.instance.white)
            offset = new int[] { -7, -9 };
        else
            offset = new int[] { 7, 9 };

        foreach (int j in offset)
        {
            if (i + j > 63 || i + j < 0) continue;
            if (Char.ToUpper(squares[i + j].piece) == 'P' && Char.IsUpper(squares[i + j].piece) == GameManager.instance.white)
            {
                return true;
            }
        }
        return false;
    }

    private bool KingKnightCheck(Square square, int i)
    {
        int[] offset = Knight.GetOffset(square.position);
        foreach (int j in offset)
        {
            if (i + j > 63 || i + j < 0) continue;
            if (Char.ToUpper(squares[i + j].piece) == 'N' && Char.IsUpper(squares[i + j].piece) == GameManager.instance.white)
            {
                return true;
            }
        }
        return false;
    }

    private void CheckDiagnolPins(int i, Square square, int kingIndex)
    {

        List<List<int>> allSquaresToCheck = diagnolOffsetDictionary[i];

        foreach (var dir in allSquaresToCheck)
        {
            bool pinPossible = dir.Contains(kingIndex);
            if (pinPossible)
            {
                GetAttackedPoints(dir, kingIndex, pinPossible);
            }
        }

        // bool stopAtA = false;
        // bool stopAtH = false;
        // int[] offset;

        // bool mod9 = ((i % 9) == (kingIndex % 9));
        // bool mod7 = ((i % 7) == (kingIndex % 7));

        // bool pinPossible = (mod9 || mod7);

        // // Get correct offsets
        // if (square.position[0] == 'a')
        // {
        //     stopAtH = true;
        //     offset = new int[] { -7, 9 };
        // }
        // else if (square.position[0] == 'h')
        // {
        //     stopAtA = true;
        //     offset = new int[] { 7, -9 };
        // }
        // else
        // {
        //     stopAtA = true;
        //     stopAtH = true;
        //     offset = new int[] { 9, -9, 7, -7 };
        // }

        // for (int k = 0; k < offset.Length; k++)
        // {
        //     bool pathBlocked = false;
        //     int possiblePinnedPiece = -1;
        //     for (int j = 1; j < 9; j++)
        //     {
        //         int index = i + (j * offset[k]);

        //         if (index < 0 || index > 63) break;

        //         if (squares[index].IsEmpty() && !pathBlocked) // Square is empty
        //         {
        //             attackedSquares.Add(index);
        //         }
        //         else if (Char.IsUpper(squares[index].piece) != GameManager.instance.white)
        //         {
        //             break; // ?
        //         }
        //         else if (Char.IsUpper(squares[index].piece) == GameManager.instance.white)
        //         {

        //             if (!pathBlocked && index == kingIndex) // This is just a check.
        //             { checkCount++; break; }
        //             else if (pathBlocked && index == kingIndex) // This is when a piece is pinned
        //             {
        //                 pinnedPieces.Add(possiblePinnedPiece);
        //                 break;
        //             }
        //             else if (!pinPossible) break;
        //             else if (!pathBlocked)
        //             {
        //                 pathBlocked = true;
        //                 possiblePinnedPiece = index;
        //                 attackedSquares.Add(index);
        //             }
        //             else if (pathBlocked && kingIndex != index) // Two friendly pieces protect king from pin
        //             {
        //                 break;
        //             }
        //         }

        //         if (stopAtA && squares[index].position[0] == 'a')
        //             break;
        //         else if (stopAtH && squares[index].position[0] == 'h')
        //             break;
        //     }
        // }
    }

    private async void CheckHorizontalPins(int i, int kingIndex, Square square)
    {

        List<List<int>> allSquaresToCheck = offsetHorizontalDictionary[i];

        foreach (var dir in allSquaresToCheck)
        {
            bool pinPossible = dir.Contains(kingIndex);
            if (pinPossible)
            {
                GetAttackedPoints(dir, kingIndex, pinPossible);
            }
        }

        // int squaresToTheLeft = i % 8;
        // int squaresToTheRight = 8 - (squaresToTheLeft + 1); // The 1 is to account for the square the piece is on

        // List<int> offsetsToTheLeft = new List<int>();
        // List<int> offsetsToTheRight = new List<int>();

        // for (int index = 1; index <= squaresToTheRight; index++)
        // {
        //     offsetsToTheRight.Add(i + index);
        // }
        // for (int index = 1; index <= squaresToTheLeft; index++)
        // {
        //     offsetsToTheLeft.Add(i - index);
        // }

        // bool pinPossible = (offsetsToTheLeft.Contains(kingIndex) || offsetsToTheRight.Contains(kingIndex));

        // GetAttackedPoints(offsetsToTheLeft, kingIndex, pinPossible);
        // GetAttackedPoints(offsetsToTheRight, kingIndex, pinPossible);
    }

    private async void CheckVerticalPins(int i, int kingIndex, Square square)
    {

        List<List<int>> allSquaresToCheck = offsetVerticalDictionary[i];

        foreach (var dir in allSquaresToCheck)
        {
            bool pinPossible = dir.Contains(kingIndex);
            if (pinPossible)
            {
                GetAttackedPoints(dir, kingIndex, pinPossible);
            }
        }

        // int squaresAbove = i / 8;
        // int squaresBelow = 8 - (squaresAbove + 1); // The 1 is to account for the square the piece is on

        // List<int> upSquaresToCheckList = new List<int>();
        // List<int> downSquaresToCheckList = new List<int>();

        // for (int j = 1; j < (int)(i / 8) + 1; j++)
        // {
        //     downSquaresToCheckList.Add(i - (j * 8));
        // }
        // for (int j = 1; j < 8 - (int)(((i) / 8)); j++)
        // {
        //     upSquaresToCheckList.Add(i + (j * 8));
        // }

        // bool pinPossible = (upSquaresToCheckList.Contains(kingIndex) || downSquaresToCheckList.Contains(kingIndex));

        // GetAttackedPoints(upSquaresToCheckList, kingIndex, pinPossible);
        // GetAttackedPoints(downSquaresToCheckList, kingIndex, pinPossible);
    }

    private void GetAttackedPoints(List<int> offsets, int kingIndex, bool pinPossible)
    {
        int possiblePinnedPiece = -1;
        bool pathBlocked = false;
        foreach (int index in offsets)
        {
            if (squares[index].IsEmpty() && !pathBlocked) // Square is empty
            {
                attackedSquares.Add(index);
            }
            else if (Char.IsUpper(squares[index].piece) != GameManager.instance.white) // Contains a freindly piece
            {
                break; // ?
            }
            else if (Char.IsUpper(squares[index].piece) == GameManager.instance.white) // Contains an enemy piece. There is some condition here. 
            {
                if (!pathBlocked && index == kingIndex) // This is just a check.
                { checkCount++; break; }
                else if (pathBlocked && index == kingIndex) // This is when a piece is pinned
                {
                    pinnedPieces.Add(possiblePinnedPiece);
                    break;
                }
                else if (!pinPossible) break;
                else if (!pathBlocked)
                {
                    pathBlocked = true;
                    possiblePinnedPiece = index;
                    attackedSquares.Add(index);
                }
                else if (pathBlocked && kingIndex != index) // Two friendly pieces protect king from pin
                {
                    break;
                }
            }
        }
    }

    private void CheckPawnAttackedPoints(int i, Square square)
    {
        int mult = (GameManager.instance.white ? 1 : -1);



    }
}
