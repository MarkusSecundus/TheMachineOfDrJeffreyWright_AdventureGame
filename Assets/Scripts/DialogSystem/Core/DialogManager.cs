using MarkusSecundus.PhysicsSwordfight.Utils.Serialization;
using MarkusSecundus.TinyDialog.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MarkusSecundus.TinyDialog
{
    public interface ITextBox
    {
        public void StartPrintout(string text, Action onClosed, DisplayMode displayMode=DisplayMode.Normal);
        public enum DisplayMode
        {
            Normal=0, FadeOut, Stay
        }
    }
    public interface IChoiceBox
    {
        public void StartPrintout(string text, IEnumerable<string> options, Action<int> onOptionChosen);
    }
    public interface IDialogContext: IExpressionContext
    {
        public ITextBox TextBox { get; }
        public IChoiceBox ChoiceBox { get; }
        public UnityEngine.Object ErrorReportContext { get; }
        public bool TryGetDialogById(string id, out Dialog dialog);
        public Expressions.ExpressionEvaluator ExpressionEvaluator { get; }
        public void InvokeSignal(string signal, ExpressionValue? argument, Action onFinishedCallback);
        public bool TryGetLabel(Dialog dialog, string labelId, out DialogNodeFullAddress label);
    }

    public class DialogManager : MonoBehaviour
    {
        [SerializeField] TextAsset[] DialoguesToLoad;
        [SerializeField] SerializableDictionary<string, DialogCallback> _callbacks;
        [SerializeField] GameObject _textBoxPrefab;
        [SerializeField] GameObject _choiceBoxPrefab;
        ITextBox _textBoxInstance;
        IChoiceBox _choiceBoxInstance;

        Dictionary<string, Dialog> _dialogues;
        Context _dialogContext;
        public DialogManager() { _dialogContext = new(this); }

        public struct CallbackKV : SerializableDictionary.IEntry<string, DialogCallback>
        {
            [SerializeField] string Signal;
            [SerializeField] DialogCallback Callback;

            public string Key { get => Signal; init => Signal=value; }
            public DialogCallback Value { get => Callback; init => Callback = value; }
        }

        void Awake()
        {
            _dialogues = new Dictionary<string, Dialog>(DialogList.LoadXml(DialoguesToLoad).Select(d => new KeyValuePair<string, Dialog>(d.Id, d)));
            if (_textBoxPrefab)
            {
                var textBox = Instantiate(_textBoxPrefab);
                textBox.SetActive(false);
                if ((_textBoxInstance = textBox.GetComponentInChildren<ITextBox>(true)).Equals(null))
                {
                    Debug.LogError($"Text box prefab does not contain any {nameof(ITextBox)} component!", this);
                }
            }
            if (_choiceBoxPrefab)
            {
                var choiceBox = Instantiate(_choiceBoxPrefab);
                choiceBox.SetActive(false);
                if ((_choiceBoxInstance = choiceBox.GetComponentInChildren<IChoiceBox>(true)).Equals(null))
                {
                    Debug.LogError($"Choice box prefab does not contain any {nameof(IChoiceBox)} component!", this);
                }
            }
        }

        public bool DialogIsRunning { get; private set; }
        public void RunDialog(string dialogName)
        {
            Debug.Log($"Starting dialog '{dialogName}'");
            DialogIsRunning = true;
            if (!_dialogues.TryGetValue(dialogName, out var toRun))
            {
                Debug.LogError($"Couldn't find dialog named '{dialogName}'", this);
                return;
            }
            var firstNode = DialogNodeInterpreter.Make(_dialogContext, toRun, 0, null);
            firstNode.Start(n => { DialogIsRunning = false; });
        }


        private Dictionary<Dialog, Dictionary<string, DialogNodeFullAddress>> _labelsByDialog = new Dictionary<Dialog, Dictionary<string, DialogNodeFullAddress>>();

        Dictionary<string, DialogNodeFullAddress> _configLabels(Dialog dialog)
        {
            Dictionary<string, DialogNodeFullAddress> labels = new();

            List<DialogNodeAddress> currentFullAddress = new();
            doSearch(dialog);
            void doSearch(DialogNodeContainer host)
            {
                for (int i = 0; i < host.Children.Length; ++i)
                {
                    var node = host.Children[i];
                    var currentAddressSegment = new DialogNodeAddress { HostContainer = host, Index = i};
                    if (node is DialogNode_Label lbl)
                    {
                        currentFullAddress.Add(currentAddressSegment);
                        labels.Add(lbl.Id, new DialogNodeFullAddress { Segments = currentFullAddress.ToArray() });
                        currentFullAddress.RemoveAt(currentFullAddress.Count - 1);
                    }
                    else if (node is DialogNodeContainer c)
                    {
                        currentFullAddress.Add(currentAddressSegment);
                        doSearch(c);
                        currentFullAddress.RemoveAt(currentFullAddress.Count - 1);
                    }
                    else if (node is DialogNode_GenericBranch choice)
                    {
                        foreach (var option in choice.AllNestedContainers)
                        {
                            currentFullAddress.Add(currentAddressSegment);
                            doSearch(option);
                            currentFullAddress.RemoveAt(currentFullAddress.Count - 1);
                        }
                    }
                }
            }
            return labels;
        }


        record Context(DialogManager _base) : ExpressionContext, IDialogContext
        {
            public ITextBox TextBox => _base._textBoxInstance;

            public IChoiceBox ChoiceBox => _base._choiceBoxInstance;

            public Expressions.ExpressionEvaluator ExpressionEvaluator => Expressions.ExpressionEvaluator.Instance;

            public UnityEngine.Object ErrorReportContext => _base;

            public void InvokeSignal(string signal, ExpressionValue? argument, Action onFinishedCallback)
            {
                if (!_base._callbacks.Values.TryGetValue(signal, out var callback))
                {
                    onFinishedCallback?.Invoke();
                    return;
                }

                var coroutine = callback.InvokeCoroutine(argument ?? default);
                if (coroutine == null)
                {
                    onFinishedCallback?.Invoke();
                }
                else 
                {
                    _base.StartCoroutine(impl());
                    IEnumerator impl()
                    {
                        yield return coroutine;
                        onFinishedCallback?.Invoke();
                    }
                }
            }

            public bool TryGetDialogById(string id, out Dialog dialog) => _base._dialogues.TryGetValue(id, out dialog);

            public bool TryGetLabel(Dialog dialog, string labelId, out DialogNodeFullAddress label)
            {
                if (labelId == null)
                {
                    label = new DialogNodeFullAddress { Segments = new[] { new DialogNodeAddress { HostContainer = dialog, Index = 0 } } };
                    return true;
                }
                if(!_base._labelsByDialog.TryGetValue(dialog, out var labels))
                {
                    labels = _base._labelsByDialog[dialog] = _base._configLabels(dialog);
                }
                return labels.TryGetValue(labelId, out label);
            }
        }
    }

    public class DialogManagerPersistentState : MonoBehaviour
    {
        private void Awake()
        {
            
        }
        private void OnDestroy()
        {

        }


        public static DialogManagerPersistentState Instance => DialogueUtilsInternal.Singleton<DialogManagerPersistentState>.Instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstantiationStartupCallback() { var _ = Instance; }
    }


















    public abstract class DialogCallback: MonoBehaviour
    {
        public abstract System.Collections.IEnumerator InvokeCoroutine(ExpressionValue argument = default);
    }
    public abstract class AtomicDialogCallback : DialogCallback
    {
        public sealed override IEnumerator InvokeCoroutine(ExpressionValue argument = default)
        {
            Invoke(argument);
            return null;
        }

        public abstract void Invoke(ExpressionValue argument = default);

        public void InvokeNoArg() => Invoke(default);
    }




}
