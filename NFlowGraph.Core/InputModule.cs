using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFlowGraph.Core
{

    public class InputModule<TOutput> : Module<bool, TOutput>
    {
        public InputModule(Func<TOutput> process)
            : base(input => process())
        {
        }
    }
}
