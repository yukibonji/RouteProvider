﻿namespace IsakSky.RouteProvider
open Route
open FParsec

module RouteParsing =
  let pidentifier = identifier (new IdentifierOptions())

  let pVerb = many1SatisfyL isAsciiUpper "1 OR more upper case letters (HTTP VERB)"

  let pTypeAnnotation = pchar ':' >>? choice [attempt <| pstring "int64" >>% Int64Seg
                                              attempt <| pstring "int" >>% IntSeg
                                              attempt <| pstring "Guid" >>% GuidSeg
                                              pstring "string" >>% StringSeg]

  let isNameStart c = isLetter (c) || c = '_'
  let isNameCont c = isLetter (c) || isDigit (c) || c = '_'
  let pname = many1Satisfy2L isNameStart isNameCont "1 or more digit, letters or underscores"

  let isConstSegChar c =
    isLetter(c) ||
    isDigit(c) ||
    match c with
    | '_' | '.' | '_' | '~' | '!'
    | '$' | '&' | ''' | '(' |')'
    | '*' | '+' | ';' | '=' | ':' | '@' -> true
    | _ -> false

  let pConstantSeg = many1Satisfy isConstSegChar |>> ConstantSeg

  let pDynSeg =
    pname .>>. opt pTypeAnnotation |>> fun (name, ann) ->
    match ann with
    | Some(segFn) -> segFn name
    | None -> Int64Seg(name)

  let pDynamicPathSeg = between (pchar '{') (pchar '}') pDynSeg

  let pPathSeg = choice [ pConstantSeg; pDynamicPathSeg ]

  let pPath = opt (pchar '/') >>. sepEndBy1 pPathSeg (pchar '/')
  let pRouteName = pstringCI "AS" >>. spaces >>. pidentifier

  let pRoute =
    tuple3
      pVerb
      (spaces >>. pPath)
      (opt (spaces >>? pRouteName))
    |>> fun (verb, segs, routeName) -> { verb=verb; routeSegments = segs; routeName = routeName }

  let pRoutes : Parser<_, unit> = spaces >>. sepEndBy1 pRoute spaces .>> eof

  let inline private testP p s =
    runParserOnString  p () "Test" s

  type RouteParseResult = Success of Route list | Failure of string

  let parseRoutes routesStr =
    match runParserOnString pRoutes () "User routes" routesStr with
    | ParserResult.Success(routes,_, _) ->
      Success(routes)
    | ParserResult.Failure (msg,_,_) ->
      Failure(msg)
