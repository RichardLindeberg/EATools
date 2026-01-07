/// Organization projection that maintains the organizations read model
namespace EATool.Infrastructure.Projections

open System
open System.Text.Json
open Microsoft.Data.Sqlite
open EATool.Domain
open EATool.Infrastructure

module OrganizationProjection =
    
    let private serializeList (items: string list) =
        JsonSerializer.Serialize(items)
    
    let private deserializeList (json: string) =
        try JsonSerializer.Deserialize<string list>(json)
        with _ -> []

    let private getUtcTimestamp () = DateTime.UtcNow.ToString("O")

    let private addOptionalParam (cmd: SqliteCommand) (name: string) (value: obj option) =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value |> Option.defaultValue (box DBNull.Value)
        cmd.Parameters.Add(p) |> ignore
    
    let private handleCreated (data: OrganizationCreatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                INSERT INTO organizations (id, name, parent_id, domains, contacts, created_at, updated_at)
                VALUES ($id, $name, $parent_id, $domains, $contacts, $created_at, $updated_at)
                ON CONFLICT(id) DO NOTHING
                """
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$name", data.Name) |> ignore
            addOptionalParam cmd "$parent_id" (data.ParentId |> Option.map box)
            cmd.Parameters.AddWithValue("$domains", serializeList data.Domains) |> ignore
            cmd.Parameters.AddWithValue("$contacts", serializeList data.Contacts) |> ignore
            cmd.Parameters.AddWithValue("$created_at", now) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore

            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle OrganizationCreated: {ex.Message}"
    
    let private handleParentAssigned (data: ParentAssignedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE organizations SET parent_id = $parent_id, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$parent_id", data.NewParentId) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ParentAssigned: {ex.Message}"
    
    let private handleParentRemoved (data: ParentRemovedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE organizations SET parent_id = NULL, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ParentRemoved: {ex.Message}"
    
    let private handleContactInfoUpdated (data: ContactInfoUpdatedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE organizations SET contacts = $contacts, updated_at = $updated_at WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.Parameters.AddWithValue("$contacts", serializeList data.NewContacts) |> ignore
            cmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle ContactInfoUpdated: {ex.Message}"
    
    let private handleDomainAdded (data: DomainAddedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            // Get current domains
            use getCmd = conn.CreateCommand()
            getCmd.CommandText <- "SELECT domains FROM organizations WHERE id = $id"
            getCmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            use reader = getCmd.ExecuteReader()
            
            let currentDomains =
                if reader.Read() && not (reader.IsDBNull(0)) then
                    deserializeList (reader.GetString(0))
                else
                    []
            
            reader.Close()
            
            // Add new domain (ensure uniqueness)
            let updatedDomains = 
                if List.contains data.Domain currentDomains then currentDomains
                else currentDomains @ [data.Domain]
            
            // Update
            use updateCmd = conn.CreateCommand()
            updateCmd.CommandText <- "UPDATE organizations SET domains = $domains, updated_at = $updated_at WHERE id = $id"
            updateCmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            updateCmd.Parameters.AddWithValue("$domains", serializeList updatedDomains) |> ignore
            updateCmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            updateCmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle DomainAdded: {ex.Message}"
    
    let private handleDomainRemoved (data: DomainRemovedData) (connString: string) : Result<unit, string> =
        try
            let now = getUtcTimestamp ()
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            // Get current domains
            use getCmd = conn.CreateCommand()
            getCmd.CommandText <- "SELECT domains FROM organizations WHERE id = $id"
            getCmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            use reader = getCmd.ExecuteReader()
            
            let currentDomains =
                if reader.Read() && not (reader.IsDBNull(0)) then
                    deserializeList (reader.GetString(0))
                else
                    []
            
            reader.Close()
            
            // Remove domain
            let updatedDomains = currentDomains |> List.filter ((<>) data.Domain)
            
            // Update
            use updateCmd = conn.CreateCommand()
            updateCmd.CommandText <- "UPDATE organizations SET domains = $domains, updated_at = $updated_at WHERE id = $id"
            updateCmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            updateCmd.Parameters.AddWithValue("$domains", serializeList updatedDomains) |> ignore
            updateCmd.Parameters.AddWithValue("$updated_at", now) |> ignore
            updateCmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle DomainRemoved: {ex.Message}"

    let private handleDeleted (data: OrganizationDeletedData) (connString: string) : Result<unit, string> =
        try
            use conn = new SqliteConnection(connString)
            conn.Open()
            
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "DELETE FROM organizations WHERE id = $id"
            cmd.Parameters.AddWithValue("$id", data.Id) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        with ex ->
            Error $"Failed to handle OrganizationDeleted: {ex.Message}"

    /// Projection handler that processes Organization events
    type Handler(connString: string) =
        interface ProjectionEngine.IProjectionHandler<OrganizationEvent> with
            member _.ProjectionName = "OrganizationProjection"
            
            member _.CanHandle(eventType: string) =
                match eventType with
                | "OrganizationCreated"
                | "ParentAssigned"
                | "ParentRemoved"
                | "ContactInfoUpdated"
                | "DomainAdded"
                | "DomainRemoved"
                | "OrganizationDeleted" -> true
                | _ -> false

            member _.Handle(envelope: EventEnvelope<OrganizationEvent>) =
                match envelope.Data with
                | OrganizationCreated data -> handleCreated data connString
                | ParentAssigned data -> handleParentAssigned data connString
                | ParentRemoved data -> handleParentRemoved data connString
                | ContactInfoUpdated data -> handleContactInfoUpdated data connString
                | DomainAdded data -> handleDomainAdded data connString
                | DomainRemoved data -> handleDomainRemoved data connString
                | OrganizationDeleted data -> handleDeleted data connString
