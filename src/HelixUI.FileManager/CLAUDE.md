# CLAUDE.md — HelixUI FileManager

## Purpose
A file explorer component with drag-drop upload, preview, folder management, and cloud storage hooks.

## Key Features
- Tree view + grid/list view toggle
- Drag-and-drop file upload (chunked for large files)
- File preview (images, PDFs, text, video thumbnails)
- Context menu (rename, delete, move, download, share)
- Breadcrumb navigation
- Search/filter within current directory
- Multi-select with bulk actions
- Storage adapter pattern: local, Azure Blob, S3, custom

## Architecture
- `IFileStorageProvider` interface for pluggable backends
- Virtual file system model (consumers implement the provider)
- Thumbnail generation via consumer callback
- Upload progress with cancellation
- Keyboard navigable (arrow keys, enter to open, delete to remove)
