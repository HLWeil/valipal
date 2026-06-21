module Execution.Tests

open Fable.Pyxpecto
open ARCExpect

let private validationPackage =
    Setup.ValidationPackage(
        name = "execution-tests",
        summary = "Tests for executing validation packages",
        description = "Exercises execution and badge creation.",
        majorVersion = 1,
        minorVersion = 0,
        patchVersion = 0,
        programmingLanguage = "fsharp",
        CriticalValidationCases = [
            testCase "critical pass" <| fun () -> Expect.isTrue true "critical test passes"
        ],
        NonCriticalValidationCases = [
            testCase "non-critical failure" <| fun () -> Expect.equal 1 2 "non-critical test fails"
        ]
    )

let private criticalFailurePackage =
    Setup.ValidationPackage(
        name = "critical-failure-tests",
        summary = "Tests for critical failure badges",
        description = "Exercises the critical failure badge branch.",
        majorVersion = 1,
        minorVersion = 0,
        patchVersion = 0,
        programmingLanguage = "fsharp",
        CriticalValidationCases = [
            testCase "critical failure" <| fun () -> Expect.equal 1 2 "critical test fails"
        ]
    )

let private validationSummaryAsync () =
    validationPackage |> Execute.ValidationAsync()

let private criticalFailureSummaryAsync () =
    criticalFailurePackage |> Execute.ValidationAsync()

let execution = testList "execution" [
    testCaseAsync "Validation runs critical and non-critical validation cases" <| async {
        let! summary = validationSummaryAsync ()

        Expect.equal summary.ValidationPackage.Name "execution-tests" "package metadata is retained"
        Expect.equal summary.Critical.Total 1 "one critical test was run"
        Expect.equal summary.Critical.Passed 1 "critical test passed"
        Expect.equal summary.NonCritical.Total 1 "one non-critical test was run"
        Expect.equal summary.NonCritical.Failed 1 "non-critical failure is recorded"
        Expect.isFalse summary.Critical.HasFailures "critical suite has no failures"
        Expect.isTrue summary.NonCritical.HasFailures "non-critical suite has a failure"
    }

    testCaseAsync "Validation retains individual test outcomes" <| async {
        let! summary = validationSummaryAsync ()
        let criticalEntries = summary.Critical.OriginalRunSummary.Value.Entries
        let nonCriticalEntries = summary.NonCritical.OriginalRunSummary.Value.Entries

        Expect.equal criticalEntries.Length 1 "critical result contains one entry"
        Expect.equal nonCriticalEntries.Length 1 "non-critical result contains one entry"
        Expect.equal criticalEntries.Head.Outcome Passed "critical entry passed"

        match nonCriticalEntries.Head.Outcome with
        | Failed message -> Expect.isTrue (message.Contains "non-critical test fails") "failure message is retained"
        | outcome -> failwithf "Expected a failed non-critical test, got %A" outcome
    }
]

let badgeCreation = testList "badge creation" [
    testCaseAsync "creates a result badge with the passed-to-total value" <| async {
        let! summary = validationSummaryAsync ()
        let badge =
            summary.Critical.OriginalRunSummary.Value
            |> BadgeCreation.ofTestResults "critical tests"

        Expect.isTrue (badge.BadgeSvgText.Contains "critical tests") "badge includes its label"
        Expect.isTrue (badge.BadgeSvgText.Contains "1/1") "badge includes passed and total tests"
    }

    testCaseAsync "creates a validation badge that includes non-critical results" <| async {
        let! summary = validationSummaryAsync ()
        let badge = summary |> BadgeCreation.ofValidationSummary "ARC validation"

        Expect.isTrue (badge.BadgeSvgText.Contains "ARC validation") "badge includes its label"
        Expect.isTrue (badge.BadgeSvgText.Contains "1/2") "badge includes all passed and total tests"
    }

    testCaseAsync "creates a critical-error badge when critical tests fail" <| async {
        let! summary = criticalFailureSummaryAsync ()
        let badge = summary |> BadgeCreation.ofValidationSummary "ARC validation"

        Expect.isTrue (badge.BadgeSvgText.Contains "ARC validation") "badge includes its label"
        Expect.isTrue (badge.BadgeSvgText.Contains "1 Critical Errors") "badge reports the critical error count"
    }

#if !FABLE_COMPILER_PYTHON
    testCase "writes a badge SVG through Execute.BadgeCreation" <| fun () ->
        let path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"arcexpect-{System.Guid.NewGuid()}.svg")

        try
            validationPackage
            |> Execute.Validation()
            |> Execute.BadgeCreation(path, "ARC validation")

            Expect.isTrue (System.IO.File.Exists path) "badge SVG is written"
            Expect.isTrue ((System.IO.File.ReadAllText path).Contains "ARC validation") "written SVG includes the label"
        finally
            if System.IO.File.Exists path then
                System.IO.File.Delete path
#endif
]
