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

        public static int[] MaterialValues = new int[] {
            0, 9, 5, 3, 3, 1
        };

        public enum WeightType
        {
            Material,
            BishopPair,
            PawnStacking,
            PassedPawns
        }

        public float[] multipliers = new float[] {
            1, 2, 1, 1
        };

        public float Eval (Board board)
        {
            float black = 0;
            float white = 0;

            /* Piece ordering: King, Queens, Rooks, Bishops, Knights, Pawns */

            int whiteMaterial = 0;
            int blackMaterial = 0;

            int whitePawnMaterial = 0;
            int blackPawnMaterial = 0;

            bool whiteBishopPair = false;
            bool blackBishopPair = false;

            bool[] whitePawnFiles = new bool[] { false, false, false, false, false, false, false, false };
            bool[] blackPawnFiles = new bool[] { false, false, false, false, false, false, false, false };

            int whitePawnsOnSameFile = 0;
            int blackPawnsOnSameFile = 0;

            int whitePassedPawns = 0;
            int blackPassedPawns = 0;

            for (int piece = 0; piece < 6; piece ++)
            {
                Pieces whitePieces = board.white.pieces [piece];
                Pieces blackPieces = board.black.pieces [piece];

                if (piece == 3 && whitePieces.pieces >= 2) whiteBishopPair = true;
                if (piece == 3 && blackPieces.pieces >= 2) blackBishopPair = true;

                if (piece == 5)
                {   
                    int minWhitePawnFile = 9;
                    int maxBlackPawnFile = -1;

                    for (int i = 0; i < whitePieces.pieces; i ++)
                    {
                        int sqr = whitePieces.currentPieces[i];

                        int mod = sqr % 8;
                        int file = (int) Mathf.Floor (((float) sqr) / 8);

                        if (file < minWhitePawnFile)
                            minWhitePawnFile = file;

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
                }
                
                whiteMaterial += whitePieces.pieces * MaterialValues[piece];
                blackMaterial += blackPieces.pieces * MaterialValues[piece];
            }

            float whiteEndgame = 1 - Mathf.Min((whiteMaterial - whitePawnMaterial) * (1 / 15), 1);
            float blackEndgame = 1 - Mathf.Min((blackMaterial - blackPawnMaterial) * (1 / 15), 1);

            white += whiteMaterial * multipliers[0];
            black += blackMaterial * multipliers[0];

            white += whiteBishopPair ? multipliers[1] : 0;
            black += blackBishopPair ? multipliers[1] : 0;

            white -= whitePawnsOnSameFile * multipliers[2];
            black -= blackPawnsOnSameFile * multipliers[2];

            white += whitePassedPawns * multipliers[3] * whiteEndgame;
            black += blackPassedPawns * multipliers[3] * blackEndgame;


            return white - black;
        }
    }
}