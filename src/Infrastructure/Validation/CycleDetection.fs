/// Cycle detection for hierarchical data structures
namespace EATool.Infrastructure.Validation

open System
open EATool.Domain

/// Cycle detection for business capabilities and other hierarchical entities
module CycleDetection =
    
    /// Check if setting a new parent would create a cycle
    /// Returns true if the proposed parent is a descendant of the child (would create a cycle)
    let wouldCreateCycleBusCapability (childId: string) (newParentId: string option) (getCapability: string -> BusinessCapability option) : bool =
        match newParentId with
        | None -> false // Can always become a root
        | Some parentId when parentId = childId -> true // Self-reference
        | Some parentId ->
            // Walk up the parent chain from parentId
            let rec walkAncestors (currentId: string) (visited: Set<string>) : bool =
                if visited.Contains currentId then
                    false // Cycle already exists in DB, shouldn't happen
                else
                    let newVisited = visited.Add currentId
                    match getCapability currentId with
                    | Some cap ->
                        match cap.ParentId with
                        | Some pid when pid = childId -> true // Found child in ancestor chain = cycle
                        | Some pid -> walkAncestors pid newVisited // Continue walking up
                        | None -> false // Reached root, no cycle
                    | None -> false // Parent doesn't exist

            walkAncestors parentId Set.empty
