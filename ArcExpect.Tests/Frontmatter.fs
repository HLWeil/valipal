module Frontmatter.Tests

open Fable.Pyxpecto
open AVPRIndex

// ── raw frontmatter strings copied from references/ ───────────────────────────
// FSharp binding-style strings cannot use triple-quoted literals because the
// source text itself contains """, so we build them with regular escaped strings.

/// ceplas-experimental@1.0.1.fsx  – binding style
let private ceplasExperimentalFsx =
    "let [<Literal>]PACKAGE_METADATA = \"\"\"(*\n---\n" +
    "Name: ceplas-experimental\n" +
    "Summary: Validates whether the ARC contains the minimal metadata to meet the CEPLAS quality criteria for a typical experimental ARC.\n" +
    "Description: |\n" +
    "    ## Critical quality criteria\n" +
    "    - ARC contains README\n" +
    "    - ARC contains any LICENSE file\n" +
    "    - Investigation contains title\n" +
    "    - Investigation contains description\n" +
    "    - Investigation contains contact\n" +
    "    - Investigation contacts contain first name, last name, email, affiliation, ORCID\n" +
    "    - At least one investigation contact must have email and affiliation\n" +
    "    - ARC contains at least one study or one assay\n" +
    "    - Every study must contain at least one annotation table\n" +
    "    - Every assay must contain at least one annotation table\n" +
    "    - ARC contains 'raw' data (e.g. raw dataset file or URL)    \n" +
    "    ## Non-Critical quality criteria    \n" +
    "    - Every investigation contact should have a valid email\n" +
    "    - Every investigation contact should have an affiliation\n" +
    "    - Every investigation contact should have an ORCID\n" +
    "    - At least one investigation contact should have role 'researcher'\n" +
    "    - At least one investigation contact should have role 'principal investigator'\n" +
    "    - Every study contains top-level metadata\n" +
    "    - Every assay contains top-level metadata\n" +
    "    - ARC annotation tables are connected\n" +
    "    - Every data entity should be derived from a Source or Sample\n" +
    "    - Every data entity should be annotated with at least one of Characteristic, Parameter, Factor\n" +
    "    - Every annotation table contains an Input\n" +
    "    - Every annotation table contains an Output\n" +
    "    - Every annotation table contains a ProtocolREF column\n" +
    "MajorVersion: 1\n" +
    "MinorVersion: 0\n" +
    "PatchVersion: 1\n" +
    "Publish: true\n" +
    "Authors:\n" +
    "  - FullName: Dominik Brilhaus\n" +
    "    Email: brilhaus@hhu.de\n" +
    "    Affiliation: CEPLAS\n" +
    "    AffiliationLink: https://ceplas.eu\n" +
    "  - FullName: Heinrich Lukas Weil\n" +
    "    Email: weil@nfdi4plants.org\n" +
    "    Affiliation: RPTU Kaiserslautern\n" +
    "    AffiliationLink: http://rptu.de/startseite\n" +
    "Tags:\n" +
    "  - Name: ceplas\n" +
    "  - Name: experimental\n" +
    "  - Name: quality-arc\n" +
    "ReleaseNotes: |\n" +
    "  - hotfix run file name pruning (\"./<filename>\" -> \"<filename>\")\n" +
    "---\n" +
    "*)\"\"\""

