using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFlowGraph.Core
{
    public class Module<TInput, TOutput> : IModule
    {
        private readonly int _numInputs;
        private readonly int _numOutputs;

        public int NumInputs { get { return _numInputs; } }
        public int NumOutputs { get { return _numOutputs; } }

        private Func<TInput, TOutput> _process { get; set; }

        public Module(Func<TInput, TOutput> process)
        {
            _process = process;

            int numInputs = 0;
            if (typeof(TInput).IsPrimitive) numInputs = 1;
            else if (typeof(TInput).Name.StartsWith("Tuple"))
            {
                numInputs = typeof(TInput).GetGenericArguments().Count();
            }
            _numInputs = numInputs;

            int numOutputs = 0;
            if (typeof(TOutput).IsPrimitive) numOutputs = 1;
            else if (typeof(TOutput).Name.StartsWith("Tuple"))
            {
                numOutputs = typeof(TOutput).GetGenericArguments().Count();
            }
            _numOutputs = numOutputs;
        }

        public void Execute(ProcessContext context)
        {
            TInput input = default(TInput);
            if (typeof(TInput).IsPrimitive)
            {
                if (context.Inputs[0] != null)
                    input = (TInput)context.Inputs[0];
            }
            else if (typeof(TInput).Name.StartsWith("Tuple"))
            {
                object[] args = new object[typeof(TInput).GetGenericArguments().Count()];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = context.Inputs[i];
                }
                input = (TInput)Activator.CreateInstance(typeof(TInput), args);
            }

            try
            {
                TOutput output = _process(input);

                if (typeof(TOutput).IsPrimitive)
                {
                    context.Outputs[0] = output;
                }
                else if (typeof(TOutput).Name.StartsWith("Tuple"))
                {
                    for (int i = 0; i < typeof(TOutput).GetGenericArguments().Count(); i++)
                    {
                        PropertyInfo p = typeof(TOutput).GetProperty(string.Format("Item{0}", i));
                        context.Outputs[i] = p.GetValue(input);
                    }
                }
            }
            catch (Exception exc)
            {
                Console.Write(exc);
                Console.ReadLine();
            }
        }
    }

}
