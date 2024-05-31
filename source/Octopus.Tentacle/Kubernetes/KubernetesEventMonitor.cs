using System;
using System.Threading;
using System.Threading.Tasks;
using Octopus.Diagnostics;
using Polly;

namespace Octopus.Tentacle.Kubernetes
{
    public interface IKubernetesEventMonitor
    {
        Task StartAsync(CancellationToken cancellationToken);
    }

    public class KubernetesEventMonitor : IKubernetesEventMonitor
    {
        readonly ISystemLog log;
        //Needs the KubernetesMetric interface

        public KubernetesEventMonitor(ISystemLog log)
        {
            this.log = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //We don't want the monitoring to ever stop
            var policy = Policy.Handle<Exception>().WaitAndRetryForeverAsync(
                    retry => TimeSpan.FromMinutes(10),
            (ex, duration) =>
            {
                log.Error(ex, "KubernetesEventMonitor: An unexpected error occured while running event caching loop, re-running in: " + duration);
            });

            await policy.ExecuteAsync(async ct => await PublishNewEvents(ct), cancellationToken);
        }

        async Task PublishNewEvents(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}