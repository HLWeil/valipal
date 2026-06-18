namespace ARCExpect

/// Simple, Fable-transpilable XML writer based on string composition.
/// Covers exactly the features required for writing JUnit-style validation_report.xml files.
module SimpleXml =

    /// Escapes a string for use as an XML attribute value (double-quoted).
    let private escapeAttr (s: string) : string =
        s
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")

    /// Escapes a string for use as XML text content.
    let private escapeText (s: string) : string =
        s
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")

    /// Represents an XML node.
    type XmlNode =
        /// An XML element: tag name, attributes as (name * value) pairs, and child nodes.
        /// Produces a self-closing tag when children is empty.
        | XmlElement of tag: string * attrs: (string * string) list * children: XmlNode list
        /// Text content inside an element.
        | XmlText of string
        /// A CDATA section.
        | XmlCData of string

    let private attrsToString (attrs: (string * string) list) : string =
        attrs
        |> List.map (fun (name, value) -> sprintf " %s=\"%s\"" name (escapeAttr value))
        |> String.concat ""

    let rec private nodeToString (node: XmlNode) : string =
        match node with
        | XmlText s  -> escapeText s
        | XmlCData s -> sprintf "<![CDATA[%s]]>" s
        | XmlElement (tag, attrs, children) ->
            let attrStr = attrsToString attrs
            match children with
            | [] -> sprintf "<%s%s/>" tag attrStr
            | _  ->
                let inner = children |> List.map nodeToString |> String.concat ""
                sprintf "<%s%s>%s</%s>" tag attrStr inner tag

    /// Renders an XML node as a string with no XML declaration.
    let renderNode (node: XmlNode) : string =
        nodeToString node

    /// Renders an XML document with the standard XML 1.0 UTF-8 declaration prepended.
    let renderDocument (root: XmlNode) : string =
        sprintf "<?xml version=\"1.0\" encoding=\"utf-8\"?>%s" (nodeToString root)
