using System.Collections;

using System.Collections.Generic;

using System;
using System.Threading;
using System.IO;

using UnityEngine;

// rnbqkbnrpppppppp8888PPPPPPPPRNBQKBNR
// 83k42b1r35B24N35K288 -- Bishop pin
// 83k42b584N32r1BK288 -- Rook pin

public class Board : MonoBehaviour
{
    // {'P': 10, 'N': 35, 'B': 35, 'R': 52.5, 'Q': 100, 'K': 1000, 'p': -10, 'n': -35, 'b': -35, 'r': -52.5, 'q': -100, 'k': -1000}

    public int whiteKingIndex;
    public int blackKingIndex;
    private int checkCount;
    private const PieceE BLACK_KING = PieceE.King | PieceE.Black;
    private const PieceE WHITE_KING = PieceE.King | PieceE.White;

    public System.Diagnostics.Stopwatch stopWatch;

    private Dictionary<char, float> pieceVal;
    // private Dictionary<char, List<int>> pieceDictionary;

    public HashSet<int> whitePieces;
    public HashSet<int> blackPieces;
    public HashSet<int> currentPieceIndex;


    public Square[] squares;
    public List<Square> squaresList;
    public List<Move> legalMoves;
    public HashSet<int> pieceIndex;
    private HashSet<int> pinnedPieces; // These pieces cannot move. 


    private Dictionary<int, HashSet<HashSet<int>>> offsetVerticalDictionary;
    private Dictionary<int, HashSet<HashSet<int>>> offsetHorizontalDictionary;
    private Dictionary<int, HashSet<HashSet<int>>> diagnolOffsetDictionary;
    private Dictionary<int, int[]> kingOffsetDictionary;
    private Dictionary<int, HashSet<int>> knightOffsetDictionary;
    private Dictionary<int, HashSet<int>> pinnedPieceDictionary;

    private Dictionary<int, HashSet<int>> whitePawnAttackDictionary;
    private Dictionary<int, HashSet<int>> whitePawnMoveDictionary;
    private Dictionary<int, HashSet<int>> blackPawnAttackDictionary;
    private Dictionary<int, HashSet<int>> blackPawnMoveDictionary;


    private HashSet<int> kingCheckedSquares;

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
        currentPieceIndex = new HashSet<int>();
        pinnedPieces = new HashSet<int>();
        attackedSquares = new HashSet<int>();
        kingCheckedSquares = new HashSet<int>();
        offsetVerticalDictionary = new Dictionary<int, HashSet<HashSet<int>>>();
        offsetHorizontalDictionary = new Dictionary<int, HashSet<HashSet<int>>>();
        diagnolOffsetDictionary = new Dictionary<int, HashSet<HashSet<int>>>();
        kingOffsetDictionary = new Dictionary<int, int[]>();
        knightOffsetDictionary = new Dictionary<int, HashSet<int>>();
        pinnedPieceDictionary = new Dictionary<int, HashSet<int>>();
        whitePawnAttackDictionary = new Dictionary<int, HashSet<int>>();
        whitePawnMoveDictionary = new Dictionary<int, HashSet<int>>();
        blackPawnAttackDictionary = new Dictionary<int, HashSet<int>>();
        blackPawnMoveDictionary = new Dictionary<int, HashSet<int>>();
        stopWatch = new System.Diagnostics.Stopwatch();
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
    public void ConvertSquareListToArray()
    {
        squares = squaresList.ToArray();
    }

    public int GetIndex(string position)
    {
        return (((8 - ((int)(position[1] - '0'))) * 8) + ((int)position[0] - 97));
    }

