using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MarkusSecundus.TinyDialog.Expressions;

namespace MarkusSecundus.TinyDialog
{
    public class NodeStuff : UnityEngine.MonoBehaviour
    {  
    }

    #region XmlNodes
    #region Root

    [XmlRoot("DialogList")]
    public record DialogList
    {
        [XmlElement("Dialog")]
        public Dialog[] Dialogs { get; set; }

        public static IEnumerable<Dialog> LoadXml(IEnumerable<UnityEngine.TextAsset> files)
        {
            var sr = new XmlSerializer(typeof(DialogList));
            foreach(var f in files)
            {
                var dialogList = (DialogList)sr.Deserialize(new StringReader(f.text));
                foreach (var d in dialogList.Dialogs)
                    yield return d;
            }
        }
    }


    public abstract record DialogNode
    {
        protected static readonly string Dummy = "<XML_DUMMY>";
        protected static ExpressionParser DefaultParser => ExpressionParser.Instance;
    }
    public abstract record DialogNodeContainer : DialogNode
    {
        [XmlElement("Text", typeof(DialogNode_Text))]
        [XmlElement("Debug", typeof(DialogNode_Debug))]
        [XmlElement("Assign", typeof(DialogNode_Assign))]
        [XmlElement("Label", typeof(DialogNode_Label))]
        [XmlElement("Jump", typeof(DialogNode_Jump))]
        [XmlElement("Call", typeof(DialogNode_Functioncall))]
        [XmlElement("Choice", typeof(DialogNode_Choice))]
        [XmlElement("Switch", typeof(DialogNode_Switch))]
        public DialogNode[] Children { get; set; }
    }
    public record Dialog : DialogNodeContainer
    {
        [XmlAttribute("id")] public string Id { get; set; }
    }
    #endregion

    #region IndividualNodes
    public record DialogNode_Text : DialogNode
    {
        [XmlText] public string _text { get => Dummy; set=> Text = InterpolatedString.MakeMultiline(value, DefaultParser); }
        [XmlIgnore] public InterpolatedString Text { get; private set; }

        [XmlAttribute("display")] public string _displayMode { get => Dummy; set => DisplayMode = new InterpolatedString(value, DefaultParser); }
        [XmlIgnore] public InterpolatedString DisplayMode { get; private set; }

    }
    public record DialogNode_Debug : DialogNode
    {
        [XmlText] public string _text { get => Dummy; set=> Text = InterpolatedString.MakeMultiline(value, DefaultParser); }
        [XmlIgnore] public InterpolatedString Text { get; private set; }

    }
    public record DialogNode_Assign : DialogNode
    {
        [XmlAttribute("variable")] public string _targetVariable { get=>Dummy; set=>TargetVariable = new InterpolatedString(value, DefaultParser); }
        [XmlAttribute("value")] public string _expression { get=>Dummy; set=>Value=new InterpolatedString(DefaultParser.Parse(value)); }
        [XmlText] public string _text { get=>Dummy; set=>Value=InterpolatedString.MakeMultiline(value, DefaultParser); }

        [XmlIgnore] public InterpolatedString TargetVariable {get;private set;}
        [XmlIgnore] public InterpolatedString Value {get;private set;}
    }
    public record DialogNode_Label : DialogNode 
    {
        [XmlAttribute("id")] public string Id { get; set; }
    }
    public record DialogNode_Jump : DialogNode 
    {
        [XmlAttribute("label")] public string _label { get=>Dummy; set=>Label=new InterpolatedString(value, DefaultParser); }
        [XmlAttribute("dialog")] public string _dialog { get => Dummy; set=>Dialog=new InterpolatedString(value, DefaultParser); }
        [XmlAttribute("if")] public string _condition { get => Dummy; set=>Condition=DefaultParser.Parse(value); }

        [XmlIgnore] public InterpolatedString Label { get; private set; }
        [XmlIgnore] public InterpolatedString Dialog { get; private set; }
        [XmlIgnore] public ExpressionNode Condition { get; private set; }
    }
    public record DialogNode_Functioncall : DialogNode
    {
        [XmlAttribute("signal")] public string _signal { get => Dummy; set=>Signal=new InterpolatedString(value, DefaultParser); }
        [XmlAttribute("argument")] public string _argument { get => Dummy; set=>Argument = DefaultParser.Parse(value); }

        [XmlIgnore] public InterpolatedString Signal { get; private set; }
        [XmlIgnore] public ExpressionNode Argument { get; private set; }
    }

