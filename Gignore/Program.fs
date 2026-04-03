module Gignore.Program

open System
open System.IO
open System.Net.Http

type arguments =
    {
        dry: bool
        list: bool
        search: bool
        compile: bool
        entries: string list
    }

    static member Empty =
        {
            dry = false
            list = false
            search = false
            compile = false
            entries = []
        }

let parseArgs (argv: string array): arguments =
    let folder (argv: string) (args: arguments)  : arguments =
         match argv with
         | "-d"
         | "--dry_run" -> { args with dry = true }
         | "list" -> { args with list = true }
         | "search" -> { args with search = true }
         | "compile" -> { args with compile = true }
         | _ -> { args with entries = argv :: args.entries }
    Array.foldBack folder argv arguments.Empty

let get_list (client: HttpClient) =
    async {
        let! response = client.GetAsync("list") |> Async.AwaitTask
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }

let compile (client: HttpClient) (types: string list) =
    async {
        let parameters = String.concat "," types
        let! response = client.GetAsync(parameters) |> Async.AwaitTask
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }

[<EntryPoint>]
let main(argv: string array): int =
    let args = parseArgs argv
    use client = new HttpClient()
    client.BaseAddress <- Uri("https://www.toptal.com/developers/gitignore/api/")
    if args.list then
        let output = get_list client |> Async.RunSynchronously
        File.WriteAllText("list.txt", output)
    elif args.search then
        let output = get_list client |> Async.RunSynchronously
        let split = output.Split(',')
        File.WriteAllLines("list.txt", split)
        Array.filter (fun (e: string) -> List.contains e args.entries) split
        |> Array.iter Console.WriteLine
    elif args.compile then
        let output = compile client args.entries |> Async.RunSynchronously
        File.WriteAllText(".gitignore", output)
    0