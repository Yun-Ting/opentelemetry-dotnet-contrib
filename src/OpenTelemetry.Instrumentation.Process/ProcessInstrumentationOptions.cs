// <copyright file="ProcessInstrumentationOptions.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace OpenTelemetry.Instrumentation.Process;

/// <summary>
/// Options to define the process metrics.
/// </summary>
public class ProcessInstrumentationOptions
{
    /// <summary>
    /// Gets the flag indicating whether Cpu time should be further broken down by its states.
    /// The Cpu state could be one of the following type: system, user, or wait.
    /// </summary>
    public bool? CpuStatesEnabled { get; } = false;
}
