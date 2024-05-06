using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Datastructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Dialog
{
    public static class UICamera
    {
        private static Camera _value;
        public static Camera Get() => _value ? _value : _value = GameObject.FindWithTag("UI_Camera")?.GetComponent<Camera>();

        private static HashSet<object> _keepers = new();

        public static void TurnOn(object tag)
        {
            if (tag.IsNil()) return;
            _keepers.Add(tag);
            if (!_keepers.IsNullOrEmpty())
                if(Get()) Get().enabled = true;
        }
        public static void TurnOff(object tag)
        {
            _keepers.Remove(tag);
            if (_keepers.IsNullOrEmpty())
                if (Get()) Get().enabled = false;
        }
    }
}
