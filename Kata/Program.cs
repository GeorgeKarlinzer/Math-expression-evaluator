using Evaluation;
using System;
using System.Text.RegularExpressions;


var negInBrac = @"(?:\(-\d+\))";


var ev = new Evaluate();

var res = ev.eval("sqrt(14&(1+1--1+-1))");
Console.WriteLine(res);