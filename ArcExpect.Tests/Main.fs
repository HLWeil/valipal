module Main.Tests

open Fable.Pyxpecto
open Fable.Core

let all = testList "All" [  
    Serialization.Tests.jUnit
    Serialization.Tests.nUnit
    Serialization.Tests.summaryJson
    TestRunner.Tests.testRunner
    Frontmatter.Tests.frontmatter
    Execution.Tests.execution
    Execution.Tests.badgeCreation
]


[<EntryPoint>]
let main argv = Pyxpecto.runTests [||] all
