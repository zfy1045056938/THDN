using UnityEngine;


    public abstract class ScriptableObjectNonAlloc:ScriptableObject
    {
        private string cacheName;

        public new string name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(cacheName))
                {
                    cacheName = base.name;
                    return cacheName;
                }
            }
        }


    }
