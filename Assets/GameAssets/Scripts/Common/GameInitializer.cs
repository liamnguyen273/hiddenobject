using System.Collections.Generic;
using UnityEngine;

namespace com.brg.Common
{
    public abstract class GameInitializer : MonoBehaviour
    {
        private List<IInitializable> _initializableList;

        public virtual void SetInitializableList(params IEnumerable<IInitializable>[] lists)
        {

        }

        public virtual void Initialize()
        {
            if (_initializableList == null)
            {
                return;
            }


        }
    }
}
