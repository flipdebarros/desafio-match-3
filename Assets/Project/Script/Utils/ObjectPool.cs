using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Project.Script.Utils
{
    public class ObjectPool<TObj> where TObj : Object
    {
        private readonly Transform _parent;
        private readonly TObj _prefab;
        private readonly HashSet<TObj> _inUse = new();
        private readonly Stack<TObj> _available = new();
        
        public ObjectPool(Transform parent, TObj prefab)
        {
            _parent = parent;
            _prefab = prefab;
        }

        public void Initialize(int initialAmount)
        {
            for (int i = 0; i < initialAmount; i++) 
                InstantiateObject();
        }

        public TObj GetNextObject()
        {
            if(_available.Count == 0) 
                InstantiateObject();

            TObj obj = _available.Pop();
            _inUse.Add(obj);
            return obj;
        }

        public void ReleaseObject(TObj obj)
        {
            _inUse.Remove(obj);
            _available.Push(obj);
        }

        private void InstantiateObject()
        {
            TObj obj = Object.Instantiate(_prefab, _parent);
            _available.Push(obj);
        }
    }
}
