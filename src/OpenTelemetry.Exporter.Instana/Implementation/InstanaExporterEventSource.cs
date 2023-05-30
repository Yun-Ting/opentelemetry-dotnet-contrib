// <copyright file="InstanaExporterEventSource.cs" company="OpenTelemetry Authors">
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

using System;
using System.Diagnostics.Tracing;
using OpenTelemetry.Internal;

namespace OpenTelemetry.Exporter.Instana.Implementation;

[EventSource(Name = "OpenTelemetry-Exporter-Instana")]
internal sealed class InstanaExporterEventSource : EventSource
{
    public static InstanaExporterEventSource Log = new();

    private InstanaExporterEventSource()
    {
    }

    [NonEvent]
    public void FailedExport(Exception ex)
    {
        if (this.IsEnabled(EventLevel.Error, EventKeywords.All))
        {
            this.FailedExport(ex.ToInvariantString());
        }
    }

    [Event(1, Message = "Failed to send spans: '{0}'", Level = EventLevel.Error)]
    public void FailedExport(string exception)
    {
        this.WriteEvent(1, exception);
    }
}