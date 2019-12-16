using UnityEngine;


    public class Collectiable : GamePieces
    {

        public bool clearBybottom=false;
        public bool clearByBomb=false;

         void Start()
        {
            matchValue =MatchValue.MUT; //default match value is MUT 
        }
        
}