    public abstract record DialogNode_GenericBranch : DialogNode
    {
        public abstract IEnumerable<DialogNodeContainer> AllNestedContainers { get; }
    }
    public record DialogNode_Choice : DialogNode_GenericBranch
    {
        [XmlText] public string _text { get => Dummy; set => Text = InterpolatedString.MakeMultiline(value, DefaultParser); }
        [XmlIgnore] public InterpolatedString Text { get; private set; }

        [XmlElement("Option")]
        public ChoiceOption[] Options { get; set; }

        public override IEnumerable<DialogNodeContainer> AllNestedContainers => Options;

        public record ChoiceOption : DialogNodeContainer
        {
            [XmlAttribute("text")] public string _inlineText { get=>Dummy; set=>Text=new InterpolatedString(value, DefaultParser); }
            [XmlElement("OptionText")] public string _multilineText { get=>Dummy; set=>Text=InterpolatedString.MakeMultiline(value, DefaultParser); }
            [XmlIgnore] public InterpolatedString Text { get; private set; }
        }
    }
    public record DialogNode_Switch : DialogNode_GenericBranch
    {
        [XmlElement("Case")]public SwitchOption[] Options { get; set; }
        [XmlElement("Default")] public DefaultOption Default { get; set; }
        public override IEnumerable<DialogNodeContainer> AllNestedContainers => Default==null? Options : Options.Concat(new DialogNodeContainer[] { Default });
        public record SwitchOption : DialogNodeContainer
        {
            [XmlAttribute("if")] public string _condition { get=>Dummy; set=>Condition = DefaultParser.Parse(value); }
            [XmlIgnore] public ExpressionNode Condition { get; private set; }
        }
        public record DefaultOption : DialogNodeContainer { }
    }
    #endregion

    #endregion

    #region DialogInterpretation

    public struct DialogNodeFullAddress
    {
        public DialogNodeAddress[] Segments;
    }
    public struct DialogNodeAddress
    {
        public DialogNodeContainer HostContainer { get; init; }
        public int Index { get; init; }
    }
    public abstract class DialogNodeInterpreter
    {
        public delegate void OnDialogEndedCallback(DialogNodeInterpreter lastState);
        protected internal IDialogContext _context { get; init; }
        protected internal DialogNodeContainer _hostContainer { get; init; }
        protected internal int _nodeIndex { get; init; }
        protected internal DialogNodeInterpreter _parent { get; init; }
        protected internal Dialog _getDialog()
        {
            DialogNodeInterpreter root = this;
            while (root._parent != null) root = root._parent;
            return (Dialog)(root._hostContainer);
        }

        public DialogNode DataUntyped => _hostContainer.Children[_nodeIndex];



        public void Start(OnDialogEndedCallback onEnded)
        {
            IsRunning = true;
            try
            {
                _start_impl(onEnded);
            }catch(Exception e)
            {
                _err($"{e.Message} - {e.StackTrace}");
                if (IsRunning)
                    RunNext(onEnded);
            }
        }
        public abstract void _start_impl(OnDialogEndedCallback onEnded);
        public bool IsRunning { get; private set; }
        public void RunNext(OnDialogEndedCallback onDialogEndedCallback) => RunNext(GetDefaultNext(), onDialogEndedCallback);
        protected void RunNext(DialogNodeInterpreter next, OnDialogEndedCallback onDialogEndedCallback)
        {
            IsRunning = false;
            if(next == null)
            {
                onDialogEndedCallback?.Invoke(this);
            }
            else
            {
                next.Start(onDialogEndedCallback);
            }
        }

        public static DialogNodeInterpreter Make(IDialogContext context, DialogNodeContainer host, int index, DialogNodeInterpreter parent)
        {
            return host.Children[index] switch
            {
                DialogNode_Text => Make<DialogNodeInterpreter_Text>(),
                DialogNode_Debug => Make<DialogNodeInterpreter_Debug>(),
                DialogNode_Assign => Make<DialogNodeInterpreter_Assign>(),
                DialogNode_Label => Make<DialogNodeInterpreter_Label>(),
                DialogNode_Jump => Make<DialogNodeInterpreter_Jump>(),
                DialogNode_Functioncall => Make<DialogNodeInterpreter_Functioncall>(),
                DialogNode_Choice => Make<DialogNodeInterpreter_Choice>(),
                DialogNode_Switch => Make<DialogNodeInterpreter_Switch> (),
                object o => throw new InvalidOperationException($"Cannot make interpreter for node {o}"),
            };
            TInterpreter Make<TInterpreter>() where TInterpreter : DialogNodeInterpreter, new() => new TInterpreter() {_context=context, _hostContainer = host, _nodeIndex = index, _parent = parent };
        }
        public static DialogNodeInterpreter Make(IDialogContext context, DialogNodeAddress address, DialogNodeInterpreter parent) => Make(context, address.HostContainer, address.Index, parent);
        public static DialogNodeInterpreter MakePath(IDialogContext context, DialogNodeFullAddress address)
        {
            return address.Segments.Aggregate((DialogNodeInterpreter)null, (parent, segment) => Make(context, segment, parent));
        }




