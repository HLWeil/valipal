module Serialization.Tests

open Fable.Pyxpecto

let jUnit = testList "jUnit" [  
    testCase "placeHolder" <| fun () ->
        Expect.equal 1 1 "1 should equal 1"
]

let nUnit = testList "nUnit" [  
    testCase "placeHolder" <| fun () ->
        Expect.equal 1 1 "1 should equal 1"
]

let summaryJson = testList "summaryJson" [  
    testCase "placeHolder" <| fun () ->
        Expect.equal 1 1 "1 should equal 1"
]