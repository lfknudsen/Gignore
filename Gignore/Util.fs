module Gignore.Util

open System

let fmt (str: string): string =
    str.Trim().ToLower()

let pair (a: 'a) (b: 'b): 'a * 'b = a,b

let split (c: char array) (s: string) : string array =
    s.Split(c, StringSplitOptions.TrimEntries ||| StringSplitOptions.RemoveEmptyEntries)