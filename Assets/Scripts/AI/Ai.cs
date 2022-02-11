using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace QuickChess
{
    public class Ai : MonoBehaviour
    {

        public GameManager gm;
        public int Depth = 4;
        public int MaxQuienceDepth = 5;
        public float allocatedMilliseconds = 6000;
        public static Ai ai;
        public static Thread thread;

        public Board board;
        public Evaluate evaluator;

        public bool white;
        public bool useOpeningBook;
        public int nodesEvaluated = 0;
        public int checkmatesFound = 0;
        public int nodesCutoff = 0;

        public System.Diagnostics.Stopwatch stopwatch;
        public UnityEngine.UI.Text moveCountText;

        public void Awake()
        {
            ai = this;
        }
        
        public void Start()
        {
            gm = GameManager.instance;
            evaluator = new Evaluate ();

            // Depth = SceneManagement.DEPTH;
        }

        public bool IsMateScore (float score)
        {
            const int mateScore = 100000;
            return Mathf.Abs (score) >= (mateScore - Depth);
        }

        public bool cancelledSearch = false;

        public float Search (int depth, float alpha, float beta)
        {
            // if (stopwatch.ElapsedMilliseconds > allocatedMilliseconds) {
            //     cancelledSearch = true;
            //     return 0;
            // }

            const int mateScore = 100000;

            if (depth != Depth)
            {
                alpha = Mathf.Max (alpha, -mateScore + (Depth - depth));
				beta = Mathf.Min (beta, mateScore - (Depth - depth));
				if (alpha >= beta) {
					return alpha;
				}
            }

            if (depth == 0)
            {
                float eval = ((SceneManagement.boolQuiesence) ? (Quiesce (0, alpha, beta)) : (evaluator.Eval (board)));
                // float eval = evaluator.Eval (board);

                return eval;
            }

            if (board.inCheckmate)
            {
                checkmatesFound++;
                board.moveGenerator.GenerateMoves ();

                if (board.moveGenerator.inCheck)
                {
                    float score = mateScore - (Depth - depth);
                    return -score;
                } else {
                    return 0;
                }
            }

            UInt64[] legalMoves = board.legalMoves;
            List<Move> formattedMoves = board.moveGenerator.FormatLegalMoves ();
            MoveOrdering.OrderMoves (board, formattedMoves);

            int castlingRights = board.castlingRights;
            bool whiteMove = board.whiteMove;

            foreach (Move move in formattedMoves) {
                int from = move.from;
                int to = move.to;

                // Move board
                bool errorCode = board.Push (from, to);

                #region Save Data

                bool wasCapturing = board.wasCapturingMove;
                bool wasCastling = board.wasCastlingMove;
                bool wasPromotion = board.wasPromotion;
                bool wasCheck = board.inCheck;
                bool wasCheckmate = board.inCheckmate;

                Pieces pawnsPromotedFrom = board.pawnsPromotedFrom;
                Pieces queensPromotedTo = board.queensPromotedTo;
                Pieces pieceCaptured = board.latestPieceCaptured;
                Pieces rooksCastled = board.rooksCastled;
                int rookCastleSquare = board.latestRookCastleSqr;
                int rookCastleSquareFrom = board.latestRookCastleSqrFrom;
                #endregion

                float eval = -Search (depth - 1, -beta, -alpha);

                board.inCheck = wasCheck;
                board.inCheckmate = wasCheckmate;

                #region Undo Move
                if (!wasPromotion)
                {
                    /* Fix error here: then add opening book: then make a system to train AI multipliers (Make it play against stockfish) */
                    board.ForcePush (to, from);
                }

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

                if (eval >= beta)
                {
                    nodesCutoff ++;
                    return beta;
                }

                if (eval > alpha)
                {
                    if (depth == Depth)
                    {
                        this.From = from;
                        this.To = to;
                    }

                    alpha = eval;
                }
            }

            return alpha;
        }

        public float Quiesce (int depth, float alpha, float beta)
        {
            // if (stopwatch.ElapsedMilliseconds > allocatedMilliseconds) {
            //     cancelledSearch = true;
            //     return 0;
            // }

            const int mateScore = 100000;

            if (board.inCheckmate)
            {
                board.moveGenerator.GenerateMoves ();

                if (board.moveGenerator.inCheck)
                {
                    float score = mateScore - (Depth - depth);
                    return -score;
                } else {
                    return 0;
                }
            }

            nodesEvaluated ++;
            float evaluation = evaluator.Eval (board);

            if (evaluation >= beta)
                return beta;
            if (evaluation > alpha)
                alpha = evaluation;

            // if (depth == 0)
            // {
            //     return alpha;
            // }

            UInt64[] legalMoves = board.moveGenerator.GenerateMoves ();
            List<Move> allMoves = board.moveGenerator.FormatLegalMoves (true);
            MoveOrdering.OrderMoves (board, allMoves, true);

            UInt64 friendlyCharacters = board.whiteMove ? board.white.GetCombinedBinary() : board.black.GetCombinedBinary();
            UInt64 enemyCharacters = board.whiteMove ? board.black.GetCombinedBinary() : board.white.GetCombinedBinary ();

            int castlingRights = board.castlingRights;
            bool whiteMove = board.whiteMove;

            foreach (Move move in allMoves)
            {
                int to = move.to;
                int from = move.from;

                // Move board
                bool errorCode = board.Push (from, to);

                #region Save Data
                bool wasCapturing = board.wasCapturingMove;
                bool wasCastling = board.wasCastlingMove;
                bool wasPromotion = board.wasPromotion;
                bool wasCheck = board.inCheck;
                bool wasCheckmate = board.inCheckmate;

                Pieces pawnsPromotedFrom = board.pawnsPromotedFrom;
                Pieces queensPromotedTo = board.queensPromotedTo;
                Pieces pieceCaptured = board.latestPieceCaptured;
                Pieces rooksCastled = board.rooksCastled;
                int rookCastleSquare = board.latestRookCastleSqr;
                int rookCastleSquareFrom = board.latestRookCastleSqrFrom;
                #endregion

                float score = -Quiesce (depth - 1, -beta, -alpha);

                board.inCheck = wasCheck;
                board.inCheckmate = wasCheckmate;

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
            }

            return alpha;
        }

        bool foundBestMove = false;
        int From = -1;
        int To = -1;
        int movesEvaled = 0;

        public void Update()
        {
            moveCountText.text = nodesEvaluated.ToString();
            if (foundBestMove)
            {
                try { Debug.Log ($"Making AI Move. {DebugC.IndexToString (From)} {DebugC.IndexToString (To)}"); }
                catch (System.Exception) { Debug.Log (From + " : " + To); }
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
            // GetBestMove ();
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

        public void BestMoveWithSearch ()
        {
            stopwatch = new System.Diagnostics.Stopwatch ();
            stopwatch.Start ();

            nodesEvaluated = 0;
            checkmatesFound = 0;
            nodesCutoff = 0;

            // This is the Ai's function that decides move to make
            CopyBoard (gm.board, ref board);

            white = gm.board.whiteMove;
            Search (Depth, float.MinValue, float.MaxValue);

            Debug.Log ($"Search was {stopwatch.ElapsedMilliseconds}ms long.");

            foundBestMove = true;
        }

        public void GetBestMove ()
        {
            if (!useOpeningBook)
            {
                BestMoveWithSearch ();
                return;
            }

            string fen = (gm.board.GetFen ());
            string replacedFen = fen.Replace("/", "");
            
            try {
                List<ToFrom> allMoves = ParseOpeningBook.Book.book[replacedFen];
                var rnd = new System.Random ();

                bool foundLegalMove = false;

                while (!foundLegalMove) {
                    CopyBoard (gm.board, ref board);

                    ToFrom resultingMove = allMoves[rnd.Next (0, allMoves.Count)];
                    this.From = resultingMove.from;
                    this.To = resultingMove.to;
                    foundLegalMove = board.Push (this.From, this.To);
                }
                
            } catch (System.Exception err)
            {
                BestMoveWithSearch ();
            }

            foundBestMove = true;
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