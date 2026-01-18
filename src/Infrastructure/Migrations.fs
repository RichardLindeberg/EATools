/// Database migrations using DbUp
namespace EATool.Infrastructure

open System
open System.Reflection
open DbUp
open DbUp.Engine
open DbUp.Engine.Preprocessors

module Migrations =
    /// Disable variable substitution to avoid interpreting bcrypt hashes with '$'
    type NoVariablePreprocessor() =
        interface IScriptPreprocessor with
            member _.Process(contents: string) = contents

    let run (config: DatabaseConfig) : Result<unit, string> =
        try
            let assembly = Assembly.GetExecutingAssembly()
            let upgrader =
                DeployChanges.To
                    .SQLiteDatabase(config.ConnectionString)
                    .WithTransactionPerScript()
                    .WithScriptsEmbeddedInAssembly(
                        assembly,
                        fun name ->
                            name.EndsWith(".sql")
                            && not (name.Contains("016_seed_development_users.sql"))
                    )
                    .WithPreprocessor(NoVariablePreprocessor())
                    .LogToAutodetectedLog()
                    .Build()

            let result = upgrader.PerformUpgrade()
            if result.Successful then Ok () else Error result.Error.Message
        with
        | ex -> Error ex.Message
