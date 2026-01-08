/// DNS domain validation helpers
namespace EATool.Domain

open System
open System.Text.RegularExpressions

module DnsValidator =
    let private labelPattern = Regex("^[A-Za-z0-9-]{1,63}$", RegexOptions.Compiled)

    let validate (domain: string) : Option<string> =
        let trimmed = domain.Trim().TrimEnd('.')
        if String.IsNullOrWhiteSpace(trimmed) then
            Some "Domain must not be empty"
        elif trimmed.Length > 253 then
            Some "Domain is too long"
        else
            let labels = trimmed.Split('.', StringSplitOptions.RemoveEmptyEntries)
            if labels.Length < 2 then
                Some $"Invalid domain: '{domain}'"
            else
                let invalidLabel =
                    labels
                    |> Array.tryFind (fun label ->
                        label.Length = 0
                        || label.StartsWith("-")
                        || label.EndsWith("-")
                        || not (labelPattern.IsMatch(label)))
                match invalidLabel with
                | Some lbl -> Some $"Invalid domain label '{lbl}' in '{domain}'"
                | None ->
                    let tld = labels.[labels.Length - 1]
                    if tld.Length < 2 then Some $"Invalid TLD in domain '{domain}'"
                    elif tld |> Seq.forall Char.IsDigit then Some $"Invalid TLD in domain '{domain}'"
                    else None

    let validateDomain (domain: string) : Result<string, string> =
        match validate domain with
        | Some err -> Error err
        | None -> Ok (domain.Trim().TrimEnd('.').ToLowerInvariant())
