using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFlowGraph.Core
{
    public class OutputModule<TInput> : Module<TInput, bool>
    {

        public OutputModule(Action<TInput> process)
            : base(input => { process(input); return true; })
        {
        }
    }

}
