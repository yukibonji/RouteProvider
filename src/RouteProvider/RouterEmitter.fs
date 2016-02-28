﻿namespace IsakSky.RouteProvider

open Route
open RouteCompilation
open System.IO
open System
open Microsoft.FSharp.Compiler.SourceCodeServices
open Utility
open System.Runtime.InteropServices
open RouteCompilation

type RouterEmissionArgs = {
  typeName: string
  parse: Route list
  outputPath: string
  inputType: bool
  returnType: bool
  nameSpace: string option
  moduleName: string option
} 

type RouterEmissionMessage =
| Enqueue of RouterEmissionArgs * AsyncReplyChannel<RouterEmissionResult>
and RouterEmissionResult = 
| IgnoredStale
| IgnoredBadExtension
| Ok
| OkSecondaryThread

type EmitterState = {
  lastWrote: DateTime
}

//module RouteEmitterUtils =
//  type OutputFileCheck = 
//  | Exists 
//  | Ok 
//  | BadExtension
//  | NotFoundOnDiskOrFile
//
//  let getSourceTokens sourceFile = 
//    let sourceTok = FSharpSourceTokenizer([], sourceFile)
//    seq {
//      for line in File.ReadLines(sourceFile) do
//        let lineTok = sourceTok.CreateLineTokenizer(line)
//        let stop = ref false
//        let tokState = ref 0L
//        while not !stop do
//          match lineTok.ScanToken(!tokState) with
//          | Some tok, state -> 
//            tokState := state
//            yield line, tok            
//          | None, _ -> stop := true          
//    }
//
//  let referencesFile (line:string) (tok:FSharpTokenInfo) (filename:string) = 
//    if tok.TokenName = "STRING_TEXT" then
//      let matchIdx = line.IndexOf(filename, tok.LeftColumn, tok.RightColumn - tok.LeftColumn, StringComparison.InvariantCulture)
//      matchIdx <> -1
//    else false
//
//  let doesSourceMentionOutputFile (sourceFile:string) (outputFile:string) =
//    let filename = Path.GetFileName(outputFile)
//    sourceFile
//    |> getSourceTokens
//    |> Seq.exists (fun (line, tok) -> referencesFile line tok filename)
//
//  let checkOutputFile (sourceFile:string) (outputFile:string) =
//    if not <| outputFile.EndsWith(".cs") && not <| outputFile.EndsWith(".dll") then
//      BadExtension
//    elif File.Exists(outputFile) then
//      Exists
//    else
//      // We need to create it, but only do that if the source file that is on disk
//      // mentions the filename. This can prevent creating tons of files if the user
//      // is currently typing the filename.
//      if doesSourceMentionOutputFile sourceFile outputFile then
//        Ok
//      else 
//        NotFoundOnDiskOrFile
      

type RouterEmitter(outputPath : string) = 
  let expire = new Event<EventHandler,EventArgs>()
  let maxWait = 200
  let waitIncr = 50
  let maxWaitNoMessage = 2000
  let waitNonExistantFile = 3000

  do
    log "[RouterEmitter]: Starting for %s" outputPath

  member private this.HandleMessage(emissionArgs:RouterEmissionArgs) =
    let path = emissionArgs.outputPath
    Directory.CreateDirectory(Path.GetDirectoryName(path)) |> ignore
    try     
      use f = File.Open(emissionArgs.outputPath, FileMode.Create, FileAccess.Write, FileShare.None)
      use sw = new StreamWriter(f)
      log "Handling message %d" (emissionArgs.GetHashCode())

      let compArgs = {
        typeName = emissionArgs.typeName
        parse = emissionArgs.parse
        inputType = emissionArgs.inputType
        returnType = emissionArgs.returnType
        outputType = CSharp
      }

      compileRoutes compArgs sw

      Ok
    with
    | :? System.IO.IOException ->
      log "Ah, someone beat us to it."
      OkSecondaryThread
  member private this.Mail : MailboxProcessor<RouterEmissionMessage> = MailboxProcessor.Start(fun inbox ->
    let rec loop (numSkipped, waited) = 
      async {
        let! msg = inbox.TryReceive(maxWaitNoMessage)
        match msg with
        | None ->
          expire.Trigger(this, new EventArgs())
          return ()
        | Some(Enqueue(args, replyChan)) ->
          if waited < maxWait then do! Async.Sleep waitIncr
          if waited < maxWait && inbox.CurrentQueueLength > 0 then
            replyChan.Reply(IgnoredStale)
            return! loop (numSkipped + 1, waited + waitIncr)
          else
            // We're on the last message, or have waited long enough
            // lets process it. maybe.
            let outputFile = args.outputPath
            if not <| outputFile.EndsWith(".cs") && not <| outputFile.EndsWith(".dll") then
              replyChan.Reply(IgnoredBadExtension)
            else
              if not <| File.Exists(outputFile) then
                // lets wait a little bit more before creating it, 
                // in case they are still working out the filename
                do! Async.Sleep(waitNonExistantFile)
                if inbox.CurrentQueueLength > 0 then
                  replyChan.Reply(IgnoredStale)
                  return! loop (numSkipped + 1, waited + waitNonExistantFile)
                else
                  replyChan.Reply <| this.HandleMessage(args)
                  return! loop (numSkipped, 0)
              else
                replyChan.Reply <| this.HandleMessage(args)
                return! loop (numSkipped, 0)
            return! loop (numSkipped, 0)

      }      
    loop (0, 0))

  member this.PostMessage(args:RouterEmissionArgs) =
    this.Mail.PostAndReply(fun chan -> Enqueue(args, chan))
  
  [<CLIEvent>]
  member this.Expired =
    expire.Publish

//  member this.Scan() =
//    let rec scanDir dir (fsFiles:ResizeArray<_>) (projFiles:ResizeArray<_>) (scriptFiles:ResizeArray<_>) =
//      for f in Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly) do
//        if f.EndsWith(".fsproj") then
//          projFiles.Add(f)
//        elif f.EndsWith(".fs") then
//          fsFiles.Add(f)
//        elif f.EndsWith(".fsx") then
//          scriptFiles.Add(f)
//
//      for d in Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly) do
//        if d <> "bin" && d <> "obj" then scanDir d fsFiles projFiles scriptFiles
//
//    let fsFiles = ResizeArray<_>()
//    let projFiles = ResizeArray<_>()
//    let scriptFiles = ResizeArray<_>()
//    let scanPath = cfg.config.Value.ResolutionFolder
//    scanDir scanPath fsFiles projFiles scriptFiles
//
//    let projFileContents = ResizeArray<_>()
//    
//    let rec findProj
//
//
//    (**
//      INPUT
//      OUTPUT
//
//    **)
//
//    let fs = seq  {
//      yield! projFiles
//      yield! scriptFiles
//      yield! fsFiles
//    }
//    ()
//    
//
//  interface System.IDisposable with
//    member this.Dispose() = ()
    



