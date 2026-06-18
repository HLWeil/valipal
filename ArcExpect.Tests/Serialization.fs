module Serialization.Tests

open Fable.Pyxpecto
open ARCExpect

// ── shared fixtures ───────────────────────────────────────────────────────────

let private passSuite =
    testList "s" [
        testCase "p1" <| fun () -> Expect.equal 1 1 "ok"
        testCase "p2" <| fun () -> Expect.equal 2 2 "ok"
    ]

let private failSuite =
    testList "s" [
        testCase "f1" <| fun () -> Expect.equal 1 2 "fail"
        testCase "f2" <| fun () -> Expect.isTrue false "also fail"
    ]

let private errSuite =
    testList "s" [
        testCase "e1" <| fun () -> raise (System.Exception "boom")
    ]

let private skipSuite =
    testList "s" [
        ptestCase "sk1" <| fun () -> ()
    ]

let private makeResults criticalSuite nonCriticalSuite =
    let crit    = PyxpectoRunner.runTestsWithResults criticalSuite
    let nonCrit = PyxpectoRunner.runTestsWithResults nonCriticalSuite
    ValidationSummary.ofExpectoTestRunSummaries(
        crit, nonCrit,
        ValidationPackageSummary.create("test-pkg", "1.2.3", "A test package", "For unit testing"))

let private allPassSummary  = makeResults passSuite passSuite
let private allFailSummary  = makeResults failSuite failSuite
let private allErrorSummary = makeResults errSuite  errSuite
let private mixedSummary    = makeResults passSuite failSuite

let private withHook =
    let crit = PyxpectoRunner.runTestsWithResults passSuite
    let nonCrit = PyxpectoRunner.runTestsWithResults (testList "s" [])
    ValidationSummary.ofExpectoTestRunSummaries(
        crit, nonCrit,
        ValidationPackageSummary.create("hook-pkg", "0.1.0", "pkg with hook", "desc",
            CQCHookEndpoint = "https://example.com/hook"))

// ── JSON ──────────────────────────────────────────────────────────────────────

let summaryJson = testList "summaryJson" [

    testList "structure" [
        testCase "output is valid JSON object with required top-level keys" <| fun () ->
            let json = Serialization.toJson allPassSummary
            Expect.isTrue (json.Contains "\"Critical\"")          "has Critical"
            Expect.isTrue (json.Contains "\"NonCritical\"")       "has NonCritical"
            Expect.isTrue (json.Contains "\"ValidationPackage\"") "has ValidationPackage"

        testCase "ValidationPackage contains Name Version Summary Description" <| fun () ->
            let json = Serialization.toJson allPassSummary
            Expect.isTrue (json.Contains "\"Name\"")        "has Name"
            Expect.isTrue (json.Contains "\"Version\"")     "has Version"
            Expect.isTrue (json.Contains "\"Summary\"")     "has Summary"
            Expect.isTrue (json.Contains "\"Description\"") "has Description"

        testCase "package metadata values are serialized correctly" <| fun () ->
            let json = Serialization.toJson allPassSummary
            Expect.isTrue (json.Contains "\"test-pkg\"") "name value"
            Expect.isTrue (json.Contains "\"1.2.3\"")    "version value"

        testCase "CQCHookEndpoint is omitted when None" <| fun () ->
            let json = Serialization.toJson allPassSummary
            Expect.isFalse (json.Contains "CQCHookEndpoint") "no hook field"

        testCase "CQCHookEndpoint is present when Some" <| fun () ->
            let json = Serialization.toJson withHook
            Expect.isTrue (json.Contains "\"CQCHookEndpoint\"")             "hook field present"
            Expect.isTrue (json.Contains "https://example.com/hook") "hook value present"
    ]

    testList "counts" [
        testCase "all-pass: correct counts in Critical" <| fun () ->
            let json = Serialization.toJson allPassSummary
            // Critical has 2 passes
            Expect.isTrue (json.Contains "\"HasFailures\": false") "HasFailures false"
            Expect.isTrue (json.Contains "\"Passed\": 2")          "Passed 2"
            Expect.isTrue (json.Contains "\"Failed\": 0")          "Failed 0"
            Expect.isTrue (json.Contains "\"Errored\": 0")         "Errored 0"

        testCase "all-fail: HasFailures is true" <| fun () ->
            let json = Serialization.toJson allFailSummary
            Expect.isTrue (json.Contains "\"HasFailures\": true") "HasFailures true"

        testCase "all-fail: Failed count is correct" <| fun () ->
            let json = Serialization.toJson allFailSummary
            Expect.isTrue (json.Contains "\"Failed\": 2") "Failed 2"

        testCase "all-error: Errored count is correct" <| fun () ->
            let json = Serialization.toJson allErrorSummary
            Expect.isTrue (json.Contains "\"Errored\": 1") "Errored 1"

        testCase "mixed: Critical passes, NonCritical fails" <| fun () ->
            let json = Serialization.toJson mixedSummary
            // critical passes (passSuite), nonCritical fails (failSuite)
            Expect.isTrue (json.Contains "\"Passed\": 2") "has 2 passed (critical)"
            Expect.isTrue (json.Contains "\"Failed\": 2") "has 2 failed (nonCritical)"
    ]

    testList "special characters" [
        testCase "newline and quote in description are escaped" <| fun () ->
            let crit = PyxpectoRunner.runTestsWithResults passSuite
            let nonCrit = PyxpectoRunner.runTestsWithResults (testList "s" [])
            let summary =
                ValidationSummary.ofExpectoTestRunSummaries(
                    crit, nonCrit,
                    ValidationPackageSummary.create("x", "1.0.0", "s", "line1\nline2 \"quoted\""))
            let json = Serialization.toJson summary
            Expect.isTrue (json.Contains "\\n")  "newline escaped"
            Expect.isTrue (json.Contains "\\\"") "quote escaped"
    ]
]

