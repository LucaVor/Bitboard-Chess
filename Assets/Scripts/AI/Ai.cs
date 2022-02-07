using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace QuickChess
{
    public class Ai : MonoBehaviour
    {
        public struct Move
        {
            public int from;
            public int to;
        }

        public GameManager gm;
        public int Depth = 4;
        public int MaxQuienceDepth = 5;
        public static Ai ai;
        public static Thread thread;

        public Board board;
        public Evaluate evaluator;

        public bool white;
        public int nodesEvaluated = 0;

        void Awake()
        {
            ai = this;
        }
        
        void Start()
        {
            gm = GameManager.instance;
            evaluator = new Evaluate ();
        }

        public float Search (int depth, float alpha, float beta, bool maximizing)
        {
            /* TODO: Make it find checkmate and scale it by how far it is, so it goes for closer checkmates */
            /* TODO: Add piece-value squares */
            if (depth == 0)
            {
                nodesEvaluated ++;
                float eval = Quiesce (MaxQuienceDepth, alpha, beta);
                return eval;
            }

            UInt64[] legalMoves = board.legalMoves;
            UInt64 friendlyCharacters = board.whiteMove ? board.white.GetCombinedBinary() : board.black.GetCombinedBinary();

            int castlingRights = board.castlingRights;
            bool whiteMove = board.whiteMove;

            float value = float.MaxValue;
            if (maximizing)
            {
                value = float.MinValue;
            }

            while (friendlyCharacters != 0)
            {
                int from = _ForwardBitScan (friendlyCharacters);

                UInt64 moves = legalMoves[from];

                while (moves != 0)
                {
                    int to = _ForwardBitScan (moves);

                    // Move board
                    bool errorCode = board.Push (from, to);

                    #region Save Data

                    bool wasCapturing = board.wasCapturingMove;
                    bool wasCastling = board.wasCastlingMove;
                    bool wasPromotion = board.wasPromotion;

                    Pieces pawnsPromotedFrom = board.pawnsPromotedFrom;
                    Pieces queensPromotedTo = board.queensPromotedTo;
                    Pieces pieceCaptured = board.latestPieceCaptured;
                    Pieces rooksCastled = board.rooksCastled;
                    int rookCastleSquare = board.latestRookCastleSqr;
                    int rookCastleSquareFrom = board.latestRookCastleSqrFrom;
                    #endregion

                    float eval = Search (depth - 1, alpha, beta, !maximizing);

                    #region Undo Move
                    if (!wasPromotion)
                        board.ForcePush (to, from);

                    if (wasCapturing)
                    {
                        pieceCaptured.AddPieceAt (to);
                    } if (wasCastling)
                    {
                        rooksCastled.RemovePieceAt (rookCastleSquare);
                        rooksCastled.AddPieceAt (rookCastleSquareFrom);
                    } if (wasPromotion)
                    {
                        queensPromotedTo.RemovePieceAt (to);
                        pawnsPromotedFrom.AddPieceAt (from);
                    }

                    board.whiteMove = whiteMove;
                    board.castlingRights = castlingRights;

                    board.legalMoves = legalMoves;
                    #endregion
                    
                    if (maximizing)
                    {
                        if (eval > value)
                        {
                            value = eval;
                            
                            if (depth == Depth)
                            {
                                this.From = from;
                                this.To = to;
                            }
                        } if (value >= beta)
                        {
                            return value;
                        }

                        alpha = Mathf.Max (alpha, value);
                    } else {
                        if (eval < value)
                        {
                            value = eval;

                            if (depth == Depth)
                            {
                                this.From = from;
                                this.To = to;
                            }
                        } if (value <= alpha)
                        {
                            return value;
                        }

                        beta = Mathf.Min (beta, value);
                    }

                    moves &= moves - 1;
                }

                friendlyCharacters &= friendlyCharacters - 1;
            }

            return value;
        }

        public float Quiesce (int depth, float alpha, float beta)
        {
            nodesEvaluated ++;
            float evaluation = evaluator.Eval (board);

            if (evaluation >= beta)
                return beta;
            if (alpha < evaluation)
                alpha = evaluation;

            if (depth == 0)
            {
                return alpha;
            }

            UInt64[] legalMoves = board.moveGenerator.GenerateMoves ();

            UInt64 friendlyCharacters = board.whiteMove ? board.white.GetCombinedBinary() : board.black.GetCombinedBinary();
            UInt64 enemyCharacters = board.whiteMove ? board.black.GetCombinedBinary() : board.white.GetCombinedBinary ();

            int castlingRights = board.castlingRights;
            bool whiteMove = board.whiteMove;

            while (friendlyCharacters != 0)
            {
                int from = _ForwardBitScan (friendlyCharacters);

                UInt64 moves = legalMoves[from];

                // Exclude all moves that aren't capturing enemy pieces (aka. Captures)
                moves &= enemyCharacters;

                while (moves != 0)
                {
                    int to = _ForwardBitScan (moves);

                    // Move board
                    bool errorCode = board.Push (from, to);

                    #region Save Data
                    bool wasCapturing = board.wasCapturingMove;
                    bool wasCastling = board.wasCastlingMove;
                    bool wasPromotion = board.wasPromotion;

                    Pieces pawnsPromotedFrom = board.pawnsPromotedFrom;
                    Pieces queensPromotedTo = board.queensPromotedTo;
                    Pieces pieceCaptured = board.latestPieceCaptured;
                    Pieces rooksCastled = board.rooksCastled;
                    int rookCastleSquare = board.latestRookCastleSqr;
                    int rookCastleSquareFrom = board.latestRookCastleSqrFrom;
                    #endregion

                    float score = -Quiesce (depth - 1, -beta, -alpha);

                    #region Undo Move
                    if (!wasPromotion)
                        board.ForcePush (to, from);

                    if (wasCapturing)
                    {
                        pieceCaptured.AddPieceAt (to);
                    } if (wasCastling)
                    {
                        rooksCastled.RemovePieceAt (rookCastleSquare);
                        rooksCastled.AddPieceAt (rookCastleSquareFrom);
                    } if (wasPromotion)
                    {
                        queensPromotedTo.RemovePieceAt (to);
                        pawnsPromotedFrom.AddPieceAt (from);
                    }

                    board.whiteMove = whiteMove;
                    board.castlingRights = castlingRights;

                    board.legalMoves = legalMoves;
                    #endregion
                    
                    if (score >= beta)
                        return beta;
                    if (score > alpha)
                        alpha = score;

                    moves &= moves - 1;
                }

                friendlyCharacters &= friendlyCharacters - 1;
            }

            return alpha;
        }

        bool foundBestMove = false;
        int From = -1;
        int To = -1;
        int movesEvaled = 0;

        public void Update()
        {
            if (foundBestMove)
            {
                foundBestMove = false;

                MakeMove (From, To);
            }
        }

        public void AI ()
        {
            if (gm == null)
            {
                gm = GameManager.instance;
            }

            thread = new Thread ( GetBestMove );
            thread.Priority = System.Threading.ThreadPriority.Highest;
            thread.Start();
        }

        public void CopyBoard (Board template, ref Board output)
        {
            int castlingRights = template.castlingRights;
            bool whiteMove = template.whiteMove;
            string fen = template.GetFen ();
            
            if (output == null)
            {
                output = new Board();
            }

            output.LoadFen (fen);
            output.whiteMove = whiteMove;
            output.castlingRights = castlingRights;

            output.RegenerateLegalMoves ();
        }

        public void GetBestMove ()
        {
            nodesEvaluated = 0;

            // This is the Ai's function that decides move to make
            CopyBoard (gm.board, ref board);

            white = gm.board.whiteMove;
            float bestEvaluation = Search (Depth, float.MinValue, float.MaxValue, white);

            foundBestMove = true;

            // UInt64[] legalMoves = gm.board.moveGenerator.GenerateMoves();
            // System.Random rnd = new System.Random();

            // int i = rnd.Next(0, 64);
            // while (true)
            // {
            //     if (legalMoves[i] != 0)
            //     {
            //         int[] ones = GetOnes (legalMoves[i]);

            //         foundBestMove = true;

            //         from = i;
            //         to = ones[rnd.Next(0,ones.Length)];

            //         return;
            //     }

            //     i = (i + 1) % 63;
            // }
        }

        public int[] GetOnes (UInt64 binary)
        {
            int oneCount = 0;
            UInt64 binaryTemp = binary;

            while (binaryTemp != 0)
            {
                oneCount ++;
                binaryTemp &= binaryTemp - 1;
            }

            int[] ones = new int[oneCount];

            int i = 0;
            while (binary != 0)
            {
                ones[i] = _ForwardBitScan (binary);
                binary &= binary - 1;
                i ++;
            }

            return ones;
        }

        public void MakeMove (int from, int to)
        {
            gm.Move (from, to, false, true);
        }

        public int _ForwardBitScan (UInt64 bin)
        {
            return (int) Unity.Burst.Intrinsics.X86.Bmi1.tzcnt_u64 (bin);
        }
    }
}