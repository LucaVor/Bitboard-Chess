using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;

/*
-- Check pesudo code --
check: wether current player is in check
checkSquares: the squares of the pieces that is putting the king in check
pieceType: the type of piece putting the king in check
directionHash: table dictating dir between two squares
betweenHash: table with positions between two squares
isSlider: wether the piece is a slider piece
square: square of current piece
directions: table with squares from square to dir
kingSquare: square of friendly king
attackedSquares: all squares that are getting attacked by enemy
directionType: table that has 0 if direction is up/down/left/right and 1 if diagonal

______________

if pieceType == KING {
    exclude moves that are in attackedSquares (MAKE SURE TO EXCLUDE KING WHEN CALCULATING)
    return
}

if more than two things are checking king {
    moves = 0
}

isPinned = false
// check if it is pinned, if so exclude all moves that aren't in directionHash
if directionHash[square][kingSquare] != NO_DIRECTION
{
    // this CAN be a pinned piece
    spots = directions[kingSquare][directionHash[square][kingSquare]]

    // USE _BitScanForward and _ReverseBitScan to get closest rook/bishop/queen

    if directionType[directionHash[square][kingSquare]] == 0 {
        if a ENEMY rook or a queen is in spots {
            if there is no piece between this piece and the rook/queen {
                isPinned = true
            }
        }
    }

    if directionType[directionHash[square][kingSquare]] == 1 {
        if a ENEMY bishop or a queen is in spots {
            if there is no piece between this piece and the bishop/queen {
                isPinned = true
            }
        }
    }
}

if isPinned
{
    exclude all moves that aren't in (spots | rook/bishop/queen)
}

if check
{
    CHECKSQUARE IS A LIST AND LOOP THROUGH ALL CHECKERS
    exclude all moves that aren't checkSquare or in betweenHash
}

______________
*/

namespace QuickChess
{
    public class MoveGeneration
    {
        public Board board;
        public UInt64[] moves;
        public UInt64 attackedSquares;
        public UInt64 possibleAttackedSquares;
        public UInt64 amalgamatedMoves = 0;

        public const int EXCLUDE_KING = 0;
        public const int INCLUDE_KING = 1;

        public MoveGeneration(Board _board)
        {
            board = _board;
        }
        
        public struct EnemyAttackingReturnData
        {
            public UInt64 attackedSquares;
            public int[] checkers;
        }

