/// <summary>
///
/// <param name="dry"></param>
/// </summary>
module Gignore.Program

open System
open System.IO
open System.Net.Http
open Gignore.CLI
open Gignore.Util

let list_file = "list.txt"

/// <summary>
/// Get the list of all template names from the API, and return them as an array.
/// </summary>
/// <param name="client"></param>
let get_list (client: HttpClient) : Async<string array> =
    async {
        if FileInfo(list_file).Exists then
            let! content = File.ReadAllLinesAsync(list_file) |> Async.AwaitTask
            return content
        else
            let! response = client.GetAsync("list") |> Async.AwaitTask
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            return content |> split [|',';'\n'|]
    }

let compile (client: HttpClient) (types: string seq) =
    async {
        let parameters = String.concat "," types
        let! response = client.GetAsync(parameters) |> Async.AwaitTask
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }

let search (searchTerms: string seq) (templates: string seq) : string seq =
    templates |>
    Seq.filter (fun (templateName: string) ->
                Seq.exists (fun (searchTerm: string) ->
                    templateName.Contains(searchTerm)) searchTerms)

let printSearchResults (args: arguments) (results: string seq) =
    if Seq.isEmpty results && args.verbose then
        Console.WriteLine "Found no matching templates."
    elif args.verbose then
        Console.WriteLine "Found following matching templates:"
        results |> Seq.iter Console.WriteLine
    else
        results |> Seq.iter Console.WriteLine

let proceed (client: HttpClient) (args: arguments) =
    async {
        let log (msg: string) (ignored_value: 'a) :'a = log args msg ignored_value
        match args.command with
        | Commands.compile ->
            let! output = compile client args.entries
            File.WriteAllTextAsync(".gitignore", output) |> Async.AwaitTask |> ignore
            if args.verbose then
                Console.WriteLine "Compiled .gitignore."
        | Commands.list ->
            let! output = get_list client
            let usedCache = (File.Exists list_file)
            if not usedCache then
                File.WriteAllLinesAsync(list_file, output) |> Async.AwaitTask |> ignore
                if args.verbose then
                    Console.WriteLine "Wrote each possible .gitignore template name to 'list.txt'."
                else ()
            elif args.verbose then
                Console.WriteLine "A file named 'list.txt' already exists, and will be used as a cache."
            else
                ()
            |> ignore

            if not (Seq.isEmpty args.entries) then
                output
                |> search args.entries
                |> printSearchResults args
            else
                ()

        | Commands.search ->
            if Seq.isEmpty args.entries then
                if args.verbose then
                    Console.WriteLine "Please provide search terms."
                exit 1
            else
                let! output: string array = get_list client
                output
                |> search args.entries
                |> printSearchResults args
        | Commands.clean ->
            File.Delete(list_file)
        | Commands.add
        | Commands.remove
        | _ -> failwith "Not implemented yet."
    }

[<EntryPoint>]
let main (argv: string array) : int =
    let args = parseArgs argv
    use client = new HttpClient()
    client.BaseAddress <- Uri("https://www.toptal.com/developers/gitignore/api/")
    proceed client args |> Async.RunSynchronously
    0
