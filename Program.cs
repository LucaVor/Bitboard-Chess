using System;
using System.Collections;
using System.Collections.Generic;

namespace QuickChess
{
    class Program
    {
        public static Board board;
        public static Dictionary<int, string> map;
        public static int maxDepth;
        static void Main(string[] args)
        {
            PreProcessing.PreProcess();

            Console.WriteLine ("Start...");
            map = new Dictionary<int, string>();
            string letters = "abcdefgh";
            
            int n = 0;
            for (int i = 1; i < 9; i ++)
            {
                foreach (char chara in letters)
                {
                    string square = chara.ToString();
                    square += i.ToString();

                    map.Add (n, square);
                    n ++;
                }
            }

            board = new Board("8/5kq1/8/8/8/2B5/1K6/8");
            // DebugBinary (board.white.GetCombinedBinary() | board.black.GetCombinedBinary(), true, board);
            // DebugBinary (board.moveGenerator.amalgamatedMoves);

            // for (int i = 0; i < 200000; i ++){
            //     board.moveGenerator.GenerateMoves();
            // }

            // maxDepth = 4;
            // Console.WriteLine(Search(maxDepth));
            // Console.WriteLine (board.fen);
            // Console.WriteLine (board.GetFen());
            while (true)
            {
                DebugBinary (board.white.GetCombinedBinary() | board.black.GetCombinedBinary(), true, board);
                string input = Console.ReadLine();
                string from = input.Split (' ', 2)[0];
                string to = input.Split(' ', 2)[1];
                board.Push (StringToIndex(from), StringToIndex(to));
                string fen = board.GetFen();
                Console.WriteLine ($"Fen: ({fen})");
            }
            // Console.WriteLine ($"End... {stopwatch.ElapsedMilliseconds}");
            
        }

        public static int Search (int depth)
        {
            if (depth == 0)
            {
                return 1;
            }

            int numPositions = 0;
            UInt64[] legalMoves = board.moveGenerator.GenerateMoves();
            
            for (int i = 0; i < 64; i ++)
            {
                int fromSquare = i;
                UInt64 moves = legalMoves[i];

                while (moves != 0)
                {
                    int ind = (int) System.Runtime.Intrinsics.X86.Bmi1.X64.TrailingZeroCount (moves);
                    string fen = board.GetFen();
                    board.Push (fromSquare, ind);
                    int positionCount = Search (depth - 1);
                    numPositions += positionCount;
                    if (positionCount != 1 && depth==maxDepth) Console.WriteLine ($"{IndexToString(fromSquare)}{IndexToString(ind)} {positionCount}");
                    board.whiteMove = !board.whiteMove;
                    board.LoadFen (fen);
                    moves &= moves - 1;
                }
            }

            // Console.WriteLine (depth);
            // DebugBinary (amalg);
            return numPositions;
        }

        public static string IndexToString (int index)
        {
            return map[index];
        }

        public static int StringToIndex (string sqr)
        {
            int square = 0;
            string letters = "abcdefgh";
            square += (int.Parse(sqr[1].ToString())-1) * 8;
            square += letters.IndexOf (sqr[0].ToString());

            return square;
        }

        public static void DebugBinary (UInt64 bit, bool format = false, Board b = null)
        {
            string binary = Convert.ToString((long)bit, 2);
            while (binary.Length < 64)
            {
                binary = "0" + binary;
            }
            string result = "";

            for (int x = 0; x < 8; x ++) {
                for (int y = 7; y >= 0; y --)
                {
                    int ind = x * 8 + y;
                    string letter = binary[ind].ToString();
                    if (format && letter == "1")
                    {
                        int pieceIndex = 0;
                        bool isWhite = false;

                        System.Action<Pieces> getPiece = delegate(Pieces x) {
                            bool inPieces = (x.bitmap & (1UL << (63-ind))) != 0;
                            if (inPieces)
                            {
                                if (Array.IndexOf (b.white.pieces, x) != -1)
                                {
                                    pieceIndex = Array.IndexOf (b.white.pieces, x);
                                    isWhite = true;
                                    return;
                                } if (Array.IndexOf (b.black.pieces, x) != -1)
                                {
                                    pieceIndex = Array.IndexOf (b.black.pieces, x);
                                    isWhite = false;
                                    return;
                                }
                            }
                        };

                        Array.ForEach (b.white.pieces, getPiece);
                        Array.ForEach (b.black.pieces, getPiece);

                        letter = PieceMap.names[pieceIndex];

                        if (!isWhite)
                        {
                            letter = letter.ToLower();
                        }
                    }
                    if (letter == "0") letter = ".";
                    result += letter + " ";
                }
                result += "\n";
            }

            Console.WriteLine (result);
        }

        public static string UInt64ToBinary(UInt64 input)
        {
            UInt32 low = (UInt32) (input & 0xFFFFFFFF);
            UInt32 high = (UInt32) (input & 0xFFFFFFFF00000000) >> 32;
            return $"{Convert.ToString(high, 2).PadLeft(32, '0')}{Convert.ToString(low, 2).PadLeft(32, '0')}";
        }

        static int MoveBinary (int binary, int index, int newIndex)
        {
            binary ^= 1 << index;
            binary |= 1 << newIndex;

            return binary;
        }
    }
}