        public UInt64[] GenerateMoves(bool psuedo = false)
        {
            moves = new UInt64[64];
            attackedSquares = 0;
            possibleAttackedSquares = 0;
            amalgamatedMoves = 0;

            UInt64 whiteBinary = board.white.GetCombinedBinary();
            UInt64 blackBinary = board.black.GetCombinedBinary();

            UInt64 blockers = whiteBinary | blackBinary;

            Pieces[] friendlyPieces = board.whiteMove ? board.white.pieces : board.black.pieces;
            Pieces[] enemyPieces = board.whiteMove ? board.black.pieces : board.white.pieces;

            UInt64 enemyRooks = enemyPieces[2].bitmap;
            UInt64 enemyBishops = enemyPieces[3].bitmap;
            UInt64 enemyQueens = enemyPieces[1].bitmap;

            UInt64 friendly = board.whiteMove ? whiteBinary : blackBinary;
            UInt64 enemy = board.whiteMove ? blackBinary : whiteBinary;

            int colour = board.whiteMove ? PieceBinary.White : PieceBinary.Black;
            int oppositeColour = board.whiteMove ? PieceBinary.Black : PieceBinary.White;

            int friendlyKingSquare = board.whiteMove ? board.white.King.currentPieces[0] : board.black.King.currentPieces[0];

            EnemyAttackingReturnData eard = GetAllAttackedSquaresExcludingKing (oppositeColour, friendly, enemy, enemyPieces, friendlyKingSquare);

            UInt64 enemyAttackedSquares = eard.attackedSquares;
            int[] checkers = eard.checkers;
            
            if (checkers[1] == -1)
            {
                checkers = new int[] {checkers[0]};
            } if (checkers[0] == -1)
            {
                checkers = new int[0];
            }
            
            // TODO: Make it only generate king moves if there is more than one checker

            bool inCheck = (enemyAttackedSquares & (1UL << friendlyKingSquare)) != 0;

            for (int pc = 0; pc < 6; pc ++)
            {
                Pieces piece = friendlyPieces[pc];

                for (int index = 0; index < piece.pieces; index ++)
                {
                    int i = piece.currentPieces[index];
                    UInt64 move = 0;

                    switch (pc)
                    {
                        case 0:                              
                            move = GetKingMoves (i, colour, friendly, enemy);
                            move = MakeLegal (
                                move, PreProcessing.KING, inCheck, checkers, i, friendlyKingSquare, enemyAttackedSquares, blockers, enemyRooks, enemyBishops, enemyQueens
                            );

                            break;
                        case 1:                              
                            move = GetQueenMoves (i, friendly, enemy);
                            move = MakeLegal (
                                move, PreProcessing.QUEEN, inCheck, checkers, i, friendlyKingSquare, enemyAttackedSquares, blockers, enemyRooks, enemyBishops, enemyQueens
                            );
                            
                            break;
                        case 2:                              
                            move = GetRookMoves (i, friendly, enemy);
                            move = MakeLegal (
                                move, PreProcessing.ROOK, inCheck, checkers, i, friendlyKingSquare, enemyAttackedSquares, blockers, enemyRooks, enemyBishops, enemyQueens
                            );
                            
                            break;
                        case 3:                              
                            move = GetBishopMoves (i, friendly, enemy);
                            move = MakeLegal (
                                move, PreProcessing.BISHOP, inCheck, checkers, i, friendlyKingSquare, enemyAttackedSquares, blockers, enemyRooks, enemyBishops, enemyQueens
                            );
                            
                            break;
                        case 4:                              
                            move = GetKnightMoves (i, friendly, enemy);
                            move = MakeLegal (
                                move, PreProcessing.KNIGHT, inCheck, checkers, i, friendlyKingSquare, enemyAttackedSquares, blockers, enemyRooks, enemyBishops, enemyQueens
                            );
                            
                            break;
                        case 5:
                            move = GetPawnMoves (i, colour, friendly, enemy);
                            move = MakeLegal (
                                move, PreProcessing.PAWN, inCheck, checkers, i, friendlyKingSquare, enemyAttackedSquares, blockers, enemyRooks, enemyBishops, enemyQueens
                            );
                            
                            break;
                    }
                    
                    if ((move & (1UL << i)) != 0)
                    {
                        move -= (1UL << i);
                    }

                    amalgamatedMoves |= move;

                    moves[i] = move;
                }
            }

            if (amalgamatedMoves == 0)
            {
                // Checkmate
                Console.WriteLine ("In checkmate.");
                board.inCheckmate = true;
            }

            return moves;
        }

