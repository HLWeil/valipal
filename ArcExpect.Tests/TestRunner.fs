module TestRunner.Tests

open Fable.Pyxpecto
open ARCExpect

// ── helpers ──────────────────────────────────────────────────────────────────

/// Run a suite with the ARCExpect runner and return the flat entry list.
let private run suite =
    async {
        let! results = PyxpectoRunner.runTestsWithResultsAsync suite
        return results.Entries
    }

let private testCaseWithResults suite name assertion =
    testCaseAsync name <| async {
        let! results = run suite
        assertion results
    }

let private passedNames  results = results |> List.choose (fun e -> match e.Outcome with Passed      -> Some e.Name | _ -> None)
let private failedNames  results = results |> List.choose (fun e -> match e.Outcome with Failed  _   -> Some e.Name | _ -> None)
let private erroredNames results = results |> List.choose (fun e -> match e.Outcome with Errored _   -> Some e.Name | _ -> None)
let private ignoredNames results = results |> List.choose (fun e -> if e.Outcome = Ignored then Some e.Name else None)

let private failedMsg name results =
    results |> List.tryPick (fun e -> match e with { Name = n; Outcome = Failed msg } when (n: string).Contains(name: string) -> Some msg | _ -> None)

let private erroredMsg name results =
    results |> List.tryPick (fun e -> match e with { Name = n; Outcome = Errored msg } when (n: string).Contains(name: string) -> Some msg | _ -> None)

// ── test data ────────────────────────────────────────────────────────────────

// These tests are not themselves testing the runner; they are just sample suites to be run by the runner in the test cases below.
// Each suite is designed to have a specific combination of outcomes, and the test cases verify that the runner produces the expected outcomes for each test.

let private passingTests =
    testList "Suite" [
        testCase "pass1" <| fun () -> Expect.equal 1 1 "1 = 1"
        testCase "pass2" <| fun () -> Expect.isTrue true "true is true"
        testCase "pass3" <| fun () -> Expect.equal "hello" "hello" "strings equal"
    ]

let private failingTests =
    testList "Suite" [
        testCase "fail1" <| fun () -> Expect.equal 1 2 "deliberately wrong"
        testCase "fail2" <| fun () -> Expect.isTrue false "deliberately false"
    ]

let private erroringTests =
    testList "Suite" [
        testCase "err1" <| fun () -> raise (System.Exception "deliberate exception")
        testCase "err2" <| fun () -> failwith "deliberate failwith"
    ]

let private pendingTests =
    testList "Suite" [
        ptestCase "pending1" <| fun () -> failwith "should not run"
        ptestCase "pending2" <| fun () -> failwith "should not run"
    ]

let private mixedTests =
    testList "Suite" [
        testCase "ok"      <| fun () -> Expect.equal 42 42 "42 = 42"
        testCase "bad"     <| fun () -> Expect.equal 1 99 "1 ≠ 99"
        testCase "throws"  <| fun () -> raise (System.InvalidOperationException "oops")
        ptestCase "skip"   <| fun () -> failwith "should not run"
    ]

let private asyncPassingTests =
    testList "Suite" [
        testCaseAsync "asyncPass" <| async { Expect.equal 2 2 "2 = 2" }
    ]

let private asyncFailingTests =
    testList "Suite" [
        testCaseAsync "asyncFail" <| async { Expect.equal 1 2 "1 ≠ 2" }
    ]

let private asyncErroringTests =
    testList "Suite" [
        testCaseAsync "asyncErr" <| async { raise (System.Exception "async exception") }
    ]

// ── tests ─────────────────────────────────────────────────────────────────────

