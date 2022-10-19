using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Evaluation
{
    public class Evaluator
    {
        readonly Dictionary<string, Func<double, double>> oneArgFuncsMap = new()
        {
            { "log", Math.Log10 },
            { "ln", Math.Log },
            { "exp", Math.Exp },
            { "sqrt", Math.Sqrt },
            { "abs", Math.Abs },
            { "atan", Math.Atan },
            { "acos", Math.Acos },
            { "asin", Math.Asin },
            { "sinh", Math.Sinh },
            { "cosh", Math.Cosh },
            { "tanh", Math.Tanh },
            { "tan", Math.Tan },
            { "sin", Math.Sin },
            { "cos", Math.Cos }
        };

        readonly Dictionary<string, Func<double, double, double>> twoArgsFuncsMap = new()
        {
            { "+", (x, y) => x + y },
            { "-", (x, y) => x - y },
            { "*", (x, y) => x * y },
            { "/", (x, y) => x / y },
            { "&", Math.Pow },
            { "e", (x, y) => x * Math.Pow(10, y) }
        };

        // Positive numbers regex
        readonly string pnr;
        // All numbers regex
        // NOTE: all negative numbers are replaced with n. Example: -123 -> n123
        // That helps open brackets, so (-2)&2 -> n2&2 -> 4, not (-2)&2 -> -2&2 -> -4
        readonly string anr;
        // Functions regex
        readonly string fr;

        public Evaluator()
        {
            pnr = @"(?:\d+\.\d+|\d+)";
            anr = @$"(?:n?{pnr})";
            fr = string.Join('|', oneArgFuncsMap.Keys.ToList());
        }

        public string Evaluate(string expr)
        {
            try
            {
                Match m;

                // To lower
                expr = expr.ToLower();

                // Catch unknown functions
                var words = Regex.Matches(expr, @"[a-zA-Z]+").Select(x => x.Value);
                if (words.Any(x => !oneArgFuncsMap.ContainsKey(x) && x != "e"))
                    return "ERROR";

                // Add global brackets
                expr = $"({expr})";

                // Remove whitespaces
                expr = expr.Replace(" ", "");

                // Replace +- and -+ with -
                expr = Regex.Replace(expr, @"\+-|-\+", "-");

                // Replace multiple -
                while ((m = Regex.Match(expr, @"(-{2,})")).Success)
                {
                    var v = m.Groups[1].Value.Length % 2 == 0 ? "+" : "-";
                    expr = Replace(expr, m, v);
                }

                // Replace all (- and (+ with (0- and (0+
                expr = Regex.Replace(expr, @"\(([+-])", "(0$1");

                // Replace all *- and /- with *n and /n 
                expr = Regex.Replace(expr, @"([*/])-", "$1n");

                // Calculate all 1ex
                while ((m = Regex.Match(expr, $@"({pnr})e(-?\+?\d+)")).Success)
                {
                    var arg1 = double.Parse(m.Groups[1].Value);
                    var arg2 = double.Parse(m.Groups[2].Value);
                    var newValue = Calculate("e", arg1, arg2);
                    expr = Replace(expr, m, newValue);
                }

                // Calculate all brackets without inner brackets
                while ((m = Regex.Match(expr, @"\([^\(\)]+\)")).Success)
                    expr = expr.Replace(m.Value, EvalInBrackets(m.Value));

                // Replace n with - if needed
                if (expr[0] == 'n')
                    expr = $"-{expr[1..]}";
                
                // If result is not a number return ERROR
                if (double.TryParse(expr, out var d)) return expr;
                else return "ERROR";
            }
            catch
            {
                // Exceptions during calculations cause returning ERROR
                return "ERROR";
            }
        }

        private string EvalInBrackets(string expr)
        {
            Match m;
            // Remove global brackets
            expr = expr[1..^1];

            if (Regex.IsMatch(expr, $@"^(?:{anr})$"))
                return expr;

            // Calculate all functions
            while ((m = Regex.Match(expr, @$"({fr})({anr})")).Success)
            {
                var arg = GetNum(m.Groups[2].Value);
                var action = m.Groups[1].Value;
                var newValue = Calculate(action, arg);
                expr = Replace(expr, m, newValue.ToString());
            }

            // Calculate all &*/+- actions
            var twoArgsFuncs = new[] { ("&", "(?!.*&.*)"), ("[*/]", ""), (@"[+-]", "") };

            foreach (var (func, addition) in twoArgsFuncs)
            {
                while ((m = Regex.Match(expr, $@"({anr})({func})({anr}){addition}")).Success)
                {
                    var action = m.Groups[2].Value;
                    var arg1 = GetNum(m.Groups[1].Value);
                    var arg2 = GetNum(m.Groups[3].Value);
                    var newValue = Calculate(action, arg1, arg2);
                    expr = Replace(expr, m, newValue);
                }
            }

            return $"{expr}";
        }

        private double GetNum(string str) =>
            str[0] == 'n' ? -double.Parse(str[1..]) : double.Parse(str);

        private string Replace(string str, Match m, string value) =>
            str[..m.Index] + value + str[(m.Index + m.Length)..];

        private string Calculate(string chr, double arg1, double? arg2 = null)
        {
            double res;
            if (arg2 is null)
                res = oneArgFuncsMap[chr](arg1);
            else
                res = twoArgsFuncsMap[chr](arg1, (double)arg2);

            if (double.IsNaN(res) || double.IsInfinity(res))
                throw new ArithmeticException();

            if (res < 0) return $"n{-res}";
            else return $"{Math.Abs(res)}";
        }
    }
}
