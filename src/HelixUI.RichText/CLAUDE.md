# CLAUDE.md — HelixUI RichText (JS Interop Context)

## Role: JS Interop Specialist (Rich Text Focus)

You own the RichTextEditor component. This wraps a proven JS editor (Tiptap/ProseMirror) with native Blazor bindings.

## Strategy
- Wrap Tiptap (ProseMirror-based, MIT license, extensible)
- C# toolbar configuration, content binding as HTML or JSON
- Extensions: mentions, tables, image upload, code blocks, markdown shortcuts
- Export: HTML, Markdown, PDF, plain text
- Collaborative editing ready (Yjs integration path)

## Key Features
- Toolbar: configurable, can be hidden for inline editing
- Two-way bind to HTML string or ProseMirror JSON
- Image handling: paste, drag-drop, upload callback
- Mentions: @ and # with async search
- Read-only mode for content display
- Character/word count
- Placeholder text support

## Interop Rules
Same as DataGrid — isolated modules, proper disposal, batched operations.
Focus trap and keyboard shortcuts must work in all render modes.
