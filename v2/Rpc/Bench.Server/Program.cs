using Bench.Common;
using CommandLine;
using Grpc.Core;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Bench.RpcSlave
{
    class Program
    {
        public static async Task Main (string[] args)
        {
            Console.WriteLine("MachineName: {0}", Environment.MachineName);
            var argsOption = new ArgsOption();
            var result = Parser.Default.ParseArguments<ArgsOption>(args)
                .WithParsed(options => argsOption = options)
                .WithNotParsed(error => { });
            Grpc.Core.Server server = new Grpc.Core.Server(new ChannelOption[]
            {
                // For Group, the received message size is very large, so here set 8000k
                new ChannelOption(ChannelOptions.MaxReceiveMessageLength, 8192000)
            })
            {
                Services = { RpcService.BindService(new RpcServiceImpl()) },
                Ports = { new ServerPort(argsOption.DnsName, argsOption.RpcPort, ServerCredentials.Insecure) }
            };
            server.Start();
            Console.WriteLine($"Server [{argsOption.DnsName}:{argsOption.RpcPort}] started");

            var pid = Process.GetCurrentProcess().Id;
            if (argsOption.PidFile != null)
            {
                Util.SaveContentToFile(argsOption.PidFile, Convert.ToString(pid), false);
            }
            await Task.Delay(Timeout.Infinite);
        }
    }
}