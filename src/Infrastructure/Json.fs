/// JSON serialization/deserialization using Thoth.Json.Net
namespace EATool.Infrastructure

open Thoth.Json.Net
open EATool.Domain

module Json =
    // Decoders
    let decodeCreateOrganizationRequest: Decoder<CreateOrganizationRequest> =
        Decode.object (fun get ->
            {
                Name = get.Required.Field "name" Decode.string
                Domains = get.Optional.Field "domains" (Decode.list Decode.string) |> Option.defaultValue []
                Contacts = get.Optional.Field "contacts" (Decode.list Decode.string) |> Option.defaultValue []
            })
    
    // Encoders
    let encodeOrganization (org: Organization): JsonValue =
        Encode.object [
            "id", Encode.string org.Id
            "name", Encode.string org.Name
            "domains", Encode.list (List.map Encode.string org.Domains)
            "contacts", Encode.list (List.map Encode.string org.Contacts)
            "createdAt", Encode.string org.CreatedAt
            "updatedAt", Encode.string org.UpdatedAt
        ]
    
    let encodePaginatedResponse<'T> (encoder: 'T -> JsonValue) (response: PaginatedResponse<'T>): JsonValue =
        Encode.object [
            "data", Encode.list (List.map encoder response.Items)
            "pagination", Encode.object [
                "page", Encode.int response.Page
                "limit", Encode.int response.Limit
                "total", Encode.int response.Total
                "pages", Encode.int ((response.Total + response.Limit - 1) / response.Limit)
            ]
        ]
    
    let encodeErrorResponse (code: string) (message: string): JsonValue =
        Encode.object [
            "code", Encode.string code
            "message", Encode.string message
        ]
