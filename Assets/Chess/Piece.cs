using System;
using System.Collections;
using System.Collections.Generic;

namespace QuickChess
{
    public class Pieces
    {
        public UInt64 bitmap;

        public int[] currentPieces;
        public int[] squareToPieces;

        public int pieces;

        public Pieces (int maxPieces)
        {
            currentPieces = new int[maxPieces];
            squareToPieces = new int[64];
        }

        const UInt64 One = 1;

        public void AddPieceAt (int square)
        {
            currentPieces[pieces] = square;
            squareToPieces[square] = pieces;
            bitmap |= One << square;
            pieces ++;
        }

        public void RemovePieceAt (int square)
        {
            int ind = squareToPieces[square];

            currentPieces[ind] = currentPieces[pieces - 1];
            squareToPieces[currentPieces[ind]] = ind;
            
            bitmap ^= One << square;

            pieces --;
        }
    }

    public class PieceMap
    {
        public Pieces King = new Pieces (1);
        public Pieces Queens = new Pieces (9);
        public Pieces Rooks = new Pieces (10);
        public Pieces Bishops = new Pieces (10);
        public Pieces Knights = new Pieces (10);
        public Pieces Pawns = new Pieces (8);

        public static string[] names = {
            "K", "Q", "R", "B", "N", "P"
        };

        public Pieces[] pieces;

        public PieceMap ()
        {
            pieces = new Pieces[] {
                King, Queens, Rooks, Bishops, Knights, Pawns
            };
        }

        public UInt64 GetCombinedBinary()
        {
            return King.bitmap | Queens.bitmap | Rooks.bitmap | Bishops.bitmap | Knights.bitmap | Pawns.bitmap;
        }
    }

    public class PieceBinary
    {
        public const int Blank = 0;
        public const int Pawn = 1;
        public const int Knight = 2;
        public const int Bishop = 3;
        public const int Rook = 5;
        public const int Queen = 6;
        public const int King = 7;
        public const int White = 8;
        public const int Black = 16;
        public const int TileType = Blank | Pawn | Knight | Bishop | Rook | Queen | King;
        public const int ColourType = White | Black;
    }

    public class PieceUtility
    {
        public static int GetColour (int piece)
        {
            return piece & PieceBinary.ColourType;
        }

        public static int GetPieceType (int piece)
        {
            return piece & PieceBinary.TileType;
        }

        public static bool IsPiece (int piece, int targetPiece)
        {
            return (piece & PieceBinary.TileType) == targetPiece;
        }

        public static bool IsPieceOr (int piece, params int[] targetPieces)
        {
            for (int index = 0; index < targetPieces.Length; index ++)
            {
                if ((piece & PieceBinary.TileType) == targetPieces[index])
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EqualColour (int piece, int colour)
        {
            return (piece & PieceBinary.ColourType) == colour;
        }
    }
}