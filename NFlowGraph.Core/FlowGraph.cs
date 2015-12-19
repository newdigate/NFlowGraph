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

        public void Connect(IModule outputModule, int outputPort, IModule inputModule, int inputPort)
        {
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
            }

        }

        public void Process()
        {
            IEnumerable<IModule> outputModules = from m in _modules
                                                 join e in _edges
                                                     on m equals e.Antecedent into g
                                                 from f in g.DefaultIfEmpty()
                                                 where f == null
                                                 select m;
            IEnumerable<IModule> t = _modules.TopologicalSort(m =>
            {
                var x = _edges.Where(e => e.Subsequent.Equals(m)).Select(e => e.Antecedent);
                return x;
            });

            Dictionary<IModule, Task<ProcessContext>> h = new Dictionary<IModule, Task<ProcessContext>>();
            foreach (IModule m in t)
            {
                h.Add(m, CreateTask(m, h));
            }

            int i = 1;
            IEnumerable<Task<ProcessContext>> outputModuleTasks = outputModules.Select(m => h[m]);
            foreach (Task<ProcessContext> outputModuleTask in outputModuleTasks)
            {
                outputModuleTask.Wait();
            }
            //Task.Factory.ContinueWhenAll(outputModuleTasks.ToArray(), t2 => { Console.Write("Done"); });                                     
        }

        public Task<ProcessContext> CreateTask(IModule module, Dictionary<IModule, Task<ProcessContext>> antecidentTasks)
        {

            IEnumerable<Task> dependents = _edges.Where(e => e.Subsequent.Equals(module)).Select(e => e.Antecedent).Select(m => antecidentTasks[m]);
            long numDependents = dependents.Count();
            if (numDependents > 0)
            {
                return Task.Factory.ContinueWhenAll(dependents.ToArray(), t =>
                {
                    var context = GetProcessContext(module, m => antecidentTasks[m].Result);
                    module.Execute(context);
                    return context;
                });
            }

            return Task.Factory.StartNew(() =>
            {
                var context = GetProcessContext(module, m => antecidentTasks[m].Result);
                module.Execute(context);
                return context;
            });
        }

        private ProcessContext GetProcessContext(IModule module, Func<IModule, ProcessContext> fnProcessContentForModule)
        {
            object[] inputs = new object[module.NumInputs];
            object[] outputs = new object[module.NumOutputs];
            for (int i = 0; i < module.NumInputs; i++)
            {

                DirectedEdge<IModule> edge = _edges.Where(e => e.Subsequent.Equals(module) && e.SubsequentInputPortNumber == i).FirstOrDefault();
                bool isInputConnected = edge != null;
                if (isInputConnected)
                {
                    IModule antecedent = edge.Antecedent;
                    ProcessContext antecedentContext = fnProcessContentForModule(antecedent);
                    inputs[i] = antecedentContext.Outputs[edge.AntecedentOutputPortNumber];
                }

            }
            return new ProcessContext() { Inputs = inputs, Outputs = outputs };
        }
    }
}
