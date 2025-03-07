﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Monitoring.TestCommon;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.UnitTestApp.Scenarios
{
    /// <summary>
    /// Synchronously spins until it receives the Continue command.
    /// </summary>
    internal class SpinWaitScenario
    {
        public static Command Command()
        {
            Command command = new(TestAppScenarios.SpinWait.Name);
            command.SetHandler((Func<CancellationToken, Task<int>>)ExecuteAsync);
            return command;
        }

        public static Task<int> ExecuteAsync(CancellationToken token)
        {
            return ScenarioHelpers.RunScenarioAsync(async logger =>
            {
                await ScenarioHelpers.WaitForCommandAsync(TestAppScenarios.SpinWait.Commands.StartSpin, logger);

                Task continueTask = Task.Run(() => ScenarioHelpers.WaitForCommandAsync(TestAppScenarios.SpinWait.Commands.StopSpin, logger));

                while (!continueTask.IsCompleted)
                {
                    Thread.SpinWait(1_000_000);
                }

                return 0;
            }, token);
        }
    }
}
