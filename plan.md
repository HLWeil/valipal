# valipal plan

This repository will contain the source code for a small python library. The library contains helpers for validating other projects called ARCs and is supposed to be used in scripts, which are run in CI pipelines.

The validation output should always follow a defined schema, which is defined in the [validation-specs.md](references/validation-specs.md) file. 

Inside the scripts, the user should fill out administrative metadata through some frontmatter interface. Then he defines unit test cases. The unit cases are then executed and the results are written to the files defined in the specs. Therefore, the user should not have to worry about the output format and can focus on defining the test cases.

The usage experience we want to have can be observed in the FSharp scripts [ceplas-experimental](references/ceplas-experimental@1.0.1.fsx) and [plant-growth](references/plant-growth@1.0.0.fsx). Currently, the python equivalents [edal](references/edal@0.0.4.py) and [treem-sequencing](references/treem-sequencing@0.0.1.py) still contain all the boilerplate code for defining the logic behind unit cases and writing the output files. The goal of this library is to abstract away all the boilerplate code and provide a simple interface for defining test cases and writing the output files.

Output format must still always adhere to the specs. How both FSharp and Python scripts still produce equivally structured output can be observed in the [validation-results](references/.arc-validate-results).