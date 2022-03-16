using System;

[Flags]
public enum PieceE
{
    None = 0b_0000_0000,  // 0
    Pawn = 0b_0000_0001,  // 1
    Rook = 0b_0000_0010,  // 2
    Knight = 0b_0000_0100,  // 4
    Bishop = 0b_0000_1000,  // 8
    Queen = 0b_0001_0000,  // 16
    King = 0b_0010_0000,  // 32
    White = 0b_0100_0000,  // 64
    Black = 0b_1000_0000,  // 128
    Color = White | Black
}

public class PieceEnum
{
    public static bool IsWhite(PieceE piece)
    {
        return (piece & PieceE.Color) == PieceE.White;
    }
    public static bool IsPawn(PieceE piece)
    {
        return (piece ^ (piece & PieceE.Color)) == (PieceE.Pawn);
    }

    public static bool IsKnight(PieceE piece)
    {
        return (piece ^ (piece & PieceE.Color)) == (PieceE.Knight);
    }

    public static bool IsBishop(PieceE piece)
    {
        return (piece ^ (piece & PieceE.Color)) == (PieceE.Bishop);
    }

    public static bool IsQueen(PieceE piece)
    {
        return (piece ^ (piece & PieceE.Color)) == (PieceE.Queen);
    }

    public static bool IsKing(PieceE piece)
    {
        return (piece ^ (piece & PieceE.Color)) == (PieceE.King);
    }

}