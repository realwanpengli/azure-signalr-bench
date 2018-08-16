﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bench.Common;
using Bench.RpcSlave.Worker.Savers;
using Bench.RpcSlave.Worker.StartTimeOffsetGenerator;
using Microsoft.AspNetCore.SignalR.Client;

namespace Bench.RpcSlave.Worker.Operations
{
    class JoinLeaveGroupOp : BaseOp
    {
        private WorkerToolkit _tk;

        public JoinLeaveGroupOp()
        {
            var opName = GetType().Name;
            Util.Log(opName.Substring(0, opName.Length - 2) + " Operation Started.");
        }
        public async Task Do(WorkerToolkit tk)
        {
            var debug = Environment.GetEnvironmentVariable("debug") == "debug" ? true : false;

            var waitTime = 5 * 1000;
            if (!debug) Console.WriteLine($"wait time: {waitTime / 1000}s");
            if (!debug) await Task.Delay(waitTime);

            _tk = tk;
            _tk.State = Stat.Types.State.SendReady;

            // setup
            Setup();
            if (!debug) await Task.Delay(5000);

            _tk.State = Stat.Types.State.SendRunning;
            if (!debug) await Task.Delay(5000);

            // send message
            await JoinLeaveGroup();

            _tk.State = Stat.Types.State.SendComplete;
            Util.Log($"Sending Complete");

        }

        protected void Setup()
        {

            if (!_tk.Init.ContainsKey(_tk.BenchmarkCellConfig.Step))
            {
                SetCallbacks();
                _tk.Init[_tk.BenchmarkCellConfig.Step] = true;
            }

        }

        protected async Task JoinLeaveGroup()
        {
            // var startTimeOffsetGenerator = new RandomGenerator(new LocalFileSaver());

            var sw = new Stopwatch();
            sw.Start();
            var tasks = new List<Task>();
            for (var i = _tk.ConnectionRange.Begin; i < _tk.ConnectionRange.End; i++)
            {
                var ind = i;
                tasks.Add(
                    Task.Run(async() =>
                    {
                        var groupNameList = _tk.BenchmarkCellConfig.GroupNameList[ind].Split(";");
                        for (var j = 0; j < groupNameList.Length; j++)
                        {
                            try
                            {
                                // await Task.Delay(startTimeOffsetGenerator.Delay(TimeSpan.FromSeconds(20)));
                                await _tk.Connections[ind - _tk.ConnectionRange.Begin].SendAsync(_tk.BenchmarkCellConfig.Step, groupNameList[j], "perf");
                            }
                            catch (Exception ex)
                            {
                                Util.Log($"{_tk.BenchmarkCellConfig.Step} failed: {ex}");
                                if (_tk.BenchmarkCellConfig.Step.Contains("join", StringComparison.OrdinalIgnoreCase))
                                    _tk.Counters.IncreaseJoinGroupFail();
                                else
                                    _tk.Counters.IncreaseLeaveGroupFail();
                            }
                        }
                    })
                );
            }
            await Task.WhenAll(tasks);
            sw.Stop();
            Util.Log($"{_tk.BenchmarkCellConfig.Step} time : {sw.Elapsed.TotalMilliseconds} ms");
        }

        protected void SetCallbacks()
        {
            Util.Log($"step: {_tk.BenchmarkCellConfig.Step}");
            for (int i = _tk.ConnectionRange.Begin; i < _tk.ConnectionRange.End; i++)
            {
                var ind = i;
                var callbackName = _tk.BenchmarkCellConfig.Step.First().ToString().ToUpper() + _tk.BenchmarkCellConfig.Step.Substring(1);
                _tk.Connections[i - _tk.ConnectionRange.Begin].On(callbackName,
                    (string thisId, string message) =>
                    {
                        if (_tk.BenchmarkCellConfig.Step.Contains("join"))
                        {
                            _tk.Counters.IncreaseJoinGroupSuccess();
                        }
                        else
                        {
                            _tk.Counters.IncreaseLeaveGroupSuccess();
                        }
                    });

            }
        }

    }
}