module Gignore.Util

open System

/// Separates columns in the list output.
let columnPadding = " | "

/// Formats strings by removing leading/trailing whitespace and converting to lower-case.
let fmt (str: string) : string = str.Trim().ToLower()

let pair (a: 'a) (b: 'b) : 'a * 'b = a, b

let split (c: char array) (s: string) : string array =
    s.Split(c, StringSplitOptions.TrimEntries ||| StringSplitOptions.RemoveEmptyEntries)

let chunkFolder (format: string -> string) (chunkAcc: string) (row: string array) : string =
    let rec aux (i: int) (acc: string) =
        if i >= Array.length row then
            acc
        else
            let infix = match i with
                        | 0 ->    ""
                        | _ -> columnPadding
            let formatter = if i = Array.length row - 1 then id
                                                        else format
            let nextAcc = (acc + infix + formatter row[i])
            aux (i + 1) nextAcc
    (aux 0 chunkAcc) + "\n"

let letterMapper (columnWidth: int) (initial: char, templates: string array array) : string =
    let pad (s: string) : string = s.PadRight(columnWidth)
    let folder = (chunkFolder pad)
    let acc = $"{string <| Char.ToUpperInvariant initial}:\n"
    Array.fold folder acc templates

/// Separate the list of templates into columns.
let makeChunks (columnCount: int) (initial: char, templates: string array)
        : char * string array array =
    initial, Array.chunkBySize columnCount templates

/// Calculate the maximum amount of columns we can have in our output table of templates.
let rec determineMaxColumnCount (padding: int) (maxConsoleWidth: int) (columnWidth: int)  =
    // Cast values; we won't truncate until end
    let maxWidth = float32 maxConsoleWidth
    let colWidth = float32 columnWidth

    // We determine the number of columns that can fit into the width without padding.
    let firstPassColCount = maxWidth / colWidth
    // Then we compute the space required for padding that amount of columns:
    let totalPadding = float32 padding * (firstPassColCount - 1f)
    // We compute the amount of space the table would actually require if it had
    // that many columns of the given size.
    let requiredWidth = firstPassColCount * colWidth + totalPadding
    if requiredWidth <= maxWidth then
        firstPassColCount |> int
    else
        firstPassColCount - 1f |> int

/// Formats the list of templates received from the server/cache into a table.
let tabular (columns: Option<int>) (templates: string array) : string =
    let longestWordLength = Array.maxBy String.length templates |> String.length
    let maxTableWidth = Console.WindowWidth |> float |> (*) 0.60 |> int
    let columnWidth = longestWordLength
    let padding = columnPadding.Length
    if Option.contains 1 columns || columnWidth > (maxTableWidth / 2) + padding then
        String.Join('\n', templates)
    else
        let columnCount =
            columns
            |> Option.defaultWith(fun () ->
                determineMaxColumnCount padding maxTableWidth columnWidth)
        if columnCount = 1 then
            String.Join('\n', templates)
        else
            templates
            |> Array.filter (fun (t: string) -> t.Length > 0)
            |> Array.groupBy (fun (template: string) -> template.Chars(0))
            |> Array.map (makeChunks columnCount)
            |> Array.map (letterMapper columnWidth)
            |> pair '\n'
            |> String.Join
