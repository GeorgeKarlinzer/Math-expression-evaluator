using Evaluation;
using System.Text.RegularExpressions;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void RandomTests()
        {
            var evaluator = new Evaluator();

            var eg = new ExpressionGenerator();

            for (int i = 0; i < 100; i++)
            {
                var expr = eg.Generate(i);
                var myRes = evaluator.Evaluate(expr);
                var correctRes = PythonEvaluator.Evaluate(expr);

                if (myRes == "ERROR" || correctRes == "ERROR")
                    Assert.AreEqual(myRes, correctRes, $" -> {expr}");
                else
                    Assert.AreEqual(double.Parse(myRes), double.Parse(correctRes), $" -> {expr}");
            }
        }
    }
}