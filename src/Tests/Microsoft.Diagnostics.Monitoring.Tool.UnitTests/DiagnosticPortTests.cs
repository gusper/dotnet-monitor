﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Monitoring.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Diagnostics.Monitoring.Tool.UnitTests
{
    public sealed class DiagnosticPortTests
    {
        private ITestOutputHelper _outputHelper;

        public DiagnosticPortTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task SimplifiedListenConfiguration()
        {
            await TestHostHelper.CreateCollectionRulesHost(_outputHelper, rootOptions => { }, host =>
            {
                IOptionsMonitor<DiagnosticPortOptions> options = host.Services.GetService<IOptionsMonitor<DiagnosticPortOptions>>();

                Assert.Equal(DiagnosticPortTestsConstants.SimplifiedDiagnosticPort, options.CurrentValue.EndpointName);
                Assert.Equal(DiagnosticPortConnectionMode.Listen, options.CurrentValue.ConnectionMode);

            }, overrideSource: DiagnosticPortTestsConstants.SimplifiedListen_EnvironmentVariables);
        }

        [Fact]
        public async Task FullListenConfiguration()
        {
            await TestHostHelper.CreateCollectionRulesHost(_outputHelper, rootOptions => { }, host =>
            {
                IOptionsMonitor<DiagnosticPortOptions> options = host.Services.GetService<IOptionsMonitor<DiagnosticPortOptions>>();

                Assert.Equal(DiagnosticPortTestsConstants.FullDiagnosticPort, options.CurrentValue.EndpointName);
                Assert.Equal(DiagnosticPortConnectionMode.Listen, options.CurrentValue.ConnectionMode);

            }, overrideSource: DiagnosticPortTestsConstants.FullListen_EnvironmentVariables);
        }

        [Fact]
        public async Task ConnectConfiguration()
        {
            await TestHostHelper.CreateCollectionRulesHost(_outputHelper, rootOptions => { }, host =>
            {
                IOptionsMonitor<DiagnosticPortOptions> options = host.Services.GetService<IOptionsMonitor<DiagnosticPortOptions>>();

                Assert.Equal(DiagnosticPortConnectionMode.Connect, options.CurrentValue.ConnectionMode);

            }, overrideSource: DiagnosticPortTestsConstants.Connect_EnvironmentVariables);
        }

        [Fact]
        public async Task SimplifiedListenOverrideConfiguration()
        {
            await TestHostHelper.CreateCollectionRulesHost(_outputHelper, rootOptions => { }, host =>
            {
                IOptionsMonitor<DiagnosticPortOptions> options = host.Services.GetService<IOptionsMonitor<DiagnosticPortOptions>>();

                Assert.Equal(DiagnosticPortTestsConstants.SimplifiedDiagnosticPort, options.CurrentValue.EndpointName);
                Assert.Equal(DiagnosticPortConnectionMode.Listen, options.CurrentValue.ConnectionMode);

            }, overrideSource: DiagnosticPortTestsConstants.AllListen_EnvironmentVariables);
        }
    }
}