using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Evaluation
{
    public class Evaluate
    {
        readonly static Dictionary<string, Func<double, double>> oneArgFuncsMap = new()
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

        readonly static Dictionary<string, Func<double, double, double>> twoArgsFuncsMap = new()
        {
            {"+", (x, y) => x + y },
            {"-", (x, y) => x - y },
            {"*", (x, y) => x * y },
            {"/", (x, y) => x / y },
            {"&", Math.Pow },
            {"e", (x, y) => x * Math.Pow(10, y) }
        };

        // Positive numbers regex
        readonly static string pnr = @"(?:\d+\.\d+|\d+)";
        // All numbers regex. NOTE: n123 = (-123)
        readonly static string anr = @$"(?:n?{pnr})";
        // Functions regex
        readonly static string fr = string.Join('|', oneArgFuncsMap.Keys.ToList());

        public string eval(string expr)
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

                // Replace all '(+num' with '(num'
                expr = expr.Replace("(+", "(");

                // Replace all *- or /- with *n or /n 
                expr = Regex.Replace(expr, @"([*/])-", "$1n");

                // Calculate all 1ex
                while ((m = Regex.Match(expr, $@"({pnr})e(-?\+?\d+)")).Success)
                {
                    var arg1 = double.Parse(m.Groups[1].Value);
                    var arg2 = double.Parse(m.Groups[2].Value);
                    var newValue = Calculate("e", arg1, arg2);
                    expr = Replace(expr, m, newValue);
                }

                // Add global brackets
                expr = $"({expr})";

                // Calculate all brackets without inner brackets
                while ((m = Regex.Match(expr, @"\([^\(\)]+\)")).Success)
                    expr = expr.Replace(m.Value, EvalInBrackets(m.Value));

                if (expr[0] == 'n')
                    expr = $"-{expr[1..]}";

                if (double.TryParse(expr, out var d))
                    return expr;
                else
                    return "ERROR";
            }
            catch
            {
                return "ERROR";
            }
        }

        private string EvalInBrackets(string expr)
        {
            Match m;
            // Remove global brackets
            expr = expr[1..^1];

            if (expr[0] == '-')
                expr = $"0{expr}";

            if (Regex.IsMatch(expr, $@"^(?:{anr})$"))
                return $"{GetNum(expr)}";

            // Calculate all functions
            while ((m = Regex.Match(expr, @$"({fr})({anr})")).Success)
            {
                var arg = GetNum(m.Groups[2].Value);
                var action = m.Groups[1].Value;
                var newValue = Calculate(action, arg);
                expr = Replace(expr, m, newValue.ToString());
            }

            var twoArgsFuncs = new[] { ("&", "(?!.*&.*)"), ("[*/]", ""), (@"[+-]", "") };

            foreach (var (func, addition) in twoArgsFuncs)
            {
                while((m = Regex.Match(expr, $@"({anr})({func})({anr}){addition}")).Success)
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

        // Get double from string that meets anr
        private static double GetNum(string str) =>
            str[0] == 'n' ? -double.Parse(str[1..]) : double.Parse(str);

        private static string Replace(string str, Match m, string value) =>
            Replace(str, m.Index, m.Length, value);

        private static string Replace(string str, int startIndex, int length, string value) =>
            str[..startIndex] + value + str[(startIndex + length)..];

        private static string Reverse(string str) =>
            new(str.Reverse().ToArray());

        private static string Calculate(string chr, double arg1, double? arg2 = null)
        {
            double res;
            if (arg2 is null)
                res = oneArgFuncsMap[chr](arg1);
            else
                res = twoArgsFuncsMap[chr](arg1, (double)arg2);

            if (double.IsNaN(res) || double.IsInfinity(res))
                throw new ArithmeticException();

            if (res < 0) return $"n{-res}";
            else return $"{res}";
        }
    }
}
