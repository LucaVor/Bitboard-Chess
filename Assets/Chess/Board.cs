using System;
using System.Collections;
using System.Collections.Generic;

namespace QuickChess
{
    public class Board
    {
        public PieceMap white;
        public PieceMap black;

        public MoveGeneration moveGenerator;
        public UInt64 One = 1;

        public UInt64[] legalMoves;

        public bool whiteMove = true;

        public const int WhiteCastling = 8;
        public const int BlackCastling = 16;
        public const int WhiteKingCastling = 2;
        public const int WhiteQueenCastling = 4;
        public const int BlackKingCastling = 32;
        public const int BlackQueenCastling = 64;

        public int castlingRights = (
            WhiteCastling | BlackCastling | WhiteKingCastling | WhiteQueenCastling | BlackKingCastling | BlackQueenCastling
        );

        public string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
        public const string StartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
        private string numbers = "0123456789";

        public bool inCheckmate = false;

        public Board(string _fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR")
        {
            white = new PieceMap();
            black = new PieceMap();

            moveGenerator = new MoveGeneration(this);

            LoadFen (_fen);
        }

        public Board (bool _whiteMove)
        {
            white = new PieceMap ();
            black = new PieceMap();

            whiteMove = _whiteMove;

            moveGenerator = new MoveGeneration (this);

            LoadFen (StartingFen);
        }

        public void ForcePush (int from, int to)
        {
            Pieces pieceType = GetPieceAt (from);

            pieceType.RemovePieceAt (from);
            pieceType.AddPieceAt (to);
        }

        public delegate void GameManagerCallbackOnCastle (int from, int to);
        public GameManagerCallbackOnCastle onCastleCallback = BlankOnCastleCallback;
        public delegate void GameManagerCallbackOnPromotion (int from, int at, bool isWhiteP);
        public GameManagerCallbackOnPromotion onPromotionCallback = BlankOnPromotionCallback;
        public delegate void GameManagerCallbackOnCheckmate ();
        public GameManagerCallbackOnCheckmate onCheckmateCallback = BlankOnCheckmateCallback;
        

        public bool wasCastlingMove = false;
        public bool wasCapturingMove = false;
        public bool wasPromotion = false;

        public Pieces latestPieceCaptured = null;
        public Pieces rooksCastled = null;
        public Pieces queensPromotedTo = null;
        public Pieces pawnsPromotedFrom = null;
        public int latestRookCastleSqr = -1;
        public int latestRookCastleSqrFrom = -1;

        public string latestError = "";
        public const string IllegalMove = "Illegal Move for piece.";
        public const string NoMovesFor = "No Moves For.";
        public const string FriendlyAt = "Friendly at destination.";
        public const string WrongTurn = "It is other players turn.";

        public static void BlankOnCastleCallback (int a, int b) {}
        public static void BlankOnPromotionCallback (int a, int b, bool c) {}
        public static void BlankOnCheckmateCallback () {}

        public bool Push (int from, int to)
        {
            // legalMoves = moveGenerator.GenerateMoves();

            if (from == to) return false;
            if (legalMoves[from] == 0) {
                // Console.WriteLine ($"Cannot make {DebugC.IndexToString (from)}{DebugC.IndexToString (from)}.");
                latestError = NoMovesFor;
                return false;
            }
            
            UInt64 friendlyBinary = whiteMove ? white.GetCombinedBinary() : black.GetCombinedBinary();
            if ((friendlyBinary & (1UL << to)) != 0)
            {
                latestError = FriendlyAt;
                return false;
            }

            UInt64 moveBinary = legalMoves[from];

            if (
                (moveBinary & (1UL << to)) == 0
            )
            {
                latestError = IllegalMove;
                return false;
            }

            UInt64 whiteBinary = white.GetCombinedBinary();
            UInt64 blackBinary = black.GetCombinedBinary();

            bool isWhite = (whiteBinary & (One << from)) != 0;
            bool isBlack = (blackBinary & (One << from)) != 0;

            UInt64 enemyBinary = whiteMove ? blackBinary : whiteBinary;

            if (!(isWhite || isBlack))
            {
                return false;
            }

            if (whiteMove != isWhite)
            {
                latestError = WrongTurn;
                return false;
            }

            Pieces pieceType = GetPieceAt (from);
            
            if ((enemyBinary & (1UL << to)) != 0)
            {
                Pieces pieceAt = GetPieceAt (to);
                latestPieceCaptured = pieceAt;

                pieceAt.RemovePieceAt (to);

                wasCapturingMove = true;
            } else {
                wasCapturingMove = false;
            }

            bool wasKingMove = pieceType == white.King || pieceType == black.King;

            pieceType.RemovePieceAt (from);
            pieceType.AddPieceAt (to);

            bool thisWasC = false;

            #region HandleCastling (Rights & Moves)
            if (from == PreProcessing.whiteKingSquare && (castlingRights & WhiteCastling) != 0 && wasKingMove)
            {
                if (to == PreProcessing.whiteKingSideCastle)
                { // White king side castle
                    white.Rooks.RemovePieceAt (PreProcessing.whiteKingSideCastle + 1);
                    white.Rooks.AddPieceAt (PreProcessing.whiteKingSideCastle - 1);
                    onCastleCallback(PreProcessing.whiteKingSideCastle + 1, PreProcessing.whiteKingSideCastle - 1);
                    thisWasC = true;
                    rooksCastled = white.Rooks;
                    latestRookCastleSqr = PreProcessing.whiteKingSideCastle - 1;
                    latestRookCastleSqrFrom = PreProcessing.whiteKingSideCastle + 1;
                } if (to == PreProcessing.whiteQueenSideCastle)
                { // White queen side castle
                    white.Rooks.RemovePieceAt (PreProcessing.whiteQueenSideCastle - 2);
                    white.Rooks.AddPieceAt (PreProcessing.whiteQueenSideCastle + 1);
                    onCastleCallback(PreProcessing.whiteQueenSideCastle - 2, PreProcessing.whiteQueenSideCastle + 1);
                    thisWasC = true;
                    rooksCastled = white.Rooks;
                    latestRookCastleSqr = PreProcessing.whiteQueenSideCastle + 1;
                    latestRookCastleSqrFrom = PreProcessing.whiteQueenSideCastle - 2;
                }

                castlingRights -= WhiteCastling;
            } if (from == PreProcessing.blackKingSquare && (castlingRights & BlackCastling) != 0 && wasKingMove)
            {
                if (to == PreProcessing.blackKingSideCastle)
                { // Black king side castle
                    black.Rooks.RemovePieceAt (PreProcessing.blackKingSideCastle + 1);
                    black.Rooks.AddPieceAt (PreProcessing.blackKingSideCastle - 1);
                    onCastleCallback(PreProcessing.blackKingSideCastle + 1, PreProcessing.blackKingSideCastle - 1);
                    thisWasC = true;
                    rooksCastled = black.Rooks;
                    latestRookCastleSqr = PreProcessing.blackKingSideCastle - 1;
                    latestRookCastleSqrFrom = PreProcessing.blackKingSideCastle + 1;
                } if (to == PreProcessing.blackQueenSideCastle)
                { // Black queen side castle
                    black.Rooks.RemovePieceAt (PreProcessing.blackQueenSideCastle - 2);
                    black.Rooks.AddPieceAt (PreProcessing.blackQueenSideCastle + 1);
                    onCastleCallback(PreProcessing.blackQueenSideCastle - 2, PreProcessing.blackQueenSideCastle + 1);
                    thisWasC = true;
                    rooksCastled = black.Rooks;
                    latestRookCastleSqr = PreProcessing.blackQueenSideCastle + 1;
                    latestRookCastleSqrFrom = PreProcessing.blackQueenSideCastle - 2;
                }

                castlingRights -= BlackCastling;
            } if (from == PreProcessing.whiteKingSideCastle + 1 && (castlingRights & WhiteKingCastling) != 0)
            {
                castlingRights -= WhiteKingCastling;
            } if (from == PreProcessing.whiteQueenSideCastle - 2 && (castlingRights & WhiteQueenCastling) != 0)
            {
                castlingRights -= WhiteQueenCastling;
            } if (from == PreProcessing.blackKingSideCastle + 1 && (castlingRights & BlackKingCastling) != 0)
            {
                castlingRights -= BlackKingCastling;
            } if (from == PreProcessing.blackQueenSideCastle - 2 && (castlingRights & BlackQueenCastling) != 0)
            {
                castlingRights -= BlackQueenCastling;
            }

            wasCastlingMove = thisWasC;
            #endregion

            wasPromotion = false;

            if (isWhite)
            {
                if (pieceType == white.Pawns)
                {
                    if (to >= 56)
                    {
                        // White promotion
                        pieceType.RemovePieceAt (to);
                        white.Queens.AddPieceAt (to);

                        pawnsPromotedFrom = white.Pawns;
                        queensPromotedTo = white.Queens;

                        onPromotionCallback(from, to, isWhite);
                        wasPromotion = true;
                    }
                }
            }

            if (isBlack)
            {
                if (pieceType == black.Pawns)
                {
                    if (to <= 7)
                    {
                        // Black promotion
                        pieceType.RemovePieceAt (to);
                        black.Queens.AddPieceAt (to);

                        pawnsPromotedFrom = black.Pawns;
                        queensPromotedTo = black.Queens;

                        onPromotionCallback(from, to, isWhite);
                        wasPromotion = true;
                    }
                }
            }

            whiteMove = !whiteMove;
            legalMoves = moveGenerator.GenerateMoves ();

            return true;
        }

        public int GetLegalMoveCount ()
        {
            int moveCount = 0;
            
            Array.ForEach (legalMoves, (x) => {
                moveCount += GetOneCount (x);
            });

            return moveCount;
        }

        public int GetOneCount (UInt64 binary)
        {
            int oneCount = 0;
            UInt64 binaryTemp = binary;

            while (binaryTemp != 0)
            {
                oneCount ++;
                binaryTemp &= binaryTemp - 1;
            }

            return oneCount;
        }

        public void RegenerateLegalMoves ()
        {
            legalMoves = moveGenerator.GenerateMoves ();
        }

        public Pieces GetPieceAt (int square)
        {
            UInt64 whiteBinary = white.GetCombinedBinary();
            UInt64 blackBinary = black.GetCombinedBinary();

            bool isWhite = (whiteBinary & (One << square)) != 0;
            bool isBlack = (blackBinary & (One << square)) != 0;

            Pieces pieceType = null;
            System.Action<Pieces> getPiece = delegate(Pieces x) {
                bool inPieces = (x.bitmap & (One << square)) != 0;
                if (inPieces)
                {
                    pieceType = x;
                }
            };

            if (isWhite) {
                Array.ForEach (white.pieces, getPiece);
            } if (isBlack) {
                Array.ForEach (black.pieces, getPiece);
            }

            return pieceType;
        }

        public void LoadFen (string _fen)
        {
            _fen = _fen.Replace ("/", "");

            white = new PieceMap();
            black = new PieceMap();

            int fenIndex = 0;
            int skipDepth = 0;

            for (int row = 7; row > -1; row --)
            {
                for (int column = 0; column < 8; column ++)
                {
                    if (skipDepth > 0)
                    {
                        skipDepth --;
                        continue;
                    }

                    int square = row * 8 + column;

                    char character = _fen[fenIndex];
                    bool isNumber = numbers.Contains (character.ToString());

                    if (isNumber)
                    {
                        skipDepth = int.Parse (character.ToString())-1;
                        fenIndex ++;
                        continue;
                    }

                    if(!isNumber)
                    {
                        switch (character.ToString())
                        {
                            case "K":
                                white.King.AddPieceAt (square);
                                break;
                            case "Q":
                                white.Queens.AddPieceAt (square);
                                break;
                            case "R":
                                white.Rooks.AddPieceAt (square);
                                break;
                            case "N":
                                white.Knights.AddPieceAt (square);
                                break;
                            case "B":
                                white.Bishops.AddPieceAt (square);
                                break;
                            case "P":
                                white.Pawns.AddPieceAt (square);
                                break;
                            case "k":
                                black.King.AddPieceAt (square);
                                break;
                            case "q":
                                black.Queens.AddPieceAt (square);
                                break;
                            case "r":
                                black.Rooks.AddPieceAt (square);
                                break;
                            case "n":
                                black.Knights.AddPieceAt (square);
                                break;
                            case "b":
                                black.Bishops.AddPieceAt (square);
                                break;
                            case "p":
                                black.Pawns.AddPieceAt (square);
                                break;
                        }
                    }
                    
                    fenIndex ++;
                }
            }

            legalMoves = moveGenerator.GenerateMoves();
        }

        public string GetFen ()
        {
            string fen = "";
            UInt64 boardBinary = white.GetCombinedBinary() | black.GetCombinedBinary();

            int zeroCount = 0;

            for (int x = 0; x < 8; x ++) {
                for (int y = 7; y >= 0; y --)
                {
                    int i =  x * 8 + y;
                    int value = ((boardBinary & (1UL << i)) != 0) ? 1 : 0;
                    
                    if(value == 1)
                    {
                        if (zeroCount > 0)
                        {
                            fen += zeroCount.ToString();
                            zeroCount = 0;
                        }

                        int pieceIndex = -1;
                        bool isWhite = false;

                        System.Action<Pieces> getPiece = delegate(Pieces x) {
                            bool inPieces = (x.bitmap & (1UL << (i))) != 0;
                            if (inPieces)
                            {
                                if (Array.IndexOf (white.pieces, x) != -1)
                                {
                                    pieceIndex = Array.IndexOf (white.pieces, x);
                                    isWhite = true;
                                    return;
                                } if (Array.IndexOf (black.pieces, x) != -1)
                                {
                                    pieceIndex = Array.IndexOf (black.pieces, x);
                                    isWhite = false;
                                    return;
                                }
                            }
                        };

                        Array.ForEach (white.pieces, getPiece);
                        Array.ForEach (black.pieces, getPiece);

                        string name = PieceMap.names[pieceIndex];

                        if (!isWhite)
                        {
                            name = name.ToLower();
                        }

                        fen += name;
                    } else {
                        zeroCount ++;
                    }

                    if (i % 8 == 0)
                    {
                        if (zeroCount > 0)
                        {
                            fen += zeroCount.ToString();
                            zeroCount = 0;
                        }

                        fen += "/";
                    }
                }
            }

            char[] charArray = fen.ToCharArray();
            Array.Reverse (charArray);
            return new string (charArray);
        }
    }
}