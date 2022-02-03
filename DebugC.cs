using System;
using System.Collections;
using System.Collections.Generic;

namespace QuickChess
{
    public static class DebugC
    {
        public static Dictionary<int, string> map;

        public static void Init()
        {
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

        public static string DebugBinary (UInt64 bit, bool format = false, Board b = null)
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
            return result;
        }
    }
}