// ── JUnit XML ─────────────────────────────────────────────────────────────────

let jUnit = testList "jUnit" [

    testList "structure" [
        testCase "starts with XML declaration" <| fun () ->
            let xml = Serialization.toJUnitXml allPassSummary
            Expect.isTrue (xml.StartsWith "<?xml") "XML declaration present"

        testCase "has testsuites root element" <| fun () ->
            let xml = Serialization.toJUnitXml allPassSummary
            Expect.isTrue (xml.Contains "<testsuites") "testsuites element"

        testCase "has testsuite child" <| fun () ->
            let xml = Serialization.toJUnitXml allPassSummary
            Expect.isTrue (xml.Contains "<testsuite") "testsuite element"

        testCase "testsuite name matches package name" <| fun () ->
            let xml = Serialization.toJUnitXml allPassSummary
            Expect.isTrue (xml.Contains "name=\"test-pkg\"") "suite name"

        testCase "testcase elements are present" <| fun () ->
            let xml = Serialization.toJUnitXml allPassSummary
            Expect.isTrue (xml.Contains "<testcase") "testcase element"
    ]

    testList "passing tests" [
        testCase "passed testcases have no failure/error/skipped child" <| fun () ->
            let xml = Serialization.toJUnitXml allPassSummary
            Expect.isFalse (xml.Contains "<failure") "no failure"
            Expect.isFalse (xml.Contains "<error")   "no error"
            Expect.isFalse (xml.Contains "<skipped") "no skipped"

        testCase "total tests count attribute is correct" <| fun () ->
            let xml = Serialization.toJUnitXml allPassSummary
            // 2 from crit + 2 from nonCrit = 4
            Expect.isTrue (xml.Contains "tests=\"4\"") "tests=4"

        testCase "failures count is 0" <| fun () ->
            let xml = Serialization.toJUnitXml allPassSummary
            Expect.isTrue (xml.Contains "failures=\"0\"") "failures=0"
    ]

    testList "failing tests" [
        testCase "failed testcase has failure element" <| fun () ->
            let xml = Serialization.toJUnitXml allFailSummary
            Expect.isTrue (xml.Contains "<failure") "failure element present"

        testCase "failure element has message attribute" <| fun () ->
            let xml = Serialization.toJUnitXml allFailSummary
            Expect.isTrue (xml.Contains "message=") "message attribute"

        testCase "failures count attribute matches" <| fun () ->
            let xml = Serialization.toJUnitXml allFailSummary
            // 2 crit + 2 nonCrit = 4 failures total
            Expect.isTrue (xml.Contains "failures=\"4\"") "failures=4"
    ]

    testList "erroring tests" [
        testCase "errored testcase has error element" <| fun () ->
            let xml = Serialization.toJUnitXml allErrorSummary
            Expect.isTrue (xml.Contains "<error") "error element present"

        testCase "error element has message attribute with exception text" <| fun () ->
            let xml = Serialization.toJUnitXml allErrorSummary
            Expect.isTrue (xml.Contains "boom") "exception message in xml"

        testCase "errors count attribute matches" <| fun () ->
            let xml = Serialization.toJUnitXml allErrorSummary
            // 1 crit + 1 nonCrit = 2
            Expect.isTrue (xml.Contains "errors=\"2\"") "errors=2"
    ]

    testList "skipped tests" [
        testCase "skipped testcase has skipped element" <| fun () ->
            let skipSummary = makeResults skipSuite skipSuite
            let xml = Serialization.toJUnitXml skipSummary
            Expect.isTrue (xml.Contains "<skipped") "skipped element present"

        testCase "skipped count attribute is correct" <| fun () ->
            let skipSummary = makeResults skipSuite skipSuite
            let xml = Serialization.toJUnitXml skipSummary
            // 1 crit + 1 nonCrit = 2
            Expect.isTrue (xml.Contains "skipped=\"2\"") "skipped=2"
    ]

    testList "XML escaping" [
        testCase "special characters in test names are escaped" <| fun () ->
            let specialSuite =
                testList "s" [
                    testCase "name with <angle> & \"quotes\"" <| fun () -> ()
                ]
            let summary = makeResults specialSuite (testList "s" [])
            let xml = Serialization.toJUnitXml summary
            Expect.isFalse (xml.Contains "<angle>")  "raw < not present"
            Expect.isTrue  (xml.Contains "&lt;")     "< escaped"
            Expect.isTrue  (xml.Contains "&amp;")    "& escaped"

        testCase "special characters in failure messages are escaped" <| fun () ->
            let specialFail =
                testList "s" [
                    testCase "f" <| fun () -> Expect.equal "<bad>" "ok" "msg with <bad> & stuff"
                ]
            let summary = makeResults specialFail (testList "s" [])
            let xml = Serialization.toJUnitXml summary
            Expect.isTrue (xml.Contains "&lt;bad&gt;") "< and > in message escaped"
    ]
]

