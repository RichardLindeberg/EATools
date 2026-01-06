namespace EATool.Domain

open System
open System.Text.RegularExpressions

module Validation =
    let ok = Ok ()

    let validateRequired (value: string) fieldName =
        if String.IsNullOrWhiteSpace value then Error [$"{fieldName} is required"] else ok

    let validateLength min max (value: string) fieldName =
        if isNull value then Error [$"{fieldName} is required"]
        elif value.Length < min || value.Length > max then Error [$"{fieldName} must be between {min} and {max} characters"] else ok

    let validateEmail (value: string) fieldName =
        let pattern = "^[^@\s]+@[^@\s]+\.[^@\s]+$"
        if Regex.IsMatch(value, pattern) then ok else Error [$"{fieldName} must be a valid email"]

    let validateUrl (value: string) fieldName =
        let isValid =
            Uri.TryCreate(value, UriKind.Absolute) |> function
            | true, uri when uri.Scheme = Uri.UriSchemeHttp || uri.Scheme = Uri.UriSchemeHttps -> true
            | _ -> false
        if isValid then ok else Error [$"{fieldName} must be a valid http/https URL"]

    let validateUuid (value: string) fieldName =
        match Guid.TryParse value with
        | true, _ -> ok
        | _ -> Error [$"{fieldName} must be a valid UUID"]

    let validateEnum (allowed: 'T list) (value: 'T) fieldName =
        if allowed |> List.contains value then ok else Error [$"{fieldName} has invalid value"]

    let (<*>) r1 r2 =
        match r1, r2 with
        | Ok (), Ok () -> ok
        | Error e1, Error e2 -> Error (e1 @ e2)
        | Error e, _ | _, Error e -> Error e
