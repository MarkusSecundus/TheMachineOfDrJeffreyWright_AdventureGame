using MarkusSecundus.Utils.Datastructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils.Extensions
{
    public class TagSearchable : MonoBehaviour
    {
        static DefaultValDict<string, List<GameObject>> _values = new(k => new());

        public static GameObject FindByTag(string tag)
        {
            if (!_values.TryGetValue(tag, out var list) || list.IsNullOrEmpty())
                return null;
            return list[0];
        }

        private void Awake()
        {
            if (gameObject.tag == null) throw new System.Exception($"Object {name} claims to be tag-searchable, but has no tag!");
            _values[gameObject.tag].Add(gameObject);
        }

        private void OnDestroy()
        {
            if (gameObject.tag != null)
                _values[gameObject.tag].Remove(gameObject);
        }
    }
}
