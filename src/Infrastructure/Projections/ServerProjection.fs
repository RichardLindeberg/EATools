/// Server projection that maintains the servers read model
namespace EATool.Infrastructure.Projections

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure

module ServerProjection =
    
    let private serializeTags (tags: string list) =
        JsonSerializer.Serialize(tags)
    
    let private deserializeTags (json: string) =
        try JsonSerializer.Deserialize<string list>(json)
        with _ -> []

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore
    
    let private getTags (connString: string) (id: string) : string list =
        use conn = new SqliteConnection(connString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT tags FROM servers WHERE id = $id"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() && not (reader.IsDBNull(0)) then
            deserializeTags (reader.GetString(0))
        else
            []

    let private handleCreated (data: ServerCreatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                INSERT INTO servers (id, hostname, environment, region, platform, criticality, owning_team, tags, created_at, updated_at)
                VALUES ($id, $hostname, $environment, $region, $platform, $criticality, $owning_team, $tags, $created_at, $updated_at)
                ON CONFLICT(id) DO NOTHING
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$hostname", data.Hostname) |> ignore
            cmd.Parameters.AddWithValue("$environment", data.Environment) |> ignore
            addOptionalParam cmd "$region" (data.Region |> Option.map box)
            addOptionalParam cmd "$platform" (data.Platform |> Option.map box)
            cmd.Parameters.AddWithValue("$criticality", data.Criticality) |> ignore
            addOptionalParam cmd "$owning_team" (data.OwningTeam |> Option.map box)
            cmd.Parameters.AddWithValue("$tags", serializeTags data.Tags) |> ignore
            cmd.Parameters.AddWithValue("$created_at", now) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ServerCreated: {ex.Message}"
    
    let private handleHostnameUpdated (data: HostnameUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE servers SET hostname = $hostname, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$hostname", data.NewHostname) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle HostnameUpdated: {ex.Message}"
    
    let private handleEnvironmentSet (data: EnvironmentSetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE servers SET environment = $environment, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$environment", data.NewEnvironment) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle EnvironmentSet: {ex.Message}"
    
    let private handleCriticalitySet (data: ServerCriticalitySetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE servers SET criticality = $criticality, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$criticality", data.NewCriticality) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle CriticalitySet: {ex.Message}"
    
    let private handleRegionUpdated (data: RegionUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE servers SET region = $region, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$region" (data.NewRegion |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle RegionUpdated: {ex.Message}"
    
    let private handlePlatformUpdated (data: PlatformUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE servers SET platform = $platform, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$platform" (data.NewPlatform |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle PlatformUpdated: {ex.Message}"
    
    let private handleOwningTeamSet (data: OwningTeamSetData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE servers SET owning_team = $owning_team, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            addOptionalParam cmd "$owning_team" (data.NewTeam |> Option.map box)
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle OwningTeamSet: {ex.Message}"
    
    let private handleTagsAdded (data: ServerTagsAddedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            let currentTags = getTags connString data.Id
            let newTags = (currentTags @ data.AddedTags) |> List.distinct
            
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE servers SET tags = $tags, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$tags", serializeTags newTags) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ServerTagsAdded: {ex.Message}"
    
    let private handleTagsRemoved (data: ServerTagsRemovedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            let currentTags = getTags connString data.Id
            let newTags = currentTags |> List.filter (fun t -> not (List.contains t data.RemovedTags))
            
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE servers SET tags = $tags, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$tags", serializeTags newTags) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ServerTagsRemoved: {ex.Message}"
    
    let private handleDeleted (data: ServerDeletedData) (connString: string) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "DELETE FROM servers WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ServerDeleted: {ex.Message}"

    /// Projection handler implementation for Server events
    type Handler(connString: string) =
        interface ProjectionEngine.IProjectionHandler<ServerEvent> with
            member _.Handle(envelope: EventEnvelope<ServerEvent>) =
                match envelope.Data with
                | ServerCreated data -> handleCreated data connString
                | HostnameUpdated data -> handleHostnameUpdated data connString
                | EnvironmentSet data -> handleEnvironmentSet data connString
                | EATool.Domain.ServerEvent.CriticalitySet data -> handleCriticalitySet data connString
                | RegionUpdated data -> handleRegionUpdated data connString
                | PlatformUpdated data -> handlePlatformUpdated data connString
                | OwningTeamSet data -> handleOwningTeamSet data connString
                | ServerTagsAdded data -> handleTagsAdded data connString
                | ServerTagsRemoved data -> handleTagsRemoved data connString
                | ServerDeleted data -> handleDeleted data connString
            
            member _.ProjectionName = "ServerProjection"
            
            member _.CanHandle(eventType: string) =
                match eventType with
                | "ServerCreated" | "HostnameUpdated" | "EnvironmentSet" | "CriticalitySet" 
                | "RegionUpdated" | "PlatformUpdated" | "OwningTeamSet" 
                | "ServerTagsAdded" | "ServerTagsRemoved" | "ServerDeleted" -> true
                | _ -> false
