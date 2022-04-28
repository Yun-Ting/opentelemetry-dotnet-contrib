// <copyright file="Program.cs" company="OpenTelemetry Authors">
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

using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter.Geneva;
using OpenTelemetry.Logs;

public class Program
{
    public static void Main()
    {
        var userInitializedCategoryToTableNameMappings = new Dictionary<string, string>
        {
            ["Company.Store"] = "Store",
            ["Company.Orders"] = "Orders",
            ["*"] = "*",
        };

        var expectedCategoryToTableNameMappings = new Dictionary<string, string>
        {
            // The category name must match "^[A-Z][a-zA-Z0-9]*$"; any character that is not allowed will be removed.
            ["Company.Customer"] = "CompanyCustomer",
            ["Company-%-Customer*Region$##"] = "CompanyCustomerRegion",

            // If the first character in the resulting string is lower-case ALPHA,
            // it will be converted to the corresponding upper -case.
            ["company.Customer"] = "CompanyCustomer",

            // After removing not allowed characters,
            // if the resulting string is still an illegal Part B name, the data will get dropped on the floor.
            ["$&-.$~!!"] = null,

            // If the resulting string is longer than 32 characters, only the first 32 characters will be taken.
            ["Company.Customer.rsLiheLClHJasBOvM.XI4uW7iop6ghvwBzahfs"] = "CompanyCustomerrsLiheLClHJasBOvM",

            // The data will be dropped on the floor as the exporter cannot deduce a valid table name.
            ["1.2"] = null,
        };

        var logRecordList = new List<LogRecord>();
        var exporterOptions = new GenevaExporterOptions
        {
            TableNameMappings = userInitializedCategoryToTableNameMappings,
            ConnectionString = "EtwSession=OpenTelemetry",
        };

        using var loggerFactory = LoggerFactory.Create(builder => builder
        .AddOpenTelemetry(options =>
        {
            options.AddInMemoryExporter(logRecordList);
        })
        .AddFilter("*", LogLevel.Trace)); // Enable all LogLevels

        // Create a test exporter to get MessagePack byte data to validate if the data was serialized correctly.
        using var exporter = new GenevaLogExporter(exporterOptions);

        ILogger passThruTableMappingsLogger, userInitializedTableMappingsLogger;

        // Verify that the category table mappings specified by the users in the Geneva Configuration are mapped correctly.
        foreach (var mapping in userInitializedCategoryToTableNameMappings)
        {
            if (mapping.Key != "*")
            {
                userInitializedTableMappingsLogger = loggerFactory.CreateLogger(mapping.Key);
                userInitializedTableMappingsLogger.LogInformation("This information does not matter.");
                int i = exporter.SerializeLogRecord(logRecordList[0]);
            }
        }

        // Verify that when the "*" = "*" were enabled, the correct table names were being deduced following the set of rules.
        foreach (var mapping in expectedCategoryToTableNameMappings)
        {
            passThruTableMappingsLogger = loggerFactory.CreateLogger(mapping.Key);
            passThruTableMappingsLogger.LogInformation("This information does not matter.");
            int i = exporter.SerializeLogRecord(logRecordList[0]);
        }
    }
}