    private void InitializePieceDictionary()
    {
        for (int i = 0; i < squares.Length; i++)
        {
            if (squares[i].piece != PieceE.None)
            {
                if ((squares[i].piece & PieceE.Color) == PieceE.White)
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
            CreatePawnDictionary(i, squares[i]);
        }
    }

    /* 
        This region creates an entire dictionary on startup of every possible move for each 
        move type. 
        Dictionary takes in an index on board, returns a hashset of all the moves possible from that position
        This is then used to check if those spaces are empty, blocked, etc. to test legal moves.
        This is much faster than calculating, at runtime, the possible positions a bishop can play
        at e3, something that is constant no matter the board. 
        Dictionary<int index, Hashset possibleIndiciesToMoveTo> 
    */

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

        HashSet<HashSet<int>> allSquaresToCheck = new HashSet<HashSet<int>>();

        for (int k = 0; k < offset.Length; k++)
        {
            HashSet<int> tempSquares = new HashSet<int>();
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

        HashSet<int> upSquaresToCheckList = new HashSet<int>();
        HashSet<int> downSquaresToCheckList = new HashSet<int>();

        for (int j = 1; j <= squaresBelow; j++)
        {
            downSquaresToCheckList.Add(index + (j * 8));
        }
        for (int j = 1; j <= squaresAbove; j++)
        {
            upSquaresToCheckList.Add(index - (j * 8));
        }

        HashSet<HashSet<int>> allSquaresToCheck = new HashSet<HashSet<int>>();

        allSquaresToCheck.Add(upSquaresToCheckList);
        allSquaresToCheck.Add(downSquaresToCheckList);

        offsetVerticalDictionary.Add(index, allSquaresToCheck);
    }

    private void CreateHorizontalDictionary(int index)
    {
        int squaresLeft = index % 8;
        int squaresRight = 8 - (squaresLeft + 1); // The 1 is to account for the square the piece is on

        HashSet<int> leftSquaresToCheckList = new HashSet<int>();
        HashSet<int> rightSquaresToCheckList = new HashSet<int>();

        for (int j = 1; j <= squaresRight; j++)
        {
            rightSquaresToCheckList.Add(index + j);
        }
        for (int j = 1; j <= squaresLeft; j++)
        {
            leftSquaresToCheckList.Add(index - j);
        }

        HashSet<HashSet<int>> allSquaresToCheck = new HashSet<HashSet<int>>();

        allSquaresToCheck.Add(leftSquaresToCheckList);
        allSquaresToCheck.Add(rightSquaresToCheckList);

        offsetHorizontalDictionary.Add(index, allSquaresToCheck);
    }

    private void CreatePawnDictionary(int i, Square square)
    {
        HashSet<int> whiteAttacks = new HashSet<int>();
        HashSet<int> whiteOffsets = new HashSet<int>();
        HashSet<int> blackAttacks = new HashSet<int>();
        HashSet<int> blackOffsets = new HashSet<int>();

        if (square.position[0] == 'a')
        {
            whiteAttacks.Add(i + -7);
            blackAttacks.Add(i + 9);
        }
        else if (square.position[0] == 'h')
        {
            whiteAttacks.Add(i + -9);
            blackAttacks.Add(i + 7);
        }
        else
        {
            whiteAttacks.Add(i - 9);
            whiteAttacks.Add(i - 7);
            blackAttacks.Add(i + 9);
            blackAttacks.Add(i + 7);
        }
        if (square.position[1] == '2')
        {
            whiteOffsets.Add(i + -16);
        }
        else if (square.position[1] == '7')
        {
            blackOffsets.Add(i + 16);
        }
        whiteOffsets.Add(i - 8);
        blackOffsets.Add(i + 8);
        whitePawnAttackDictionary.Add(i, whiteAttacks);
        whitePawnMoveDictionary.Add(i, whiteOffsets);
        blackPawnAttackDictionary.Add(i, blackAttacks);
        blackPawnMoveDictionary.Add(i, blackOffsets);
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

        HashSet<int> officialOffset = new HashSet<int>();
        for (int i = 0; i < offset.Length; i++)
        {
            if (index + offset[i] > 63 || index + offset[i] < 0) continue;
            officialOffset.Add(index + offset[i]);
        }

        knightOffsetDictionary.Add(index, officialOffset);
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
            offset = new int[] { 1, 8, 9 };
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
            offset = new int[] { 1, 8, -8, -7, 9 };
        }
        // On right side
        else if (square.position[0] == 'h')
        {
            offset = new int[] { -1, 8, -8, -9, 7 };
        }
        else
        {
            offset = new int[] { 1, -1, 8, -8, 7, -7, 9, -9 };
        }

        int[] officialOffset = new int[offset.Length];
        for (int i = 0; i < offset.Length; i++)
        {
            officialOffset[i] = index + offset[i];
        }

        kingOffsetDictionary.Add(index, officialOffset);
    }

    #endregion


    private void GetKingIndex()
    {
        for (int i = 0; i < squares.Length; i++)
        {
            if (squares[i].piece == (WHITE_KING))
            {
                // 6
                whiteKingIndex = i;
            }
            else if (squares[i].piece == (BLACK_KING))
            {
                blackKingIndex = i;
            }
        }
    }

    // Takes current board and produces fen value
    private string CreateFen()
    {
        string fen = "";
        int countEmpty = 0;
        int count = 0;
        foreach (Square square in squares)
        {
            count++;

            if (square.IsEmpty())
            {
                countEmpty++;
            }
            else
            {
                if (countEmpty != 0)
                {
                    fen += countEmpty.ToString();
                    countEmpty = 0;
                }
                fen += square.piece;
            }
            if (count % 8 == 0)
            {
                if (countEmpty != 0)
                {
                    fen += countEmpty.ToString();
                    countEmpty = 0;
                }
                fen += "/";
            }
        }
        return fen;
    }

