namespace ARCExpect

/// Simple, Fable-transpilable JSON writer based on string composition.
/// Covers exactly the features required for writing validation_summary.json files.
module SimpleJson =

    /// Escapes special characters in a JSON string value.
    let private escapeString (s: string) : string =
        s
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\b", "\\b")
            .Replace("\f", "\\f")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")

    /// Represents a JSON value.
    type JsonValue =
        | JNull
        | JBool   of bool
        | JInt    of int
        | JFloat  of float
        | JString of string
        | JArray  of JsonValue list
        /// An ordered list of key/value pairs. Duplicate keys are not checked.
        | JObject of (string * JsonValue) list

    let rec private renderCore (indentSize: int) (level: int) (value: JsonValue) : string =
        let indent      = String.replicate (level       * indentSize) " "
        let childIndent = String.replicate ((level + 1) * indentSize) " "
        let nl  = if indentSize > 0 then "\n" else ""
        let sep = if indentSize > 0 then " "  else ""
        match value with
        | JNull     -> "null"
        | JBool b   -> if b then "true" else "false"
        | JInt  i   -> string i
        | JFloat f  -> sprintf "%g" f
        | JString s -> sprintf "\"%s\"" (escapeString s)
        | JArray items ->
            if items.IsEmpty then "[]"
            else
                let inner =
                    items
                    |> List.map (fun v -> childIndent + renderCore indentSize (level + 1) v)
                    |> String.concat ("," + nl)
                sprintf "[%s%s%s%s]" nl inner nl indent
        | JObject fields ->
            if fields.IsEmpty then "{}"
            else
                let inner =
                    fields
                    |> List.map (fun (k, v) ->
                        sprintf "%s\"%s\":%s%s"
                            childIndent
                            (escapeString k)
                            sep
                            (renderCore indentSize (level + 1) v))
                    |> String.concat ("," + nl)
                sprintf "{%s%s%s%s}" nl inner nl indent

    /// Renders a JsonValue as a compact JSON string (no whitespace).
    let render (value: JsonValue) : string =
        renderCore 0 0 value

    /// Renders a JsonValue as a pretty-printed JSON string with the given number of spaces per indent level.
    let renderIndented (indentSize: int) (value: JsonValue) : string =
        renderCore indentSize 0 value
