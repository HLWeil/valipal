namespace ARCExpect

open Fable.Pyxpecto
open Fable.Pyxpecto.Model
open Fable.Pyxpecto.Pyxpecto.Util


    /// Represents the outcome of a single test execution.
    type TestOutcome =
        | Passed
        | Failed of message: string
        | Errored of message: string
        | Ignored

    /// Represents the result of a single test in a run.
    type TestRunEntry = {
        Name: string
        Outcome: TestOutcome
    }

    /// Aggregated results of a test run, capturing each individual test outcome.
    /// Unlike <see cref="TestRunSummary"/>, every test is tracked individually with its outcome.
    type TestRunResults = {
        Entries: TestRunEntry list
    } with
        member this.Passed  = this.Entries |> List.filter (fun e -> e.Outcome = Passed)
        member this.Failed  = this.Entries |> List.filter (fun e -> match e.Outcome with Failed _  -> true | _ -> false)
        member this.Errored = this.Entries |> List.filter (fun e -> match e.Outcome with Errored _ -> true | _ -> false)
        member this.Ignored = this.Entries |> List.filter (fun e -> e.Outcome = Ignored)

module PyxpectoRunner =

    let private runFlatTest (ft: Fable.Pyxpecto.Model.FlatTest) : Async<TestRunEntry> =
        let name = ft.fullname
        async {
            match ft.test with
            | Fable.Pyxpecto.Model.TestCode.Sync body ->
                try
                    body ()
                    return { Name = name; Outcome = Passed }
                with
                | :? AssertException as exn ->
                    return { Name = name; Outcome = Failed exn.Message }
                | e ->
                    return { Name = name; Outcome = Errored e.Message }
            | Fable.Pyxpecto.Model.TestCode.Async body ->
                try
                    do! body
                    return { Name = name; Outcome = Passed }
                with
                | :? AssertException as exn ->
                    return { Name = name; Outcome = Failed exn.Message }
                | e ->
                    return { Name = name; Outcome = Errored e.Message }
        }

    /// <summary>
    /// Runs the given test suite and returns a <see cref="TestRunResults"/> capturing each test's individual outcome.
    /// Unlike the standard runner, this function does not throw or call <c>Environment.Exit</c> on failures or errors.
    /// Pending and unfocused tests are recorded as <see cref="TestOutcome.Ignored"/>.
    /// </summary>
    let runTestsWithResults (tests: Fable.Pyxpecto.Model.TestCase) : TestRunResults =
        let runner = CustomTestRunner(tests)
        let runTests, pendingTests, unfocusedTests = sortTests runner

        let run =
            async {
                let entries = ResizeArray<TestRunEntry>()
                for ft in runTests do
                    let! entry = runFlatTest ft
                    entries.Add(entry)
                for ft in pendingTests do
                    entries.Add({ Name = ft.fullname; Outcome = Ignored })
                for ft in unfocusedTests do
                    entries.Add({ Name = ft.fullname; Outcome = Ignored })
                return entries |> Seq.toList
            }

        { Entries = run |> Async.RunSynchronously }
