namespace ARCExpect

open AVPRIndex
open Fable.Pyxpecto

open Fable.Pyxpecto.Model

type ARCValidationPackage = 
    {
        Metadata: ValidationPackageMetadata
        CriticalValidationCases: TestCase
        NonCriticalValidationCases: TestCase
    } with
        static member create (
            metadata: ValidationPackageMetadata,
            criticalValidationCases: TestCase,
            nonCriticalValidationCases: TestCase
        ) =
            {
                Metadata = metadata
                CriticalValidationCases = criticalValidationCases
                NonCriticalValidationCases = nonCriticalValidationCases
            }

        static member create (
            metadata: ValidationPackageMetadata,
            criticalValidationCasesList: TestCase list,
            nonCriticalValidationCasesList: TestCase list,
            ?CQCHookEndpoint: string
        ) =
            let criticalCases = testList "Critical" criticalValidationCasesList
            let nonCriticalCases = testList "NonCritical" nonCriticalValidationCasesList

            ARCValidationPackage.create(
                metadata = metadata, 
                criticalValidationCases = criticalCases, 
                nonCriticalValidationCases = nonCriticalCases
            )

        static member create (
            metadata: ValidationPackageMetadata,
            ?CriticalValidationCasesList: TestCase list,
            ?NonCriticalValidationCasesList: TestCase list
        ) =
            ARCValidationPackage.create(
                metadata = metadata, 
                criticalValidationCasesList = defaultArg CriticalValidationCasesList [], 
                nonCriticalValidationCasesList = defaultArg NonCriticalValidationCasesList []
            )