        public UInt64 MakeLegal (UInt64 moves, int pieceType, bool inCheck, int[] checkers, int square, int kingSquare, UInt64 enemyAttacked, UInt64 blockers, UInt64 enemyRooks, UInt64 enemyBishops, UInt64 enemyQueens)
        {
            if (pieceType == PreProcessing.KING) {
                moves &= ~(enemyAttacked);
                return moves;
            }

            if (checkers.Length > 1) {
                return 0;
            }

            bool isPinned = false;
            Direction dir = PreProcessing.directionBetween[kingSquare,square];

            UInt64 pinnedFile = 0UL;

            if (dir != Direction.None)
            {
                UInt64 spots = PreProcessing.directions[(int)dir,kingSquare];
                // USE _BitScanForward and _ReverseBitScan to get closest rook/bishop/queen

                if (PreProcessing.Diagonals[(int)dir] == 0) {
                    UInt64 spotRookMask = spots & (enemyRooks | enemyQueens);

                    if (spotRookMask != 0) {
                        int pinnerIndex = 0;
                    
                        if (PreProcessing.Upwards[(int)dir] == 1)
                        {
                            pinnerIndex = (int) Bmi1.X64.TrailingZeroCount (spotRookMask);
                        } else {
                            pinnerIndex = _ReverseBitScan (spotRookMask);
                        }

                        pinnedFile |= (1UL << pinnerIndex);

                        UInt64 pieceToRQ = PreProcessing.squaresBetween[square,pinnerIndex];
                        pinnedFile |= pieceToRQ;

                        if ((pieceToRQ & blockers) == 0) {
                            Console.WriteLine (pieceType + " is pinned by " + Program.IndexToString(pinnerIndex));
                            isPinned = true;
                        }
                    }
                }

                if (PreProcessing.Diagonals[(int)dir] == 1) {
                    UInt64 spotBishopMask = spots & (enemyRooks | enemyQueens);

                    if (spotBishopMask != 0) {
                        int pinnerIndex = 0;
                    
                        if (PreProcessing.Upwards[(int)dir] == 1)
                        {
                            pinnerIndex = (int) Bmi1.X64.TrailingZeroCount (spotBishopMask);
                        } else {
                            pinnerIndex = _ReverseBitScan (spotBishopMask);
                        }

                        pinnedFile |= (1UL << pinnerIndex);

                        UInt64 pieceToBQ = PreProcessing.squaresBetween[square,pinnerIndex];
                        pinnedFile |= pieceToBQ;

                        if ((pieceToBQ & blockers) == 0) {
                            isPinned = true;
                        }
                    }
                }
            }

            if (isPinned)
            {
                moves &= pinnedFile;
            }

            if (inCheck)
            {
                for (int c = 0; c < checkers.Length; c ++)
                {
                    UInt64 betweenKing = PreProcessing.squaresBetween[kingSquare,checkers[c]] | (1UL << checkers[c]);
                    moves &= betweenKing;
                }
            }

            return moves;
        }

        public EnemyAttackingReturnData GetAllAttackedSquaresExcludingKing (int colour, UInt64 friend, UInt64 enemy, Pieces[] friendlyPieces, int targetKing)
        {
            UInt64 allAttacked = 0UL;

            UInt64 whiteBinary = board.white.GetCombinedBinary();
            UInt64 blackBinary = board.black.GetCombinedBinary();

            int[] checkers = new int[]{-1, -1};
            int currentChecker = 0;

            for (int pc = 0; pc < 6; pc ++)
            {
                Pieces piece = friendlyPieces[pc];

                for (int index = 0; index < piece.pieces; index ++)
                {
                    int i = piece.currentPieces[index];
                    UInt64 move = 0;

                    switch (pc)
                    {
                        case 0:                              
                            move = GetKingMoves (i, colour, enemy, friend, EXCLUDE_KING);

                            break;
                        case 1:                              
                            move = GetQueenMoves (i, enemy, friend, EXCLUDE_KING);
                            
                            break;
                        case 2:                              
                            move = GetRookMoves (i, enemy, friend, EXCLUDE_KING);
                            
                            break;
                        case 3:                              
                            move = GetBishopMoves (i, enemy, friend, EXCLUDE_KING);
                            
                            break;
                        case 4:                              
                            move = GetKnightMoves (i, enemy, friend, EXCLUDE_KING);
                            
                            break;
                        case 5:
                            move = GetPawnMoves (i, colour, enemy, friend, EXCLUDE_KING);
                            
                            break;
                    }

                    if ((move & (1UL << targetKing)) != 0)
                    {
                        if (currentChecker >= 2)
                        {
                            Console.WriteLine ("More than two checkers! Ilegal position!");
                        } else {
                            checkers[currentChecker] = i;
                            currentChecker ++;
                        }
                    }

                    allAttacked |= move;
                }
            }

            return new EnemyAttackingReturnData () {
                checkers = checkers,
                attackedSquares = allAttacked
            };

        }

