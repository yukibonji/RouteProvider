# RouteProvider

An F# Type provider that generates types suitable for routing in a web application.

## Example: 

``` Fsharp
[<Literal>]
let routes = """
  GET projects/{projectId} as getProject
  PUT projects/{foo:string} as updateProject
  POST projects/{projectId:int} as createProject
  GET projects/{projectId}/comments/{commentId} as getProjectComments
"""
[<Literal>]
let outputPath = __SOURCE_DIRECTORY__ + "\MyRoutes.fs"

type Dummy = IsakSky.RouteProvider<
    "MyRoutes", // name of generated type
    routes,     // string of routes to base routes on
    false,      // add a generic input type?
    false,      // add a generic output type?
    outputPath>

open MyNamespace.MyModule

let router : MyRoutes =
  { getProject = fun p -> printfn "Hi project %d" p
    updateProject = fun ps -> printfn "Hi project string %s" ps
    getProjectComments = fun p c -> printfn "Hi project comment %d %d" p c
    createProject = fun p -> printfn "Creating project %d" p
    notFound = None }
```

You can use ```int64```, ```int```, or ```string``` as type annotations. The default is ```int64```.

Now we can use the router like this:

    router.DispatchRoute("GET", "projects/4321/comments/1234) // You can also pass a System.Uri
    -> "You asked for project 4321 and comment 1234"

You can also build paths in a typed way like this:

    let url = MyNamespace.MyModule.getProjectComments 123L 4L
    -> "/projects/123/comments/4"
    
To integrate with the web library you are using, you can specify that you want a generic input and output type. For example, if you pass in true for both, the signature of the Dispatch function will be like this:
    
    member DispatchRoute : context:'TContext * verb:string * uri:Uri -> 'TReturn

## Example

Example using Suave, utilizing both input and return types:

![Example](/demo.png?raw=true "Example")

## Brief demo in visual studio:

https://youtu.be/QTDNGyVx5Vo

## Roadmap / planned features
- Add support for Guids

## Comparison with other approaches

| Project         | Route definition mechanism                             | Bidirectional? | Type safety   |
|-----------------|:-------------------------------------------------------|:---------------|:--------------|
| ASP.NET MVC     | Reflection on attributes and method naming conventions | No             | Limited       |
| Freya           | Uri Templates                                          | Yes            | None          | 
| Suave.IO        | F# sprintf format string                               | No             | Yes           |
| bidi (Clojure)  | Data                                                   | Yes            | None          |
| Ruby on Rails   | Internal Ruby DSL                                      | Yes            | None          |
| Yesod (Haskell) | Types generated from Route DSL via Template Haskell    | Yes            | Full          |
| RouteProvider   | Types generated from Route DSL via #F Type Provider    | Yes            | Full          |

## Installation

You can install it via Nuget:

```Install-Package RouteProvider -Pre```

## How does it work?

It generates FSharp code. Here is an example of what would get generated for this route definition:

```FSharp
[<Literal>]
let routes = """
  GET projects/{projectId} as getProject
  GET projects/{projectId}/comments/{commentId} as getProjectComments
  PUT projects/{projectId:int} as updateProject
  GET projects/statistics
  GET people/{name:string} as getPerson
"""
```

Generated code:

```FSharp
namespace MyNamespace

open System
module MyModule =
  let getProject (projectId:int64) =
      "projects/" + projectId.ToString()
  let getProjectComments (projectId:int64) (commentId:int64) =
      "projects/" + projectId.ToString() + "comments/" + commentId.ToString()
  let updateProject (projectId:int) =
      "projects/" + projectId.ToString()
  let GET__projects_statistics  =
      "projects/statistics/"
  let getPerson (name:string) =
      "people/" + name

  module Internal =
    let fakeBaseUri = new Uri("http://a.a")

    exception RouteNotMatchedException of string * string

  type MyRoutes<'TContext, 'TReturn> =
    { getProject: 'TContext->int64->'TReturn
      getProjectComments: 'TContext->int64->int64->'TReturn
      updateProject: 'TContext->int->'TReturn
      GET__projects_statistics: 'TContext->'TReturn
      getPerson: 'TContext->string->'TReturn
      notFound: ('TContext->string->string->'TReturn) option }

    member inline private this.HandleNotFound(context, verb, path) =
      match this.notFound with
      | None -> raise (Internal.RouteNotMatchedException (verb, path))
      | Some(notFound) -> notFound context verb path

    member this.DispatchRoute(context:'TContext, verb:string, path:string) : 'TReturn =
      let parts = path.Split('/')
      let start = if parts.[0] = "" then 1 else 0
      let endOffset = if parts.Length > 0 && parts.[parts.Length - 1] = "" then 1 else 0
      match parts.Length - start - endOffset with
      | 4 ->
        if String.Equals(parts.[0 + start],"projects") then
          let mutable projectId = 0L
          if Int64.TryParse(parts.[1 + start], &projectId) then
            if String.Equals(parts.[2 + start],"comments") then
              let mutable commentId = 0L
              if Int64.TryParse(parts.[3 + start], &commentId) then
                if verb = "GET" then this.getProjectComments context projectId commentId
                else this.HandleNotFound(context, verb, path)
              else this.HandleNotFound(context, verb, path)
            else this.HandleNotFound(context, verb, path)
          else this.HandleNotFound(context, verb, path)
        else this.HandleNotFound(context, verb, path)
      | 2 ->
        if String.Equals(parts.[0 + start],"people") then
          if verb = "GET" then this.getPerson context (parts.[1 + start])
          else this.HandleNotFound(context, verb, path)
        elif String.Equals(parts.[0 + start],"projects") then
          let mutable int64ArgDepth_1 = 0L
          let mutable intArgDepth_1 = 0
          if String.Equals(parts.[1 + start],"statistics") then
            if verb = "GET" then this.GET__projects_statistics context
            else this.HandleNotFound(context, verb, path)
          elif Int64.TryParse(parts.[1 + start], &int64ArgDepth_1) then
            if verb = "GET" then this.getProject context int64ArgDepth_1
            else this.HandleNotFound(context, verb, path)
          elif Int32.TryParse(parts.[1 + start], &intArgDepth_1) then
            if verb = "PUT" then this.updateProject context intArgDepth_1
            else this.HandleNotFound(context, verb, path)
          else this.HandleNotFound(context, verb, path)
        else this.HandleNotFound(context, verb, path)
      | _ ->
        this.HandleNotFound(context, verb, path)

    member this.DispatchRoute(context:'TContext, verb:string, uri:Uri) : 'TReturn =
      // Ensure we have an Absolute Uri, or just about every method on Uri chokes
      let uri = if uri.IsAbsoluteUri then uri else new Uri(Internal.fakeBaseUri, uri)
      let path = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped)
      this.DispatchRoute(context, verb, path)
```

