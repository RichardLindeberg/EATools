/// Email validation helpers
namespace EATool.Domain

open System
open System.Net.Mail

module EmailValidator =
    let validate (email: string) : Option<string> =
        let trimmed = email.Trim()
        if String.IsNullOrWhiteSpace(trimmed) then
            Some "Email must not be empty"
        elif trimmed.Length > 254 then
            Some "Email is too long"
        else
            try
                let addr = MailAddress(trimmed)
                // Ensure normalized address matches input (case-insensitive local part is acceptable)
                if String.Equals(addr.Address, trimmed, StringComparison.OrdinalIgnoreCase) then
                    None
                else
                    Some $"Invalid email address: '{email}'"
            with _ -> Some $"Invalid email address: '{email}'"

    let validateEmail (email: string) : Result<string, string> =
        match validate email with
        | Some err -> Error err
        | None -> Ok email
