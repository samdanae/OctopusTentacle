using System;
using Octopus.Server.Extensibility.HostServices.Configuration;

namespace Octopus.Shared.Configuration
{
    /// <summary>
    /// Machine-wide Octopus configuration settings.
    /// </summary>
    public interface IDeploymentProcessConfiguration : IModifiableConfiguration
    {
        /// <summary>
        /// Gets the directory that Octopus Server should use to store downloaded packages.
        /// </summary>
        string CacheDirectory { get; }

        int DaysToCachePackages { get; set; }
        int MaxConcurrentTasks { get; set; }
    }
}