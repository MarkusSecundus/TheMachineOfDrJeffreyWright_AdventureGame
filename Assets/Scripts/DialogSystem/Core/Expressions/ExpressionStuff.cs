using MarkusSecundus.Utils.Datastructs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;

namespace MarkusSecundus.TinyDialog.Expressions
{
    public interface IExpressionVisitor<TRet, TContext>
    {
        public TRet Visit(CompositeExpression toVisit, TContext ctx);
        public TRet Visit(LiteralExpression toVisit, TContext ctx);
        public TRet Visit(VariableExpression toVisit, TContext ctx);
    }
    public abstract record ExpressionNode{
        public abstract TRet Accept<TRet, TContext>(IExpressionVisitor<TRet, TContext> visitor, TContext ctx);
    }

    public record CompositeExpression : ExpressionNode
    {
        public string FunctionName { get; init; }
        public IReadOnlyList<ExpressionNode> Arguments { get; init; }

        public override TRet Accept<TRet, TContext>(IExpressionVisitor<TRet, TContext> visitor, TContext ctx) => visitor.Visit(this, ctx);

        public override string ToString() => $"({FunctionName} {Arguments.MakeString()})";
    }

    public record LiteralExpression : ExpressionNode
    {
        public ExpressionValue Value { get; init; }
        public override TRet Accept<TRet, TContext>(IExpressionVisitor<TRet, TContext> visitor, TContext ctx) => visitor.Visit(this, ctx);

        public override string ToString() => $"'{Value.StringValue}'";
    }
    public record VariableExpression : ExpressionNode
    {
        public string Name { get; init; }
        public override TRet Accept<TRet, TContext>(IExpressionVisitor<TRet, TContext> visitor, TContext ctx) => visitor.Visit(this, ctx);
        public override string ToString() => $"{Name}";
    }

    public struct ExpressionValue : IEquatable<ExpressionValue>
    {
        object _value;
        public ExpressionValue(string s) => _value = s;
        public ExpressionValue(double d) => _value = d;
        public string StringValue => _value switch
        {
            null => "",
            string s => s,
            IConvertible c => c.ToString(DefaultCulture), //double implements IConvertible
            object o => o.ToString()
        };
        public double NumberValue => _value switch
        {
            null => 0,
            double d => d,
            object o => TryParseDouble(o.ToString(), out var d) ? d : 0d
        };
        public bool BoolValue => _value switch
        {
            null => false,
            double d => d != 0,
            string s => !string.IsNullOrWhiteSpace(s) && (!TryParseDouble(s, out var d) || d != 0),
            object o => throw new InvalidOperationException($"While casting to bool...{nameof(ExpressionValue)} holds some type ({o.GetType()}) that is neither string nor double - This should not happen!")
        };
        public bool IsString => _value is string;
        public bool IsNumber => _value is double;

        public static implicit operator ExpressionValue(string s) => new ExpressionValue(s);
        public static implicit operator ExpressionValue(double d) => new ExpressionValue(d);

        public override string ToString() => StringValue;
        public override bool Equals(object obj) => obj is ExpressionValue e && Equals(e);
        public bool Equals(ExpressionValue other) => object.Equals(_value, other._value);
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;

        public static bool TryParseDouble(string str, out double d) => double.TryParse(str, NumberStyles.Any, DefaultCulture, out d);
        public static IFormatProvider DefaultCulture => CultureInfo.InvariantCulture;
    }

