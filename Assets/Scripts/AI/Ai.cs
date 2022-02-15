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
        public static Ai ai;
        public static Thread thread;

        public Board board;
        public Evaluate evaluator;

        public bool white;
        public bool useOpeningBook;
        public int nodesEvaluated = 0;
        public int checkmatesFound = 0;
        public int nodesCutoff = 0;

        public int millisecondLimit = 5000;
        public bool inSearch = false;

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

            millisecondLimit = SceneManagement.DEPTH;

            // Depth = SceneManagement.DEPTH;
        }

        public bool IsMateScore (float score)
        {
            const int mateScore = 1000000;
            return Mathf.Abs (score) >= (mateScore - Depth);
        }

        public bool cancelledSearch = false;
        public Dictionary<Move, float> superficialEvaluations;

        public float Search (int depth, float alpha, float beta, bool max)
        {
            if (abortSearch)
            {
                abortedSearch = true;
                return 0;
            }

            const int mateScore = 1000000;

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
                // float eval = ((SceneManagement.boolQuiesence) ? (Quiesce (0, alpha, beta)) : (evaluator.Eval (board)));
                float eval = evaluator.Eval (board);
                if (!max)
                {
                    eval *= -1;
                }

                return eval;
            }

            if (board.inCheckmate)
            {
                checkmatesFound++;
                board.moveGenerator.GenerateMoves ();

                if (board.inCheck)
                {
                    float score = mateScore - (Depth - depth);
                    return -score;
                } else {
                    return 0;
                }
            }

            UInt64[] legalMoves = board.legalMoves;
            List<Move> formattedMoves = board.moveGenerator.FormatLegalMoves ();
            MoveOrdering.OrderMoves (board, formattedMoves, (depth == Depth) && (depth > 1));

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
                bool wasEnPassant = board.wasEnPassant;

                Pieces pawnsPromotedFrom = board.pawnsPromotedFrom;
                Pieces queensPromotedTo = board.queensPromotedTo;
                Pieces pieceCaptured = board.latestPieceCaptured;
                Pieces rooksCastled = board.rooksCastled;
                int rookCastleSquare = board.latestRookCastleSqr;
                int rookCastleSquareFrom = board.latestRookCastleSqrFrom;
                int enPassantPieceCaptured = board.latestEnPassantPawnCaptured;
                UInt64 enPassantFile = board.enPassantFile;
                #endregion

                float eval = -Search (depth - 1, -beta, -alpha, !max);
                superficialEvaluations.Add (move, eval);

                board.inCheck = wasCheck;
                board.inCheckmate = wasCheckmate;

                #region Undo Move
                if (!wasPromotion)
                {
                    board.ForcePush (to, from);
                }

                if (wasCapturing)
                {
                    if (!wasEnPassant)
                        pieceCaptured.AddPieceAt (to);
                    else
                        pieceCaptured.AddPieceAt (enPassantPieceCaptured);

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
                board.enPassantFile = enPassantFile;

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

        bool foundBestMove = false;
        int From = -1;
        int To = -1;
        int movesEvaled = 0;
        int currentDepth = 0;

        public void Update()
        {
            moveCountText.text = currentDepth.ToString ();

            if (inSearch){
                if (stopwatch.ElapsedMilliseconds > millisecondLimit && currentDepth > 5)
                {
                    abortSearch = true;
                    inSearch = false;
                }
            }
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

        public static void CopyBoard (Board template, ref Board output)
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

        public float SearchWithDepth (int depth)
        {
            nodesEvaluated = 0;
            checkmatesFound = 0;
            nodesCutoff = 0;

            // This is the Ai's function that decides move to make
            CopyBoard (gm.board, ref board);

            white = gm.board.whiteMove;
            Depth = depth;
            return Search (depth, float.MinValue, float.MaxValue, white);
        }

        public bool abortedSearch = false;
        public bool abortSearch = false;

        public void BestMoveWithSearch ()
        {
            stopwatch = new System.Diagnostics.Stopwatch ();
            stopwatch.Start ();

            inSearch = true;

            const int MaximumDepth = 20;

            abortedSearch = false;
            abortSearch = false;

            ToFrom previousBest = new ToFrom ();
            superficialEvaluations = new Dictionary<Move, float> ();

            for (int i = 1; i <= MaximumDepth; i += 1)
            {
                /*
                    LUCA, don't delete this message until it is done.
                    You must make it so that you can exclude specfic
                    moves from fen in ParseOpeningBook, reparse, and recache.
                */

                currentDepth = i;
                float best = SearchWithDepth (i);
                MoveOrdering.SetMoveEvaluations (superficialEvaluations);
                superficialEvaluations = new Dictionary<Move, float> ();

                if (abortedSearch)
                {
                    this.To = previousBest.to;
                    this.From = previousBest.from;
                    foundBestMove = true;
                    Debug.Log ($"Aborted at depth {i}, returning {DebugC.IndexToString (this.From)}, {DebugC.IndexToString (this.To)}");
                    return;
                } else {
                    previousBest.to = this.To;
                    previousBest.from = this.From;
                    foundBestMove = false;

                    if (IsMateScore (best))
                    {
                        foundBestMove = true;
                        return;
                    }

                    Debug.Log ($"Searched at depth {i}, searching at higher depth. ({DebugC.IndexToString (this.From)}, {DebugC.IndexToString (this.To)})");
                }
            }

            Debug.Log ($"Search was {stopwatch.ElapsedMilliseconds}ms long.");
        }

        public void GetBestMove ()
        {
            ToFrom stockFishBest = GameManager.instance.GetBestMoveStockfish ();
            this.From = stockFishBest.from;
            this.To = stockFishBest.to;
            foundBestMove = true;

            return;
            
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

                foundBestMove = true;
            } catch (System.Exception err)
            {
                BestMoveWithSearch ();
            }
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