using com.brg.Common.Random;
using System.Collections.Generic;
using UnityEngine;

namespace com.brg.Utilities
{
    public static class GameObjectExtensions
    {
        public static IEnumerable<T> GetDirectOrderedChildComponents<T>(this GameObject go) where T : MonoBehaviour
        {
            var tempStacks = new List<T>();
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                var child = go.transform.GetChild(i);
                if (child.GetComponent<T>())
                {
                    tempStacks.Add(child.GetComponent<T>());
                }            
            }

            return tempStacks;
        }

        public static IEnumerable<T> GetDirectOrderedChildComponents<T>(this Transform transform) where T : MonoBehaviour
        {
            return transform.gameObject.GetDirectOrderedChildComponents<T>();
        }

        public static void DeleteAllChildren(this Transform transform)
        {
            var count = transform.childCount;

            var list = new List<Transform>();
            for (int i = 0; i < count; ++i)
            {
                list.Add(transform.GetChild(i));
            }

            foreach (var child in list)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        
        public static void DeleteAllChildrenImmediately(this Transform transform)
        {
            var count = transform.childCount;

            var list = new List<Transform>();
            for (int i = 0; i < count; ++i)
            {
                list.Add(transform.GetChild(i));
            }

            foreach (var child in list)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        public static void SetGOActive(this Component component, bool value)
        {
            component.gameObject.SetActive(value);
        }
    }
}
