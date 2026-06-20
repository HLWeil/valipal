namespace AVPRIndex

open Domain
open System
open YAMLicious
open ARCExpect.Helper

[<AutoOpen>]
module Frontmatter = 

    type FrontmatterLanguage =
        | FSharpFrontmatter
        | PythonFrontmatter

        static member fromString (str: string) =
            match str.ToLowerInvariant() with
            | "fsharp" | "fs" | "f#" -> FrontmatterLanguage.FSharpFrontmatter
            | "python" | "py" -> FrontmatterLanguage.PythonFrontmatter
            | _ -> failwith $"unsupported frontmatter language: {str}"

        static member toString (lang: FrontmatterLanguage) =
            match lang with
            | FSharpFrontmatter -> "FSharp"
            | PythonFrontmatter -> "Python"

    module FSharp =
        /// the frontmatter start string if the package uses yaml frontmatter as comment
        let [<Literal>] frontMatterCommentStart = "(*\n---"
        /// the frontmatter end string if the package uses yaml frontmatter as comment
        let [<Literal>] frontMatterCommentEnd = "---\n*)"

        /// the frontmatter start string if the package uses yaml frontmatter as a string binding to be re-used in the package code
        let [<Literal>] frontmatterBindingStart = "let [<Literal>]PACKAGE_METADATA = \"\"\"(*\n---"
        /// the frontmatter end string if the package uses yaml frontmatter as a string binding to be re-used in the package code
        let [<Literal>] frontmatterBindingEnd = "---\n*)\"\"\""


        let containsCommentFrontmatter (str: string) =
            str.StartsWith(frontMatterCommentStart, StringComparison.Ordinal) && str.Contains(frontMatterCommentEnd)

        let containsBindingFrontmatter (str: string) =
            str.StartsWith(frontmatterBindingStart, StringComparison.Ordinal) && str.Contains(frontmatterBindingEnd)

        let tryExtractFromString (str: string) =
            let norm = String.replaceLineEndings "\n" str
            if containsCommentFrontmatter norm then
                norm.Substring(
                    frontMatterCommentStart.Length, 
                    (norm.IndexOf(frontMatterCommentEnd, StringComparison.Ordinal) - frontMatterCommentStart.Length))
                |> Some
            elif containsBindingFrontmatter norm then
                norm.Substring(
                    frontmatterBindingStart.Length, 
                    (norm.IndexOf(frontmatterBindingEnd, StringComparison.Ordinal) - frontmatterBindingStart.Length))
                |> Some
            else 
                None

        let extractFromString (str: string) =
            match tryExtractFromString str with
            | Some frontmatter -> frontmatter
            | None -> failwith $"""
input 

{str}

has no correctly formatted FSharp frontmatter."""

    module Python =
        /// the frontmatter start string if the package uses yaml frontmatter as comment
        let [<Literal>] frontMatterCommentStart = "\"\"\"\n---"
        /// the frontmatter end string if the package uses yaml frontmatter as comment
        let [<Literal>] frontMatterCommentEnd = "---\n\"\"\""

        /// the frontmatter start string if the package uses yaml frontmatter as a string binding to be re-used in the package code
        let [<Literal>] frontmatterBindingStart = "PACKAGE_METADATA = \"\"\"\n---"
        /// the frontmatter end string if the package uses yaml frontmatter as a string binding to be re-used in the package code
        let [<Literal>] frontmatterBindingEnd = "---\n\"\"\""

        let containsCommentFrontmatter (str: string) =
            str.StartsWith(frontMatterCommentStart, StringComparison.Ordinal) && str.Contains(frontMatterCommentEnd)

        let containsBindingFrontmatter (str: string) =
            str.StartsWith(frontmatterBindingStart, StringComparison.Ordinal) && str.Contains(frontmatterBindingEnd)

        let tryExtractFromString (str: string) =
            let norm = String.replaceLineEndings "\n" str
            if containsCommentFrontmatter norm then
                norm.Substring(
                    frontMatterCommentStart.Length, 
                    (norm.IndexOf(frontMatterCommentEnd, StringComparison.Ordinal) - frontMatterCommentStart.Length))
                |> Some
            elif containsBindingFrontmatter norm then
                norm.Substring(
                    frontmatterBindingStart.Length, 
                    (norm.IndexOf(frontmatterBindingEnd, StringComparison.Ordinal) - frontmatterBindingStart.Length))
                |> Some
            else 
                None

        let extractFromString (str: string) =
            match tryExtractFromString str with
            | Some frontmatter -> frontmatter
            | None -> failwith $"""
input 

{str}

has no correctly formatted Python frontmatter."""

    let tryExtractFromString (lang:FrontmatterLanguage) (str: string) =
        match lang with
        | FSharpFrontmatter -> FSharp.tryExtractFromString str
        | PythonFrontmatter -> Python.tryExtractFromString str

    let extractFromString (lang:FrontmatterLanguage) (str: string) =
        match lang with
        | FSharpFrontmatter -> FSharp.extractFromString str
        | PythonFrontmatter -> Python.extractFromString str

    let private authorDecoder =
        Decode.object (fun get ->
            Author.create(
                fullName         = get.Required.Field "FullName"        Decode.string,
                ?Email           = get.Optional.Field "Email"           Decode.string,
                ?Affiliation     = get.Optional.Field "Affiliation"     Decode.string,
                ?AffiliationLink = get.Optional.Field "AffiliationLink" Decode.string
            )
        )

    let private ontologyAnnotationDecoder =
        Decode.object (fun get ->
            OntologyAnnotation.create(
                name                 = get.Required.Field "Name"                Decode.string,
                ?TermSourceRef       = get.Optional.Field "TermSourceREF"       Decode.string,
                ?TermAccessionNumber = get.Optional.Field "TermAccessionNumber" Decode.string
            )
        )

    let private metadataDecoder =
        Decode.object (fun get ->
            ValidationPackageMetadata.create(
                name                = get.Required.Field "Name"         Decode.string,
                summary             = get.Required.Field "Summary"      Decode.string,
                description         = get.Required.Field "Description"  Decode.string,
                majorVersion        = get.Required.Field "MajorVersion" Decode.int,
                minorVersion        = get.Required.Field "MinorVersion" Decode.int,
                patchVersion        = get.Required.Field "PatchVersion" Decode.int,
                programmingLanguage = "",
                ?PreReleaseVersionSuffix    = get.Optional.Field "PreReleaseVersionSuffix"    Decode.string,
                ?BuildMetadataVersionSuffix = get.Optional.Field "BuildMetadataVersionSuffix" Decode.string,
                ?Publish            = get.Optional.Field "Publish"         Decode.bool,
                ?Authors            = get.Optional.Field "Authors"         (Decode.array authorDecoder),
                ?Tags               = get.Optional.Field "Tags"            (Decode.array ontologyAnnotationDecoder),
                ?ReleaseNotes       = get.Optional.Field "ReleaseNotes"    Decode.string,
                ?CQCHookEndpoint    = get.Optional.Field "CQCHookEndpoint" Decode.string
            )
        )

    type ValidationPackageMetadata with
        
        static member extractFromString (lang: FrontmatterLanguage) (str: string) =
            let frontmatter = tryExtractFromString lang str
            match frontmatter with
            | Some frontmatter ->
                let result = frontmatter |> Decode.read |> metadataDecoder
                result.ProgrammingLanguage <- FrontmatterLanguage.toString lang
                result
            | None ->
                failwith $"""
string 

{str}

has no correctly formatted {lang}."""

        static member tryExtractFromString (lang: FrontmatterLanguage) (str: string) =
            try 
                let vpm = ValidationPackageMetadata.extractFromString lang str 
                Some vpm
            with e ->
                printfn $"error parsing package metadata: {e.Message}"
                None

        static member extractFromScript (scriptPath: string) =

            let lang = 
                match Path.getExtension(scriptPath).ToLowerInvariant() with
                | ".fsx" -> FrontmatterLanguage.FSharpFrontmatter
                | ".py" -> FrontmatterLanguage.PythonFrontmatter
                | ext -> failwith $"unsupported script extension: {ext}"

            scriptPath
            |> File.readAllText
            |> ValidationPackageMetadata.extractFromString lang

        static member tryExtractFromScript (scriptPath: string) =
            try 
                ValidationPackageMetadata.extractFromScript scriptPath |> Some
            with e ->
                printfn $"error parsing package metadata: {e.Message}"
                None

    type ValidationPackageIndex with

        static member create (
            repoPath: string, 
            lastUpdated: System.DateTimeOffset
        ) = 
            ValidationPackageIndex.create(
                repoPath = repoPath,
                lastUpdated = lastUpdated,
                metadata = ValidationPackageMetadata.extractFromScript(repoPath)
            )