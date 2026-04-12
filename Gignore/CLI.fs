module Gignore.CLI

open System
open Gignore.Util

/// <summary>
/// The possible distinct commands that this programme can execute.
/// </summary>
type Commands =
    /// <summary>
    /// The default command if none other is specified;
    /// generates a new<br/><c>.gitignore</c> based
    /// on the template names given as unstructured arguments.
    /// </summary>
    | compile = ' '

    /// <summary>
    /// Write the name of each possible template to <c>list.txt</c>.
    /// No unstructured arguments are accepted.
    /// </summary>
    | list = 'l'

    /// <summary>
    /// Does the same as <c>list</c>, but if supplied with one or more arguments,
    /// will print all template names that contain any of the given strings.
    /// At least one unstructured argument is required.
    /// </summary>
    | search = 's'

    /// <summary>
    /// Instead of compiling a new <br/><c>.gitignore</c>, migrate the current one to
    /// include the new template name(s) you specify.
    /// </summary>
    | add = 'a'

    /// <summary>
    /// Instead of compiling a new <br/><c>.gitignore</c>, migrate the current one to
    /// no longer include the template name(s) you specify.
    /// </summary>
    | remove = 'r'

    /// <summary>
    /// Re-generate the <c>.gitignore</c> based on TopTal's signature(s) in the file.
    /// </summary>
    | clean = 'c'

let strToCmd (str: string) : Option<Commands> =
    match str with
    | "compile" -> Some Commands.compile
    | "list" -> Some Commands.list
    | "search" -> Some Commands.search
    | "add" -> Some Commands.add
    | "remove" -> Some Commands.remove
    | "clean" -> Some Commands.clean
    | _ -> None

/// Record for holding information gathered by parsing command-line arguments.
type arguments =
    {
        /// <summary>
        /// If true, will print the URL to make a GET request to and then exit.
        /// </summary>
        dry: bool

        /// <summary>
        /// Whether the stdout printing should limit itself to the absolute minimum required.
        /// </summary>
        verbose: bool

        /// <summary>
        /// If true, the `list` and `search` commands will save the list of templates to a file
        /// on disk, and read from this file in the future if you run either of the commands.
        /// Defaults to `true`.
        /// Running the `clean` command will delete the cache file.
        /// </summary>
        useCache: bool

        /// <summary>
        /// The `list` command will print its output with the given number of columns.
        /// Default is 3. If set to 1, it is equivalent to `-s`.
        /// </summary>
        columns: Option<int>

        /// <summary>
        /// The specific command to be executed by this programme.
        /// </summary>
        command: Commands

        /// <summary>
        /// Holds the unstructured arguments; template variables or search terms.
        /// </summary>
        entries: string seq
    }


    /// <summary>
    /// Create a new empty record:
    /// <list type="bullet|number|table">
    ///     <listheader>
    ///         <term>Field</term>
    ///         <description>Default value</description>
    ///     </listheader>
    ///     <item>
    ///         <term>dry</term>
    ///         <description>false</description>
    ///     </item>
    ///     <item>
    ///         <term>verbose</term>
    ///         <description>true</description>
    ///     </item>
    ///     <item>
    ///         <term>useCache</term>
    ///         <description>true</description>
    ///     </item>
    ///     <item>
    ///         <term>command</term>
    ///         <description>Commands.compile</description>
    ///     </item>
    ///     <item>
    ///         <term>entries</term>
    ///         <description>an empty <see cref="Microsoft.FSharp.Collections.list">list</see></description>
    ///     </item>
    /// </list>
    /// </summary>
    static member Empty =
        { dry = false
          verbose = true
          useCache = true
          columns = None
          command = Commands.compile
          entries = []
        }

let addEntry (entry: string) (args: arguments) : arguments =
    { args with
        entries = Seq.insertAt 0 (fmt entry) args.entries }

let parser (argv: string array) (args: arguments): arguments =
    let rec aux (i: int) (args: arguments) : arguments =
        if i >= argv.Length then
            args
        else
        let arg = argv[i]
        match arg.Trim().ToLower() with
        | "-d"
        | "--dry_run" -> aux (i + 1) { args with dry = true }
        | "-q"
        | "--quiet" -> aux (i + 1) { args with verbose = false }
        | "-v"
        | "--verbose" -> aux (i + 1) { args with verbose = true }
        | "-c"
        | "--clean" -> aux (i + 1) { args with useCache = false }
        | "-t"
        | "--columns" ->
            if i >= argv.Length - 1 then
                Console.Error.WriteLine($"The option '{arg}' was expected to have an argument.")
                exit 1
            else
                let nextElement = argv[i + 1]
                let success, value = Int32.TryParse nextElement
                if not success || value <= 0 then
                    Console.Error.WriteLine($"The option '{arg}' was given an argument that could not
                                            be parsed as a positive integer: {nextElement}.")
                    exit 1
                else
                    aux (i + 2) { args with columns = Some value }
        | v -> aux (i + 1) (addEntry v args)
    aux 0 args

/// <summary>
/// Parse command-line arguments and return an instance of the
/// <see cref="Gignore.Program.arguments">arguments</see> record type.
/// </summary>
/// <param name="argv"> This is expected to be the system command-line arguments
/// as supplied to the entry-point.
/// </param>
let parseArgs (argv: string array) : arguments =
    if argv.Length = 0 then
        arguments.Empty
    else
        let cmd = argv[0].ToLower()

        let nextIndex, arg =
            strToCmd cmd
            |> Option.map (fun cmd -> 1, { arguments.Empty with command = cmd })
            |> Option.defaultValue (0, arguments.Empty)

        parser argv[nextIndex..] arg

let log (args: arguments) (msg: string) (ignored_value: 'a) =
    if args.verbose = true then
        Console.WriteLine(msg)
    ignored_value