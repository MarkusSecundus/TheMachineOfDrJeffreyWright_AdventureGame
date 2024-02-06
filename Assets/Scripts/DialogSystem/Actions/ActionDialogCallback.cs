using MarkusSecundus.TinyDialog.Expressions;
using MarkusSecundus.TinyDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Assets.Scripts.DialogSystem.Actions
{
    public class ActionDialogCallback : AtomicDialogCallback
    {
        public UnityEvent ToInvoke;
        public override void Invoke(ExpressionValue argument = default) => ToInvoke?.Invoke();
    }

}
