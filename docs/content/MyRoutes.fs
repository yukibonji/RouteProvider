// Generated by RouteProvider 0.0.0.0
namespace MyNamespace

open System
module MyModule =
  let getProject (projectId:int64) =
      "projects/" + projectId.ToString()
  let updateProject (foo:string) =
      "projects/" + foo
  let createProject (projectName:string) =
      "projects/" + projectName
  let getProjectComments (projectId:int64) (commentId:int64) =
      "projects/" + projectId.ToString() + "comments/" + commentId.ToString()

  module Internal =
    let fakeBaseUri = new Uri("http://a.a")

    exception RouteNotMatchedException of string * string

  type MyRoutes =
    { getProject: int64->unit
      updateProject: string->unit
      createProject: string->unit
      getProjectComments: int64->int64->unit
      notFound: (string->string->unit) option }

    member inline private this.HandleNotFound(verb, path) =
      match this.notFound with
      | None -> raise (Internal.RouteNotMatchedException (verb, path))
      | Some(notFound) -> notFound verb path

    member this.DispatchRoute(verb:string, path:string) : unit =
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
                if verb = "GET" then this.getProjectComments projectId commentId
                else this.HandleNotFound(verb, path)
              else this.HandleNotFound(verb, path)
            else this.HandleNotFound(verb, path)
          else this.HandleNotFound(verb, path)
        else this.HandleNotFound(verb, path)
      | 2 ->
        if String.Equals(parts.[0 + start],"projects") then
          let mutable projectId = 0L
          if Int64.TryParse(parts.[1 + start], &projectId) then
            if verb = "GET" then this.getProject projectId
            elif verb = "POST" then this.createProject (parts.[1 + start])
            elif verb = "PUT" then this.updateProject (parts.[1 + start])
            else this.HandleNotFound(verb, path)
          else
            if verb = "POST" then this.createProject (parts.[1 + start])
            elif verb = "PUT" then this.updateProject (parts.[1 + start])
            else this.HandleNotFound(verb, path)
        else this.HandleNotFound(verb, path)
      | _ ->
        this.HandleNotFound(verb, path)

    member this.DispatchRoute(verb:string, uri:Uri) : unit =
      // Ensure we have an Absolute Uri, or just about every method on Uri chokes
      let uri = if uri.IsAbsoluteUri then uri else new Uri(Internal.fakeBaseUri, uri)
      let path = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped)
      this.DispatchRoute(verb, path)