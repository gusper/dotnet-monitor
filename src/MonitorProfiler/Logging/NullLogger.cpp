// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "NullLogger.h"

std::shared_ptr<NullLogger> NullLogger::Instance = std::make_shared<NullLogger>();
