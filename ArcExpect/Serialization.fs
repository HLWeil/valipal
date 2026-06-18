namespace ARCExpect

open ARCExpect.Helper
open ARCExpect.SimpleJson
open ARCExpect.SimpleXml
open System.IO

module Serialization =

    /// Combines a TestRunResults sequence into a single TestRunResults.
    let combineTestRunResults (results: seq<TestRunResults>) : TestRunResults =
        results
        |> Seq.reduce (fun r1 r2 -> { Entries = r1.Entries @ r2.Entries })

    // ── helpers ──────────────────────────────────────────────────────────

    let private entriesFromResult (result: ValidationResult) : TestRunEntry list =
        match result.OriginalRunSummary with
        | Some s -> s.Entries
        | None   -> []

    // ── JSON ─────────────────────────────────────────────────────────────

    let private resultToJson (r: ValidationResult) : JsonValue =
        JObject [
            "HasFailures", JBool r.HasFailures
            "Total",       JInt  r.Total
            "Passed",      JInt  r.Passed
            "Failed",      JInt  r.Failed
            "Errored",     JInt  r.Errored
        ]

    let private packageToJson (pkg: ValidationPackageSummary) : JsonValue =
        let fields = [
            "Name",        JString pkg.Name
            "Version",     JString pkg.Version
            "Summary",     JString pkg.Summary
            "Description", JString pkg.Description
        ]
        let fields =
            match pkg.CQCHookEndpoint with
            | Some ep -> fields @ [ "CQCHookEndpoint", JString ep ]
            | None    -> fields
        JObject fields

    let toJson (summary: ValidationSummary) : string =
        JObject [
            "Critical",          resultToJson summary.Critical
            "NonCritical",       resultToJson summary.NonCritical
            "ValidationPackage", packageToJson summary.ValidationPackage
        ]
        |> renderIndented 4

    let writeJson (path: string) (summary: ValidationSummary) =
        File.WriteAllText(path, toJson summary)

    // ── JUnit XML ────────────────────────────────────────────────────────

    // junit does not have an official xml spec; this targets GitLab CI:
    // https://docs.gitlab.com/ee/ci/junit_test_reports.html

    let private junitTestCase (entry: TestRunEntry) : XmlNode =
        let child =
            match entry.Outcome with
            | Passed      -> []
            | Failed  msg -> [ XmlElement("failure", [ "message", msg ], []) ]
            | Errored msg -> [ XmlElement("error",   [ "message", msg ], []) ]
            | Ignored     -> [ XmlElement("skipped", [],                 []) ]
        XmlElement("testcase", [ "name", entry.Name ], child)

    let private junitSuite (name: string) (entries: TestRunEntry list) : XmlNode =
        let failures = entries |> List.sumBy (fun e -> match e.Outcome with Failed  _ -> 1 | _ -> 0)
        let errors   = entries |> List.sumBy (fun e -> match e.Outcome with Errored _ -> 1 | _ -> 0)
        let skipped  = entries |> List.sumBy (fun e -> if e.Outcome = Ignored then 1 else 0)
        XmlElement("testsuite",
            [ "name",     name
              "tests",    string entries.Length
              "failures", string failures
              "errors",   string errors
              "skipped",  string skipped ],
            entries |> List.map junitTestCase)

    let toJUnitXml (summary: ValidationSummary) : string =
        let allEntries = entriesFromResult summary.Critical @ entriesFromResult summary.NonCritical
        let failures   = allEntries |> List.sumBy (fun e -> match e.Outcome with Failed  _ -> 1 | _ -> 0)
        let errors     = allEntries |> List.sumBy (fun e -> match e.Outcome with Errored _ -> 1 | _ -> 0)
        XmlElement("testsuites",
            [ "tests",    string allEntries.Length
              "failures", string failures
              "errors",   string errors ],
            [ junitSuite summary.ValidationPackage.Name allEntries ])
        |> renderDocument

    let writeJUnitXml (path: string) (summary: ValidationSummary) =
        File.WriteAllText(path, toJUnitXml summary)

    // ── NUnit v2 XML ─────────────────────────────────────────────────────

    // spec: http://nunit.org/docs/files/TestResult.xml

    let private nunitTestCase (entry: TestRunEntry) : XmlNode =
        let executed, result, success =
            match entry.Outcome with
            | Passed      -> "True",  "Success", "True"
            | Failed  _
            | Errored _   -> "True",  "Failure", "False"
            | Ignored     -> "False", "Ignored", "False"
        let failureChild =
            match entry.Outcome with
            | Failed  msg | Errored msg ->
                [ XmlElement("failure", [],
                    [ XmlElement("message", [], [ XmlCData msg ]) ]) ]
            | Ignored ->
                [ XmlElement("reason", [],
                    [ XmlElement("message", [], [ XmlText "Ignored" ]) ]) ]
            | Passed -> []
        XmlElement("test-case",
            [ "name",     entry.Name
              "executed", executed
              "result",   result
              "success",  success
              "asserts",  "0" ],
            failureChild)

    let toNUnitXml (summary: ValidationSummary) : string =
        let allEntries = entriesFromResult summary.Critical @ entriesFromResult summary.NonCritical
        let errors   = allEntries |> List.sumBy (fun e -> match e.Outcome with Errored _ -> 1 | _ -> 0)
        let failures = allEntries |> List.sumBy (fun e -> match e.Outcome with Failed  _ -> 1 | _ -> 0)
        let ignored  = allEntries |> List.sumBy (fun e -> if e.Outcome = Ignored then 1 else 0)
        let success  = errors = 0 && failures = 0
        XmlElement("test-results",
            [ "name",         summary.ValidationPackage.Name
              "total",        string allEntries.Length
              "errors",       string errors
              "failures",     string failures
              "ignored",      string ignored
              "not-run",      "0"
              "inconclusive", "0"
              "skipped",      "0"
              "invalid",      "0" ],
            [ XmlElement("test-suite",
                [ "type",    "Assembly"
                  "name",    summary.ValidationPackage.Name
                  "executed","True"
                  "result",  (if success then "Success" else "Failure")
                  "success", (if success then "True"    else "False")
                  "asserts", "0" ],
                [ XmlElement("results", [],
                    allEntries |> List.map nunitTestCase) ]) ])
        |> renderDocument

    let writeNUnitXml (path: string) (summary: ValidationSummary) =
        File.WriteAllText(path, toNUnitXml summary)