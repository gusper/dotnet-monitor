﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Diagnostics.Tools.Monitor.CollectionRules.Options.Triggers
{
    internal interface IAspNetActionPathFilters
    {
        public string[] IncludePaths { get; }

        public string[] ExcludePaths { get; }
    }
}
