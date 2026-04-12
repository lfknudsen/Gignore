module Gignore.Program

open System
open System.IO
open System.Net.Http
open Gignore.CLI
open Gignore.Util

/// The filename to use for the file which stores the results of calling the list endpoint.
let list_filename = "list.txt"
/// The location of the config.toml file.
let CONFIG_DIR = AppDomain.CurrentDomain.BaseDirectory
/// The location of the txt file which stores the cached results of calling the list endpoint.
let list_file = Path.Join(CONFIG_DIR, list_filename)

/// The domain to access; this will be the base URL of the HTTP client.
let domain = "https://www.toptal.com/developers/gitignore/api/"


let setupClient(baseUrl: string) : HttpClient =
    let client = new HttpClient()
    client.BaseAddress <- Uri(baseUrl)
    client

/// <summary>
/// Get the list of all template names from the API, and return them as an array.
/// The list will be saved to a file on disk, and in the future, if `useCache` is
/// `true`, the templates will be read from this file.
/// </summary>
/// <param name="args"> Command-line arguments. </param>
/// <param name="cacheFilename">The file to write the formatted list of templates to.
///     If the file already exists, returns the contents of the file instead.</param>
/// <param name="client">The HTTP client to use</param>
let get_list (args: arguments) (cacheFilename: string) (client: HttpClient) : Async<string array> =
    async {
        if args.useCache && FileInfo(cacheFilename).Exists then
            let! content = File.ReadAllLinesAsync(cacheFilename) |> Async.AwaitTask
            return content
        else
            let! response = client.GetAsync("list") |> Async.AwaitTask
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let list = content |> split [|',';'\n'|]
            File.WriteAllLinesAsync(cacheFilename, list) |> Async.AwaitTask |> ignore
            return list
    }

let compile (client: HttpClient) (types: string seq) =
    async {
        let parameters = String.concat "," types
        let! response = client.GetAsync(parameters) |> Async.AwaitTask
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }

let search (searchTerms: string seq) (templates: string array) : string array =
    templates |>
    Array.filter (fun (templateName: string) ->
                Seq.exists (fun (searchTerm: string) ->
                    templateName.Contains(searchTerm)) searchTerms)

let printSearchResults (args: arguments) (results: string seq) =
    if args.verbose then
        Console.WriteLine $"Found {Seq.length results} matching template(s)."
    results |> Seq.iter Console.WriteLine

let proceed (cacheFile: string) (args: arguments) (client: HttpClient) =
    async {
        match args.command with
        | Commands.compile ->
            let! content = compile client args.entries
            let! result = File.WriteAllTextAsync(".gitignore", content) |> Async.AwaitTask
            if args.verbose then
                Console.WriteLine "Compiled .gitignore."
            return result
        | Commands.list ->
            let! output: string array = get_list args cacheFile client
            tabular args.columns output |> Console.WriteLine
        | Commands.search ->
            if Seq.isEmpty args.entries then
                if args.verbose then
                    Console.WriteLine "Please provide search terms."
                exit 1
            else
                let! output: string array = get_list args cacheFile client
                output
                |> search args.entries
                |> printSearchResults args
        | Commands.clean ->
            try
                File.Delete(cacheFile)
                Console.WriteLine($"Deleted the cached {cacheFile}.")
            with
            | :? SystemException as ex -> Console.WriteLine($"Could not delete the file:\n{ex.Message}")

        | Commands.add
        | Commands.remove
        | _ -> failwith "Not implemented yet."
    }

[<EntryPoint>]
let main (argv: string array) : int =
    let args = parseArgs argv

    domain
    |> setupClient
    |> proceed list_file args
    |> Async.RunSynchronously
    0
