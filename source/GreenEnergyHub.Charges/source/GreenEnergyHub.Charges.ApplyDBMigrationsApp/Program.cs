using System;

namespace GreenEnergyHub.Charges.ApplyDBMigrationsApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = ConnectionStringFactory.GetConnectionString(args);
            var filter = EnvironmentFilter.GetFilter(args);
            var isDryRun = args.Contains("dryRun");

            // This is the new way of doing it, but for backwards compatibility, the "old way" is still default.
            // var upgrader = UpgradeFactory.GetUpgradeEngine(connectionString, filter, isDryRun);
            var upgrader = UpgradeFactory.GetUpgradeEngine(connectionString);

            var result = upgrader.PerformUpgrade();

            return ResultReporter.ReportResult(result);
        }
    }
}