        protected string _eval(InterpolatedString str) => str.ToString(_context, _context.ExpressionEvaluator);
        protected ExpressionValue _eval(ExpressionNode node) => node == null ? null : _context.ExpressionEvaluator.Evaluate(node, _context);
        protected void _err(string errorMessage) => UnityEngine.Debug.LogError(errorMessage, _context.ErrorReportContext);
        protected DialogNodeInterpreter GetDefaultNext()
        {
            var nextIndex = _nodeIndex + 1;
            if(nextIndex < _hostContainer.Children.Length)
            {
                return DialogNodeInterpreter.Make(_context, _hostContainer, nextIndex, _parent);
            }
            else
            {
                return _parent?.GetDefaultNext();
            }
        }
    }
    public abstract class DialogNodeInterpreter<TNode> :DialogNodeInterpreter where TNode: DialogNode
    {
        public TNode Data => (TNode)DataUntyped;
    }

    public class DialogNodeInterpreter_Debug : DialogNodeInterpreter<DialogNode_Debug>
    {
        public override void _start_impl(OnDialogEndedCallback onEnded)
        {
            UnityEngine.Debug.Log($"Dialog-Debug: {_eval(Data.Text)}", _context.ErrorReportContext);
            RunNext(onEnded);
        }
    }
    public class DialogNodeInterpreter_Text: DialogNodeInterpreter<DialogNode_Text>
    {
        public override void _start_impl(OnDialogEndedCallback onEnded)
        {
            var text = _eval(Data.Text);
            if (_context.TextBox.IsNil())
            {
                _err($"Trying to use Text dialogue node but no TextBox is available!");
                RunNext(onEnded);
            }
            else
            {
                if (!Enum.TryParse<ITextBox.DisplayMode>(_eval(Data.DisplayMode), true, out var displayMode)) displayMode = default;
                _context.TextBox.StartPrintout(text, () =>
                {
                    RunNext(onEnded);
                }, displayMode);
            }
        }
    }
    public class DialogNodeInterpreter_Assign : DialogNodeInterpreter<DialogNode_Assign>
    {
        public override void _start_impl(OnDialogEndedCallback onEndedCallback)
        {
            _context.TrySetVariable(_eval(Data.TargetVariable), _eval(Data.Value));
            RunNext(onEndedCallback);
        }
    }
    public class DialogNodeInterpreter_Label : DialogNodeInterpreter<DialogNode_Label>
    {
        public override void _start_impl(OnDialogEndedCallback onEndedCallback) => RunNext(GetDefaultNext(), onEndedCallback);
    }
    public class DialogNodeInterpreter_Jump : DialogNodeInterpreter<DialogNode_Jump>
    {
        public override void _start_impl(OnDialogEndedCallback onEndedCallback)
        {
            if (Data.Condition!=null && !_eval(Data.Condition).BoolValue)
            {
                RunNext(onEndedCallback);
                return;
            }
            var labelId = Data.Label.IsEmpty?null: _eval(Data.Label);
            Dialog dialog;
            if (Data.Dialog.IsEmpty)
            {
                dialog = _getDialog();
            }
            else
            {
                var dialogId = _eval(Data.Dialog);
                if(!_context.TryGetDialogById(dialogId, out dialog))
                {
                    _err($"Cannot find dialog '{dialogId}'");
                    RunNext(onEndedCallback);
                    return;
                }
            }
            if(!_context.TryGetLabel(dialog, labelId, out var labelPath))
            {
                _err($"Cannot find label '{labelId}' in dialog '{dialog.Id}'");
                RunNext(onEndedCallback);
                return;
            }
            var label = MakePath(_context, labelPath);
            RunNext(label, onEndedCallback);
        }

    }
    public class DialogNodeInterpreter_Functioncall : DialogNodeInterpreter<DialogNode_Functioncall>
    {
        public override void _start_impl(OnDialogEndedCallback onEndedCallback)
        {
            var signal = _eval(Data.Signal);
            var arg = _eval(Data.Argument);
            _context.InvokeSignal(signal, arg, () =>
            {
                RunNext(onEndedCallback);
            });
        }
    }
    public class DialogNodeInterpreter_Choice : DialogNodeInterpreter<DialogNode_Choice>
    {
        public override void _start_impl(OnDialogEndedCallback onEndedCallback)
        {
            if (_context.ChoiceBox.IsNil())
            {
                _err($"Trying to use choice node while no ChoiceBox is available!");
                RunNext(onEndedCallback);
                return;
            }
            _context.ChoiceBox.StartPrintout(_eval(Data.Text), Data.Options.Select(op => _eval(op.Text)), i =>
            {
                var chosenBranch = Data.Options[i];
                var nextNode = DialogNodeInterpreter.Make(_context, chosenBranch, 0, this);
                RunNext(nextNode, onEndedCallback);
            });
        }
    }
    public class DialogNodeInterpreter_Switch : DialogNodeInterpreter<DialogNode_Switch>
    {
        public override void _start_impl(OnDialogEndedCallback onEndedCallback)
        {
            DialogNodeContainer chosenBranch = Data.Default;
            foreach(var option in Data.Options)
            {
                var condition = _eval(option.Condition);
                if (condition.BoolValue)
                {
                    chosenBranch = option;
                    break;
                }
            }
            if(chosenBranch == null)
            {
                RunNext(onEndedCallback);
            }
            else
            {
                var nextNode = DialogNodeInterpreter.Make(_context, chosenBranch, 0, this);
                RunNext(nextNode, onEndedCallback);
            }
        }
    }