    // Function to be used for debugging to see board output
    // when something goes wrong.
    public void PrintBoard(string path)
    {
        string stringToPrint = "";
        int count = 0;
        foreach (Square square in squares)
        {
            count++;
            if (square.piece == PieceE.None)
            {
                stringToPrint += "   |";
            }
            else
            {
                stringToPrint += " " + square.piece + " |";
            }
            if (count % 8 == 0)
            {
                stringToPrint += " " + ((8 - (count / 8) + 1)).ToString() + "\n";
                for (int i = 0; i < 8; i++)
                {
                    stringToPrint += "————";
                }
                stringToPrint += "\n";
            }
        }
        for (int i = 0; i < 8; i++)
        {
            stringToPrint += "————";
        }
        stringToPrint += "\n";
        stringToPrint += " a | b | c | d | e | f | g | h ";
        stringToPrint += "\n\n\nFen: " + CreateFen();
        File.WriteAllText(path, stringToPrint);

    }

    /*
     * Returns a set of psuedo legal moves in string form such as e2e4
     * Returns a set of possibly illegal moves. These are then parsed
     * through and the illegal moves will be thrown out in 
     * the GetLegalMoves() function.
    */
    public void GetPsuedoLegalMoves()
    {
        // Clear is faster than setting object to new instance.
        pieceIndex.Clear();
        attackedSquares.Clear();
        pinnedPieceDictionary.Clear();
        pinnedPieces.Clear();
        kingCheckedSquares.Clear();
        checkmate = false;
        stalemate = false;
        // Make pieceIndex a set of all the pieces from whitePieces and blackPieces.
        pieceIndex.UnionWith(whitePieces);
        pieceIndex.UnionWith(blackPieces);
        checkCount = 0;

        currentPieceIndex = (GameManager.instance.white ? whitePieces : blackPieces);
        // IMPORTANT: pieceIndex is CRUCIAL to the function of the CalculatePos() function in AI.cs
        // DO NOT DELTE IT unless being replaced. 


        // Get pinned pieces and attacked squares
        foreach (int i in (GameManager.instance.white ? blackPieces : whitePieces))
        {

            switch (squares[i].piece ^ (squares[i].piece & PieceE.Color)) // This gets just the piece value
            {
                case PieceE.Pawn:
                    stopWatch.Start();
                    CheckPawnAttacks(i, (GameManager.instance.white ? whiteKingIndex : blackKingIndex), squares[i]);
                    stopWatch.Stop();
                    break;
                case PieceE.Bishop:
                    GetBishopAttacks(i, (GameManager.instance.white ? whiteKingIndex : blackKingIndex));
                    break;
                case PieceE.Rook:
                    GetRookAttacks(i, (GameManager.instance.white ? whiteKingIndex : blackKingIndex));
                    break;
                case PieceE.Queen:
                    GetQueenAttacks(i, (GameManager.instance.white ? whiteKingIndex : blackKingIndex));
                    break;
                case PieceE.Knight:
                    GetKnightAttacks(i, (GameManager.instance.white ? whiteKingIndex : blackKingIndex));
                    break;
                case PieceE.King:
                    GetKingAttacks(i);
                    break;
                default:
                    throw new Exception("Error during piece identification");
                    break;
            }
        }

        legalMoves.Clear();


        if (checkCount >= 2) { GetKingMoves(squares[GameManager.instance.white ? whiteKingIndex : blackKingIndex], GameManager.instance.white ? whiteKingIndex : blackKingIndex); return; }


        foreach (int i in (GameManager.instance.white ? whitePieces : blackPieces))
        {
            switch (squares[i].piece ^ (squares[i].piece & PieceE.Color)) // Gets just the piece not the color
            {
                case PieceE.Pawn:
                    GetPawnMoves(squares[i], i);
                    break;
                case PieceE.Rook:
                    GetRookMoves(squares[i], i);
                    break;
                case PieceE.Knight:
                    GetKnightMoves(squares[i], i);
                    break;
                case PieceE.Bishop:
                    GetBishopMoves(squares[i], i);
                    break;
                case PieceE.Queen:
                    GetQueenMoves(squares[i], i);
                    break;
                default:
                    break;
            }
        }
        GetKingMoves(squares[GameManager.instance.white ? whiteKingIndex : blackKingIndex], GameManager.instance.white ? whiteKingIndex : blackKingIndex);
        if (checkCount == 0) GetCastleMove();
    }

    private void GetBishopAttacks(int i, int kingIndex)
    {
        CheckDiagnolPins(i, squares[i], kingIndex);
    }

    private void GetQueenAttacks(int i, int kingIndex)
    {
        CheckHorizontalPins(i, kingIndex, squares[i]);
        CheckVerticalPins(i, kingIndex, squares[i]);
        CheckDiagnolPins(i, squares[i], kingIndex);
    }

