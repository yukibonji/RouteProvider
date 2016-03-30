// Generated by RouteProvider 0.0.0.0
namespace Ns3

open System
module MyModule =
  let getProjStats  =
      "projects/statistics/"
  let getProjAction (action:string) =
      "projects/" + action

  module Internal =
    let fakeBaseUri = new Uri("http://a.a")

    exception RouteNotMatchedException of string * string

  type MyRoutes3<'TContext, 'TReturn> =
    { getProjStats: 'TContext->'TReturn
      getProjAction: 'TContext->string->'TReturn
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
      | 2 ->
        if String.Equals(parts.[0 + start],"projects") then
          if String.Equals(parts.[1 + start],"statistics") then
            if verb = "GET" then this.getProjStats context
            else this.HandleNotFound(context, verb, path)
          else
            if verb = "GET" then this.getProjAction context (parts.[1 + start])
            else this.HandleNotFound(context, verb, path)
        else this.HandleNotFound(context, verb, path)
      | _ ->
        this.HandleNotFound(context, verb, path)

    member this.DispatchRoute(context:'TContext, verb:string, uri:Uri) : 'TReturn =
      // Ensure we have an Absolute Uri, or just about every method on Uri chokes
      let uri = if uri.IsAbsoluteUri then uri else new Uri(Internal.fakeBaseUri, uri)
      let path = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped)
      this.DispatchRoute(context, verb, path)
