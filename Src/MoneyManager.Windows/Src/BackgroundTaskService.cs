﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using MoneyManager.Foundation.Interfaces;

namespace MoneyManager.Windows
{
    public class BackgroundTaskService : IBackgroundTaskService
    {
        private Dictionary<string, string> Tasks => new Dictionary<string, string>
        {
            {"ClearTransactionBackgroundTask", "MoneyManager.Tasks.Windows"}
        };

        public async Task RegisterTasksAsync()
        {
            foreach (var kvTask in Tasks)
            {
                if (BackgroundTaskRegistration.AllTasks.Any(task => task.Value.Name == kvTask.Key))
                    break;

                await RegisterTaskAsync(kvTask.Key, kvTask.Value);
            }
        }

        private async Task RegisterTaskAsync(string taskName, string taskNamespace)
        {
            var requestAccess = await BackgroundExecutionManager.RequestAccessAsync();

            if (requestAccess == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity ||
                requestAccess == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity)
            {
                var taskBuilder = new BackgroundTaskBuilder
                {
                    Name = taskName,
                    TaskEntryPoint = string.Format("{0}.{1}", taskNamespace, taskName)
                };
                taskBuilder.SetTrigger(new TimeTrigger(15, false));

                taskBuilder.Register();
            }
        }
    }
}
