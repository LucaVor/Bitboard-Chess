using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickChess
{
    public class GamePiece : MonoBehaviour
    {
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public Mesh[] pieceMeshes;
        public Material whiteMat;
        public Material blackMat;

        public Vector3 velocity;
        public PieceRenderingType current_prt;
        public PieceRenderingColour current_prc;

        public float pieceSpeed = 5;
        public float simSpeed = 1;

        public static bool inAnimation = false;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void SetRendering(PieceRenderingType prt, PieceRenderingColour prc)
        {
            current_prt = prt;
            current_prc = prc;

            meshFilter.mesh = pieceMeshes[(int) prt];
            if (prc == PieceRenderingColour.White)
            {
                meshRenderer.material = whiteMat;
            } else {
                meshRenderer.material = blackMat;
            }
        }

        public void SetTargetSquare (int sqr, Vector3 targetPos, GamePiece potentialTarget = null)
        {
            var coroutine = AnimationCoroutine (sqr, targetPos, potentialTarget);
            StartCoroutine (coroutine);
        }

        public IEnumerator AnimationCoroutine (int sqr, Vector3 target, GamePiece target_piece = null)
        {
            inAnimation = true;

            Vector3 origin = transform.position;

            // float time = Vector3.Distance (origin, target) / pieceSpeed;
            float time = 3f;

            float xf = target.x;
            float yf = target.y - 0.5f * -GameManager.Gravity * (time * time);
            float zf = target.z;

            float vx = (xf - origin.x) / time;
            float vy = (yf - origin.y) / time;
            float vz = (zf - origin.z) / time;

            Vector3 v = new Vector3 (vx, vy, vx);

            for (float t = 0; t < time; t += Time.deltaTime * simSpeed)
            {
                float oxf = origin.x + vx * t;
                float oyf = (origin.y + vy * t) - 0.5f * GameManager.Gravity * (t * t);
                float ozf = origin.z + vz * t;

                Vector3 pos = new Vector3 (oxf, oyf, ozf);
                transform.position = pos;

                if (target_piece != null){
                    if (Vector3.Distance (target_piece.transform.position, pos) < 7)
                    {
                        // DestroyImmediate (target_piece.transform.gameObject);
                        Destroy (target_piece.transform.gameObject, 4);
                    }
                }

                yield return null;
            }

            transform.position = target;
            inAnimation = false;
        }
    }
}