using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Evaluation
{
    public class Evaluate
    {
        private Dictionary<string, Func<double, double>> oneArgFuncsMap = new()
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
            { "cos", Math.Cos },
            { "e", x => Math.Pow(10, x) }
        };

        private Dictionary<string, Func<double, double, double>> twoArgsFuncsMap = new()
        {
            {"+", (x, y) => x + y },
            {"-", (x, y) => x - y },
            {"*", (x, y) => x * y },
            {"/", (x, y) => x / y },
            {"&", (x, y) => Math.Pow(x, y) },
            {"e", (x, y) => x * Math.Pow(10, y) },
        };

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

                // Add global brackets
                expr = "(" + expr + ")";

                // Calculate all 1ex
                while ((m = Regex.Match(expr, @"([\d\.]+)e(-?\+?\d+)")).Success)
                {
                    var newValue = Calculate("e", m.Groups[1].Value, m.Groups[2].Value);
                    expr = Replace(expr, m, newValue);
                }

                // Inverse powers (for simplified colculation)
                //expr = InvertPowers(expr);

                // Replace multiple -
                while ((m = Regex.Match(expr, @"(-{2,})")).Success)
                {
                    var v = m.Groups[1].Value.Length % 2 == 0 ? "+" : "-";
                    expr = Replace(expr, m, v);
                }

                expr = Regex.Replace(expr, @"(\+-)|(-\+)", "-");

                // Calculate all brackets without inner brackets
                while ((m = Regex.Match(expr, @"\([^\(\)]+\)")).Success)
                    expr = expr.Replace(m.Value, EvalSimpleInBrackets(m.Value));

                if (double.TryParse(expr, out var d))
                    return d.ToString();
                else
                    return "ERROR";
            }
            catch
            {
                return "ERROR";
            }
        }

        private string EvalSimpleInBrackets(string expr)
        {
            // Positive numbers regex
            var pnr = @"\d+\.\d+";
            // All numbers regex
            var anr = @$"(?:{pnr})|(?:-{pnr})|(?:n{pnr})|(?:-n{pnr})";


            Match m;
            // Remove brackets
            expr = expr.Replace("(", "");
            expr = expr.Replace(")", "");

            // Calculate all functions
            while ((m = Regex.Match(expr, @"([a-z]+)(-?[\d\.]+)")).Success)
            {
                var funcName = m.Groups[1].Value;
                var newValue = Calculate(funcName, m.Groups[2].Value);
                expr = Replace(expr, m, newValue.ToString());
            }

            // Calculate all &
            while ((m = Regex.Match(expr, @"(\d+)&(-?[\d\.]+)")).Success)
            {
                var newValues = Calculate("&", m.Groups[1].Value, m.Groups[2].Value);
                expr = Replace(expr, m, newValues);
            }

            // Calculate all * /
            while ((m = Regex.Match(expr, SimpleRegex("[*/]"))).Success)
            {
                var chr = m.Groups[2].Value;
                var newValue = Calculate(chr, m.Groups[1].Value, m.Groups[3].Value);
                expr = Replace(expr, m, newValue);
            }

            // Calculate all + -
            while ((m = Regex.Match(expr, SimpleRegex("[+-]"))).Success)
            {
                var chr = m.Groups[2].Value;
                var newValue = Calculate(chr, m.Groups[1].Value, m.Groups[3].Value);
                expr = Replace(expr, m, newValue);
            }

            return double.Parse(expr).ToString();
        }

        private string SimpleRegex(string chr) =>
            $@"(-?[\d\.]+)({chr})(-?[\d\.]+)";

        private string Replace(string str, Match m, string value) =>
            Replace(str, m.Index, m.Length, value);

        private string Replace(string str, int startIndex, int length, string value) =>
            str[..startIndex] + value + str[(startIndex + length)..];

        private string Reverse(string str) =>
            new string(str.Reverse().ToArray());

        private string Calculate(string chr, string arg1, string arg2 = null)
        {
            double res;
            var v1 = double.Parse(arg1);
            if (arg2 is null)
                res = oneArgFuncsMap[chr](v1);
            else
            {
                var v2 = double.Parse(arg2);
                res = twoArgsFuncsMap[chr](v1, v2);
            }

            if (double.IsNaN(res) || double.IsInfinity(res))
                throw new ArithmeticException();

            return res.ToString();
        }

        private string InvertPowers(string str)
        {
            Match m; 

            foreach(var match in Regex.Matches(str, @"&(\d+)").Cast<Match>())
            {
                str = Replace(str, match, $"&{Reverse(match.Groups[1].Value)}");
            }

            foreach (var match in Regex.Matches(str, @"[^&]((?:\d+&)+\d+)").Cast<Match>())
            {
                str = Replace(str, match, $"{match.Value[0]}{Reverse(match.Groups[1].Value)}");
            }

            while((m = Regex.Match(str, @"\)((?:&\d+)+)")).Success)
            {
                var closeNum = 1;
                var openNum = 0;
                var i = m.Index;
                while(closeNum != openNum && --i >= 0)
                {
                    if (str[i] == ')')
                        closeNum++;
                    if (str[i] == '(')
                        openNum++;
                }
                var begin = str[..i];
                var reversed = Reverse(m.Groups[1].Value);
                var middle = str.Substring(i, m.Index + 1 - i);
                var end = str[(m.Index + m.Length)..];

                str = $"{begin}{reversed}{middle}{end}";
            }

            return str;
        }
    }
}
