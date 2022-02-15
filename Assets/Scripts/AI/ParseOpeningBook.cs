using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace QuickChess
{
    [System.Serializable] public struct ToFrom /* A simple structure, we use this instead of move because it is less complex */
    {
        public int to;
        public int from;
    }
    [System.Serializable]
    public class ParseOpeningBook : MonoBehaviour
    {
        public static ParseOpeningBook Book;

        public int openingPly = 11;
        Dictionary<ToFrom, List<Move>> legalMoveHash = new Dictionary<ToFrom, List<Move>> ();
        public Dictionary<string, List<ToFrom>> book; /* You index with book[ { e2e4 } ] = { d7d5, b8c6 } */

        string moveLine = "e4 e5 Nf3 Nc6 Bc4 Bc5 c3 Nf6 d3 d6 b4";
        public TextAsset openingBookTXT;

        public bool parseBook;
        public bool loadBookFromFile;

        public string outputFilepath = "C:\\Users\\lbvor\\Bitboard Chess\\Assets\\Scripts\\AI\\OpeningBookBytes.txt";
        public int iteration = 0;

        private string text;
        public string fenToTest;
        public bool searchHistory;

        public bool CacheToMemory;

        void Awake ()
        {
            Book = this;
        }

        public void Start ()
        {
            outputFilepath = SceneManagement.openingBookFilepath;
            if (loadBookFromFile)
            {
                LoadBytes ();
            }
        }

        void Update ()
        {
            if (parseBook)
            {
                parseBook = false;
                text = openingBookTXT.text;
                Parse ();
            }

            if (searchHistory)
            {
                searchHistory = false;
                List<ToFrom> allMoves = book[fenToTest.Replace("/", "")];

                foreach (ToFrom move in allMoves)
                {
                    Debug.Log ($"{DebugC.IndexToString(move.from)}, {DebugC.IndexToString(move.to)}");
                }
            }

            if (CacheToMemory)
            {
                CacheToMemory = false;
                byte[] DictionaryBytes = DictionaryToBytes<string, List<ToFrom>> (book);
                WriteBytesToFile (outputFilepath, DictionaryBytes);
            }
        }
        
        public List<string> files = new List<string> { 
            "a", "b", "c", "d", "e", "f", "g", "h"
        };

        public void LoadBytes ()
        {
            byte[] dictionaryBytes = ReadBytesFromFile (outputFilepath);
            book = BytesToDictionary<string, List<ToFrom>> (dictionaryBytes);
        }

        public void ThreadedParse ()
        {
            Thread thread = new Thread (new ThreadStart ( Parse ));
            thread.Priority = System.Threading.ThreadPriority.Highest;
            thread.Start ();
        }

        public void Parse ()
        {
            book = new Dictionary<string, List<ToFrom>> ();
            string[] lines = text.Split ('\n');

            string sf = Board.StartingFen;
            
            foreach (string opening in lines)
            {
                Board board = new Board ();

                /* Opening is one line */
                board.whiteMove = true;
                board.LoadFen (sf);

                string[] strMoves = opening.Split (' ');
                List<ToFrom> moves = new List<ToFrom> ();

                bool breakOutOfLine = false;

                for (int i = 0; i < Mathf.Min(openingPly, strMoves.Length); i ++)
                {
                    string san = (strMoves[i]).Replace ("+", "").Replace ("$", "").Replace ("x", "").Replace ("-", "");

                    ToFrom move = new ToFrom ();

                    var allMoves = board.moveGenerator.FormatLegalMoves ();
                    foreach (Move legalMove in allMoves)
                    {
                        int lf = legalMove.from;
                        int lt = legalMove.to;
                        int pieceMoved = legalMove.pieceType;

                        if (san == "OO") /* Castling on king it is OO because we removed - */
                        {
                            if (pieceMoved == PreProcessing.KING && lt - lf == 2)
                            {
                                move.from = lf;
                                move.to = lt;
                                break;
                            }

                        } if (san == "OOO") /* Castling on queen */
                        {
                            if (pieceMoved == PreProcessing.KING && lt - lf == -2)
                            {
                                move.from = lf;
                                move.to = lt;
                                break;
                            }
                        }

                        if (files.Contains (san[0].ToString ()))
                        {
                            if (pieceMoved != PreProcessing.PAWN)
                            {
                                continue;
                            }

                            int sanFileIndex = files.IndexOf (san[0].ToString());
                            int moveFile = lf % 8;
                            int moveFileTo = lt % 8;
                            int toRank = (int) Mathf.Floor (((float) lt) / 8);

                            if (sanFileIndex == moveFile)
                            {
                                char sanFile = san[san.Length - 2];
                                string sanRank = san[san.Length - 1].ToString();

                                if (files.IndexOf (sanFile.ToString()) == moveFileTo) {
                                    if (sanRank == (toRank + 1).ToString ()) {
                                        move.from = lf;
                                        move.to = lt;
                                        break;
                                    }
                                }
                            }
                        } else {
                            char pieceMoving = san[0];
                            int _pieceMoving = PieceType (pieceMoving);

                            if (_pieceMoving != pieceMoved)
                            {
                                continue;
                            }

                            int sanFileIndex = files.IndexOf (san[0].ToString());
                            int moveFile = lf % 8;
                            int moveFileTo = lt % 8;
                            int fromRank = (int) Mathf.Floor (((float) lf) / 8);
                            int toRank = (int) Mathf.Floor (((float) lt) / 8);

                            char sanFile = san[san.Length - 2];
                            string sanRank = san[san.Length - 1].ToString();

                            if (files.IndexOf (sanFile.ToString()) == moveFileTo) {
                                if (sanRank == (toRank + 1).ToString ()) {
                                    if (san.Length == 4)
                                    {
                                        char specific = san[1];
                                        if (files.Contains (specific.ToString()))
                                        {
                                            if (files.IndexOf (specific.ToString()) != moveFile)
                                            {
                                                continue;
                                            }
                                        } else {
                                            if (specific.ToString () != (fromRank + 1).ToString ())
                                            {
                                                continue;
                                            }
                                        }
                                    }

                                    move.from = lf;
                                    move.to = lt;
                                    break;
                                }
                            }
                            
                        }
                    }

                    if (breakOutOfLine)
                    {
                        break;
                    }

                    bool pushed = board.Push (move.from, move.to);
                    moves.Add (move);
                }

                if (breakOutOfLine)
                {
                    break;
                }

                board.whiteMove = true;
                board.LoadFen (sf);
                string fen = Board.StartingFen.Replace("/", "");

                for (int c = 0; c < moves.Count - 1; c ++)
                {
                    if (book.ContainsKey (fen))
                    {
                        book[fen].Add (moves[c]);
                    } else {
                        book.Add (fen, new List<ToFrom> ());
                        book[fen].Add (moves[c]);
                    }

                    board.Push (moves[c].from, moves[c].to);

                    fen = board.GetFen ().Replace("/", "");
                }

                iteration ++;
            }

            /* Output file path: "C:\Users\lbvor\Bitboard Chess\Assets\Scripts\AI\OpeningBookBytes.txt" */
        }

        public int PieceType (char pieceChar)
        {
            switch (pieceChar)
            {
                case 'K':
                    return PreProcessing.KING;
                    break;
                case 'Q':
                    return PreProcessing.QUEEN;
                    break;
                case 'R':
                    return PreProcessing.ROOK;
                    break;
                case 'B':
                    return PreProcessing.BISHOP;
                    break;
                case 'N':
                    return PreProcessing.KNIGHT;
                    break;
                case 'P':
                    return PreProcessing.PAWN;
                    break;
            }

            return -1;
        }

        public bool SanIsCapture (string san)
        {
            return san.Length == 5;
        }

        public bool SanIsPromotion (string san)
        {
            return san.Contains ("=");
        }

        public Dictionary<TK, TV> BytesToDictionary<TK, TV> (byte[] byteArray)
        {
            BinaryFormatter bf = new BinaryFormatter ();

            using (MemoryStream ms = new MemoryStream ())
            {
                    ms.Write (byteArray, 0, byteArray.Length);
                    ms.Seek (0, SeekOrigin.Begin);

                    Dictionary<TK, TV> dict = (Dictionary<TK, TV>) bf.Deserialize (ms);
                    return dict;
            }
        }

        public byte[] DictionaryToBytes<TK, TV> (Dictionary<TK, TV> obj)
        {
            BinaryFormatter bf = new BinaryFormatter ();
            
            using (MemoryStream ms = new MemoryStream ())
            {
                bf.Serialize (ms, obj);

                return ms.ToArray ();
            }
        }

        public void WriteBytesToFile (string filename, byte[] byteArray)
        {
            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                fs.Write(byteArray, 0, byteArray.Length);
            }
        }

        public byte[] ReadBytesFromFile (string filename)
        {
            byte[] fileData = null;

            using (FileStream fs = File.OpenRead (filename))
            {
                using (BinaryReader binaryReader = new BinaryReader (fs))
                {
                    fileData = binaryReader.ReadBytes ((int)fs.Length);
                }
            }

            return fileData;
        }
    }

    class ListComparer : IEqualityComparer<List<ToFrom>>
    {
        public bool Equals(List<ToFrom> x, List<ToFrom> y)
        {
            if (x.Count != y.Count)
            {
                return false;
            }

            for (int i = 0; i < x.Count; i ++)
            {
                ToFrom tf0 = (ToFrom) x[i];
                ToFrom tf1 = (ToFrom) y[i];

                bool sameFrom = tf0.from == tf1.from;
                bool sameTo = tf0.to == tf1.to;

                if (!(sameFrom && sameTo))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(List<ToFrom> obj)
        {
            int hashcode = 0;
            foreach (ToFrom t in obj)
            {
                hashcode ^= t.GetHashCode();
            }
            return hashcode;
        }
    }
}