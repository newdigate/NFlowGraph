using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFlowGraph.Core
{
    public class DirectedEdge<TNode>
    {
        public TNode Antecedent { get; set; }
        public int AntecedentOutputPortNumber { get; set; }

        public TNode Subsequent { get; set; }
        public int SubsequentInputPortNumber { get; set; }
    }
}
