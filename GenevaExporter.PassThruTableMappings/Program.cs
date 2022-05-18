using Microsoft.Extensions.Logging;

namespace GenevaExporter.PassThruTableMappings // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                // config to write the logs onto "Store.txt"
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

            Store s = new Store(loggerFactory);
            Item item = new Item("item");
            s.sell(item);
        }

        public class Store
        {
            ILogger storeLogger;
            public Store(ILoggerFactory factory)
            {
                storeLogger = factory.CreateLogger("Company.Store");
            }

            public void sell(Item item)
            {
                storeLogger.LogInformation("selling {item} {name}.", "item", item.name);
            }
        }

        public class Item
        {
            public string name { get; set; }
            public Item(string name)
            {
                this.name = name;
            }
        }
    }
}
