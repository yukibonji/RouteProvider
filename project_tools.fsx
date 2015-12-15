#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System
open System.IO
#if MONO
#else
#load "packages/SourceLink.Fake/tools/Fake.fsx"
open SourceLink
#endif

#r @"bin\RouteProvider\RouteProvider.dll"
open IsakSky.RouteProvider.RouteCompiler

let routes = """
  GET projects/{projectId} as getProject
  GET projects/{projectId}/comments/{commentId} as getProjectComments
  PUT projects/{projectId:int} as updateProject
  GET projects/statistics
  GET people/{name:string} as getPerson
"""

let genSample (opts:RouteProviderOptions) =
  let tpCall =
    match opts.inputTypeName, opts.returnTypeName with
    | Some(inputTypeName), Some(returnTypeName) ->
      sprintf "type MyRoutes = IsakSky.RouteProvider<routes, \"%s\", \"%s\">" inputTypeName returnTypeName
    | Some(inputTypeName), None ->
      sprintf "type MyRoutes = IsakSky.RouteProvider<routes, \"%s\">" inputTypeName
    | None, None ->
      "type MyRoutes = IsakSky.RouteProvider<routes>"
    | _ -> failwith "Logic error"

  let cfg = sprintf "Configuration:\n\n    %s\n" tpCall

  let code = buildCSharpCode opts
  let codelines = code.Split([|"\r\n"; "\r"; "\n"|], (StringSplitOptions.None))
  let description =
    match opts.inputTypeName, opts.returnTypeName with
    | Some(inputTypeName), Some(returnTypeName) ->
      sprintf "Generated code with input type \"%s\" and return type \"%s\"" inputTypeName returnTypeName
    | Some(inputTypeName), None ->
      sprintf "Generated code with input type \"%s\"" inputTypeName
    | None, None ->
      "Generated code"
    | _ -> failwith "Logic error"
    
  let offsetcode = 
    codelines 
    |> Seq.map (fun s -> sprintf "%s\n    " s)
    |> String.concat ""
  sprintf "%s\n\n%s:\n\n    [lang=csharp]\n    %s" cfg description offsetcode

let opts1 =
  { typeName = "MyRoutes"
    inputTypeName = None
    returnTypeName = None
    routesStr = routes
    config = None }
let targetDir = "docs/content"
let targetFile1 = targetDir @@ "notypes.md"
ensureDirectory targetDir
System.IO.File.WriteAllText(targetFile1, (genSample opts1))

let opts2 =
  { opts1 with inputTypeName = Some "Microsoft.Owin.IOwinContext" }
let code2 = buildCSharpCode opts2
let targetFile2 = targetDir @@ "input_type.md"
System.IO.File.WriteAllText(targetFile2, (genSample opts2))

let opts3 =
  { opts2 with 
      returnTypeName = Some "HttpWebResponse" } 
let code3 = buildCSharpCode opts3
let targetFile3 = targetDir @@ "input_and_return_type.md"
ensureDirectory targetDir
System.IO.File.WriteAllText(targetFile3, (genSample opts3))


