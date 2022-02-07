using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickChess
{
    public enum SoundEffects
    {
        Move, Check, Checkmate, Illegal, Start
    }

    public enum PieceRenderingType
    {
        King, Queens, Rooks, Bishops, Knights, Pawns
    };

    public enum PieceRenderingColour
    {
        White, Black
    }

    public class GameManager : MonoBehaviour
    {
        public int Iterations;
        public UInt64[] moves;

        public Color legalMoveColour;

        public GameObject blackSquarePref;
        public GameObject whiteSquarePref;
        public GameObject piecePref;

        public Transform gameBoard;
        public LayerMask squareLayer;

        public Dictionary<GameObject, int> squares;
        public Vector3[] map;
        public GamePiece[] pieces;
        public GamePiece[] pieceMap;
        public Tile[] tileMap;

        public GameObject hoveringIndicator;

        public int from = -1;
        public float gravity;
        public static float Gravity;

        public Board board;
        public static GameManager instance;

        public bool initialUpdate = true;

        public Vector3 whiteCameraPosition = new Vector3 (-2.88f, 33, -32.4f);
        public float whiteXRotation = 51.9f;
        public float whiteYRotation = 0.42f;

        public Vector3 blackCameraPosition = new Vector3 (-2.88f, 33, 28.17f);
        public float blackXRotation = 52.9f;
        public float blackYRotation = 180.69f;

        public GameObject checkmateUI;

        public bool multiplayer = true;

        public bool whitePlayer;
        public UnityEngine.AudioSource audioSource;
        public UnityEngine.AudioSource ashersAudioSource;
        public UnityEngine.AudioClip moveSound;
        public UnityEngine.AudioClip checkSound;
        public UnityEngine.AudioClip checkmateSound;
        public UnityEngine.AudioClip illegalMove;
        public UnityEngine.AudioClip gameStart;
        public UnityEngine.AudioClip ashersMusic;

        public string fen = "8/4PKb1/8/4pk2/8/8/8/8";

        void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        public void Init(bool whiteMove = false)
        {
            if (whiteMove)
            {
                Camera.main.transform.position = whiteCameraPosition;
                Camera.main.transform.localEulerAngles = new Vector3 (
                    whiteXRotation, whiteYRotation, 0
                );
            } else {
                Camera.main.transform.position = blackCameraPosition;
                Camera.main.transform.localEulerAngles = new Vector3 (
                    blackXRotation, blackYRotation, 0
                );
            }

            whitePlayer = whiteMove;

            PreProcessing.PreProcess();
            DebugC.Init();

            board = new Board(fen);
            board.onCastleCallback = OnCastle;
            board.onPromotionCallback = OnPromotion;
            board.onCheckmateCallback = OnCheckmate;

            Debug.Log ("Playing as white: " + whiteMove);

            tileMap = new Tile[64];

            // Generate grid
            squares = new Dictionary<GameObject, int>();
            map = new Vector3[64];
            int i = 0;
            bool blackSquare = true;
            for (float x = -8f; x < 8f; x += 2f)
            {
                for (float y = -8f; y < 8f; y += 2f)
                {
                    Vector3 position = new Vector3(y, 0, x);
                    GameObject square = Instantiate (blackSquare ? blackSquarePref : whiteSquarePref);

                    tileMap[i] = square.GetComponent<Tile> ();

                    square.transform.parent = gameBoard;
                    square.transform.localPosition = position;
                    square.transform.localScale = whiteSquarePref.transform.localScale;
                    square.transform.localRotation = whiteSquarePref.transform.localRotation;
                    square.SetActive (true);

                    blackSquare = !blackSquare;
                    squares.Add(square, i);
                    map[i] = position * 3;
                    i ++; 
                }
                blackSquare = !blackSquare;
            }

            Setup();
            GameManager.Play (SoundEffects.Start);
        }

        void Start()
        {
            if (!multiplayer)
            {
                Init (
                    // (UnityEngine.Random.Range(0,100)<50)?false:true
                    true
                );

                if (!whitePlayer)
                {
                    Ai.ai.AI();
                }
            }
        }

        public bool search;
        public int depth;

        // Update is called once per frame
        void Update()
        {
            // if (search)
            // {
            //     Ai.ai.board = null;
            //     Debug.Log (Ai.ai.Search (depth));
            //     search = false;
            // }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Space) && Input.GetKeyDown(KeyCode.P))
            {
                ashersAudioSource.volume = 0;
            } if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Space) && Input.GetKeyDown(KeyCode.O))
            {
                ashersAudioSource.volume = 0.5f;
            }

            if (GamePiece.inAnimation)
            {
                return;
            }
            
            Gravity = gravity;

            Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;
            bool hitSquare = Physics.Raycast (ray.origin, ray.direction, out hit, Mathf.Infinity, squareLayer);

            if (hitSquare)
            {
                int square = squares[hit.transform.gameObject];
                Tile tile = tileMap[square];

                hoveringIndicator.SetActive (true);
                hoveringIndicator.transform.position = tile.transform.position;
            } else {
                hoveringIndicator.SetActive (false);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (hitSquare)
                {
                    from = squares[hit.transform.gameObject];
                }
            } else if (Input.GetMouseButton (0))
            {
                // Holding on "from"
                UInt64 movesAtFrom = board.legalMoves[from];

                UInt64 whiteBinary = board.white.GetCombinedBinary();
                UInt64 blackBinary = board.black.GetCombinedBinary();
                bool show = true;
                if ((whiteBinary & (1UL << from)) != 0 && !whitePlayer)
                {
                    show = false;
                } if ((blackBinary & (1UL << from)) != 0 && whitePlayer)
                {
                    show = false;
                }

                while (movesAtFrom != 0 && show)
                {
                    int to_square = (int) Unity.Burst.Intrinsics.X86.Bmi1.tzcnt_u64 (movesAtFrom);
                    tileMap[to_square].SetColourThisFrame (legalMoveColour);

                    movesAtFrom &= movesAtFrom - 1;
                }
            }
            
            if (Input.GetMouseButtonUp(0))
            {            
                if (hitSquare)
                {
                    int to = squares[hit.transform.gameObject];
                    // board.Push (from, to);
                    Move (from, to);
                }
            }
        }

        void OnCastle(int f, int t)
        {
            GamePiece pieceToMove = pieceMap[f];

            pieceToMove.SetTargetSquare (t, map[t]);

            pieceMap[t] = pieceMap[f];
            pieceMap[f] = null;
        }

        void OnPromotion (int from, int at, bool isWhiteP)
        {
            GamePiece pieceToChange = pieceMap[from];

            if (isWhiteP)
            {
                pieceToChange.SetRendering (
                    PieceRenderingType.Queens, PieceRenderingColour.White
                );
            } else
            {
                pieceToChange.SetRendering (
                    PieceRenderingType.Queens, PieceRenderingColour.Black
                );
            }
        }

        public void Move(int f, int t, bool fromOther = false, bool fromAi = false)
        {
            if (!fromOther)
            {
                UInt64 whiteBinary = board.white.GetCombinedBinary();
                UInt64 blackBinary = board.black.GetCombinedBinary();
                
                if ((whiteBinary & (1UL << from)) != 0 && !whitePlayer)
                {
                    return;
                } if ((blackBinary & (1UL << from)) != 0 && whitePlayer)
                {
                    return;
                }

                if (multiplayer) ClientSend.SendMove (f, t);
            }

            GamePiece pieceToMove = pieceMap[f];
            bool isCaptureMove = pieceMap[t] != null;
            bool isLegal = board.Push (f, t);

            if (!isLegal)
            {
                GameManager.Play (SoundEffects.Illegal);
                return;
            }
            
            if (!isCaptureMove)
                pieceToMove.SetTargetSquare (t, map[t]);
            else {
                pieceToMove.SetTargetSquare (t, map[t], pieceMap[t]);
                Fracture.PermittedShatter = pieceMap[t].gameObject;
            }


            pieceMap[t] = pieceMap[f];
            pieceMap[f] = null;

            bool inCheck = board.moveGenerator.inCheck;
            
            if (!inCheck)
                GameManager.Play (SoundEffects.Move);
            else
                GameManager.Play (SoundEffects.Check);

            if (!multiplayer && !fromAi)
                Ai.ai.AI();
        }

        public void OnCheckmate()
        {
            GameManager.Play (SoundEffects.Checkmate);
            checkmateUI.SetActive (true);
        }

        void Setup ()
        {
            // Initialize pieces
            pieces = new GamePiece[32];
            pieceMap = new GamePiece[64];
            for (int i = 0; i < 64; i ++) pieceMap[i] = null;

            int currentPiece = 0;

            for (int pc = 0; pc < 6; pc ++)
            {
                Pieces boardPieces = board.white.pieces[pc];
                for (int n = 0; n < boardPieces.pieces; n ++)
                {
                    int sqr = boardPieces.currentPieces[n];
                    PieceRenderingType prt = (PieceRenderingType) pc;

                    Vector3 pos = map[sqr];

                    GameObject piece = Instantiate (piecePref);
                    piece.SetActive (true);
                    GamePiece gamePiece = piece.GetComponent<GamePiece> ();
                    gamePiece.transform.parent = gameBoard;
                    gamePiece.transform.localScale = Vector3.one * 40f;
                    gamePiece.transform.position = pos;
                    gamePiece.SetRendering (prt, PieceRenderingColour.White);
                    pieces[currentPiece] = gamePiece;
                    pieceMap[sqr] = gamePiece;

                    currentPiece ++;
                }
            }

            for (int pc = 0; pc < 6; pc ++)
            {
                Pieces boardPieces = board.black.pieces[pc];
                for (int n = 0; n < boardPieces.pieces; n ++)
                {
                    int sqr = boardPieces.currentPieces[n];
                    PieceRenderingType prt = (PieceRenderingType) pc;

                    Vector3 pos = map[sqr];

                    GameObject piece = Instantiate (piecePref);
                    piece.SetActive (true);
                    GamePiece gamePiece = piece.GetComponent<GamePiece> ();
                    gamePiece.transform.parent = gameBoard;
                    gamePiece.transform.localScale = Vector3.one * 40f;
                    gamePiece.transform.position = pos;
                    gamePiece.SetRendering (prt, PieceRenderingColour.Black);
                    pieces[currentPiece] = gamePiece;
                    pieceMap[sqr] = gamePiece;

                    currentPiece ++;
                }
            }
        }

        public static void Play (SoundEffects sfx)
        {
            /*
            public enum SoundEffects
            {
                Move, Check, Checkmate, Illegal, Start
            }
            */

            if (sfx == SoundEffects.Move)
            {
                instance.audioSource.clip = instance.moveSound;
            } if (sfx == SoundEffects.Check)
            {
                instance.audioSource.clip = instance.checkSound;
            } if (sfx == SoundEffects.Checkmate)
            {
                instance.audioSource.clip = instance.checkmateSound;
            } if (sfx == SoundEffects.Illegal)
            {
                instance.audioSource.clip = instance.illegalMove;
            } if (sfx == SoundEffects.Start)
            {
                instance.audioSource.clip = instance.gameStart;
            }

            instance.audioSource.Play();
        }
    }
}