        public int TARGET_KING_SQUARE = 0;

        public UInt64 GetKingMoves (int square, int colour, UInt64 friend, UInt64 enemy, int useKingMask = INCLUDE_KING)
        {
            UInt64 moves = PreProcessing.precomputedKingMoves[square];

            if (useKingMask == EXCLUDE_KING) return moves;

            moves &= ~friend;

            UInt64 blockers = friend | enemy;

            UInt64 castlingMoves = colour == PieceBinary.White ? PreProcessing.precomputedWhiteKingCastling[square] : PreProcessing.precomputedBlackKingCastling[square];
            bool canCastle = (board.castlingRights & colour) != 0;

            if (canCastle && castlingMoves != 0)
            {
                int kingSide = colour == PieceBinary.White ? Board.WhiteKingCastling : Board.BlackKingCastling;
                int queenSide = colour == PieceBinary.White ? Board.WhiteQueenCastling : Board.BlackQueenCastling;
                UInt64 kingSidePieceMask = colour == PieceBinary.White ? PreProcessing.whiteKingSidePieceCheckingMask : PreProcessing.blackKingSidePieceCheckingMask;
                UInt64 queenSidePieceMask = colour == PieceBinary.White ? PreProcessing.whiteQueenSidePieceCheckingMask : PreProcessing.blackQueenSidePieceCheckingMask;

                if (
                    (board.castlingRights & kingSide) == 0 || (blockers & kingSidePieceMask) != 0
                ) {
                    castlingMoves &= ~PreProcessing.kingSideMask;
                }

                if (
                    (board.castlingRights & queenSide) == 0 || (blockers & queenSidePieceMask) != 0
                ) {
                    castlingMoves &= ~PreProcessing.queenSideMask;
                }

            } else {
                castlingMoves = 0;
            }

            attackedSquares |= moves;
            possibleAttackedSquares |= moves;
            
            return moves | castlingMoves;
        }

        public UInt64 GetQueenMoves (int square, UInt64 friend, UInt64 enemy, int useKingMask = INCLUDE_KING)
        {
            if (useKingMask == EXCLUDE_KING) enemy &= ~(1UL << TARGET_KING_SQUARE);

            UInt64 moves = GetBishopMoves (square, friend, enemy, useKingMask) | GetRookMoves (square, friend, enemy, useKingMask);
            attackedSquares |= moves;
            possibleAttackedSquares |= moves;
            return moves;
        }

        public UInt64 GetPawnMoves (int square, int colour, UInt64 friend, UInt64 enemy, int useKingMask = INCLUDE_KING)
        {
            UInt64 moves = (colour == PieceBinary.White) ? PreProcessing.precomputedWhitePawnMoves[square] : PreProcessing.precomputedBlackPawnMoves[square];
            UInt64 attacks = (colour == PieceBinary.White) ? PreProcessing.precomputedWhitePawnAttacks[square] : PreProcessing.precomputedBlackPawnAttacks[square];
            
            if (useKingMask == EXCLUDE_KING) return attacks;

            possibleAttackedSquares |= attacks;
            
            attacks &= enemy;
            moves &= ~(friend | enemy);

            int offset = colour == PieceBinary.White ? 16 : -16;
            if (
                (moves & (1UL << (square + offset))) != 0 && (
                    (friend | enemy) & (1UL << (square + (offset / 2)))
                ) != 0)
            {
                moves ^= 1UL << (square + offset);
            }

            attackedSquares |= attacks;

            return moves | attacks;
        }

        public UInt64 GetKnightMoves(int square, UInt64 friend, UInt64 enemy, int useKingMask = INCLUDE_KING)
        {
            UInt64 moves = PreProcessing.precomputedKnightAttacks[square];
            moves &= ~friend;

            attackedSquares |= moves;
            possibleAttackedSquares |= moves;

            return moves;
        }

