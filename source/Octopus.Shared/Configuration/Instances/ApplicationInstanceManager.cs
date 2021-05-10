﻿using System;
using System.IO;
using Octopus.Diagnostics;
using Octopus.Shared.Util;

namespace Octopus.Shared.Configuration.Instances
{
    class ApplicationInstanceManager : IApplicationInstanceManager
    {
        readonly IOctopusFileSystem fileSystem;
        readonly ISystemLog log;
        readonly IApplicationInstanceStore instanceStore;
        readonly IApplicationInstanceSelector instanceSelector;
        readonly StartUpInstanceRequest startUpInstanceRequest;

        public ApplicationInstanceManager(
            StartUpInstanceRequest startUpInstanceRequest,
            IOctopusFileSystem fileSystem,
            ISystemLog log,
            IApplicationInstanceStore instanceStore,
            IApplicationInstanceSelector instanceSelector)
        {
            this.startUpInstanceRequest = startUpInstanceRequest;
            this.fileSystem = fileSystem;
            this.log = log;
            this.instanceStore = instanceStore;
            this.instanceSelector = instanceSelector;
        }

        public void CreateDefaultInstance(string configurationFile, string? homeDirectory = null)
        {
            CreateInstance(ApplicationInstanceRecord.GetDefaultInstance(startUpInstanceRequest.ApplicationName), configurationFile, homeDirectory);
        }

        public void CreateInstance(string instanceName, string configurationFile, string? homeDirectory = null)
        {
            (configurationFile, homeDirectory) = ValidateConfigDirectory(configurationFile, homeDirectory);
            EnsureConfigurationFileExists(configurationFile, homeDirectory);
            WriteHomeDirectory(configurationFile, homeDirectory);
            RegisterInstanceInIndex(instanceName, configurationFile);
        }

        public void DeleteInstance(string instanceName)
        {
            instanceStore.DeleteInstance(instanceName);
        }

        void RegisterInstanceInIndex(string instanceName, string configurationFile)
        {
            // If completely dynamic (no instance or configFile provided) then dont bother setting anything
            if (startUpInstanceRequest is StartUpDynamicInstanceRequest)
                return;
            
            var instance = new ApplicationInstanceRecord(instanceName, configurationFile);
            instanceStore.RegisterInstance(instance);
        }

        void WriteHomeDirectory(string configurationFile, string homeDirectory)
        {
            var keyValueStore = new XmlFileKeyValueStore(fileSystem, configurationFile);
            var homeConfig = new WritableHomeConfiguration(startUpInstanceRequest.ApplicationName, keyValueStore, instanceSelector);
            log.Info($"Setting home directory to: {homeDirectory}");
            homeConfig.SetHomeDirectory(homeDirectory);
        }

        void EnsureConfigurationFileExists(string configurationFile, string homeDirectory)
        {
            // Ensure we can write configuration file
            string configurationDirectory = Path.GetDirectoryName(configurationFile) ?? homeDirectory;
            fileSystem.EnsureDirectoryExists(configurationDirectory);
            if (!fileSystem.FileExists(configurationFile))
            {
                log.Info("Creating empty configuration file: " + configurationFile);
                fileSystem.OverwriteFile(configurationFile, @"<?xml version='1.0' encoding='UTF-8' ?><octopus-settings></octopus-settings>");
            }
            else
            {
                log.Info($"Configuration file at {configurationFile} already exists.");
            }
        }

        (string ConfigurationFilePath, string HomeDirectory) ValidateConfigDirectory(string? configurationFile, string? homeDirectory)
        {
            // If home directory is not provided, we should try use the config file path if provided, otherwise fallback to cwd
            homeDirectory ??= (string.IsNullOrEmpty(configurationFile) ?
                "." :
                (Path.GetDirectoryName(PathHelper.ResolveRelativeWorkingDirectoryFilePath(configurationFile)) ?? "."));
            
            // Current "Indexed" installs require configuration file to be provided.
            // We can therefore assume that if its missing, it will end up being created in the cwd
            if (string.IsNullOrEmpty(configurationFile))
            {
                configurationFile = $"{startUpInstanceRequest.ApplicationName}.config";
            }
            
            // If configuration File isn't rooted, root it to the home directory 
            if (!Path.IsPathRooted(configurationFile))
            {
                configurationFile = Path.Combine(homeDirectory, configurationFile);
            }
            
            // get the configurationPath for writing, even if it is a relative path
            configurationFile = PathHelper.ResolveRelativeWorkingDirectoryFilePath(configurationFile);
            
            return (configurationFile, homeDirectory);
        }
    }
}