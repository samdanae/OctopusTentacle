﻿using System;
using Octopus.Platform.Deployment.Packages;

namespace Octopus.Platform.Deployment.Messages.Deploy.Steps
{
    public class StepAction
    {
        public string ActionId { get; private set; }
        public string Name { get; private set; }
        public string ActionType { get; private set; }
        public PackageMetadata Package { get; private set; }

        public StepAction(string actionId, string name, string actionType, PackageMetadata package)
        {
            ActionId = actionId;
            Name = name;
            ActionType = actionType;
            Package = package;
        }
    }
}