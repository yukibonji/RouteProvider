// Generated by RouteProvider 0.0.0.0
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
    type TryParseState =
    | Untried = 0
    | Success = 1
    | Failed = 2

    let tryParseInt32 (s:string, parseState: TryParseState byref, result: int byref) =
      match parseState with
      | TryParseState.Failed
      | TryParseState.Success -> ()
      | _ ->
        parseState <- match Int32.TryParse(s, &result) with
        | true -> TryParseState.Success
        | false -> TryParseState.Failed
      parseState = TryParseState.Success

    let tryParseInt64 (s:string, parseState: TryParseState byref, result: int64 byref) =
      match parseState with
      | TryParseState.Failed
      | TryParseState.Success -> ()
      | _ ->
        parseState <- match Int64.TryParse(s, &result) with
        | true -> TryParseState.Success
        | false -> TryParseState.Failed
      parseState = TryParseState.Success

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
          let mutable projectId_parseState = Internal.TryParseState.Untried
          if Internal.tryParseInt64(parts.[1 + start], &projectId_parseState, &projectId) then
            if String.Equals(parts.[2 + start],"comments") then
              let mutable commentId = 0L
              let mutable commentId_parseState = Internal.TryParseState.Untried
              if Internal.tryParseInt64(parts.[3 + start], &commentId_parseState, &commentId) then
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
          let mutable int64ArgDepth_1_parseState = Internal.TryParseState.Untried
          let mutable intArgDepth_1 = 0
          let mutable intArgDepth_1_parseState = Internal.TryParseState.Untried
          if String.Equals(parts.[1 + start],"statistics") then
            if verb = "GET" then this.GET__projects_statistics context
            else this.HandleNotFound(context, verb, path)
          elif Internal.tryParseInt64(parts.[1 + start], &int64ArgDepth_1_parseState, &int64ArgDepth_1) then
            if verb = "GET" then this.getProject context int64ArgDepth_1
            else this.HandleNotFound(context, verb, path)
          elif Internal.tryParseInt32(parts.[1 + start], &intArgDepth_1_parseState, &intArgDepth_1) then
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
