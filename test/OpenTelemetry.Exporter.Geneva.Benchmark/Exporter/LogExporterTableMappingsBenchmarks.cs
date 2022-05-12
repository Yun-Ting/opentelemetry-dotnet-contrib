// <copyright file="LogExporterTableMappingsBenchmarks.cs" company="OpenTelemetry Authors">
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


|                                      Method |  size |         Mean |        Error |       StdDev |    Gen 0 | Allocated |
|-------------------------------------------- |------ |-------------:|-------------:|-------------:|---------:|----------:|
|          NoCacheVersionWhenTheRuleIsEnabled |   100 |     99.59 us |     1.949 us |     2.796 us |   3.1738 |     25 KB |
| CacheVersionWhenTheRuleIsEnabledUniqueNames |   100 |     95.53 us |     1.886 us |     3.098 us |   3.1738 |     25 KB |
|    CacheVersionWhenTheRuleIsEnabledhitCache |   100 |     95.10 us |     1.724 us |     2.180 us |   3.1738 |     25 KB |
|          NoCacheVersionWhenTheRuleIsEnabled |  1000 |    994.11 us |    19.304 us |    22.231 us |  32.2266 |    250 KB |
| CacheVersionWhenTheRuleIsEnabledUniqueNames |  1000 |    945.03 us |    18.442 us |    25.244 us |  32.2266 |    250 KB |
|    CacheVersionWhenTheRuleIsEnabledhitCache |  1000 |  1,124.59 us |    22.334 us |    35.424 us |  31.2500 |    250 KB |
|          NoCacheVersionWhenTheRuleIsEnabled | 10000 | 11,574.85 us |   227.957 us |   312.030 us | 312.5000 |  2,500 KB |
| CacheVersionWhenTheRuleIsEnabledUniqueNames | 10000 | 64,148.69 us | 1,267.462 us | 2,317.626 us | 250.0000 |  2,500 KB |
|    CacheVersionWhenTheRuleIsEnabledhitCache | 10000 |  9,513.01 us |   189.406 us |   360.365 us | 312.5000 |  2,500 KB |

*/

namespace OpenTelemetry.Exporter.Geneva.Benchmark
{
    [MemoryDiagnoser]
    public class LogExporterTableMappingsBenchmarks
    {
        private static readonly int maxCapactiy = 10000;
        private readonly ILoggerFactory loggerFactoryNoCache;
        private readonly ILoggerFactory loggerFactoryWithCache;
        private readonly List<ILogger> uniqueLoggersConfiguredNoCache = new(maxCapactiy);
        private readonly List<ILogger> uniqueLoggersConfiguredWithCache = new(maxCapactiy);
        private readonly List<ILogger> identicalLoggersConfiguredWithCache = new(maxCapactiy);

        public LogExporterTableMappingsBenchmarks()
        {
            this.loggerFactoryNoCache = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(loggerOptions =>
                {
                    loggerOptions.AddGenevaLogExporter(exporterOptions =>
                    {
                        exporterOptions.ConnectionString = "EtwSession=OpenTelemetry";
                        exporterOptions.TableNameMappings = new Dictionary<string, string>
                        {
                            ["*"] = "*",
                        };
                    });
                });
            });

            for (int i = 0; i < maxCapactiy; ++i)
            {
                this.uniqueLoggersConfiguredNoCache.Add(this.loggerFactoryNoCache.CreateLogger("Company-%-Customer*Region$##" + (i + maxCapactiy).ToString()));
            }

            this.loggerFactoryWithCache = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(loggerOptions =>
                {
                    loggerOptions.AddGenevaLogExporter(exporterOptions =>
                    {
                        exporterOptions.ConnectionString = "EtwSession=OpenTelemetry";
                        exporterOptions.TableNameMappings = new Dictionary<string, string>
                        {
                            ["*"] = "*",
                            ["cacheEnabled"] = "true",
                        };
                    });
                });
            });

            for (int i = 0; i < maxCapactiy; ++i)
            {
                this.uniqueLoggersConfiguredWithCache.Add(this.loggerFactoryWithCache.CreateLogger("Company-%-Customer*Region$##" + (i + maxCapactiy).ToString()));
            }

            for (int i = 0; i < maxCapactiy; ++i)
            {
                this.identicalLoggersConfiguredWithCache.Add(this.loggerFactoryWithCache.CreateLogger("Company-%-Customer*Region$##100000"));
            }
        }

        [Benchmark]
        [Arguments(100)]
        [Arguments(1000)]
        [Arguments(10000)]
        public void NoCacheVersionWhenTheRuleIsEnabled(int size)
        {
            for (int i = 0; i < size; ++i)
            {
                this.uniqueLoggersConfiguredNoCache[i].LogInformation("Hello from {storeName} {number}.", "Kyoto", 2);
            }
        }

        [Benchmark]
        [Arguments(100)]
        [Arguments(1000)]
        [Arguments(10000)]
        public void CacheVersionWhenTheRuleIsEnabledUniqueNames(int size)
        {
            for (int i = 0; i < size; ++i)
            {
                this.uniqueLoggersConfiguredWithCache[i].LogInformation("Hello from {storeName} {number}.", "Kyoto", 2);
            }
        }

        [Benchmark]
        [Arguments(100)]
        [Arguments(1000)]
        [Arguments(10000)]
        public void CacheVersionWhenTheRuleIsEnabledhitCache(int size)
        {
            for (int i = 0; i < size; ++i)
            {
                this.identicalLoggersConfiguredWithCache[i].LogInformation("Hello from {storeName} {number}.", "Kyoto", 2);
            }
        }
    }
}
