// Generated by RouteProvider 0.0.1 args_hash:63614876
namespace MyNamespace

open System
module MyModule =
  let getProject (projectId:int64) =
      "projects/" + projectId.ToString()
  let updateProject (projectId:int64) =
      "projects/" + projectId.ToString()
  let getProjectComments (projectName:string) (commentId:int64) =
      "projects/" + projectName + "comments/" + commentId.ToString()

  module Internal =
    let fakeBaseUri = new Uri("http://a.a")

    exception RouteNotMatchedException of string * string

  type MyRoutes<'TContext, 'TReturn> =
    { getProject: 'TContext->int64->'TReturn
      updateProject: 'TContext->int64->'TReturn
      getProjectComments: 'TContext->string->int64->'TReturn
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
          if String.Equals(parts.[2 + start],"comments") then
            let mutable commentId = 0L
            if Int64.TryParse(parts.[3 + start], &commentId) then
              if verb = "GET" then this.getProjectComments context (parts.[1 + start]) commentId
              else this.HandleNotFound(context, verb, path)
            else this.HandleNotFound(context, verb, path)
          else this.HandleNotFound(context, verb, path)
        else this.HandleNotFound(context, verb, path)
      | 2 ->
        if String.Equals(parts.[0 + start],"projects") then
          let mutable projectId = 0L
          if Int64.TryParse(parts.[1 + start], &projectId) then
            if verb = "PUT" then this.updateProject context projectId
            elif verb = "GET" then this.getProject context projectId
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

    static member Router(getProject: 'TContext->int64->'TReturn,
                         updateProject: 'TContext->int64->'TReturn,
                         getProjectComments: 'TContext->string->int64->'TReturn,
                         ?notFound: 'TContext->string->string->'TReturn) : MyRoutes<_,_> =
      { getProject = getProject
        updateProject = updateProject
        getProjectComments = getProjectComments
        notFound = notFound}