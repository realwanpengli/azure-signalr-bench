using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bench.Common;
using Bench.Common.Config;
using Bench.RpcSlave.Worker.Operations;
using Microsoft.AspNetCore.SignalR.Client;

namespace Bench.RpcSlave.Worker
{
    class SigWorker
    {
        private WorkerToolkit _tk = new WorkerToolkit();

        public void LoadJobs(JobConfig jobConfig)
        {
            _tk.JobConfig = jobConfig;
        }

        public void LoadBenchmarkCellConfig(BenchmarkCellConfig benchmarkCellConfig)
        {
            _tk.BenchmarkCellConfig = benchmarkCellConfig;
        }

        public void LoadConnectionConfig(ConnectionConfigList connectionConfigList)
        {
            _tk.ConnectionConfigList = connectionConfigList;
        }

        public void LoadConnectionRange(Range range)
        {
            _tk.ConnectionRange = range;
        }

        public async Task<Stat.Types.State> ProcessJob(string opName)
        {
            // process operations
            //GetPipeline().ForEach(opName =>
            //{
            var newOp = opName;
            if (opName.Substring(0, 2).ToLower() == "up")
            {
                var isNumeric = int.TryParse(opName.Substring(2), out int n);
                if (isNumeric)
                {
                    // filter the following number after "up"
                    newOp = "up";
                    _tk.CurSending = _tk.CurSending + Convert.ToUInt64(opName.Substring(2));
                }
            }
            var tuple = OperationFactory.CreateOp(newOp, _tk);
            var obj = tuple.Item1;
            var type = tuple.Item2;
            dynamic op = Convert.ChangeType(obj, type);
            await op.Do(_tk);
            //});

            return _tk.State;
        }

        public List<string> GetPipeline()
        {
            Console.WriteLine($"pipeline: {_tk.JobConfig.Pipeline}");
            return _tk.JobConfig.Pipeline;
        }

        public List<Tuple<string, ulong>> GetCounters()
        {
            return _tk.Counters.GetAll();
        }

        public void UpdateState(Stat.Types.State state)
        {
            _tk.State = state;
        }

        public Stat.Types.State GetState()
        {
            return _tk.State;
        }

        public StrgList GetConnectionIds()
        {
            var ids = new StrgList();
            _tk.ConnectionIds.ForEach(id => ids.List.Add(id));
            return ids;
        }
    }
}