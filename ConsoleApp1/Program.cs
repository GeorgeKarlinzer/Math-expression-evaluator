// See https://aka.ms/new-console-template for more information
using Evaluation;
using NUnit.Framework;
using System.Text.RegularExpressions;
using Tests;

var evaluator = new Evaluator();

var eg = new ExpressionGenerator();

evaluator.Evaluate("-13 * (0 & 14) / -7");

for (int i = 0; i < 100; i++)
{
    var expr = eg.Generate(i);
    var exprP = Regex.Replace(expr, @"& (-\d+\.?\d*)", @"&($1)");
    Console.WriteLine(expr);
    Console.WriteLine(exprP);
    Console.WriteLine(evaluator.Evaluate(expr));
    Console.WriteLine(PythonEvaluator.Evaluate(exprP));
    Console.WriteLine();
}