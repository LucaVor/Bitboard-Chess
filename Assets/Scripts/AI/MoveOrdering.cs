using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickChess
{
    public class MoveOrdering : MonoBehaviour
    {
        public static int[] moveEvals = new int[218]; /* 218 is the maximum amount of legal moves in chess */
        public static Move bestMove = new Move(); /* For iterative deepening */

        public static bool MoveEquals (Move a, Move b)
        {
            return a.from == b.from && a.to == b.to;
        }

        public static void OrderMoves (Board board, List<Move> moves, bool useID = false)
        {
            for (int i = 0; i < moves.Count; i ++)
            {
                int eval = 0;

                if (moves[i].isCapture)
                {
                    eval += 10 * Move.PieceValues[moves[i].pieceCapturing] - Move.PieceValues[moves[i].pieceType];
                    eval += 1000;
                }

                if (((1UL << moves[i].to) & board.moveGenerator.pawnAttackedMoves) != 0)
                {
                    eval -= 200;
                }

                eval += Move.PieceValues[moves[i].pieceType] * 2;

                if (MoveEquals (bestMove, moves[i]) && useID)
                {
                    Debug.Log ("Looking at best move for previous depth iteration.");
                    eval += 100000;
                }

                moveEvals[i] = eval;
            }

            for (int i = 0; i < moves.Count - 1; i++) {
				for (int j = i + 1; j > 0; j--) {
					int swapIndex = j - 1;
					if (moveEvals[swapIndex] < moveEvals[j]) {
						(moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
						(moveEvals[j], moveEvals[swapIndex]) = (moveEvals[swapIndex], moveEvals[j]);
					}
				}
			}
        }
    }
}