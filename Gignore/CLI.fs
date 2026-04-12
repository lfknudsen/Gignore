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
    ///         <description>Default</description>
    ///     </listheader>
    ///     <item>
    ///         <term>dry</term>
    ///         <description>false</description>
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
          command = Commands.compile
          entries = [] }

let addEntry (entry: string) (args: arguments) : arguments =
    { args with
        entries = Seq.insertAt 0 (fmt entry) args.entries }

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
        let folder (argv: string) (args: arguments) : arguments =
            match argv.Trim().ToLower() with
            | "-d"
            | "--dry_run" -> { args with dry = true }
            | "-q"
            | "--quiet" -> { args with verbose = false }
            | "-v"
            | "--verbose" -> { args with verbose = true }
            | v -> addEntry v args

        let cmd = argv[0].ToLower()

        let nextIndex, arg =
            strToCmd cmd
            |> Option.map (fun cmd -> 1, { arguments.Empty with command = cmd })
            |> Option.defaultValue (0, arguments.Empty)

        Array.foldBack folder argv[nextIndex..] arg

let log (args: arguments) (msg: string) (ignored_value: 'a) =
    if args.verbose = false then
        Console.WriteLine(msg)
        ignored_value
    else
        ignored_value