let testRunner = testList "testRunner" [

    testList "passing tests" [
        testCaseWithResults passingTests "all entries are Passed" <| fun results ->
            Expect.equal results.Length 3 "3 tests"
            Expect.equal (passedNames results).Length 3 "all passed"

        testCaseWithResults passingTests "no failures or errors" <| fun results ->
            Expect.isEmpty (failedNames  results) "no failures"
            Expect.isEmpty (erroredNames results) "no errors"

        testCaseWithResults passingTests "test names are preserved" <| fun results ->
            let names = results |> List.map (fun e -> e.Name)
            Expect.isTrue (names |> List.exists (fun n -> n.Contains "pass1")) "pass1 present"
            Expect.isTrue (names |> List.exists (fun n -> n.Contains "pass2")) "pass2 present"
            Expect.isTrue (names |> List.exists (fun n -> n.Contains "pass3")) "pass3 present"
    ]

    testList "failing tests" [
        testCaseWithResults failingTests "all entries are Failed" <| fun results ->
            Expect.equal results.Length 2 "2 tests"
            Expect.equal (failedNames results).Length 2 "all failed"

        testCaseWithResults failingTests "failed outcomes carry messages" <| fun results ->
            for e in results do
                match e.Outcome with
                | Failed msg -> Expect.isFalse (msg = "") "message is non-empty"
                | other      -> failwithf "Expected Failed but got %A for %s" other e.Name

        testCaseWithResults failingTests "failure message contains expected text" <| fun results ->
            let msg = failedMsg "fail1" results
            Expect.isSome msg "fail1 message present"
            Expect.isTrue (msg.Value.Contains "deliberately wrong") "message text correct"

        testCaseWithResults failingTests "no errors or ignores" <| fun results ->
            Expect.isEmpty (erroredNames results) "no errors"
            Expect.isEmpty (ignoredNames results) "no ignores"
    ]

    testList "erroring tests" [
        testCaseWithResults erroringTests "all entries are Errored" <| fun results ->
            Expect.equal results.Length 2 "2 tests"
            Expect.equal (erroredNames results).Length 2 "all errored"

        testCaseWithResults erroringTests "errored outcomes carry exception messages" <| fun results ->
            for e in results do
                match e.Outcome with
                | Errored msg -> Expect.isFalse (msg = "") "message is non-empty"
                | other       -> failwithf "Expected Errored but got %A for %s" other e.Name

        testCaseWithResults erroringTests "exception message is captured for explicit raise" <| fun results ->
            let msg = erroredMsg "err1" results
            Expect.isSome msg "err1 message present"
            Expect.isTrue (msg.Value.Contains "deliberate exception") "message text correct"

        testCaseWithResults erroringTests "exception message is captured for failwith" <| fun results ->
            let msg = erroredMsg "err2" results
            Expect.isSome msg "err2 message present"
            Expect.isTrue (msg.Value.Contains "deliberate failwith") "message text correct"

        testCaseWithResults erroringTests "no failures or ignores" <| fun results ->
            Expect.isEmpty (failedNames  results) "no failures"
            Expect.isEmpty (ignoredNames results) "no ignores"
    ]

    testList "pending (skipped) tests" [
        testCaseWithResults pendingTests "pending tests are recorded as Ignored" <| fun results ->
            Expect.equal results.Length 2 "2 tests"
            Expect.equal (ignoredNames results).Length 2 "both ignored"

        testCaseWithResults pendingTests "no passed / failed / errored from pending tests" <| fun results ->
            Expect.isEmpty (passedNames  results) "no passes"
            Expect.isEmpty (failedNames  results) "no failures"
            Expect.isEmpty (erroredNames results) "no errors"
    ]

    testList "mixed suite" [
        testCaseWithResults mixedTests "counts are correct" <| fun results ->
            Expect.equal results.Length              4 "4 entries total"
            Expect.equal (passedNames  results).Length 1 "1 passed"
            Expect.equal (failedNames  results).Length 1 "1 failed"
            Expect.equal (erroredNames results).Length 1 "1 errored"
            Expect.equal (ignoredNames results).Length 1 "1 ignored"

        testCaseAsync "TestRunResults members agree" <| async {
            let! res = PyxpectoRunner.runTestsWithResultsAsync mixedTests
            Expect.equal res.Passed.Length  1 "Passed member"
            Expect.equal res.Failed.Length  1 "Failed member"
            Expect.equal res.Errored.Length 1 "Errored member"
            Expect.equal res.Ignored.Length 1 "Ignored member"
        }
    ]

    testList "async tests" [
        testCaseWithResults asyncPassingTests "async passing test is Passed" <| fun results ->
            Expect.equal results.Length 1 "1 test"
            Expect.equal (passedNames results).Length 1 "passed"

        testCaseWithResults asyncFailingTests "async failing test is Failed" <| fun results ->
            Expect.equal results.Length 1 "1 test"
            Expect.equal (failedNames results).Length 1 "failed"

        testCaseWithResults asyncErroringTests "async throwing test is Errored" <| fun results ->
            Expect.equal results.Length 1 "1 test"
            Expect.equal (erroredNames results).Length 1 "errored"
            let msg = erroredMsg "asyncErr" results
            Expect.isSome msg "message present"
            Expect.isTrue (msg.Value.Contains "async exception") "async error message correct"
    ]

    testList "empty suite" [
        testCaseWithResults (testList "Empty" []) "no entries from empty testList" <| fun results ->
            Expect.isEmpty results "no entries"
    ]

    testList "combineTestRunResults" [
        testCaseAsync "combines entries from two results" <| async {
            let! r1 = PyxpectoRunner.runTestsWithResultsAsync passingTests
            let! r2 = PyxpectoRunner.runTestsWithResultsAsync failingTests
            let combined = Serialization.combineTestRunResults [r1; r2]
            Expect.equal combined.Entries.Length 5 "3 + 2 = 5 entries"
        }

        testCaseAsync "order is preserved: first suite entries come first" <| async {
            let! r1 = PyxpectoRunner.runTestsWithResultsAsync passingTests
            let! r2 = PyxpectoRunner.runTestsWithResultsAsync failingTests
            let combined = Serialization.combineTestRunResults [r1; r2]
            let first3 = combined.Entries |> List.take 3
            Expect.equal (first3 |> List.filter (fun e -> e.Outcome = Passed)).Length 3 "first 3 all passed"
        }
    ]
]
