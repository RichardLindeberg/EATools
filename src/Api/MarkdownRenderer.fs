namespace EATool.Api

open System
open System.Text.RegularExpressions

/// Markdown to HTML renderer with XSS prevention
module MarkdownRenderer =
    
    /// Escape HTML special characters to prevent XSS
    let private escapeHtml (text: string) =
        text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;")
    
    /// Convert markdown to HTML with sanitized output
    let renderMarkdown (markdown: string) : string =
        let mutable html = markdown
        
        // Code blocks (must be before inline code)
        html <- Regex.Replace(html, "```([a-z]*)\n([\s\S]*?)\n```", fun m ->
            let lang = m.Groups.[1].Value
            let code = escapeHtml (m.Groups.[2].Value)
            sprintf "<pre><code class=\"language-%s\">%s</code></pre>" lang code)
        
        // Inline code (backticks)
        html <- Regex.Replace(html, "`([^`]+)`", fun m ->
            let code = escapeHtml (m.Groups.[1].Value)
            sprintf "<code>%s</code>" code)
        
        // Links [text](url)
        html <- Regex.Replace(html, "\[([^\]]+)\]\(([^)]+)\)", fun m ->
            let text = escapeHtml (m.Groups.[1].Value)
            let url = escapeHtml (m.Groups.[2].Value)
            sprintf "<a href=\"%s\" target=\"_blank\" rel=\"noopener noreferrer\">%s</a>" url text)
        
        // Headings (h1-h6)
        html <- Regex.Replace(html, "^#{6}\s+(.+)$", "<h6>$1</h6>", RegexOptions.Multiline)
        html <- Regex.Replace(html, "^#{5}\s+(.+)$", "<h5>$1</h5>", RegexOptions.Multiline)
        html <- Regex.Replace(html, "^#{4}\s+(.+)$", "<h4>$1</h4>", RegexOptions.Multiline)
        html <- Regex.Replace(html, "^#{3}\s+(.+)$", "<h3>$1</h3>", RegexOptions.Multiline)
        html <- Regex.Replace(html, "^#{2}\s+(.+)$", "<h2>$1</h2>", RegexOptions.Multiline)
        html <- Regex.Replace(html, "^#\s+(.+)$", "<h1>$1</h1>", RegexOptions.Multiline)
        
        // Bold (**text** or __text__)
        html <- Regex.Replace(html, "\*\*(.+?)\*\*", "<strong>$1</strong>")
        html <- Regex.Replace(html, "__(.+?)__", "<strong>$1</strong>")
        
        // Italic (*text* or _text_)
        html <- Regex.Replace(html, "\*(.+?)\*", "<em>$1</em>")
        html <- Regex.Replace(html, "_(.+?)_", "<em>$1</em>")
        
        // Unordered lists
        let listEvaluator (m: Match) = sprintf "<li>%s</li>" (escapeHtml (m.Groups.[1].Value))
        html <- Regex.Replace(html, "(?:^|\n)[-*+]\s+(.+)", listEvaluator, RegexOptions.Multiline)
        html <- Regex.Replace(html, "(<li>.*?</li>)", "<ul>$1</ul>", RegexOptions.Singleline)
        
        // Blockquotes
        html <- Regex.Replace(html, "^>\s+(.+)$", "<blockquote>$1</blockquote>", RegexOptions.Multiline)
        
        // Line breaks (double newline = paragraph)
        html <- Regex.Replace(html, "\n\n+", "</p><p>")
        html <- sprintf "<p>%s</p>" html
        
        // Preserve already escaped content
        html <- Regex.Replace(html, "&lt;p&gt;", "<p>")
        html <- Regex.Replace(html, "&lt;/p&gt;", "</p>")
        html <- Regex.Replace(html, "&lt;br&gt;", "<br>")
        
        html
    
    /// Create an HTML wrapper with styling
    let wrapHtml (title: string option) (htmlContent: string) : string =
        let titleHtml = match title with Some t -> sprintf "<h1>%s</h1>" (escapeHtml t) | None -> ""
        sprintf """<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>%s</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 900px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f9f9f9;
        }
        h1, h2, h3, h4, h5, h6 {
            color: #222;
            margin-top: 24px;
            margin-bottom: 16px;
            font-weight: 600;
        }
        h1 { font-size: 2em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }
        h2 { font-size: 1.5em; }
        h3 { font-size: 1.25em; }
        code {
            background-color: #f6f8fa;
            padding: 0.2em 0.4em;
            margin: 0;
            font-size: 85%%;
            border-radius: 3px;
            font-family: "SFMono-Regular", Consolas, "Liberation Mono", Menlo, monospace;
        }
        pre {
            background-color: #f6f8fa;
            padding: 16px;
            border-radius: 6px;
            overflow-x: auto;
            margin: 0;
        }
        pre code {
            background-color: transparent;
            padding: 0;
            margin: 0;
            font-size: 100%%;
        }
        a {
            color: #0366d6;
            text-decoration: none;
        }
        a:hover {
            text-decoration: underline;
        }
        blockquote {
            padding: 0 1em;
            color: #6a737d;
            border-left: 0.25em solid #dfe2e5;
            margin: 0;
        }
        ul {
            padding-left: 2em;
        }
        li {
            margin-bottom: 0.25em;
        }
        table {
            border-collapse: collapse;
            width: 100%%;
            margin: 16px 0;
        }
        table th, table td {
            border: 1px solid #dfe2e5;
            padding: 6px 13px;
        }
        table tr:nth-child(2n) {
            background-color: #f6f8fa;
        }
    </style>
</head>
<body>
    %s
    %s
</body>
</html>""" (escapeHtml (title |> Option.defaultValue "API Documentation")) titleHtml htmlContent
