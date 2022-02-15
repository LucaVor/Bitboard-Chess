using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickChess
{
    public class Evaluate
    {
        public static float[] whitePawnPieceSquareTable = new float[] {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5, -5,-10,  0,  0,-10, -5,  5,
            5, 10, 10,-20,-20, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0
        };

        public static float[] whiteKnightPieceSquareTable = new float[] {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 16, 15, 15, 16,  0,-30,
            -30,  5, 15, 15, 15, 15,  5,-30,
            -30,  0, 15, 15, 15, 15,  0,-30,
            -30,  5, 16, 15, 15, 16,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50
        };

        public static float[] whiteBishopPieceSquareTable = new float[] {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  5,  5,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20
        };

        public static float[] whiteRookPieceSquareTable = new float[] {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            0,  0,  10,  5,  5,  20,  0,  0
        };

        public static float[] whiteQueenPieceSquareTable = new float[] {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            0,  0,  0,  5,  5,  0,  0,  0
        };

        public static float[] whiteKingMidPieceSquareTable = new float[] {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            0,  10,  0,  5,  5,  0,  20,  0
        };

        public static float[] whiteKingEndPieceSquareTable = new float[] {
            -50,-40,-30,-20,-20,-30,-40,-50,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-30,  0,  0,  0,  0,-30,-30,
            -50,-30,-30,-30,-30,-30,-30,-50
        };

        public static float[] arrCentreManhattanDistance = new float[] {
            6, 5, 4, 3, 3, 4, 5, 6,
            5, 4, 3, 2, 2, 3, 4, 5,
            4, 3, 2, 1, 1, 2, 3, 4,
            3, 2, 1, 0, 0, 1, 2, 3,
            3, 2, 1, 0, 0, 1, 2, 3,
            4, 3, 2, 1, 1, 2, 3, 4,
            5, 4, 3, 2, 2, 3, 4, 5,
            6, 5, 4, 3, 3, 4, 5, 6
        };


        public static float[] whitePawnTransposition = new float[64];
        public static float[] blackPawnTransposition = new float[64];
        public static float[] whiteRookTransposition = new float[64];
        public static float[] blackRookTransposition = new float[64];
        public static float[] whiteBishopTransposition = new float[64];
        public static float[] blackBishopTransposition = new float[64];
        public static float[] whiteKnightTransposition = new float[64];
        public static float[] blackKnightTransposition = new float[64];
        public static float[] whiteKingTranspositionMid = new float[64];
        public static float[] blackKingTranspositionMid = new float[64];
        public static float[] whiteKingTranspositionEnd = new float[64];
        public static float[] blackKingTranspositionEnd = new float[64];
        public static float[] whiteQueenTransposition = new float[64];
        public static float[] blackQueenTransposition = new float[64];

        public Evaluate ()
        {
            for (int i = 0; i < 64; i ++)
            {
                int pawnValue = (int) whitePawnPieceSquareTable[pieceSquareIndex(i)];
                whitePawnTransposition[i] = pawnValue;
                pawnValue = (int) whitePawnPieceSquareTable[flipIndex(pieceSquareIndex(i))];
                blackPawnTransposition[i] = pawnValue;

                int rookValue = (int) whiteRookPieceSquareTable[pieceSquareIndex(i)];
                whiteRookTransposition[i] = rookValue;
                rookValue = (int) whiteRookPieceSquareTable[flipIndex(pieceSquareIndex(i))];
                blackRookTransposition[i] = rookValue;

                int knightValue = (int) whiteKnightPieceSquareTable[pieceSquareIndex(i)];
                whiteKnightTransposition[i] = knightValue;
                knightValue = (int) whiteKnightPieceSquareTable[flipIndex(pieceSquareIndex(i))];
                blackKnightTransposition[i] = knightValue;

                int bishopValue = (int) whiteBishopPieceSquareTable[pieceSquareIndex(i)];
                whiteBishopTransposition[i] = bishopValue;
                bishopValue = (int) whiteBishopPieceSquareTable[flipIndex(pieceSquareIndex(i))];
                blackBishopTransposition[i] = bishopValue;

                int queenValue = (int) whiteQueenPieceSquareTable[pieceSquareIndex(i)];
                whiteQueenTransposition[i] = queenValue;
                queenValue = (int) whiteQueenPieceSquareTable[flipIndex(pieceSquareIndex(i))];
                blackQueenTransposition[i] = queenValue;

                int kingMidValue = (int) whiteKingMidPieceSquareTable[pieceSquareIndex(i)];
                whiteKingTranspositionMid[i] = kingMidValue;
                kingMidValue = (int) whiteKingMidPieceSquareTable[flipIndex(pieceSquareIndex(i))];
                blackKingTranspositionMid[i] = kingMidValue;

                int kingEndValue = (int) whiteKingEndPieceSquareTable[pieceSquareIndex(i)];
                whiteKingTranspositionEnd[i] = kingEndValue;
                kingEndValue = (int) whiteKingEndPieceSquareTable[flipIndex(pieceSquareIndex(i))];
                blackKingTranspositionEnd[i] = kingEndValue;
            }
        }

        public int pieceSquareIndex (int psi)
        {
            return (int) ((56 + psi) - Mathf.Floor (((float)psi) / 8f) * 16);
        }

        public int flipIndex (int ind)
        {
            return pieceSquareIndex (ind);
        }

        public static int[] MaterialValues = new int[] {
            0, 9, 5, 3, 3, 1
        };

        public enum WeightType
        {
            Material,
            BishopPair,
            PawnStacking,
            PassedPawns,
            Transposition,
            KingDstToCentre,
            PawnNearPromotion
        }

        public float[] multipliers_b = new float[] {
            25f, 2, 1.5f, 1, 0.045f, 3.5f, 3
        };

        public float[] multipliers_e = new float[] {
            25f, 3, 1, 4, 0.01f, 5, 6
        };

        public float whiteEndgame = 0;
        public float blackEndgame = 0;

        public float Eval (Board board)
        {
            float black = 0;
            float white = 0;

            /* Piece ordering: King, Queens, Rooks, Bishops, Knights, Pawns */

            int whiteMaterial = 0;
            int blackMaterial = 0;

            int whitePawnMaterial = 0;
            int blackPawnMaterial = 0;

            int whiteQueenMaterial = 0;
            int blackQueenMaterial = 0;

            bool whiteBishopPair = false;
            bool blackBishopPair = false;

            bool[] whitePawnFiles = new bool[] { false, false, false, false, false, false, false, false };
            bool[] blackPawnFiles = new bool[] { false, false, false, false, false, false, false, false };

            int whitePawnsOnSameFile = 0;
            int blackPawnsOnSameFile = 0;

            int whitePassedPawns = 0;
            int blackPassedPawns = 0;

            int whitePawnNearPromotion = 0;
            int blackPawnNearPromotion = 0;

            for (int piece = 0; piece < 6; piece ++)
            {
                Pieces whitePieces = board.white.pieces [piece];
                Pieces blackPieces = board.black.pieces [piece];

                if (piece == 3 && whitePieces.pieces >= 2) whiteBishopPair = true;
                if (piece == 3 && blackPieces.pieces >= 2) blackBishopPair = true;

                if (piece == 1)
                {
                    whiteQueenMaterial += whitePieces.pieces * MaterialValues[piece];
                    blackQueenMaterial += blackPieces.pieces * MaterialValues[piece];
                }

                if (piece == 5)
                {   
                    int minWhitePawnFile = 9;
                    int maxWhitePawnFile = -1;
                    int maxBlackPawnFile = -1;
                    int minBlackPawnFile = 9;

                    for (int i = 0; i < whitePieces.pieces; i ++)
                    {
                        int sqr = whitePieces.currentPieces[i];

                        int mod = sqr % 8;
                        int file = (int) Mathf.Floor (((float) sqr) / 8);

                        if (file < minWhitePawnFile)
                            minWhitePawnFile = file;
                        if (file > maxWhitePawnFile)
                            maxWhitePawnFile = file;

                        if (whitePawnFiles[mod]) whitePawnsOnSameFile ++;
                        whitePawnFiles[mod] = true;
                    }

                    for (int i = 0; i < blackPieces.pieces; i ++)
                    {
                        int sqr = blackPieces.currentPieces[i];

                        int mod = sqr % 8;
                        int file = (int) Mathf.Floor (((float) sqr) / 8);

                        if (file > maxBlackPawnFile)
                            maxBlackPawnFile = file;
                        if (file < minBlackPawnFile)
                            minBlackPawnFile = file;

                        if (blackPawnFiles[mod]) blackPawnsOnSameFile ++;
                        blackPawnFiles[mod] = true;
                    }

                    for (int i = 0; i < whitePieces.pieces; i ++)
                    {
                        int sqr = whitePieces.currentPieces[i];

                        int file = (int) Mathf.Floor (((float) sqr) / 8);

                        if (file > maxBlackPawnFile)
                            whitePassedPawns ++;
                    }

                    for (int i = 0; i < blackPieces.pieces; i ++)
                    {
                        int sqr = blackPieces.currentPieces[i];

                        int file = (int) Mathf.Floor (((float) sqr) / 8);

                        if (file < minWhitePawnFile)
                            blackPassedPawns ++;
                    }

                    whitePawnMaterial += whitePieces.pieces * MaterialValues[piece];
                    blackPawnMaterial += blackPieces.pieces * MaterialValues[piece];

                    whitePawnNearPromotion = maxWhitePawnFile;
                    blackPawnNearPromotion = 7-minBlackPawnFile;
                }
                
                whiteMaterial += whitePieces.pieces * MaterialValues[piece];
                blackMaterial += blackPieces.pieces * MaterialValues[piece];
            }

            whiteEndgame = 1 - Mathf.Min((whiteMaterial - (whitePawnMaterial - whiteQueenMaterial)) * (1f / 21f), 1);
            blackEndgame = 1 - Mathf.Min((blackMaterial - (blackPawnMaterial - blackQueenMaterial)) * (1f / 21f), 1);
            
            float whiteTranspositionValue = 0;
            float blackTranspositionValue = 0;

            for (int piece = 0; piece < 6; piece ++)
            {
                /* Piece ordering: King, Queens, Rooks, Bishops, Knights, Pawns */

                Pieces whitePieces = board.white.pieces [piece];
                Pieces blackPieces = board.black.pieces [piece];

                float wtv = 0;
                float btv = 0;

                for (int pieceIndex = 0; pieceIndex < whitePieces.pieces; pieceIndex ++)
                {
                    int square = whitePieces.currentPieces[pieceIndex];

                    switch (piece) {
                        case 0:
                            wtv += whiteKingTranspositionMid[square] * (1 - whiteEndgame);
                            wtv += whiteKingTranspositionEnd[square] * whiteEndgame;
                            break;
                        case 1:
                            wtv += whiteQueenTransposition[square];
                            break;
                        case 2:
                            wtv += whiteRookTransposition[square];
                            break;
                        case 3:
                            wtv += whiteBishopTransposition[square];
                            break;
                        case 4:
                            wtv += whiteKnightTransposition[square];
                            break;
                        case 5:
                            wtv += whitePawnTransposition[square];
                            break;
                    }
                }

                for (int pieceIndex = 0; pieceIndex < blackPieces.pieces; pieceIndex ++)
                {
                    int square = blackPieces.currentPieces[pieceIndex];

                    switch (piece) {
                        case 0:
                            btv += blackKingTranspositionMid[square] * (1 - blackEndgame);
                            btv += blackKingTranspositionEnd[square] * blackEndgame;
                            break;
                        case 1:
                            btv += blackQueenTransposition[square];
                            break;
                        case 2:
                            btv += blackRookTransposition[square];
                            break;
                        case 3:
                            btv += blackBishopTransposition[square];
                            break;
                        case 4:
                            btv += blackKnightTransposition[square];
                            break;
                        case 5:
                            btv += blackPawnTransposition[square];
                            break;
                    }
                }

                whiteTranspositionValue += wtv;
                blackTranspositionValue += btv;
            }

            int whiteKingSquare = board.white.King.currentPieces[0];
            int blackKingSquare = board.black.King.currentPieces[0];

            white += whiteMaterial * Lerp(multipliers_b[0], multipliers_e[0], whiteEndgame);
            black += blackMaterial * Lerp(multipliers_b[0], multipliers_e[0], blackEndgame);

            white += whiteBishopPair ? Lerp(multipliers_b[1], multipliers_e[1], whiteEndgame) : 0;
            black += blackBishopPair ? Lerp(multipliers_b[1], multipliers_e[1], blackEndgame) : 0;

            white -= whitePawnsOnSameFile * Lerp(multipliers_b[2], multipliers_e[2], whiteEndgame);
            black -= blackPawnsOnSameFile * Lerp(multipliers_b[2], multipliers_e[2], blackEndgame);

            white += whitePassedPawns * Lerp(multipliers_b[3], multipliers_e[3], whiteEndgame) * whiteEndgame;
            black += blackPassedPawns * Lerp(multipliers_b[3], multipliers_e[3], blackEndgame) * blackEndgame;

            white += whiteTranspositionValue * Lerp(multipliers_b[4], multipliers_e[4], whiteEndgame);
            black += blackTranspositionValue * Lerp(multipliers_b[4], multipliers_e[4], blackEndgame);

            white += arrCentreManhattanDistance[blackKingSquare] * Lerp(multipliers_b[5], multipliers_e[5], whiteEndgame) * blackEndgame;
            black += arrCentreManhattanDistance[whiteKingSquare] * Lerp(multipliers_b[5], multipliers_e[5], blackEndgame) * whiteEndgame;

            white += whitePawnNearPromotion * Lerp(multipliers_b[6], multipliers_e[6], whiteEndgame) * whiteEndgame;
            black += blackPawnNearPromotion * Lerp(multipliers_b[6], multipliers_e[6], blackEndgame) * blackEndgame;

            float eval = (white - black);
            float persp = board.whiteMove ? 1 : -1;

            return eval;
        }

        public float Lerp (float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}