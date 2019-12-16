using Mirror;
using UnityEngine;


    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance;

        public bool IsGameover=false;
        void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void UpdateMoves()
        {
            throw new System.NotImplementedException();
        }
    }
