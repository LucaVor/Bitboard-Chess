using System;
using System.Collections;
using System.Collections.Generic;

namespace QuickChess
{
    public enum Direction
    {
        North,
        South,
        East,
        West,
        NorthEast,
        NorthWest,
        SouthEast,
        SouthWest,
        None
    };

    public class PreProcessing
    {
        public static UInt64[,] directions;
        public static Direction[,] directionBetween;
        public static UInt64[,] squaresBetween;
        public static UInt64[] precomputedKnightAttacks;
        public static UInt64[] precomputedBlackPawnMoves;
        public static UInt64[] precomputedWhitePawnMoves;
        public static UInt64[] precomputedBlackPawnAttacks;
        public static UInt64[] precomputedWhitePawnAttacks;
        public static UInt64[] precomputedKingMoves;
        public static UInt64[] precomputedBlackKingCastling;
        public static UInt64[] precomputedWhiteKingCastling;

        public const int whiteKingSquare = 4;
        public const int blackKingSquare = 60;
        public const int whiteKingSideCastle = 6;
        public const int whiteQueenSideCastle = 2;
        public const int blackKingSideCastle = 62;
        public const int blackQueenSideCastle = 58;

        public const int PAWN = 0;
        public const int ROOK = 1;
        public const int KNIGHT = 2;
        public const int BISHOP = 3;
        public const int QUEEN = 4;
        public const int KING = 5;

        public static int[] Sliders = {
            0, 1, 0, 1, 1, 0
        };

        public static int[] Diagonals = {
            0, 0, 0, 0, 1, 1, 1, 1
        };

        public static int[] Upwards = {
            1, 0, 1, 0, 1, 1, 0, 0
        };

        public const UInt64 kingSideMask = (1UL << whiteKingSideCastle) | (1UL << blackKingSideCastle);
        public const UInt64 queenSideMask = (1UL << whiteQueenSideCastle) | (1UL << blackQueenSideCastle);

        public const UInt64 whiteKingSidePieceCheckingMask = (1UL << 5) | (1UL << 6);
        public const UInt64 whiteQueenSidePieceCheckingMask = (1UL << 1) | (1UL << 2) | (1UL << 3);
        public const UInt64 blackKingSidePieceCheckingMask = (1UL << 61) | (1UL << 62);
        public const UInt64 blackQueenSidePieceCheckingMask = (1UL << 57) | (1UL << 58) | (1UL << 59);

        public const int UP =    8;
        public const int DOWN = -8;
        public const int RIGHT = 1;
        public const int LEFT = -1;
        
        public static Dictionary<Direction, int[]> directionOffsets = new Dictionary<Direction, int[]>() {
            {Direction.North, new int[] {0, 1}},
            {Direction.South, new int[] {0, -1}},
            {Direction.East, new int[] {1, 0}},
            {Direction.West, new int[] {-1, 0}},
            {Direction.NorthEast, new int[] {1, 1}},
            {Direction.NorthWest, new int[] {-1, 1}},
            {Direction.SouthEast, new int[] {1, -1}},
            {Direction.SouthWest, new int[] {-1, -1}}
        };

