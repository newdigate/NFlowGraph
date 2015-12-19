using NFlowGraph.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFlowGraph.Tests
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void First()
        {
            TextInputModule consoleIn = new TextInputModule("shaggly");
            InputModule<long> numberGenerator = new InputModule<long>(() => 1);
            InputModule<long> numberGenerator2 = new InputModule<long>(() => 2);
            Module<Tuple<long, long>, long> adder = new Module<Tuple<long, long>, long>(a => { return a.Item1 + a.Item2; });
            OutputModule<long> output = new OutputModule<long>((l) => Console.WriteLine(l));
            OutputModule<string> stringOutput = new OutputModule<string>((l) => Console.WriteLine(l));

            FlowGraph g = new FlowGraph();
            g.Connect(numberGenerator, 0, adder, 0);
            g.Connect(numberGenerator2, 0, adder, 1);
            g.Connect(adder, 0, output, 0);
            g.Connect(consoleIn, 0, stringOutput, 0);
            g.Process();
        }
    }
}
