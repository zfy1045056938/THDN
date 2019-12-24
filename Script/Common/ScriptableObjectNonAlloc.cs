using UnityEngine;
using Mirror;

    public abstract class ScriptableObjectNonAlloc:NetworkBehaviour
    {
        private string cacheName;

        public new string name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(cacheName))
                
                    cacheName = base.name;
                    return cacheName;
                
            }set{
                cacheName=base.name=value;
            }
        }

    }
