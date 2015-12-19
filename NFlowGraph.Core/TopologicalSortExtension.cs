using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFlowGraph.Core
{
    public static class TopologicalSortExtension
    {
        public static IEnumerable<T> TopologicalSort<T>(this IEnumerable<T> nodes, Func<T, IEnumerable<T>> connected)
        {
            var dependenciesPerModule = nodes.ToDictionary(node => node, node => new HashSet<T>(connected(node)));
            while (dependenciesPerModule.Count > 0)
            {
                var elem = dependenciesPerModule.FirstOrDefault(x => x.Value.Count == 0);
                if (elem.Key == null)
                {
                    throw new ArgumentException("Cyclic connections are not allowed");
                }
                dependenciesPerModule.Remove(elem.Key);
                foreach (var selem in dependenciesPerModule)
                {
                    selem.Value.Remove(elem.Key);
                }
                yield return elem.Key;
            }
        }
    }
}
