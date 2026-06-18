module TestRunner.Tests

open Fable.Pyxpecto

let testRunner = testList "testRunner" [  
    testCase "placeHolder" <| fun () ->
        Expect.equal 1 1 "1 should equal 1"
]