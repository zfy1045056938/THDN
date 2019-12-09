using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.Mathematics;



[RequireComponent(typeof(SpriteRenderer))]
public class GamePieces : MonoBehaviour
    {
        public int xIndex;
        public int yIndex;

        private Board m_board;
        private bool is_isMoving = false;

        public InterType inter = InterType.SmootherStep;
        
        //Score
        public int scoreValue = 20;

        public AudioClip clearSound;
        
        
        
        
        public MatchValue matchValue=MatchValue.MUT;

        public void Init(int x, int y)
        {
            this.xIndex = x;
            this.yIndex = y;
        }
        
        
        public void Move(int x, int y, float moveTime)
        {
            if (!is_isMoving)
            {
                 StartCoroutine(MoveRoutline(new Vector3(x, y), moveTime));
            }
           
        }

        ///        
        private IEnumerator MoveRoutline(Vector3 pos, float moveTime)
        {
            Vector3 startPos = transform.position;
            //
            bool reachedDestination = false;

            float elapsedTime = 0f;

            is_isMoving = true;

            //
            while (!reachedDestination)
            {
                if (Vector3.Distance(transform.position, pos) < 0.01f)
                {
                    reachedDestination = true;
                    //
                    if (m_board != null)
                    {
                        m_board.PlaceGamePieces(this, (int) pos.x, (int) pos.y);
                    }

                    break;
                }


                //
                elapsedTime += Time.deltaTime;

                float t = math.clamp(elapsedTime / moveTime, 0f, 1f);

                switch (inter)
                {
                    case InterType.Linear:
                        break;
                    default:
                        break;
                }

                //
                transform.position = Vector3.Lerp(startPos, pos, t);
                //
                yield return null;
            }

            is_isMoving = false;
        }
    

        public void SetCoord(int gpX, int gpY)
        {
            this.xIndex = gpX;
            this.yIndex = gpY;
        }

        public GamePieces ChangeColor(GamePieces targetPiece)
        {
            return null;
        }
    }
