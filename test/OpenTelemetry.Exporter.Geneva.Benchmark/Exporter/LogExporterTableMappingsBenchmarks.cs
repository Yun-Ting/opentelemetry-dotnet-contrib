﻿// <copyright file="LogExporterTableMappingsBenchmarks.cs" company="OpenTelemetry Authors">
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
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;

/*
// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1706 (21H2)
Intel Xeon CPU E5-1650 v4 3.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.203
  [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
  DefaultJob : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT


|                                              Method |         Mean |      Error |     StdDev |   Gen 0 | Allocated |
|---------------------------------------------------- |-------------:|-----------:|-----------:|--------:|----------:|
|     CategoryTableNameMappingsDefinedInConfiguration |     1.135 us |  0.0226 us |  0.0302 us |  0.0324 |     256 B |
|        PassThruTableNameMappingsWhenTheRuleIsEnbled |     1.199 us |  0.0239 us |  0.0378 us |  0.0324 |     256 B |
| PassThruTableNameMappingsWhenTheRuleIsEnbledNoCache | 2,549.623 us | 49.8186 us | 80.4478 us | 62.5000 | 512,003 B |
*/

namespace OpenTelemetry.Exporter.Geneva.Benchmark
{
    [MemoryDiagnoser]
    public class LogExporterTableMappingsBenchmarks
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger storeALogger;
        private readonly ILogger storeBLogger;

        private static readonly int maxCapacity = 1000;
        private static int sequenceSize = maxCapacity * 2;
        private readonly List<ILogger> loggers = new(maxCapacity);
        private Random random = new Random(97);

        public LogExporterTableMappingsBenchmarks()
        {
            this.loggerFactory = LoggerFactory.Create(builder =>
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

            this.storeALogger = this.loggerFactory.CreateLogger("Company.StoreA");
            this.storeBLogger = this.loggerFactory.CreateLogger("Company.StoreB");

            for (int i = 0; i < maxCapacity; ++i)
            {
                this.loggers.Add(this.loggerFactory.CreateLogger("Company-%-Customer*Region$##" + (i + maxCapacity).ToString()));
            }
        }

        [Benchmark]
        public void CategoryTableNameMappingsDefinedInConfiguration()
        {
            this.storeALogger.LogInformation("Hello from {storeName} {number}.", "Kyoto", 2);
        }

        [Benchmark]
        public void PassThruTableNameMappingsWhenTheRuleIsEnbled()
        {
            this.storeBLogger.LogInformation("Hello from {storeName} {number}.", "Kyoto", 2);
        }

        [Benchmark]
        public void PassThruTableNameMappingsWhenTheRuleIsEnbledWithCache()
        {
            this.loggers[this.random.Next(0, maxCapacity)].LogInformation("Hello from {storeName} {number}.", "Kyoto", 2);
        }
    }
}
