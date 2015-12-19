using NFlowGraph.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
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
            OutputModule<long> output = new OutputModule<long>((l) => { });
            OutputModule<string> stringOutput = new OutputModule<string>((l) => { } );

            FlowGraph g = new FlowGraph();
            g.Connect(numberGenerator, 0, adder, 0);
            g.Connect(numberGenerator2, 0, adder, 1);
            g.Connect(adder, 0, output, 0);
            g.Connect(consoleIn, 0, stringOutput, 0);
            System.Diagnostics.Stopwatch w = new Stopwatch();
            w.Start();
            for (int i = 0; i < 10000; i++) {    
                g.Process();
            }
            w.Stop();
            Console.Out.Write(w.Elapsed.TotalSeconds);
        }
    }
}
