// <copyright file="Meat.cs" company="OpenTelemetry Authors">
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace OpenTelemetry.Tests.Stress;

public partial class Program
{
    private const int MaxCapacity = 1000;
    private static List<ILogger> loggers = new(MaxCapacity);
    private static Random random = new Random(97);

    public static void Main()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(loggerOptions =>
            {
                loggerOptions.AddGenevaLogExporter(exporterOptions =>
                {
                    exporterOptions.ConnectionString = "EtwSession=OpenTelemetry";
                    exporterOptions.PrepopulatedFields = new Dictionary<string, object>
                    {
                        ["cloud.role"] = "BusyWorker",
                        ["cloud.roleInstance"] = "CY1SCH030021417",
                        ["cloud.roleVer"] = "9.0.15289.2",
                    };

                    exporterOptions.TableNameMappings = new Dictionary<string, string>
                    {
                        ["Company.StoreA"] = "Store",
                        ["*"] = "*",
                    };
                });
            });
        });

        for (int i = 0; i < MaxCapacity; ++i)
        {
            loggers.Add(loggerFactory.CreateLogger("Company-%-Customer*Region$##" + (i + MaxCapacity).ToString()));
        }

        Stress();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void Run()
    {
        loggers[random.Next(0, MaxCapacity)].LogInformation("Hello from {storeName} {number}.", "Kyoto", 2);
    }
}
