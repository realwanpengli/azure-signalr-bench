using System;
using System.Collections.Generic;
using System.Text;

namespace JenkinsScript
{
    public class AgentConfig
    {
        public string Master { get; set; }
        public List<string> Slaves { get; set; }
        public string AppServer { get; set; }
        public int RpcPort { get; set; }
        public int SshPort { get; set; }
        public string User { get; set; }
        public string Repo { get; set; }
        public string Password { get; set; }
        public string Prefix { get; set; }
        public string Location { get; set; }

        // slave vm
        public string SlaveVmSize { get; set; }
        public int SlaveVmCount { get; set; }

        // app server
        public string AppSvrVmSize { get; set; }
        public int AppSvrVmCount { get; set; }
        // service
        public string SvcVmSize { get; set; }
        public int SvcVmCount { get; set; }
        // bench
        public string BenchVmSize { get; set; }

        public string Ssh { get; set; }

        public string ImageId { get; set; }

    }
}