    private void GetRookAttacks(int i, int kingIndex)
    {
        CheckHorizontalPins(i, kingIndex, squares[i]);
        CheckVerticalPins(i, kingIndex, squares[i]);
    }

    /*
     * Parses through the psuedo legal moves and 
     * throws out the illegal moves
     * Returns a set of proper legal moves.
    */
    public Move[] GetLegalMoves()
    {
        GetPsuedoLegalMoves();
        if (legalMoves.Count == 0 && checkCount == 0)
        {
            stalemate = true;
            // PrintBoard("Assets/Resources/PrintedBoard.txt");
        }
        else if (legalMoves.Count == 0 && checkCount > 0)
        {
            checkmate = true;
            // PrintBoard("Assets/Resources/PrintedBoard.txt");
        }
        // foreach (var i in legalMoves)
        // {
        //     Debug.Log(i.ToString());
        // }
        return legalMoves.ToArray();
    }

    public void MakeMove(Move move)
    {
        if (move.isCastle)
        {
            if (move.castle.Equals("BLACK-QUEEN"))
            {
                squares[0].piece = PieceE.None;
                squares[3].piece = PieceE.Rook | PieceE.Black;
                squares[2].piece = BLACK_KING;
                squares[4].piece = PieceE.None;
                blackPieces.Remove(0);
                blackPieces.Remove(4);
                blackPieces.Add(2);
                blackPieces.Add(3);

                pieceIndex.Remove(0);
                pieceIndex.Remove(4);
                pieceIndex.Add(2);
                pieceIndex.Add(3);
                blackKingIndex = 2;
            }
            else if (move.castle.Equals("BLACK-KING"))
            {
                squares[7].piece = PieceE.None;
                squares[5].piece = PieceE.Rook | PieceE.Black;
                squares[6].piece = BLACK_KING;
                squares[4].piece = PieceE.None;
                blackPieces.Remove(7);
                blackPieces.Remove(4);
                blackPieces.Add(6);
                blackPieces.Add(5);

                pieceIndex.Remove(7);
                pieceIndex.Remove(4);
                pieceIndex.Add(6);
                pieceIndex.Add(5);
                blackKingIndex = 6;
            }
            else if (move.castle.Equals("WHITE-QUEEN"))
            {
                squares[56].piece = PieceE.None;
                squares[59].piece = PieceE.Rook | PieceE.White;
                squares[58].piece = WHITE_KING;
                squares[60].piece = PieceE.None;
                whitePieces.Remove(56);
                whitePieces.Remove(60);
                whitePieces.Add(58);
                whitePieces.Add(59);

                pieceIndex.Remove(56);
                pieceIndex.Remove(60);
                pieceIndex.Add(58);
                pieceIndex.Add(59);
                whiteKingIndex = 58;

            }
            else if (move.castle.Equals("WHITE-KING"))
            {
                squares[63].piece = PieceE.None;
                squares[61].piece = PieceE.Rook | PieceE.White;
                squares[62].piece = WHITE_KING;
                squares[60].piece = PieceE.None;
                whitePieces.Remove(63);
                whitePieces.Remove(60);
                whitePieces.Add(62);
                whitePieces.Add(61);

                pieceIndex.Remove(63);
                pieceIndex.Remove(60);
                pieceIndex.Add(62);
                pieceIndex.Add(61);
                whiteKingIndex = 62;
            }
            PrintBoard("Assets/Resources/CastleBoard.txt");
            GameManager.instance.white = !GameManager.instance.white;
            return;
        }

        // For debugging purposes, in the likely scenario something breaks and this happens. 
        // if (Char.ToUpper(move.newSquare.piece) == 'K' && Char.ToUpper(move.original.piece) != 'K')
        // {
        //     Debug.Log("King taken by: " + move.original.piece + " by performing move: " + move.ToString());
        //     PrintBoard("Assets/Resources/KingTakenBoard.txt");
        // }

        if (PieceEnum.IsPawn(move.original.piece)) // If the piece is a pawn
        {
            if (move.newSquare.position[1] == '8' || move.newSquare.position[1] == '1')
            {
                if (!PieceEnum.IsWhite(move.original.piece))
                    move.newSquare.piece = PieceE.Queen | PieceE.Black;
                else
                    move.newSquare.piece = PieceE.Queen | PieceE.White;
                move.original.piece = PieceE.None;
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
        move.original.piece = PieceE.None;
        if (GameManager.instance.white)
        {
            if (PieceEnum.IsKing(move.newSquare.piece))
                whiteKingIndex = move.newSquare.index;
            whitePieces.Remove(move.original.index);
            whitePieces.Add(move.newSquare.index);
            move.tookOppositeColor = blackPieces.Remove(move.newSquare.index);
        }
        else
        {
            if (PieceEnum.IsKing(move.newSquare.piece))
                blackKingIndex = move.newSquare.index;
            blackPieces.Remove(move.original.index);
            blackPieces.Add(move.newSquare.index);
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
                squares[0].piece = PieceE.Rook | PieceE.Black;
                squares[3].piece = PieceE.None;
                squares[2].piece = PieceE.None;
                squares[4].piece = BLACK_KING;
                blackPieces.Add(4);
                blackPieces.Remove(3);
                blackPieces.Remove(2);
                blackPieces.Add(0);

                pieceIndex.Add(4);
                pieceIndex.Remove(3);
                pieceIndex.Remove(2);
                pieceIndex.Add(0);
                blackKingIndex = 4;
            }
            else if (move.castle.Equals("BLACK-KING"))
            {
                squares[7].piece = PieceE.Rook | PieceE.Black;
                squares[5].piece = PieceE.None;
                squares[6].piece = PieceE.None;
                squares[4].piece = BLACK_KING;
                blackPieces.Add(4);
                blackPieces.Remove(5);
                blackPieces.Remove(6);
                blackPieces.Add(7);

                pieceIndex.Add(4);
                pieceIndex.Remove(5);
                pieceIndex.Remove(6);
                pieceIndex.Add(7);
                blackKingIndex = 4;
            }
            else if (move.castle.Equals("WHITE-QUEEN"))
            {
                squares[56].piece = PieceE.Rook | PieceE.White;
                squares[59].piece = PieceE.None;
                squares[58].piece = PieceE.None;
                squares[60].piece = WHITE_KING;
                whitePieces.Add(60);
                whitePieces.Remove(59);
                whitePieces.Remove(58);
                whitePieces.Add(56);

                pieceIndex.Add(60);
                pieceIndex.Remove(59);
                pieceIndex.Remove(58);
                pieceIndex.Add(56);
                whiteKingIndex = 60;
            }
            else if (move.castle.Equals("WHITE-KING"))
            {
                squares[63].piece = PieceE.Rook | PieceE.White;
                squares[61].piece = PieceE.None;
                squares[62].piece = PieceE.None;
                squares[60].piece = WHITE_KING;
                whitePieces.Add(60);
                whitePieces.Remove(61);
                whitePieces.Remove(62);
                whitePieces.Add(63);

                pieceIndex.Add(60);
                pieceIndex.Remove(61);
                pieceIndex.Remove(62);
                pieceIndex.Add(63);
                whiteKingIndex = 60;
            }
            return;
        }


        if (move.pawnPromote)
        {
            if (move.newSquare.piece == (PieceE.Pawn | PieceE.Black))
            {
                move.original.piece = (PieceE.Pawn | PieceE.Black);
            }
            else
            {
                move.original.piece = (PieceE.Pawn | PieceE.White);
            }
            move.newSquare.piece = move.pieceNew;
        }
        else
        {
            move.original.piece = move.pieceOriginal;
            move.newSquare.piece = move.pieceNew;
        }
        if (GameManager.instance.white)
        {
            if (PieceEnum.IsKing(move.original.piece))
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
            if (PieceEnum.IsKing(move.original.piece))
                blackKingIndex = move.original.index;
            blackPieces.Add(move.original.index);
            blackPieces.Remove(move.newSquare.index);
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
        if (squares[0].pieceMoved == false && squares[4].pieceMoved == false && squares[4].piece == (BLACK_KING) && squares[0].piece == (PieceE.Rook | PieceE.Black))
        {
            // Now check if their are any pieces in the way.
            for (int i = 1; i < 4; i++)
            {
                if (!squares[i].IsEmpty() || attackedSquares.Contains(i))
                {
                    return;
                }
            }
            legalMoves.Add(new Move(squares[4], squares[2], -1, isCastle: true, castle: "BLACK-QUEEN"));
        }
    }

    /* 
     * Checks black king side castle availability
     * Adds legal move is available
     */
    private void CheckBlackKingSideCastle()
    {
        if (squares[7].pieceMoved == false && squares[4].pieceMoved == false && squares[4].piece == (BLACK_KING) && squares[7].piece == (PieceE.Rook | PieceE.Black))
        {
            // Now check if their are any pieces in the way.
            for (int i = 4; i <= 6; i++)
            {
                if (!squares[i].IsEmpty() || attackedSquares.Contains(i))
                {
                    return;
                }
            }
            legalMoves.Add(new Move(squares[4], squares[6], -1, isCastle: true, castle: "BLACK-KING"));
        }
    }

    /* 
     * Checks white queen side castle availability
     * Adds legal move is available
     */
    private void CheckWhiteQueenSideCastle()
    {
        if (squares[56].pieceMoved == false && squares[60].pieceMoved == false && squares[60].piece == (WHITE_KING) && squares[56].piece == (PieceE.Rook | PieceE.White))
        {
            // Now check if their are any pieces in the way.
            for (int i = 58; i < 60; i++)
            {
                if (!squares[i].IsEmpty() || attackedSquares.Contains(i))
                {
                    return;
                }
            }
            legalMoves.Add(new Move(squares[60], squares[58], -1, isCastle: true, castle: "WHITE-QUEEN"));
        }
    }

    /* 
     * Checks white king side castle availability
     * Adds legal move is available
     */
    private void CheckWhiteKingSideCastle()
    {
        if (squares[63].pieceMoved == false && squares[60].pieceMoved == false && squares[60].piece == (WHITE_KING) && squares[63].piece == (PieceE.Rook | PieceE.White))
        {
            // Now check if their are any pieces in the way.
            for (int i = 61; i < 63; i++)
            {
                if (!squares[i].IsEmpty() || attackedSquares.Contains(i))
                {
                    return;
                }
            }
            // We do this last because this is the longest operation.
            legalMoves.Add(new Move(squares[60], squares[62], -1, isCastle: true, castle: "WHITE-KING"));
        }
    }

    /* 
     * Uppercase is white pieces. Lowercase is black pieces.
     * If the pieces are on the correct spots and the pieces both have not moved and the king is not in check.
     * Add the move as valid. 
    */
    private void GetCastleMove()
    {
        if (GameManager.instance.white)
        {
            CheckWhiteKingSideCastle();
            CheckWhiteQueenSideCastle();
            return;
        }
        CheckBlackKingSideCastle();
        CheckBlackQueenSideCastle();

    }

    #endregion

    private void GetKingMoves(Square square, int i)
    {
        int[] offset = kingOffsetDictionary[i];

        foreach (int index in offset)
        {
            bool requirement = (squares[index].IsEmpty() || PieceEnum.IsWhite(squares[index].piece) != GameManager.instance.white);
            if (!attackedSquares.Contains(index) && requirement)
            {
                legalMoves.Add(new Move(squares[i], squares[index], index));
            }

        }
    }

    private void GetPawnMoves(Square square, int i)
    {
        if (checkCount == 1 && pinnedPieces.Contains(i)) return;
        if (square.position[1] == '8' || square.position[1] == '1')
        {
            if (!PieceEnum.IsWhite(square.piece))
                square.piece = PieceE.Queen | PieceE.Black;
            else
                square.piece = PieceE.Queen | PieceE.White;
            GetQueenMoves(square, i);
            return;
        }

        // Make this part a dictionary similar to how we do it with other pieces except separate it by color.

        int mult = (GameManager.instance.white ? -1 : 1);

        HashSet<int> attackOffset = (GameManager.instance.white ? whitePawnAttackDictionary[i] : blackPawnAttackDictionary[i]);
        HashSet<int> offset = (GameManager.instance.white ? whitePawnMoveDictionary[i] : blackPawnMoveDictionary[i]);

        var pinned = pinnedPieces.Contains(i);

        if (!pinned && checkCount == 0)
        {
            foreach (int index in attackOffset)
            {
                if (!squares[index].IsEmpty() && PieceEnum.IsWhite(squares[index].piece) != GameManager.instance.white)
                {
                    legalMoves.Add(new Move(squares[i], squares[index], index));
                }
            }
            foreach (int index in offset)
            {
                if (Math.Abs(index - i) == 16)
                {
                    if (!squares[i + (8 * mult)].IsEmpty()) continue;
                }
                if (squares[index].IsEmpty())
                {
                    legalMoves.Add(new Move(squares[i], squares[index], index));
                }
            }
        }
        else if (pinnedPieces.Contains(i))
        {
            HashSet<int> pinnedSquares = pinnedPieceDictionary[i];
            if (pinnedSquares.Overlaps(attackOffset))
            {
                foreach (int index in attackOffset)
                {
                    if (pinnedSquares.Contains(index) && !squares[index].IsEmpty() && PieceEnum.IsWhite(squares[index].piece) != GameManager.instance.white)
                    {
                        legalMoves.Add(new Move(squares[i], squares[index], index));
                        break;
                    }
                }
            }
            return;
        }
        if (checkCount == 1)
        {
            if (kingCheckedSquares.Overlaps(attackOffset))
            {
                foreach (int index in attackOffset)
                {
                    if (kingCheckedSquares.Contains(index) && !squares[index].IsEmpty() && PieceEnum.IsWhite(squares[index].piece) != GameManager.instance.white)
                    {
                        legalMoves.Add(new Move(squares[i], squares[index], index));
                        break;
                    }
                }
            }
            if (kingCheckedSquares.Overlaps(offset))
            {
                foreach (int index in offset)
                {
                    if (Math.Abs(index - i) == 16)
                    {
                        if (!squares[i + (8 * mult)].IsEmpty()) continue;
                    }
                    if (kingCheckedSquares.Contains(index) && squares[index].IsEmpty())
                    {
                        legalMoves.Add(new Move(squares[i], squares[index], index));
                        break;
                    }
                }
            }
            return;
        }

    }

    private void GetKnightMoves(Square square, int i)
    {
        if (pinnedPieces.Contains(i))
        {
            return;
        }

        var offset = knightOffsetDictionary[i];

        if (checkCount == 1 && kingCheckedSquares.Overlaps(offset))
        {
            foreach (var index in offset)
            {
                bool requirement = squares[index].IsEmpty()
                    || PieceEnum.IsWhite(squares[index].piece) != GameManager.instance.white;
                if (kingCheckedSquares.Contains(index) && requirement)
                {
                    var m = new Move(square, squares[index], index);
                    legalMoves.Add(m);
                }
            }
            return;
        }
        else if (checkCount == 1 && !kingCheckedSquares.Overlaps(offset))
        { return; }

        foreach (var index in offset)
        {
            if (squares[index].IsEmpty()
                || PieceEnum.IsWhite(squares[index].piece) != GameManager.instance.white)
            {
                var m = new Move(square, squares[index], index);
                legalMoves.Add(m);
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

        HashSet<int> possiblePinnedSquares = new HashSet<int>();
        HashSet<HashSet<int>> allSquaresToCheck = offsetHorizontalDictionary[i];

        bool pinned = pinnedPieces.Contains(i);
        if (pinned)
        {
            possiblePinnedSquares = pinnedPieceDictionary[i];
        }

        foreach (HashSet<int> dir in allSquaresToCheck)
        {
            GetMovesInDir(dir, square, pinned, possiblePinnedSquares);
        }
    }

    private void GetVerticalMoves(Square square, int i)
    {

        HashSet<int> possiblePinnedSquares = new HashSet<int>();
        HashSet<HashSet<int>> allSquaresToCheck = offsetVerticalDictionary[i];

        bool pinned = pinnedPieces.Contains(i);
        if (pinned)
        {
            possiblePinnedSquares = pinnedPieceDictionary[i];
        }

        foreach (HashSet<int> dir in allSquaresToCheck)
        {
            GetMovesInDir(dir, square, pinned, possiblePinnedSquares);
        }
    }

    private void GetDiagnolMoves(Square square, int i)
    {
        HashSet<HashSet<int>> allSquaresToCheck = diagnolOffsetDictionary[i];
        HashSet<int> possiblePinnedSquares = new HashSet<int>();

        bool pinned = pinnedPieces.Contains(i);
        if (pinned)
        {
            possiblePinnedSquares = pinnedPieceDictionary[i];
        }

        foreach (var dir in allSquaresToCheck)
        {
            GetMovesInDir(dir, square, pinned, possiblePinnedSquares);
        }
    }

    private void GetMovesInDir(HashSet<int> dir, Square square, bool pinned, HashSet<int> possiblePinnedSquares)
    {
        if (checkCount == 0)
        {
            if (!pinned)
            {
                foreach (int index in dir)
                {
                    if (squares[index].IsEmpty()) // square is empty add to possible moves
                    {
                        legalMoves.Add(new Move(square, squares[index], index));
                    }
                    else if (PieceEnum.IsWhite(squares[index].piece) != GameManager.instance.white) // Square ContainsKey enemy piece, piece can be taken then break 
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
            else
            {
                if (!possiblePinnedSquares.Overlaps(dir))
                {
                    return;
                }
                /* 
                    If a diagnol, a bishop for example, can see any one of the squares of the same squares 
                    that are being attacked by a pinned piece it must be able to see the attacking piece and 
                    all pinned squares alike.
                */
                foreach (int index in possiblePinnedSquares)
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                }
                return;
            }
        }
        if (!pinned && checkCount == 1)
        {
            if (!kingCheckedSquares.Overlaps(dir))
            {
                return;
            }
            foreach (int index in dir)
            {
                if (squares[index].IsEmpty() && kingCheckedSquares.Contains(index))
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                }
                else if (kingCheckedSquares.Contains(index) && PieceEnum.IsWhite(squares[index].piece) != GameManager.instance.white)
                {
                    legalMoves.Add(new Move(square, squares[index], index));
                    break;
                }
                else if (squares[index].IsEmpty())
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            return;
        }
        else if (pinned && checkCount == 1) return;
    }


    private void CheckDiagnolPins(int i, Square square, int kingIndex)
    {

        HashSet<HashSet<int>> allSquaresToCheck = diagnolOffsetDictionary[i];

        foreach (var dir in allSquaresToCheck)
        {
            bool pinPossible = dir.Contains(kingIndex);
            // if (pinPossible)
            // {
            GetAttackedPoints(i, dir, kingIndex, pinPossible);
            // }
        }
    }

    private void CheckHorizontalPins(int i, int kingIndex, Square square)
    {

        HashSet<HashSet<int>> allSquaresToCheck = offsetHorizontalDictionary[i];

        foreach (var dir in allSquaresToCheck)
        {
            bool pinPossible = dir.Contains(kingIndex);
            // if (pinPossible)
            // {
            GetAttackedPoints(i, dir, kingIndex, pinPossible);
            // }
        }
    }

    private void CheckVerticalPins(int i, int kingIndex, Square square)
    {

        HashSet<HashSet<int>> allSquaresToCheck = offsetVerticalDictionary[i];

        foreach (var dir in allSquaresToCheck)
        {
            bool pinPossible = dir.Contains(kingIndex);
            // if (pinPossible)
            // {
            GetAttackedPoints(i, dir, kingIndex, pinPossible);
            // }
        }
    }

    private void GetKnightAttacks(int i, int kingIndex)
    {
        var offsets = knightOffsetDictionary[i];
        foreach (var index in offsets)
        {
            if (index == kingIndex)
            {
                checkCount++;
                kingCheckedSquares.Add(i);
            }
            else
            {
                attackedSquares.Add(index);
            }
        }
    }

    private void GetAttackedPoints(int i, HashSet<int> offsets, int kingIndex, bool pinPossible)
    {
        if (!pinPossible)
        {
            foreach (int index in offsets)
            {
                if (squares[index].IsEmpty())
                {
                    attackedSquares.Add(index);
                }
                else if (PieceEnum.IsWhite(squares[index].piece) == GameManager.instance.white)
                {
                    attackedSquares.Add(index);
                    break;
                }
                else return;
            }
            return;
        }
        if (!currentPieceIndex.Overlaps(offsets))
        {
            foreach (int index in offsets)
            {
                if (squares[index].IsEmpty())
                    attackedSquares.Add(index);
                else return;
            }
        }
        if (!pieceIndex.Overlaps(offsets))
        {
            attackedSquares.UnionWith(offsets);
            return;
        }



        int possiblePinnedPiece = -1;
        bool pathBlocked = false;
        HashSet<int> aSetOfPossiblePinnedPieces = new HashSet<int>();
        bool isChecked = false;
        // If there are no other pieces on the offsets then add them all to attacked squares

        foreach (int index in offsets)
        {

            if (isChecked)
            {
                attackedSquares.Add(index);
                break;
            }
            if (squares[index].IsEmpty() && !pathBlocked) // Square is empty
            {
                attackedSquares.Add(index);
                aSetOfPossiblePinnedPieces.Add(index);
            }
            else if (pathBlocked && squares[index].IsEmpty())
            {
                continue;
            }
            else if (PieceEnum.IsWhite(squares[index].piece) != GameManager.instance.white) // Contains a freindly piece for opposite color of GameManager.instance.white
            {
                attackedSquares.Add(index);
                break; // ?
            }
            else if (PieceEnum.IsWhite(squares[index].piece) == GameManager.instance.white) // Contains a enemy piece for opposite color of GameManager.instance.white
            {
                if (!pathBlocked && index == kingIndex) // This is just a check.
                {
                    checkCount++;
                    isChecked = true;
                    aSetOfPossiblePinnedPieces.Add(i);
                    kingCheckedSquares = aSetOfPossiblePinnedPieces;
                    continue;
                }
                else if (pathBlocked && index == kingIndex) // This is when a piece is pinned
                {
                    pinnedPieces.Add(possiblePinnedPiece);
                    aSetOfPossiblePinnedPieces.Add(i);
                    pinnedPieceDictionary.Add(possiblePinnedPiece, aSetOfPossiblePinnedPieces);
                    break;
                }
                else if (!pathBlocked) // A possible pinned piece.
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

    private void GetKingAttacks(int index)
    {
        int[] offsets = kingOffsetDictionary[index];
        foreach (int i in offsets)
        {
            attackedSquares.Add(i);
        }
    }

    private void CheckPawnAttacks(int index, int kingIndex, Square square)
    {
        // int mult = (GameManager.instance.white ? 1 : -1);

        HashSet<int> attackOffset = (GameManager.instance.white ? blackPawnAttackDictionary[index] : whitePawnAttackDictionary[index]);

        foreach (var i in attackOffset)
        {
            attackedSquares.Add(i);
            if (i == kingIndex)
            {
                checkCount++;
                kingCheckedSquares.Add(index);
            }
        }
    }
}
