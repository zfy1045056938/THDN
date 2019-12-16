using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class BoardShuffle : MonoBehaviour
{
    public void SuffleList(List<GamePieces> normalPieces)
    {
        int maxCount =normalPieces.Count;
        //
        for(int i=0;i<maxCount-1;i++){
            int r = Random.Range(0,maxCount);

            //
            if(r==i){continue;}

            //
            GamePieces tmp = normalPieces[r];
            normalPieces[r]= normalPieces[i];
            normalPieces[i]=tmp;

        }
    }

    public List<GamePieces> RemoveNormalPieces(GamePieces[,] m_allGamePieces)
    {
       List<GamePieces> normalPieces = new List<GamePieces>();
       //
       int width = m_allGamePieces.GetLength(0);
       int height=m_allGamePieces.GetLength(1);

       //
       for(int i=0;i<width;i++){
           for(int j=0;j<height;j++){
               if(m_allGamePieces[i,j]!=null){
                   //
                   Bomb bomb =m_allGamePieces[i,j].GetComponent<Bomb>();
                   Collectiable collectiable = m_allGamePieces[i,j].GetComponent<Collectiable>();
                   //
                   if(bomb ==null && collectiable ==null){
                       normalPieces.Add(m_allGamePieces[i,j]);
                       //
                       m_allGamePieces[i,j]=null;
                   }
               }
           }
       }
       return normalPieces;
    }

    public void MovePieces(GamePieces[,] m_allGamePieces, float swapTime)
    {
      int width = m_allGamePieces.GetLength(0);
      int height=m_allGamePieces.GetLength(1);

      //
      for(int i=0; i<width;i++){
          for(int j=0;j<height;j++){
              if(m_allGamePieces[i,j]!=null){
                  m_allGamePieces[i,j].Move(i,j,swapTime);
              }
          }
      }
    }
}
