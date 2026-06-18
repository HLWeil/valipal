namespace ARCExpect

open AnyBadge.NET
open Fable.Pyxpecto.Model
open System.IO
open AVPRIndex

type Setup =
    
    static member Metadata(
        frontmatter: string,
        programmingLanguage: FrontmatterLanguage
    ) =
        ValidationPackageMetadata.extractFromString programmingLanguage frontmatter

    static member ValidationPackage(
        metadata: ValidationPackageMetadata,
        ?CriticalValidationCases: TestCase list,
        ?NonCriticalValidationCases: TestCase list
    ) =
        ARCValidationPackage.create(
            metadata = metadata,
            ?CriticalValidationCasesList = CriticalValidationCases,
            ?NonCriticalValidationCasesList = NonCriticalValidationCases
        )

    static member ValidationPackage(
        name: string,
        summary: string,
        description: string,
        majorVersion: int,
        minorVersion: int,
        patchVersion: int,
        programmingLanguage: string,
        ?Publish: bool,
        ?Authors: Author array,
        ?Tags: OntologyAnnotation array,
        ?ReleaseNotes: string,
        ?CriticalValidationCases: TestCase list,
        ?NonCriticalValidationCases: TestCase list,
        ?CQCHookEndpoint: string
    ) =
        Setup.ValidationPackage(
            metadata = ValidationPackageMetadata.create(
                name = name,
                summary = summary,
                description = description,
                majorVersion = majorVersion,
                minorVersion = minorVersion,
                patchVersion = patchVersion,
                programmingLanguage = programmingLanguage,
                ?Publish = Publish,
                ?Authors = Authors,
                ?Tags = Tags,
                ?ReleaseNotes = ReleaseNotes,
                ?CQCHookEndpoint = CQCHookEndpoint
            ),
            ?CriticalValidationCases = CriticalValidationCases,
            ?NonCriticalValidationCases = NonCriticalValidationCases
        )

open System.Collections.Generic
type Execute =

// ------------------ New API with ARCValidationPackage, metadata support and custom summaries ------------------
    
    static member Validation (
        ?Payload: Dictionary<string, obj>
    ) =
        fun (arcValidationPackage: ARCValidationPackage) ->

            let criticalResults = PyxpectoRunner.runTestsWithResults arcValidationPackage.CriticalValidationCases
            let nonCriticalResults = PyxpectoRunner.runTestsWithResults arcValidationPackage.NonCriticalValidationCases
        
            ValidationSummary.ofExpectoTestRunSummaries(
                criticalSummary = criticalResults,
                nonCriticalSummary = nonCriticalResults,
                package = ValidationPackageSummary.create(arcValidationPackage.Metadata),
                ?Payload = Payload
            )

    static member SummaryCreation(
        path: string
    ) =  
        fun (validationSummary: ValidationSummary) -> 
            Serialization.writeJson path validationSummary 

    static member JUnitReportCreation(
        path: string
    ) =
        fun (validationSummary: ValidationSummary) -> 
            Serialization.writeJUnitXml path validationSummary

    static member BadgeCreation(
        path: string,
        labelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Color>,
        ?DefaultColor: Color
    ) =
        fun (validationSummary: ValidationSummary) -> 

            validationSummary
            |> BadgeCreation.ofValidationSummary(
                labelText,
                ?ValueSuffix = ValueSuffix,
                ?Thresholds = Thresholds,
                ?DefaultColor = DefaultColor
            )
            |> fun b -> b.WriteBadge(path)

    static member ValidationPipeline(
        basePath: string,
        ?BadgeLabelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Color>,
        ?DefaultColor: Color,
        ?Payload: Dictionary<string, obj>
    ) =
        fun (arcValidationPackage: ARCValidationPackage) ->

            let labelText = defaultArg BadgeLabelText $"{arcValidationPackage.Metadata.Name}@{ValidationPackageMetadata.getSemanticVersionString arcValidationPackage.Metadata}"

            let foldername = $"{arcValidationPackage.Metadata.Name}@{ValidationPackageMetadata.getSemanticVersionString arcValidationPackage.Metadata}"

            let resultFolder = Path.Combine(basePath, ".arc-validate-results", foldername)
            let summaryPath = Path.Combine(resultFolder, "validation_summary.json")
            let badgePath = Path.Combine(resultFolder, "badge.svg")
            let jUnitPath = Path.Combine(resultFolder, "validation_report.xml")

            Directory.CreateDirectory(resultFolder) |> ignore

            let results = 
                arcValidationPackage
                |> Execute.Validation(?Payload = Payload)

            results |> Execute.SummaryCreation(summaryPath)
            results |> Execute.JUnitReportCreation(jUnitPath)
            results
            |> Execute.BadgeCreation(
                badgePath, 
                labelText, 
                ?ValueSuffix = ValueSuffix, 
                ?Thresholds = Thresholds, 
                ?DefaultColor = DefaultColor
            )

// ------------------ Legacy API without ARCValidationPackage, metadata, or custom Summaries ------------------

    //static member Validation (validationCases: Test) = performTest validationCases

    //static member JUnitSummaryCreation(
    //    path: string,
    //    ?Verbose: bool
    //) =
    //    let verbose = defaultArg Verbose false
    //    fun (validationResults: Impl.TestRunSummary) -> writeJUnitSummary verbose path validationResults

    //static member BadgeCreation(
    //    path: string,
    //    labelText: string,
    //    ?ValueSuffix: string,
    //    ?Thresholds: Map<int, Color>,
    //    ?DefaultColor: Color
    //) =
    //    fun (validationResults: Impl.TestRunSummary) -> 
    //        validationResults
    //        |> BadgeCreation.ofTestResults(
    //            labelText,
    //            ?ValueSuffix = ValueSuffix,
    //            ?Thresholds = Thresholds,
    //            ?DefaultColor = DefaultColor
    //        )
    //        |> fun b -> b.WriteBadge(path)

    //static member ValidationPipeline(
    //    jUnitPath: string,
    //    badgePath: string,
    //    labelText: string,
    //    ?ValueSuffix: string,
    //    ?Thresholds: Map<int, Color>,
    //    ?DefaultColor: Color
    //) =
    //    fun (validationCases: Test) ->

    //        let results = 
    //            validationCases
    //            |> Execute.Validation

    //        results
    //        |> Execute.JUnitSummaryCreation(jUnitPath)

    //        results
    //        |> Execute.BadgeCreation(badgePath, labelText, ?ValueSuffix = ValueSuffix, ?Thresholds = Thresholds, ?DefaultColor = DefaultColor)

    //static member ValidationPipeline(
    //    basePath: string,
    //    packageName: string,
    //    ?BadgeLabelText: string,
    //    ?ValueSuffix: string,
    //    ?Thresholds: Map<int, Color>,
    //    ?DefaultColor: Color
    //) =
    //    fun (validationCases: Test) ->

    //        let resultFolder = Path.Combine(basePath, ".arc-validate-results", packageName)
    //        let badgePath = Path.Combine(resultFolder, "badge.svg")
    //        let jUnitPath = Path.Combine(resultFolder, "validation_report.xml")

    //        Directory.CreateDirectory(resultFolder) |> ignore

    //        let results = 
    //            validationCases
    //            |> Execute.Validation

    //        results
    //        |> Execute.JUnitSummaryCreation(jUnitPath)

    //        let labelText = defaultArg BadgeLabelText packageName

    //        results
    //        |> Execute.BadgeCreation(badgePath, labelText, ?ValueSuffix = ValueSuffix, ?Thresholds = Thresholds, ?DefaultColor = DefaultColor)