        public UInt64 GetBishopMoves(int square, UInt64 friend, UInt64 enemy, int useKingMask = INCLUDE_KING)
        {
            if (useKingMask == EXCLUDE_KING) enemy &= ~(1UL << TARGET_KING_SQUARE);

            UInt64 blockers = friend | enemy;
            UInt64 moves = 0UL;
            
            moves |= PreProcessing.directions[(int) Direction.NorthEast, square];
            if ((PreProcessing.directions[(int) Direction.NorthEast, square] & blockers) != 0)
            {
                int blockerIndex = (int) Bmi1.X64.TrailingZeroCount (
                    PreProcessing.directions[(int) Direction.NorthEast, square] & blockers
                );

                moves &= ~PreProcessing.directions[(int) Direction.NorthEast, blockerIndex];
                if ((friend & (1UL << blockerIndex)) != 0)
                {
                    moves ^= (1UL << blockerIndex);
                } if ((enemy & (1UL << blockerIndex)) != 0)
                {
                    moves |= (1UL << blockerIndex);
                } if ((friend & (1UL << blockerIndex)) != 0 && useKingMask == EXCLUDE_KING)
                {
                    moves |= (1UL << blockerIndex);
                }
            }

            moves |= PreProcessing.directions[(int) Direction.NorthWest, square];
            if ((PreProcessing.directions[(int) Direction.NorthWest, square] & blockers) != 0)
            {
                int blockerIndex = (int) Bmi1.X64.TrailingZeroCount (
                    PreProcessing.directions[(int) Direction.NorthWest, square] & blockers
                );

                moves &= ~PreProcessing.directions[(int) Direction.NorthWest, blockerIndex];
                if ((friend & (1UL << blockerIndex)) != 0)
                {
                    moves ^= (1UL << blockerIndex);
                } if ((enemy & (1UL << blockerIndex)) != 0)
                {
                    moves |= (1UL << blockerIndex);
                } if ((friend & (1UL << blockerIndex)) != 0 && useKingMask == EXCLUDE_KING)
                {
                    moves |= (1UL << blockerIndex);
                }
            }

            moves |= PreProcessing.directions[(int) Direction.SouthEast, square];
            if ((PreProcessing.directions[(int) Direction.SouthEast, square] & blockers) != 0)
            {
                int blockerIndex = _ReverseBitScan (
                    PreProcessing.directions[(int) Direction.SouthEast, square] & blockers
                );

                moves &= ~PreProcessing.directions[(int) Direction.SouthEast, blockerIndex];
                if ((friend & (1UL << blockerIndex)) != 0)
                {
                    moves ^= (1UL << blockerIndex);
                } if ((enemy & (1UL << blockerIndex)) != 0)
                {
                    moves |= (1UL << blockerIndex);
                } if ((friend & (1UL << blockerIndex)) != 0 && useKingMask == EXCLUDE_KING)
                {
                    moves |= (1UL << blockerIndex);
                }
            }

            moves |= PreProcessing.directions[(int) Direction.SouthWest, square];
            if ((PreProcessing.directions[(int) Direction.SouthWest, square] & blockers) != 0)
            {
                int blockerIndex = _ReverseBitScan (
                    PreProcessing.directions[(int) Direction.SouthWest, square] & blockers
                );

                moves &= ~PreProcessing.directions[(int) Direction.SouthWest, blockerIndex];
                if ((friend & (1UL << blockerIndex)) != 0)
                {
                    moves ^= (1UL << blockerIndex);
                } if ((enemy & (1UL << blockerIndex)) != 0)
                {
                    moves |= (1UL << blockerIndex);
                } if ((friend & (1UL << blockerIndex)) != 0 && useKingMask == EXCLUDE_KING)
                {
                    moves |= (1UL << blockerIndex);
                }
            }

            attackedSquares |= moves;
            possibleAttackedSquares |= moves;

            return moves;
        }

