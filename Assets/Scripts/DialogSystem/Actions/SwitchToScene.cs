using MarkusSecundus.TinyDialog.Expressions;
using MarkusSecundus.TinyDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.DialogSystem.Actions
{
    public class SwitchToScene : AtomicDialogCallback
    {
        public string DestinationScene;

        public FadeEffect FadeIn;

        public override void Invoke(ExpressionValue _)
        {
            if (FadeIn)
                FadeIn.FadeIn(impl);
            else
                impl();
            void impl()=> SceneManager.LoadScene(DestinationScene);
        }
    }
}
