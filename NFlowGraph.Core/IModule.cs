using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFlowGraph.Core
{
    public interface IModule
    {
        int NumInputs { get; }
        int NumOutputs { get; }
        void Execute(ProcessContext context);
    }

}