        public static void PreProcess()
        {
            directions = new UInt64[8,64];
            squaresBetween = new UInt64[64,64];
            directionBetween = new Direction[64,64];
            
            precomputedKnightAttacks = new UInt64[64];
            precomputedWhitePawnMoves = new UInt64[64];
            precomputedBlackPawnMoves = new UInt64[64];
            precomputedWhitePawnAttacks = new UInt64[64];
            precomputedBlackPawnAttacks = new UInt64[64];
            precomputedKingMoves = new UInt64[64];
            precomputedBlackKingCastling = new UInt64[64];
            precomputedWhiteKingCastling = new UInt64[64];

            UInt64 One = (UInt64) 1;

            for (int x0 = 0; x0 < 8; x0 ++)
            {
                for (int y0 = 0; y0 < 8; y0 ++)
                {
                    int i0 = y0 * 8 + x0;

                    for (int x1 = 0; x1 < 8; x1 ++)
                    {
                        for (int y1 = 0; y1 < 8; y1 ++)
                        {
                            int i1 = y1 * 8 + x1;
                            if (i0 == i1) continue;

                            bool isDiagonal = IsDiagonal (x0, y0, x1, y1);
                            bool isHV = IsHV (x0, y0, x1, y1);

                            if (!(isDiagonal || isHV)) {
                                directionBetween[i0,i1] = Direction.None;
                                squaresBetween[i0,i1] = 0UL;
                                continue;
                            }

                            if (isDiagonal && y1 > y0 && x1 > x0)
                            {
                                directionBetween[i0,i1] = Direction.NorthEast;
                            } if (isDiagonal && y1 > y0 && x1 < x0)
                            {
                                directionBetween[i0,i1] = Direction.NorthWest;
                            } if (isDiagonal && y1 < y0 && x1 > x0)
                            {
                                directionBetween[i0,i1] = Direction.SouthEast;
                            } if (isDiagonal && y1 < y0 && x1 < x0)
                            {
                                directionBetween[i0,i1] = Direction.SouthWest;
                            }

                            if (isHV && y1 > y0)
                            {
                                directionBetween[i0,i1] = Direction.North;
                            } if (isHV && y1 < y0)
                            {
                                directionBetween[i0,i1] = Direction.South;
                            } if (isHV && x1 > x0)
                            {
                                directionBetween[i0,i1] = Direction.East;
                            } if (isHV && x1 < x0)
                            {
                                directionBetween[i0,i1] = Direction.West;
                            }

                            Direction dirBetween = directionBetween[i0,i1];
                            UInt64 binary = 0;

                            int offsetX = directionOffsets[dirBetween][0];
                            int offsetY = directionOffsets[dirBetween][1];

                            int tx = x0 + offsetX;
                            int ty = y0 + offsetY;

                            while ((tx != x1) || (ty != y1))
                            {
                                int ti = ty * 8 + tx;
                                binary |= One << ti;

                                tx += offsetX;
                                ty += offsetY;
                            }

                            squaresBetween[i0,i1] = binary;
                        }
                    }
                }
            }

            for (int x = 0; x < 8; x ++)
            {
                for (int y = 0; y < 8; y ++)
                {
                    int i = y * 8 + x;

                    // Get cardinal directions
                    for (int d = 0; d < 8; d ++)
                    {
                        UInt64 binary = 0;

                        int offsetX = directionOffsets[(Direction)d][0];
                        int offsetY = directionOffsets[(Direction)d][1];

                        int tx = x + offsetX;
                        int ty = y + offsetY;

                        while ((tx >= 0 && tx < 8) && (ty >= 0 && ty < 8))
                        {
                            int ti = ty * 8 + tx;

                            binary |= One << ti;

                            tx += offsetX;
                            ty += offsetY;
                        }
                        
                        directions[d,i] = binary;
                    }
                
                    // Get knight moves
                    /*
                    . . . . . . . .
                    . . 1 . 1 . . .
                    . 1 . . . 1 . .
                    . . . N . . . .
                    . 1 . . . 1 . .
                    . . 1 . 1 . . .
                    . . . . . . . .
                    . . . . . . . .
                    */

                    #region KnightCalculations
                    UInt64 knightMoves = 0;

                    if (y < 6 && x < 7)
                    {
                        knightMoves |= 1UL << (i + (UP * 2)) + (RIGHT * 1);
                    } 
                    if (y < 6 && x > 0)
                    {
                        knightMoves |= 1UL << (i + (UP * 2)) - (RIGHT * 1);
                    } if (y >= 2 && x < 7)
                    {
                        knightMoves |= 1UL << (i - (UP * 2)) + (RIGHT * 1);
                    } if (y >= 2 && x > 0)
                    {
                        knightMoves |= 1UL << (i - (UP * 2)) - (RIGHT * 1);
                    } if (y < 7 && x < 6)
                    {
                        knightMoves |= 1UL << (i + (UP * 1)) + (RIGHT * 2);
                    } if (y < 7 && x >= 2)
                    {
                        knightMoves |= 1UL << (i + (UP * 1)) - (RIGHT * 2);
                    } if (y > 0 && x < 6)
                    {
                        knightMoves |= 1UL << (i - (UP * 1)) + (RIGHT * 2);
                    } if (y > 0 && x >= 2)
                    {
                        knightMoves |= 1UL << (i - (UP * 1)) - (RIGHT * 2);
                    }

                    precomputedKnightAttacks[i] = knightMoves;
                    #endregion
                
                    #region Pawns
                    UInt64 whitePawnMoves = 0;
                    UInt64 blackPawnMoves = 0;

                    UInt64 whitePawnAttacks = 0;
                    UInt64 blackPawnAttacks = 0;

                    if (y < 7 && x < 7)
                    {
                        whitePawnAttacks |= 1UL << i + UP + RIGHT;
                    } if (y < 7 && x > 0)
                    {
                        whitePawnAttacks |= 1UL << i + UP - RIGHT;
                    } if (y > 0 && x < 7)
                    {
                        blackPawnAttacks |= 1UL << i - UP + RIGHT;
                    } if (y > 0 && x > 0)
                    {
                        blackPawnAttacks |= 1UL << i - UP - RIGHT;
                    }

                    if (y < 7)
                    {
                        whitePawnMoves |= 1UL << i + UP;
                    } if (y > 0)
                    {
                        blackPawnMoves |= 1UL << i + DOWN;
                    }

                    if (y == 1)
                    {
                        whitePawnMoves |= 1UL << i + UP * 2;
                    } if (y == 6)
                    {
                        blackPawnMoves |= 1UL << i + DOWN * 2;
                    }

                    precomputedWhitePawnMoves[i] = whitePawnMoves;
                    precomputedBlackPawnMoves[i] = blackPawnMoves;
                    precomputedWhitePawnAttacks[i] = whitePawnAttacks;
                    precomputedBlackPawnAttacks[i] = blackPawnAttacks;
                    #endregion
                
                    #region King
                    UInt64 kingMoves = 0;
                    if (y < 7) kingMoves |= 1UL << (i + UP);
                    if (y > 0) kingMoves |= 1UL << (i + DOWN);
                    if (x < 7) kingMoves |= 1UL << (i + RIGHT);
                    if (x > 0) kingMoves |= 1UL << (i + LEFT);
                    if (y < 7 && x < 7) kingMoves |= 1UL << (i + UP + RIGHT);
                    if (y > 0 && x > 0) kingMoves |= 1UL << (i + DOWN + LEFT);
                    if (y < 7 && x > 0) kingMoves |= 1UL << (i + UP + LEFT);
                    if (y > 0 && x < 7) kingMoves |= 1UL << (i + DOWN + RIGHT);

                    precomputedKingMoves[i] = kingMoves;
                    #endregion                
                
                    #region Castling
                    UInt64 whiteCastlingValue = 0;
                    UInt64 blackCastlingValue = 0;

                    if (i == whiteKingSquare)
                    {
                        whiteCastlingValue |= (1UL << whiteKingSideCastle);
                        whiteCastlingValue |= (1UL << whiteQueenSideCastle);
                    }

                    if (i == blackKingSquare)
                    {
                        blackCastlingValue |= (1UL << blackKingSideCastle);
                        blackCastlingValue |= (1UL << blackQueenSideCastle);
                    }

                    precomputedWhiteKingCastling[i] = whiteCastlingValue;
                    precomputedBlackKingCastling[i] = blackCastlingValue;
                    #endregion
                }
            }
        }

        public static bool IsDiagonal (float x0, float y0, float x1, float y1)
        {
            /*
            . . . .
            . . . X
            . . Y .
            . X . Y
            */

            return (x1 - x0) == (y1 - y0);
        }

        public static bool IsHV (float x0, float y0, float x1, float y1)
        {
            return (x0 == x1) ^ (y0 == y1);
        }
    }
}