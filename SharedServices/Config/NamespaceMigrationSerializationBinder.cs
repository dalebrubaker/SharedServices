using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace BruSoftware.SharedServices.Config;

/// <summary>
/// Thanks to https://stackoverflow.com/questions/9908913/handling-namespace-changes-with-typenamehandling-all
/// </summary>
public class NamespaceMigrationSerializationBinder : DefaultSerializationBinder
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    private static readonly HashSet<INamespaceMigration> s_migrations = new();

    public void Add(INamespaceMigration migration)
    {
        s_migrations.Add(migration);
    }

    public override Type BindToType(string assemblyName, string typeName)
    {
        try
        {
            assemblyName = assemblyName.Replace("BruTrader18", "BruSoftware");
            assemblyName = assemblyName.Replace("BruTrader19", "BruSoftware");
            assemblyName = assemblyName.Replace("BruTrader20", "BruSoftware");
            typeName = typeName.Replace("BruTrader18", "BruSoftware");
            typeName = typeName.Replace("BruTrader19", "BruSoftware");
            typeName = typeName.Replace("BruTrader20", "BruSoftware");
            typeName = typeName.Replace("NinjaTrader.NinjaScript", "BruSoftware.NinjaScript");

            var migration = s_migrations.SingleOrDefault(p => p.FromAssembly == assemblyName && p.FromTypeName == typeName);
            if (migration != null)
            {
                return migration.ToType;
            }

            // See Custom/JsonMigrations.cs and CustomCharts/JsonMigrations.cs and NinjaScriptMigrations for the current migrations
            var result = base.BindToType(assemblyName, typeName);
            return result;
        }
        catch (JsonSerializationException ex)
        {
            if (!typeName.EndsWith("BruFills")
                && !typeName.EndsWith("BruExecutions")
                && !typeName.EndsWith("Breakeven")
                && !typeName.EndsWith("BruVwap1Tr")
                && !typeName.EndsWith("BruDonchianTranslated")
                && !typeName.EndsWith("BruBacktestPosition")
                && !typeName.EndsWith("BruBacktest")
                && !typeName.EndsWith("BruBacktestEquity"))
            {
                // Deleted indicators.
                s_logger.Error(ex, "{Message}", ex.Message);
            }
            throw;
        }
    }
}

public interface INamespaceMigration
{
    string FromAssembly { get; }

    string FromTypeName { get; }

    Type ToType { get; }
}

public class NamespaceMigration : INamespaceMigration
{
    public NamespaceMigration(string fromAssembly, string fromTypeName, Type toType)
    {
        FromAssembly = fromAssembly;
        FromTypeName = fromTypeName;
        ToType = toType;
    }

    public string FromAssembly { get; }
    public string FromTypeName { get; }
    public Type ToType { get; }

    public override string ToString()
    {
        return $"From {FromAssembly} {FromTypeName} to {ToType}";
    }
}