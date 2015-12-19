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
        private readonly Type[] _inputTypes;
        private readonly Type[] _outputTypes;

        public int NumInputs { get { return _numInputs; } }
        public int NumOutputs { get { return _numOutputs; } }
        public Type[] InputTypes { get { return _inputTypes; } }
        public Type[] OutputTypes { get { return _outputTypes; } }

        private Func<TInput, TOutput> _process { get; set; }
        protected Func<TInput, TOutput> Process { get { return _process; } set { _process = value; } }

        public Module(Func<TInput, TOutput> process) : this()
        {
            _process = process;
        }
        
        public Module()
        {
            int numInputs = 0;
            Type[] inputTypes = null;
            if ((typeof(TInput).IsPrimitive) || (typeof(TInput).Equals(typeof(String)))) { 
                numInputs = 1;
                inputTypes = new Type[] { typeof(TInput) }; 
            } else if (typeof(TInput).Name.StartsWith("Tuple"))
            {
                inputTypes = typeof(TInput).GetGenericArguments().ToArray();
                numInputs = inputTypes.Length;
            }

            _numInputs = numInputs;
            _inputTypes = inputTypes;

            int numOutputs = 0;
            Type[] outputTypes = null;
            if ((typeof(TOutput).IsPrimitive) || (typeof(TOutput).Equals(typeof(String)))) {
                numOutputs = 1;
                outputTypes = new Type[] { typeof(TOutput) };
            }
            else if (typeof(TOutput).Name.StartsWith("Tuple"))
            {
                outputTypes = typeof(TOutput).GetGenericArguments().ToArray();
                numOutputs = outputTypes.Length;
            }
            _numOutputs = numOutputs;
            _outputTypes = outputTypes;
        }

        public void Execute(ProcessContext context)
        {
            TInput input = default(TInput);
            if ((typeof(TInput).IsPrimitive) || (typeof(TInput).Equals(typeof(String))))
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

                if ((typeof(TOutput).IsPrimitive) || (typeof(TOutput).Equals(typeof(String))))
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
                int i = 0;
            }
        }
    }

}