    public delegate ExpressionValue CompositeExpressionFunction(params ExpressionValue[] args);
    public interface IExpressionContext
    {
        public bool TryGetFunctionBySignature(string name, int argumentCount, out CompositeExpressionFunction function);
        public bool TryGetVariable(string name, out ExpressionValue variableValue);
        public bool TrySetVariable(string name, ExpressionValue valueToSet);

    }
    public partial record ExpressionContext : IExpressionContext
    {
        protected readonly Dictionary<string, ExpressionValue> _variables = new Dictionary<string, ExpressionValue>();
        protected virtual CompositeExpressionFunction MatchFunction(string name, int argumentCount)=> (name, argumentCount) switch
        {
            ("+", 1) => args => + args[0].NumberValue,
            ("-", 1) => args => - args[0].NumberValue,
            ("+", 2) => args => args[0].NumberValue + args[1].NumberValue,
            ("-", 2) => args => args[0].NumberValue - args[1].NumberValue,
            ("*", 2) => args => args[0].NumberValue * args[1].NumberValue,
            ("/", 2) => args => args[0].NumberValue / args[1].NumberValue,
            ("max", 2) => args => args[0].NumberValue >= args[1].NumberValue ? args[0] : args[1],
            ("min", 2) => args => args[0].NumberValue <= args[1].NumberValue ? args[0] : args[1],
            ("==", 2) => args => args[0].Equals(args[1])?1:0,
            ("!=", 2) => args => args[0].Equals(args[1])?0:1,
            ("<", 2) => args => args[0].NumberValue < args[1].NumberValue?0:1,
            (">", 2) => args => args[0].NumberValue > args[1].NumberValue?0:1,
            ("<=", 2) => args => args[0].NumberValue <= args[1].NumberValue?0:1,
            (">=", 2) => args => args[0].NumberValue >= args[1].NumberValue?0:1,
            ("true", 0) => args => 1,
            ("false", 0) => args => 0,
            ("bool", 1) => args => args[0].BoolValue ? 1 : 0,
            ("not" or "!", 1) => args => args[0].BoolValue ? 0 : 1,
            ("concat", _) => args => string.Concat(args.Select(a => a.StringValue)),
            ("substring", 2) => args => args[0].StringValue.Substring((int)args[1].NumberValue),
            ("substring", 3) => args => args[0].StringValue.Substring((int)args[1].NumberValue, (int)args[2].NumberValue),
            ("get", 1) => args => TryGetVariable(args[0].StringValue, out var ret)?ret:null,
            ("platform", 0) => args=> Application.platform.ToString(),
            _ => default
        };

        public bool TryGetVariable(string name, out ExpressionValue variableValue)
            => _variables.TryGetValue(name, out variableValue);
        public bool TryGetFunctionBySignature(string name, int argumentCount, out CompositeExpressionFunction ret)
            => (ret = MatchFunction(name, argumentCount)) != default;

        public bool TrySetVariable(string name, ExpressionValue valueToSet)
        {
            _variables[name] = valueToSet;
            return true;
        }
        public void LoadDefinitions(XmlRepresentation xml)
        {
            foreach (var def in xml.Definitions) _variables.Add(def.Name, def.Value);
        }

        [XmlRoot("Variables")]
        public struct XmlRepresentation
        {
            public struct Entry
            {
                [XmlAttribute("name")]public string Name { get; set; }
                [XmlText] public string Value { get; set; }
            }
            [XmlElement("Var")]
            public Entry[] Definitions;
        }
    }
    public class ExpressionEvaluator
    {
        public static readonly ExpressionEvaluator Instance = new ExpressionEvaluator();

        public ExpressionValue Evaluate(ExpressionNode expr, IExpressionContext ctx) => expr.Accept(Impl.Instance, ctx);

        private class Impl : IExpressionVisitor<ExpressionValue, IExpressionContext>
        {
            public static Impl Instance = new Impl();
            public ExpressionValue Visit(CompositeExpression toVisit, IExpressionContext ctx)
            {
                if (ctx.TryGetFunctionBySignature(toVisit.FunctionName, toVisit.Arguments.Count, out var function))
                {
                    return function(toVisit.Arguments.Select(e => e.Accept(this, ctx)).ToArray());
                }
                DialogueUtilsInternal.DebugLogError($"Trying to call non-existent function with name '{toVisit.FunctionName}' and {toVisit.Arguments} arguments!");
                return default;
            }

            public ExpressionValue Visit(LiteralExpression toVisit, IExpressionContext ctx) => toVisit.Value;

            public ExpressionValue Visit(VariableExpression toVisit, IExpressionContext ctx)
            {
                if (ctx.TryGetVariable(toVisit.Name, out var value))
                    return value;
                DialogueUtilsInternal.DebugLogError($"No variable called '{toVisit.Name}'");
                return default;
            }
        }
    }

    public class ExpressionParser
    {
        public static readonly ExpressionParser Instance = new ExpressionParser();

        public ExpressionNode Parse(string text) => Parse(Tokenize(text));