/// plant-growth@1.0.0.fsx  – binding style
let private plantGrowthFsx =
    "let [<Literal>]PACKAGE_METADATA = \"\"\"(*\n---\n" +
    "Name: plant-growth\n" +
    "Summary: Validates if the ARC contains the necessary metadata to describe conditions for plant growth.\n" +
    "Description: |\n" +
    "    Validates if the ARC contains an annotation table with protocol type \"Plant Growth Protocol\" and if it exists, whether it contains the following fields:\n" +
    "\n" +
    "        Critical fields:\n" +
    "        - organism (OBI:0100026)\n" +
    "        - growth day length (DPBO:0000041)\n" +
    "        - light intensity exposure (PECO:0007224)        \n" +
    "        - temperature day (DPBO:0000007)\n" +
    "        - temperature night (DPBO:0000008)       \n" +
    "\n" +
    "        Non critical fields:\n" +
    "        - genotype (EFO:0000513)\n" +
    "        - study type (PECO:0007231)\n" +
    "        - Reference Time Point (NCIT:C82576)\n" +
    "        - growth plot design (DPBO:0000001)\n" +
    "        - plant growth medium exposure (PECO:0007147)\n" +
    "        - humidity day (DPBO:0000005)\n" +
    "        - humidity night (DPBO:0000006)\n" +
    "        - plant nutrient exposure (PECO:0007241)\n" +
    "        - abiotic plant exposure (PECO:0007191)\n" +
    "        - biotic plant exposure (PECO:0007357)\n" +
    "        - watering exposure (PECO:0007383)\n" +
    "\n" +
    "MajorVersion: 1\n" +
    "MinorVersion: 0\n" +
    "PatchVersion: 0\n" +
    "Publish: true\n" +
    "Authors:\n" +
    "  - FullName: Heinrich Lukas Weil\n" +
    "    Email: weil@nfdi4plants.org\n" +
    "    Affiliation: RPTU Kaiserslautern\n" +
    "    AffiliationLink: http://rptu.de/startseite\n" +
    "Tags:\n" +
    "  - Name: validation\n" +
    "  - Name: growth\n" +
    "  - Name: plant\n" +
    "ReleaseNotes: |\n" +
    "  - initial release\n" +
    "---\n" +
    "*)\"\"\""

/// edal@0.0.4.py – comment style
let private edalPy =
    "\"\"\"\n---\n" +
    "Name: edal\n" +
    "MajorVersion: 0\n" +
    "MinorVersion: 0\n" +
    "PatchVersion: 4\n" +
    "Summary: e!DAL validation package for submission\n" +
    "Description: |\n" +
    "  This python package validates ARCs for the e!DAL\n" +
    "  PGP research data repository.\n" +
    "Publish: true\n" +
    "Authors:\n" +
    "  - FullName: Jonathan Bauer\n" +
    "    Email: bauer@nfdi4plants.org\n" +
    "    Affiliation: University of Freiburg\n" +
    "    AffiliationLink: https://uni-freiburg.de\n" +
    "Tags:\n" +
    "  - Name: e!DAL\n" +
    "  - Name: data-submission\n" +
    "ReleaseNotes: |\n" +
    "  - fixed folder output path again\n" +
    "CQCHookEndpoint: https://mira.ipk-gatersleben.de/submit\n" +
    "---\n\"\"\""

/// treem-sequencing@0.0.1.py – comment style
let private treemSequencingPy =
    "\"\"\"\n---\n" +
    "Name: treem-sequencing\n" +
    "MajorVersion: 0\n" +
    "MinorVersion: 0\n" +
    "PatchVersion: 1\n" +
    "Summary: TreeM RNA Sequencing ARC Validation\n" +
    "Description: |\n" +
    "  This python package validates ARCs for the TreeM consortium RNA Sequencing.\n" +
    "Publish: true\n" +
    "Authors:\n" +
    "  - FullName: Kristian Peters\n" +
    "    Email: kristian.peters@computational.bio.uni-giessen.de\n" +
    "    Affiliation: JLU Giessen\n" +
    "    AffiliationLink: https://www.uni-giessen.de\n" +
    "Tags:\n" +
    "  - Name: TreeM\n" +
    "  - Name: RNA-Sequencing\n" +
    "ReleaseNotes: |\n" +
    "  - initial release\n" +
    "---\n\"\"\""


// ── helpers ───────────────────────────────────────────────────────────────────

let private parseFSharp str =
    ValidationPackageMetadata.extractFromString FSharpFrontmatter str

let private parsePython str =
    ValidationPackageMetadata.extractFromString PythonFrontmatter str

let private tryParseFSharp str =
    ValidationPackageMetadata.tryExtractFromString FSharpFrontmatter str

let private tryParsePython str =
    ValidationPackageMetadata.tryExtractFromString PythonFrontmatter str


// ── tests ─────────────────────────────────────────────────────────────────────

