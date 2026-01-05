/// Database migrations using DbUp
namespace EATool.Infrastructure

open System
open System.Reflection
open DbUp
open DbUp.Engine

module Migrations =
    let run (config: DatabaseConfig) : Result<unit, string> =
        try
            let assembly = Assembly.GetExecutingAssembly()
            let upgrader =
                DeployChanges.To
                    .SQLiteDatabase(config.ConnectionString)
                    .WithTransactionPerScript()
                    .WithScriptsEmbeddedInAssembly(assembly, fun name -> name.EndsWith(".sql"))
                    .LogToAutodetectedLog()
                    .Build()

            let result = upgrader.PerformUpgrade()
            if result.Successful then Ok () else Error result.Error.Message
        with
        | ex -> Error ex.Message