        public struct Token
        {
            public enum TokenType {_INVALID=0, LBRA, RBRA, IDENTIFIER, LITERAL};
            public TokenType Type { get; init; }
            public int IndexInInputStream { get; init; }
            public int Length => Text?.Length ?? 1;
            public string Text { get; init; }
        }
        public static readonly Regex TokenizerRegex = new Regex(@"^\s*
(?<shouldNotHappen>[^\u0000-\uffff])
|(?<LBRA>\()
|(?<RBRA>\))
|(?<NUMBER_LITERAL>([0-9]+(\.[0-9]*)?(e[-]?[0-9]+)?))
|(?<STRING_LITERAL>('([^']|(''))*'))
|(?<IDENTIFIER>([^\s\(\)\{\}]+))\s*", RegexOptions.IgnorePatternWhitespace); //for some reason, the 1st pattern in the or doesn't recognize occurences other than the very first - had to put some additional random difficult-to-satisfy pattern to the beginning
        public IEnumerable<Token> Tokenize(string expression)
        {
            int begin = 0;
            while (begin < expression.Length)
            {
                var match = TokenizerRegex.Match(expression, begin);
                if (!match.Success)
                    break;
                if (IsGroup("LBRA", out var str, out var index))
                {
                    yield return new Token { Type = Token.TokenType.LBRA, IndexInInputStream = index};
                }
                else if (IsGroup("RBRA", out str, out index))
                {
                    yield return new Token { Type = Token.TokenType.RBRA, IndexInInputStream = index };
                }
                else if (IsGroup("NUMBER_LITERAL", out str, out index))
                {
                    yield return new Token { Type = Token.TokenType.LITERAL, Text =  str, IndexInInputStream = index };
                }
                else if (IsGroup("STRING_LITERAL", out str, out index))
                {
                    yield return new Token { Type = Token.TokenType.LITERAL, Text = ParseStringLiteral(str), IndexInInputStream = index };
                }
                else if (IsGroup("IDENTIFIER", out str, out index))
                {
                    yield return new Token { Type = Token.TokenType.IDENTIFIER, Text = str, IndexInInputStream = index };
                }
                else break;

                bool IsGroup(string group, out string value, out int index)
                {
                    var g = match.Groups[group];
                    value = g.Value;
                    index = g.Index;
                    if (g.Success)
                    {
                        begin = g.Index + g.Length;
                        return true;
                    }
                    return false;
                }
                string ParseStringLiteral(string str) => str.Substring(1, str.Length-2).Replace("''", "'");
            }
        }

        public ExpressionNode Parse(IEnumerable<Token> tokenStream)
        {
            using var it = tokenStream.GetEnumerator();

            IEnumerable<ExpressionNode> nextLayer(bool waitForRbra)
            {
                while (true)
                {
                    if (!MoveNext())
                    {
                        if (waitForRbra)throw Error();
                        else yield break;
                    }
                    else if (it.Current.Type == Token.TokenType.RBRA)
                    {
                        if (waitForRbra) yield break;
                        else throw Error();
                    }
                    else if (it.Current.Type == Token.TokenType.IDENTIFIER)
                        yield return new VariableExpression { Name = it.Current.Text };
                    else if (it.Current.Type == Token.TokenType.LITERAL)
                        yield return new LiteralExpression { Value = it.Current.Text };
                    else if (it.Current.Type == Token.TokenType.LBRA)
                    {
                        if (!MoveNext() || it.Current.Type != Token.TokenType.IDENTIFIER) throw Error();
                        var functionName = it.Current.Text;
                        var args = nextLayer(true).ToArray();
                        yield return new CompositeExpression { FunctionName = functionName, Arguments = args };
                    }
                    else throw Error();
                }
            }

            Token lastToken = default;
            bool MoveNext()
            {
                var ret = it.MoveNext();
                if (ret) lastToken = it.Current;
                return ret;
            }
            Exception Error() => new ArgumentException($"Syntax error! - token {lastToken.Type}::'{lastToken.Text}' on position {lastToken.IndexInInputStream}");


            if (!nextLayer(false).TryGetTheOnlyElement(out var ret) || MoveNext())
                throw Error();
            return ret;
        }
    }

    internal static class DialogueUtilsInternal
    {
        public static void DebugLogError(string message, UnityEngine.Object target = null) => Debug.LogError($"TinyDialog: {message}", target);
    }


}