let frontmatter = testList "frontmatter" [

    // ── extraction helpers ────────────────────────────────────────────────────

    testList "FSharp frontmatter extraction" [

        testCase "binding style: tryExtractFromString returns Some for valid binding" <| fun () ->
            let result = FSharp.tryExtractFromString ceplasExperimentalFsx
            Expect.isSome result "expected Some"

        testCase "binding style: extracted YAML contains Name key" <| fun () ->
            let yaml = FSharp.extractFromString ceplasExperimentalFsx
            Expect.isTrue (yaml.Contains "Name:") "contains Name key"

        testCase "comment style: tryExtractFromString returns None for python string" <| fun () ->
            let result = FSharp.tryExtractFromString edalPy
            Expect.isNone result "Python string should not parse as FSharp"

        testCase "extractFromString throws on invalid input" <| fun () ->
            Expect.throws (fun () -> FSharp.extractFromString "no frontmatter here" |> ignore)
                          "should throw on missing frontmatter"
    ]

    testList "Python frontmatter extraction" [

        testCase "comment style: tryExtractFromString returns Some for valid string" <| fun () ->
            let result = Python.tryExtractFromString edalPy
            Expect.isSome result "expected Some"

        testCase "comment style: extracted YAML contains Name key" <| fun () ->
            let yaml = Python.extractFromString edalPy
            Expect.isTrue (yaml.Contains "Name:") "contains Name key"

        testCase "tryExtractFromString returns None for FSharp string" <| fun () ->
            let result = Python.tryExtractFromString ceplasExperimentalFsx
            Expect.isNone result "FSharp string should not parse as Python"

        testCase "extractFromString throws on invalid input" <| fun () ->
            Expect.throws (fun () -> Python.extractFromString "no frontmatter here" |> ignore)
                          "should throw on missing frontmatter"
    ]

    // ── FSharp package parsing ────────────────────────────────────────────────

    testList "ceplas-experimental@1.0.1 (FSharp)" [

        testCase "Name is parsed correctly" <| fun () ->
            let m = parseFSharp ceplasExperimentalFsx
            Expect.equal m.Name "ceplas-experimental" "Name"

        testCase "MajorVersion / MinorVersion / PatchVersion" <| fun () ->
            let m = parseFSharp ceplasExperimentalFsx
            Expect.equal m.MajorVersion 1 "Major"
            Expect.equal m.MinorVersion 0 "Minor"
            Expect.equal m.PatchVersion 1 "Patch"

        testCase "Summary is non-empty and mentions CEPLAS" <| fun () ->
            let m = parseFSharp ceplasExperimentalFsx
            Expect.isFalse (m.Summary = "") "Summary not empty"
            Expect.isTrue (m.Summary.Contains "CEPLAS") "Summary mentions CEPLAS"

        testCase "Description is non-empty" <| fun () ->
            let m = parseFSharp ceplasExperimentalFsx
            Expect.isFalse (m.Description = "") "Description not empty"

        testCase "Publish is true" <| fun () ->
            let m = parseFSharp ceplasExperimentalFsx
            Expect.isTrue m.Publish "Publish"

        testCase "Two authors are parsed" <| fun () ->
            let m = parseFSharp ceplasExperimentalFsx
            Expect.equal m.Authors.Length 2 "two authors"

        testCase "First author FullName and Email" <| fun () ->
            let a = (parseFSharp ceplasExperimentalFsx).Authors.[0]
            Expect.equal a.FullName "Dominik Brilhaus" "first author name"
            Expect.equal a.Email   "brilhaus@hhu.de"  "first author email"

        testCase "Second author FullName" <| fun () ->
            let m = parseFSharp ceplasExperimentalFsx
            Expect.equal m.Authors.[1].FullName "Heinrich Lukas Weil" "second author name"

        testCase "Three tags are parsed with correct names" <| fun () ->
            let names = (parseFSharp ceplasExperimentalFsx).Tags |> Array.map (fun t -> t.Name)
            Expect.equal names.Length 3 "three tags"
            Expect.isTrue (names |> Array.contains "ceplas")       "tag ceplas"
            Expect.isTrue (names |> Array.contains "experimental") "tag experimental"
            Expect.isTrue (names |> Array.contains "quality-arc")  "tag quality-arc"

        testCase "ReleaseNotes is non-empty" <| fun () ->
            let m = parseFSharp ceplasExperimentalFsx
            Expect.isFalse (m.ReleaseNotes = "") "ReleaseNotes not empty"

        testCase "CQCHookEndpoint is empty (not set)" <| fun () ->
            let m = parseFSharp ceplasExperimentalFsx
            Expect.equal m.CQCHookEndpoint "" "no CQCHookEndpoint"

        testCase "ProgrammingLanguage is set to FSharp" <| fun () ->
            let m = parseFSharp ceplasExperimentalFsx
            Expect.equal m.ProgrammingLanguage "FSharp" "ProgrammingLanguage"

        testCase "tryExtractFromString returns Some" <| fun () ->
            Expect.isSome (tryParseFSharp ceplasExperimentalFsx) "tryExtractFromString Some"
    ]

    testList "plant-growth@1.0.0 (FSharp)" [

        testCase "Name is parsed correctly" <| fun () ->
            let m = parseFSharp plantGrowthFsx
            Expect.equal m.Name "plant-growth" "Name"

        testCase "Version is 1.0.0" <| fun () ->
            let m = parseFSharp plantGrowthFsx
            Expect.equal m.MajorVersion 1 "Major"
            Expect.equal m.MinorVersion 0 "Minor"
            Expect.equal m.PatchVersion 0 "Patch"

        testCase "One author parsed with correct name" <| fun () ->
            let m = parseFSharp plantGrowthFsx
            Expect.equal m.Authors.Length 1 "one author"
            Expect.equal m.Authors.[0].FullName "Heinrich Lukas Weil" "author name"

        testCase "Three tags parsed including plant" <| fun () ->
            let names = (parseFSharp plantGrowthFsx).Tags |> Array.map (fun t -> t.Name)
            Expect.equal names.Length 3 "three tags"
            Expect.isTrue (names |> Array.contains "plant") "tag plant"

        testCase "ProgrammingLanguage is FSharp" <| fun () ->
            let m = parseFSharp plantGrowthFsx
            Expect.equal m.ProgrammingLanguage "FSharp" "ProgrammingLanguage"
    ]

    // ── Python package parsing ────────────────────────────────────────────────

    testList "edal@0.0.4 (Python)" [

        testCase "Name is parsed correctly" <| fun () ->
            let m = parsePython edalPy
            Expect.equal m.Name "edal" "Name"

        testCase "Version is 0.0.4" <| fun () ->
            let m = parsePython edalPy
            Expect.equal m.MajorVersion 0 "Major"
            Expect.equal m.MinorVersion 0 "Minor"
            Expect.equal m.PatchVersion 4 "Patch"

        testCase "Summary is correct" <| fun () ->
            let m = parsePython edalPy
            Expect.equal m.Summary "e!DAL validation package for submission" "Summary"

        testCase "Description is non-empty" <| fun () ->
            let m = parsePython edalPy
            Expect.isFalse (m.Description = "") "Description not empty"

        testCase "Publish is true" <| fun () ->
            let m = parsePython edalPy
            Expect.isTrue m.Publish "Publish"

        testCase "Author fields are correct" <| fun () ->
            let a = (parsePython edalPy).Authors.[0]
            Expect.equal a.FullName        "Jonathan Bauer"           "FullName"
            Expect.equal a.Email           "bauer@nfdi4plants.org"    "Email"
            Expect.equal a.Affiliation     "University of Freiburg"   "Affiliation"
            Expect.equal a.AffiliationLink "https://uni-freiburg.de"  "AffiliationLink"

        testCase "Two tags parsed with correct names" <| fun () ->
            let names = (parsePython edalPy).Tags |> Array.map (fun t -> t.Name)
            Expect.equal names.Length 2 "two tags"
            Expect.isTrue (names |> Array.contains "e!DAL")           "tag e!DAL"
            Expect.isTrue (names |> Array.contains "data-submission") "tag data-submission"

        testCase "ReleaseNotes is non-empty" <| fun () ->
            let m = parsePython edalPy
            Expect.isFalse (m.ReleaseNotes = "") "ReleaseNotes not empty"

        testCase "CQCHookEndpoint is set" <| fun () ->
            let m = parsePython edalPy
            Expect.equal m.CQCHookEndpoint "https://mira.ipk-gatersleben.de/submit" "CQCHookEndpoint"

        testCase "ProgrammingLanguage is Python" <| fun () ->
            let m = parsePython edalPy
            Expect.equal m.ProgrammingLanguage "Python" "ProgrammingLanguage"

        testCase "tryExtractFromString returns Some" <| fun () ->
            Expect.isSome (tryParsePython edalPy) "tryExtractFromString Some"
    ]

    testList "treem-sequencing@0.0.1 (Python)" [

        testCase "Name is parsed correctly" <| fun () ->
            let m = parsePython treemSequencingPy
            Expect.equal m.Name "treem-sequencing" "Name"

        testCase "Version is 0.0.1" <| fun () ->
            let m = parsePython treemSequencingPy
            Expect.equal m.MajorVersion 0 "Major"
            Expect.equal m.MinorVersion 0 "Minor"
            Expect.equal m.PatchVersion 1 "Patch"

        testCase "Summary is correct" <| fun () ->
            let m = parsePython treemSequencingPy
            Expect.equal m.Summary "TreeM RNA Sequencing ARC Validation" "Summary"

        testCase "Author name and affiliation" <| fun () ->
            let a = (parsePython treemSequencingPy).Authors.[0]
            Expect.equal a.FullName    "Kristian Peters" "author name"
            Expect.equal a.Affiliation "JLU Giessen"     "affiliation"

        testCase "Two tags with correct names" <| fun () ->
            let names = (parsePython treemSequencingPy).Tags |> Array.map (fun t -> t.Name)
            Expect.equal names.Length 2 "two tags"
            Expect.isTrue (names |> Array.contains "TreeM")          "tag TreeM"
            Expect.isTrue (names |> Array.contains "RNA-Sequencing") "tag RNA-Sequencing"

        testCase "No CQCHookEndpoint" <| fun () ->
            let m = parsePython treemSequencingPy
            Expect.equal m.CQCHookEndpoint "" "no hook"

        testCase "ProgrammingLanguage is Python" <| fun () ->
            let m = parsePython treemSequencingPy
            Expect.equal m.ProgrammingLanguage "Python" "ProgrammingLanguage"

        testCase "tryExtractFromString returns Some" <| fun () ->
            Expect.isSome (tryParsePython treemSequencingPy) "tryExtractFromString Some"
    ]

    // ── FrontmatterLanguage helpers ───────────────────────────────────────────

    testList "FrontmatterLanguage" [

        testCase "fromString recognises fsharp variants" <| fun () ->
            Expect.equal (FrontmatterLanguage.fromString "fsharp") FSharpFrontmatter "fsharp"
            Expect.equal (FrontmatterLanguage.fromString "fs")     FSharpFrontmatter "fs"
            Expect.equal (FrontmatterLanguage.fromString "f#")     FSharpFrontmatter "f#"

        testCase "fromString recognises python variants" <| fun () ->
            Expect.equal (FrontmatterLanguage.fromString "python") PythonFrontmatter "python"
            Expect.equal (FrontmatterLanguage.fromString "py")     PythonFrontmatter "py"

        testCase "fromString is case-insensitive" <| fun () ->
            Expect.equal (FrontmatterLanguage.fromString "FSHARP") FSharpFrontmatter "upper FSHARP"
            Expect.equal (FrontmatterLanguage.fromString "Python") PythonFrontmatter "mixed Python"

        testCase "fromString throws on unknown language" <| fun () ->
            Expect.throws (fun () -> FrontmatterLanguage.fromString "ruby" |> ignore)
                          "unsupported language should throw"

        testCase "toString round-trips" <| fun () ->
            Expect.equal (FrontmatterLanguage.toString FSharpFrontmatter) "FSharp" "FSharp"
            Expect.equal (FrontmatterLanguage.toString PythonFrontmatter) "Python" "Python"
    ]

    // ── ValidationPackageMetadata version helpers ─────────────────────────────

    testList "ValidationPackageMetadata.getSemanticVersionString" [

        testCase "stable version from ceplas (1.0.1)" <| fun () ->
            let m = parseFSharp ceplasExperimentalFsx
            Expect.equal (ValidationPackageMetadata.getSemanticVersionString m) "1.0.1" "semver string"

        testCase "stable version from edal (0.0.4)" <| fun () ->
            let m = parsePython edalPy
            Expect.equal (ValidationPackageMetadata.getSemanticVersionString m) "0.0.4" "semver string"
    ]
]