    #endregion



    #region StringInterpolation

    public struct InterpolatedString
    {

        public static InterpolatedString MakeMultiline(string text, ExpressionParser parser = null)
        {
            using var rdr = new StringReader(text);
            var bld = new StringBuilder();
            
            string line = null;
            while (string.IsNullOrWhiteSpace((line = rdr.ReadLine()))) ;
            if (line == null) return default(InterpolatedString);
            var whitespaceCount = line.TakeWhile(char.IsWhiteSpace).Count();
            for(;line!=null; line = rdr.ReadLine())
            {
                if(!string.IsNullOrWhiteSpace(line))
                {
                    foreach (var c in line.SkipWhile((c, i) => i < whitespaceCount && char.IsWhiteSpace(c)))
                        bld.Append(c);
                }
                bld.Append('\n');
            }

            return new(bld.ToString(), parser);
        }
        public InterpolatedString(string text, ExpressionParser parser=null)
        {
            parser ??= ExpressionParser.Instance;
            Segments = MakeSegments(text, parser).ToArray();
        }
        public InterpolatedString(ExpressionNode expression) => Segments = new Segment[] { new Segment(expression) };


        readonly IReadOnlyList<Segment> Segments;
        struct Segment
        {
            public Segment(string s) => _value = s;
            public Segment(ExpressionNode n) => _value = n;
            public object _value{ get;}
            public string ToString(IExpressionContext ctx, ExpressionEvaluator evaluator) => _value switch
            {
                string s => s,
                ExpressionNode n => evaluator.Evaluate(n, ctx).StringValue,
                _ => throw new InvalidOperationException("this should not happen!")
            };
        }
        public bool IsEmpty => Segments == null;
        public string ToString(IExpressionContext ctx, ExpressionEvaluator evaluator=null) =>Segments==null?"":string.Concat(Segments.Select(s => s.ToString(ctx, evaluator??=ExpressionEvaluator.Instance)));

        private static IEnumerable<Segment> MakeSegments(string text, ExpressionParser parser)
        {
            const string ExpressionBegin = "${", ExpressionEnd = "}";

            while (!string.IsNullOrEmpty(text))
            {
                var (left, right) = text.SplitByFirstOccurence(ExpressionBegin);
                yield return new Segment(left);
                if (string.IsNullOrEmpty(right)) 
                    yield break;
                var (expression, rest) = right.SplitByFirstOccurence(ExpressionEnd);
                yield return new Segment(parser.Parse(parser.Tokenize(expression)));
                text = rest;
            }
        }
    }
    #endregion
}
