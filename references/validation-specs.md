## Validation

The process of assessing quality parameters of an ARC is further referred to as _validation_ of the ARC against a [_validation package_](#validation-packages), where the _validation package_ is an arbitrary set of [validation cases](#validation-cases) that the ARC MUST pass to qualify as _valid_ in regard to the _validation package_.

### Validation cases

A **validation case** is the atomic unit of a [validation package](#validation-packages) describing a single, deterministic and reproducible requirement that the ARC MUST satisfy in order to qualify as _valid_ in regard to it.

Format and scope of these cases naturally vary depending on the type of ARC, aim of the containing validation package and tools used for creating and performing the validation. 
Therefore, no further requirements are made on the format of validation cases.

  example:

  > The following example shows a validation case simply defined using natural language.

  ```
  All Sample names in this ARC must be prefixed with the string "Sample_"
  ```

  Any ARC where all sample names are prefixed with the string "Sample_" would be considered valid in regard to this validation case.

### Validation packages

A **validation package** bundles a collection of [validation cases](#validation-cases) that the ARC MUST pass to qualify as _valid_ in regard to the _validation package_ with instructions on how to perform the validation and summarize the results.

Validation packages

- MUST be executable. 
  This can for example be achieved by implementing them in a programming language, a shell script, or a workflow language.

- MUST validate an ARC against all contained validation cases upon execution.

- MUST have a globally unique name.
  This will eventually be enforced by a central validation package registry

- SHOULD be versioned using [semantic versioning](https://semver.org/)

- MUST be enriched with the following mandatory metadata in an appropriate way (e.g. via yaml frontmatter, tables in a database, etc.):
  | Field | Type | Description |
  | --- | --- | --- |
  | Name | string | the name of the package |
  | Version | string | the version of the package |
  | Summary | string | a single sentence description (<=50 words) of the package |
  | Description | string | an unconstrained free text description of the package |

- MAY be enriched with the following optional metadata in an appropriate way (e.g. via yaml frontmatter, tables in a database, etc.):
  | Field | Type | Description |
  | --- | --- | --- |
  | HookEndpoint | string | An URL to trigger subsequent events based on the result of executing the validation package in a CQC context, see [Continuous quality control](#continuous-quality-control) and [ARC Apps](#arc-apps) |

- MAY be enriched with any additional metadata in an appropriate way (e.g. via yaml frontmatter, tables in a database, etc.).

- MUST create a `validation_report.*` file upon execution that summarizes the results of validating the ARC against the cases defined in the validation package.
  The format of this file SHOULD be of an established test result format such as [JUnit XML](https://github.com/windyroad/JUnit-Schema) or [TAP](https://testanything.org/).

- MUST create a `badge.svg` file upon execution that visually summarizes the results of validating the ARC against the validation cases defined in the validation package.
  The information displayed SHOULD be derivable from the `validation_report.*` file and MUST include the _Name_ of the validation package.

- MUST create a `validation_summary.json` file upon execution, which contains the mandatory and optional metadata specified above, and a high-level summary of the execution of the validation package following this schema:
  <details>
  <summary>validation_summary.json schema</summary>

  ```json
  {
    "$schema": "http://json-schema.org/draft-04/schema#",
    "type": "object",
    "properties": {
      "Critical": {
        "type": "object",
        "properties": {
          "HasFailures": {
            "type": "boolean"
          },
          "Total": {
            "type": "integer"
          },
          "Passed": {
            "type": "integer"
          },
          "Failed": {
            "type": "integer"
          },
          "Errored": {
            "type": "integer"
          }
        },
        "required": [
          "HasFailures",
          "Total",
          "Passed",
          "Failed",
          "Errored"
        ]
      },
      "NonCritical": {
        "type": "object",
        "properties": {
          "HasFailures": {
            "type": "boolean"
          },
          "Total": {
            "type": "integer"
          },
          "Passed": {
            "type": "integer"
          },
          "Failed": {
            "type": "integer"
          },
          "Errored": {
            "type": "integer"
          }
        },
        "required": [
          "HasFailures",
          "Total",
          "Passed",
          "Failed",
          "Errored"
        ]
      },
      "ValidationPackage": {
        "type": "object",
        "properties": {
          "Name": {
            "type": "string"
          },
          "Version": {
            "type": "string"
          },
          "Summary": {
            "type": "string"
          },
          "Description": {
            "type": "string"
          },
          "HookEndpoint": {
            "type": "string"
          }
        },
        "required": [
          "Name",
          "Version",
          "Summary",
          "Description"
        ]
      }
    },
    "required": [
      "Critical",
      "NonCritical",
      "ValidationPackage"
    ]
  }
  ```

  </details>

- SHOULD aggregate the result files in an appropriately named subdirectory.

### Reference implementation

A reference implementation for creating validation cases, validation packages, and validating ARCs against them is provided in the [arc-validate software suite](https://github.com/nfdi4plants/arc-validate)

## Continuous quality control

In addition to manually validate ARCs against validation packages, ARCs MAY be continuously validated against validation packages using a continuous integration (CI) system. 
This process is further referred to as _Continuous Quality Control_ (CQC) of the ARC. CQC can be triggered by any event that is supported by the CI system, e.g. a push to a branch of the ARC repository or a pull request.

### The cqc branch

To make sure that validation results are bundled with ARCs but do not pollute their commit history, validation results MUST be stored in a separate branch of the ARC repository.
This branch:

- MUST be named `cqc`
- MUST be an [orphan branch](https://git-scm.com/docs/git-checkout#Documentation/git-checkout.txt---orphanltnew-branchgt)
- MUST NOT be merged into any other branch. 
- MUST contain the following folder structure:

  `{$branch}/{$package}`:

  ```
  cqc branch root
  └── {$branch}
      └── {$package}
  ```
  
  where:
  - `{$branch}` is the name of the branch the validation was run on
  - `{$package}` is the name of the validation package the validation was run against. 
    this folder then MUST contain the files `validation_report.*` and `badge.svg` as described in the [validation package specification](#validation-packages).
    This folder MAY also be suffixed by the version of the validation package via a `@` character followed by the version number of the validation package: `{$package}@{$version}`, e.g. `package1@1.0.0`.

  example:

  > This example shows the validation results of the `main` and `branch-1` branches of the ARC repository against the `package1` and `package2` validation packages. for `package2`, an optional version hint of the package is included in the folder name:

  ```
  cqc-branch-root
  ├── branch-1
  │   ├── package1
  │   │   ├── badge.svg
  │   │   └── validation_report.xml
  │   └── package2@2.0.0
  │       ├── badge.svg
  │       └── validation_report.xml
  └── main
      ├── package1
      │   ├── badge.svg
      │   └── validation_report.xml
      └── package2@2.0.0
          ├── badge.svg
          └── validation_report.xml
  ```

Commits to the `cqc` branch MUST contain the commit hash of the commit that was validated in the commit message.