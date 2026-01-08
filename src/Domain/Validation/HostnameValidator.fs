/// Hostname validation helpers
namespace EATool.Domain

open System
open System.Text.RegularExpressions

module HostnameValidator =
    // Allow underscores inside labels (not leading/trailing), alphanumerics, hyphens
    let private labelPattern = Regex("^[A-Za-z0-9](?:[A-Za-z0-9_-]{0,61}[A-Za-z0-9])?$", RegexOptions.Compiled)

    let validate (hostname: string) : Option<string> =
        let trimmed = hostname.Trim().TrimEnd('.')
        if String.IsNullOrWhiteSpace(trimmed) then
            Some "Hostname must not be empty"
        elif trimmed.Length > 253 then
            Some "Hostname is too long"
        else
            let labels = trimmed.Split('.', StringSplitOptions.RemoveEmptyEntries)
            let invalidLabel = labels |> Array.tryFind (fun lbl -> not (labelPattern.IsMatch(lbl)))
            match invalidLabel with
            | Some lbl -> Some $"Invalid hostname label '{lbl}' in '{hostname}'"
            | None -> None

    let validateHostname (hostname: string) : Result<string, string> =
        match validate hostname with
        | Some err -> Error err
        | None -> Ok (hostname.Trim().TrimEnd('.').ToLowerInvariant())