        public UInt64 GetRookMoves (int square, UInt64 friend, UInt64 enemy, int useKingMask = INCLUDE_KING)
        {
            if (useKingMask == EXCLUDE_KING) enemy &= ~(1UL << TARGET_KING_SQUARE);

            UInt64 blockers = friend | enemy;
            UInt64 moves = 0UL;
            
            moves |= PreProcessing.directions[(int) Direction.North, square];
            if ((PreProcessing.directions[(int) Direction.North, square] & blockers) != 0)
            {
                int blockerIndex = (int) Bmi1.X64.TrailingZeroCount (
                    PreProcessing.directions[(int) Direction.North, square] & blockers
                );

                moves &= ~PreProcessing.directions[(int) Direction.North, blockerIndex];
                if ((friend & (1UL << blockerIndex)) != 0)
                {
                    moves ^= (1UL << blockerIndex);
                } if ((enemy & (1UL << blockerIndex)) != 0)
                {
                    moves |= (1UL << blockerIndex);
                } if ((friend & (1UL << blockerIndex)) != 0 && useKingMask == EXCLUDE_KING)
                {
                    moves |= (1UL << blockerIndex);
                }
            }

            moves |= PreProcessing.directions[(int) Direction.East, square];
            if ((PreProcessing.directions[(int) Direction.East, square] & blockers) != 0)
            {
                int blockerIndex = (int) Bmi1.X64.TrailingZeroCount (
                    PreProcessing.directions[(int) Direction.East, square] & blockers
                );

                moves &= ~PreProcessing.directions[(int) Direction.East, blockerIndex];
                if ((friend & (1UL << blockerIndex)) != 0)
                {
                    moves ^= (1UL << blockerIndex);
                } if ((enemy & (1UL << blockerIndex)) != 0)
                {
                    moves |= (1UL << blockerIndex);
                } if ((friend & (1UL << blockerIndex)) != 0 && useKingMask == EXCLUDE_KING)
                {
                    moves |= (1UL << blockerIndex);
                }
            }

            moves |= PreProcessing.directions[(int) Direction.West, square];
            if ((PreProcessing.directions[(int) Direction.West, square] & blockers) != 0)
            {
                // TODO: Try making it Bmi1.X64.TrailingZeroCount to speed things up
                int blockerIndex = _ReverseBitScan (
                    PreProcessing.directions[(int) Direction.West, square] & blockers
                );

                moves &= ~PreProcessing.directions[(int) Direction.West, blockerIndex];
                if ((friend & (1UL << blockerIndex)) != 0)
                {
                    moves ^= (1UL << blockerIndex);
                } if ((enemy & (1UL << blockerIndex)) != 0)
                {
                    moves |= (1UL << blockerIndex);
                } if ((friend & (1UL << blockerIndex)) != 0 && useKingMask == EXCLUDE_KING)
                {
                    moves |= (1UL << blockerIndex);
                }
            }

            moves |= PreProcessing.directions[(int) Direction.South, square];
            if ((PreProcessing.directions[(int) Direction.South, square] & blockers) != 0)
            {
                int blockerIndex = _ReverseBitScan (
                    PreProcessing.directions[(int) Direction.South, square] & blockers
                );

                moves &= ~PreProcessing.directions[(int) Direction.South, blockerIndex];
                if ((friend & (1UL << blockerIndex)) != 0)
                {
                    moves ^= (1UL << blockerIndex);
                } if ((enemy & (1UL << blockerIndex)) != 0)
                {
                    moves |= (1UL << blockerIndex);
                } if ((friend & (1UL << blockerIndex)) != 0 && useKingMask == EXCLUDE_KING)
                {
                    moves |= (1UL << blockerIndex);
                }
            }

            attackedSquares |= moves;
            possibleAttackedSquares |= moves;

            return moves;
        }

        public int _ReverseBitScan (UInt64 bin)
        {
            for (int i = 0; i < 6; i ++)
                bin |= bin >> (1 << i);

            bin ^= (bin >> 1);
            int index = (int) System.Runtime.Intrinsics.X86.Bmi1.X64.TrailingZeroCount (bin);

            return index;
        }
    }
}