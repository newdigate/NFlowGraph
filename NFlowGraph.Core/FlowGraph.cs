using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFlowGraph.Core
{
    public class FlowGraph
    {
        public readonly ISet<IModule> _modules = new HashSet<IModule>();
        public readonly ISet<DirectedEdge<IModule>> _edges = new HashSet<DirectedEdge<IModule>>();
        
        private IEnumerable<IModule> _topologicalOrder = null;
        private Dictionary<IModule, IEnumerable<IModule>> _antecedentsPerModule = new Dictionary<IModule, IEnumerable<IModule>>();
        private bool _topologicalOrderDirty = true;

        public void Connect(IModule outputModule, int outputPort, IModule inputModule, int inputPort)
        {
            if (outputPort >= outputModule.NumOutputs)
                throw new InvalidOperationException();

            if (inputPort >= inputModule.NumInputs)
                throw new InvalidOperationException();

            if (outputModule.OutputTypes[outputPort] != inputModule.InputTypes[inputPort])
                throw new InvalidOperationException();

            if (!_modules.Contains(outputModule))
                _modules.Add(outputModule);

            if (!_modules.Contains(inputModule))
                _modules.Add(inputModule);

            bool edgeAlreadyExists = (from x in _edges
                                      where x.Antecedent.Equals(outputModule) &&
                                            x.AntecedentOutputPortNumber == outputPort &&
                                            x.Subsequent.Equals(inputModule) &&
                                            x.SubsequentInputPortNumber == inputPort
                                      select true).FirstOrDefault();
            if (!edgeAlreadyExists)
            {
                _edges.Add(new DirectedEdge<IModule>()
                {
                    Antecedent = outputModule,
                    AntecedentOutputPortNumber = outputPort,
                    Subsequent = inputModule,
                    SubsequentInputPortNumber = inputPort
                });
                _topologicalOrderDirty = true;
                _antecedentsPerModule.Clear();
            }

        }

        public void Process()
        {
            //IEnumerable<IModule> outputModules = from m in _modules
            //                                     join e in _edges
            //                                         on m equals e.Antecedent into g
            //                                     from f in g.DefaultIfEmpty()
            //                                     where f == null
            //                                     select m;
            if (_topologicalOrderDirty)
                _topologicalOrder = _modules.TopologicalSort(m =>  _edges.Where(e => e.Subsequent.Equals(m)).Select(e => e.Antecedent)).ToList();

            Dictionary<IModule, Task<ProcessContext>> h = new Dictionary<IModule, Task<ProcessContext>>();
            foreach (IModule m in _topologicalOrder) {
                h.Add(m, CreateTask(m, h));
            }

            int i = 1;
            //IEnumerable<Task<ProcessContext>> outputModuleTasks = outputModules.Select(m => h[m]);
            foreach (Task<ProcessContext> outputModuleTask in h.Values)
            {
                outputModuleTask.Wait();
            }
            //Task.Factory.ContinueWhenAll(outputModuleTasks.ToArray(), t2 => { Console.Write("Done"); });                                     
        }

        public Task<ProcessContext> CreateTask(IModule module, Dictionary<IModule, Task<ProcessContext>> antecedentTasks)
        {
            if (!_antecedentsPerModule.ContainsKey(module))
                _antecedentsPerModule[module] = _edges.Where(e => e.Subsequent.Equals(module)).Select(e => e.Antecedent);
            IEnumerable<Task> dependents = dependents = _antecedentsPerModule[module].Select(m => antecedentTasks[m]);
            long numDependents = dependents.Count();
            if (numDependents > 0)
            {
                var context = GetProcessContext(module, m => antecedentTasks[m].Result);
                return Task.Factory.ContinueWhenAll(dependents.ToArray(), t => {
                    module.Execute(context);
                    return context;
                });
            }
            {
                var context = GetProcessContext(module, m => antecedentTasks[m].Result );
                return Task.Factory.StartNew(() => {
                    module.Execute(context);
                    return context;
                });
            }
        }

        private ProcessContext GetProcessContext(IModule module, Func<IModule, ProcessContext> fnProcessContentForModule)
        {
            object[] inputs = new object[module.NumInputs];
            object[] outputs = new object[module.NumOutputs];
            for (int i = 0; i < module.NumInputs; i++) {

                DirectedEdge<IModule> edge = _edges.Where(e => e.Subsequent.Equals(module) && e.SubsequentInputPortNumber == i).FirstOrDefault();
                bool isInputConnected = edge != null;
                if (isInputConnected) {
                    IModule antecedent = edge.Antecedent;
                    ProcessContext antecedentContext = fnProcessContentForModule(antecedent);
                    inputs[i] = antecedentContext.Outputs[edge.AntecedentOutputPortNumber];
                }

            }
            return new ProcessContext() { Inputs = inputs, Outputs = outputs };
        }
    }
}
