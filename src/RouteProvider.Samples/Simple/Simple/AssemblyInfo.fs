﻿namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Simple")>]
[<assembly: AssemblyProductAttribute("RouteProvider")>]
[<assembly: AssemblyDescriptionAttribute("A type provider for web routing")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
