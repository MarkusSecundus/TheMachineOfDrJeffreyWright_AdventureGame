using MarkusSecundus.TinyDialog.Expressions;
using MarkusSecundus.TinyDialog;
using UnityEngine.SceneManagement;
using MarkusSecundus.Utils.Behaviors.Cosmetics;

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