// ── NUnit XML ─────────────────────────────────────────────────────────────────

let nUnit = testList "nUnit" [

    testList "structure" [
        testCase "starts with XML declaration" <| fun () ->
            let xml = Serialization.toNUnitXml allPassSummary
            Expect.isTrue (xml.StartsWith "<?xml") "XML declaration present"

        testCase "has test-results root element" <| fun () ->
            let xml = Serialization.toNUnitXml allPassSummary
            Expect.isTrue (xml.Contains "<test-results") "test-results element"

        testCase "has test-suite element" <| fun () ->
            let xml = Serialization.toNUnitXml allPassSummary
            Expect.isTrue (xml.Contains "<test-suite") "test-suite element"

        testCase "has results element" <| fun () ->
            let xml = Serialization.toNUnitXml allPassSummary
            Expect.isTrue (xml.Contains "<results") "results element"

        testCase "has test-case elements" <| fun () ->
            let xml = Serialization.toNUnitXml allPassSummary
            Expect.isTrue (xml.Contains "<test-case") "test-case element"

        testCase "root name attribute matches package name" <| fun () ->
            let xml = Serialization.toNUnitXml allPassSummary
            Expect.isTrue (xml.Contains "name=\"test-pkg\"") "root name"
    ]

    testList "passed test attributes" [
        testCase "passed test-case has executed=True result=Success success=True" <| fun () ->
            let xml = Serialization.toNUnitXml allPassSummary
            Expect.isTrue (xml.Contains "executed=\"True\"")  "executed"
            Expect.isTrue (xml.Contains "result=\"Success\"") "result"
            Expect.isTrue (xml.Contains "success=\"True\"")   "success"

        testCase "passed test-case has no failure child" <| fun () ->
            let xml = Serialization.toNUnitXml allPassSummary
            Expect.isFalse (xml.Contains "<failure") "no failure"

        testCase "suite result=Success when all pass" <| fun () ->
            let xml = Serialization.toNUnitXml allPassSummary
            Expect.isTrue (xml.Contains "result=\"Success\"") "suite success"

        testCase "suite success=True when all pass" <| fun () ->
            let xml = Serialization.toNUnitXml allPassSummary
            Expect.isTrue (xml.Contains "success=\"True\"") "suite success attr"
    ]

    testList "failed test attributes" [
        testCase "failed test-case has result=Failure success=False" <| fun () ->
            let xml = Serialization.toNUnitXml allFailSummary
            Expect.isTrue (xml.Contains "result=\"Failure\"") "result Failure"
            Expect.isTrue (xml.Contains "success=\"False\"")  "success False"

        testCase "failed test-case has failure child with CDATA message" <| fun () ->
            let xml = Serialization.toNUnitXml allFailSummary
            Expect.isTrue (xml.Contains "<failure")    "failure element"
            Expect.isTrue (xml.Contains "<![CDATA[")   "CDATA section"
            Expect.isTrue (xml.Contains "<message")    "message element"

        testCase "failures count in root matches" <| fun () ->
            let xml = Serialization.toNUnitXml allFailSummary
            Expect.isTrue (xml.Contains "failures=\"4\"") "failures=4"

        testCase "suite result=Failure when any fail" <| fun () ->
            let xml = Serialization.toNUnitXml allFailSummary
            // test-results attributes contain success="False" or result="Failure"
            Expect.isTrue (xml.Contains "result=\"Failure\"") "suite failure"
    ]

    testList "errored test attributes" [
        testCase "errored test-case has result=Failure success=False" <| fun () ->
            let xml = Serialization.toNUnitXml allErrorSummary
            Expect.isTrue (xml.Contains "result=\"Failure\"") "result Failure"
            Expect.isTrue (xml.Contains "success=\"False\"")  "success False"

        testCase "errored test-case has failure child containing exception message" <| fun () ->
            let xml = Serialization.toNUnitXml allErrorSummary
            Expect.isTrue (xml.Contains "<failure")  "failure element"
            Expect.isTrue (xml.Contains "boom")      "exception message in CDATA"

        testCase "errors count in root matches" <| fun () ->
            let xml = Serialization.toNUnitXml allErrorSummary
            Expect.isTrue (xml.Contains "errors=\"2\"") "errors=2"
    ]

    testList "ignored test attributes" [
        testCase "ignored test-case has executed=False result=Ignored" <| fun () ->
            let skipSummary = makeResults skipSuite skipSuite
            let xml = Serialization.toNUnitXml skipSummary
            Expect.isTrue (xml.Contains "executed=\"False\"") "not executed"
            Expect.isTrue (xml.Contains "result=\"Ignored\"") "result Ignored"

        testCase "ignored test-case has reason/message child" <| fun () ->
            let skipSummary = makeResults skipSuite skipSuite
            let xml = Serialization.toNUnitXml skipSummary
            Expect.isTrue (xml.Contains "<reason")  "reason element"
            Expect.isTrue (xml.Contains "<message") "message element"

        testCase "ignored count in root matches" <| fun () ->
            let skipSummary = makeResults skipSuite skipSuite
            let xml = Serialization.toNUnitXml skipSummary
            Expect.isTrue (xml.Contains "ignored=\"2\"") "ignored=2"
    ]

    testList "total count" [
        testCase "total attribute is sum of all entries" <| fun () ->
            let xml = Serialization.toNUnitXml allPassSummary
            // 2 critical + 2 nonCritical
            Expect.isTrue (xml.Contains "total=\"4\"") "total=4"
    ]
]
