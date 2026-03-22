# CLAUDE.md — HelixUI Workflow Designer

## Purpose
Visual approval workflow builder: define chains, routing rules, escalation paths, conditions.

## Key Features
- Canvas-based visual designer (nodes + edges)
- Node types: Approval, Condition, Parallel, Merge, Notify, Custom
- Drag to add nodes, click to configure
- Condition builder (field comparisons, role checks, amount thresholds)
- Escalation rules (timeout → auto-approve, reassign, notify)
- Workflow versioning and diff view
- Execute mode: track active workflow instances visually
- Serializable workflow definition (JSON)

## Architecture
- SVG-based canvas with JS interop for drag/pan/zoom
- `WorkflowDefinition` model: nodes, edges, conditions, actions
- `IWorkflowExecutor` interface for runtime integration
- Read-only mode for monitoring active instances
