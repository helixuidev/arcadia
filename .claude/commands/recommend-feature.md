---
description: Rigorous, evidence-based feature recommendation audit for Arcadia Controls — scans the codebase, analyzes competitors, identifies gaps, and produces a scored roadmap.
---

# Feature Recommendation Audit — Arcadia Controls

You are the product strategist for Arcadia Controls, a commercial Blazor component library. Conduct a rigorous, evidence-based audit to recommend features that will drive revenue, adoption, and competitive differentiation.

**CRITICAL RULE**: Do NOT propose features that already exist. For EVERY recommendation, you MUST grep the codebase first. If you get caught proposing something we already have, the entire audit is worthless.

---

## Phase 1: Deep Product Inventory

### 1a. Package-by-package capability scan
For EACH package in `src/`, run:
```bash
# Parameters (the public API surface)
grep -rn '\[Parameter\]' src/<Package>/ --include="*.cs" --include="*.razor" | grep -v obj | grep -v bin
# Public methods (the programmatic API)
grep -rn 'public async\|public ValueTask\|public Task\|public void' src/<Package>/ --include="*.cs" | grep -v obj | grep -v test | grep -v get | grep -v set
# JS interop (hidden capabilities devs might not know about)
grep -rn 'InvokeAsync\|InvokeVoidAsync' src/<Package>/ --include="*.cs" | grep -v obj
# Events
grep -rn 'EventCallback' src/<Package>/ --include="*.cs" --include="*.razor" | grep -v obj
```

### 1b. Build a detailed feature matrix
For each package, document:

**Charts**:
- List every chart type (read the Components/Charts/ directory)
- For EACH chart: what parameters does it support? (animation, export, tooltips, legends, axes, zoom, pan, streaming, annotations, trendlines, thresholds)
- What shared capabilities come from ChartBase? (read Core/ChartBase.cs)
- What JS interop exists? (read Core/ChartInteropService.cs)
- What data processing exists? (downsampling, scales, layout engine)

**DataGrid**:
- List every feature parameter on ArcadiaDataGrid.razor.cs
- What editing modes exist? (inline, batch, what else?)
- What export formats? (CSV, Excel, PDF — read Services/)
- What filtering types? (quick filter, typed filters, column filters)
- What grouping/aggregation capabilities?
- What virtualization? (row virtualization, infinite scroll)
- What column features? (pin, reorder, resize, hide, freeze, stacked headers)
- What selection modes?
- What clipboard operations?
- What state persistence?
- What conditional formatting?

**DashboardKit**:
- Every DragGrid parameter and capability
- Every DragGridItem parameter
- What drag modes? What animation system? What persistence?

**UI Components**:
- List every component in src/Arcadia.UI/Components/
- For each: what does it do, what parameters, what variants?
- Which are interactive (JS interop) vs pure Blazor?

**FormBuilder**:
- Every field type
- Validation system capabilities
- Wizard/multi-step features
- Schema-driven form generation
- Layout options

**Gauge, Notifications, Theme, Core**:
- Key capabilities of each

### 1c. Capability summary
Output a summary table:
| Package | Components | Parameters | Public Methods | JS Interop Methods | Events |
|---------|-----------|------------|----------------|-------------------|--------|

## Phase 2: Competitive Intelligence

### 2a. Tier 1 competitors (commercial, direct threat)
Research each one — search their current feature pages, changelogs, and "what's new" blog posts:

**Syncfusion Blazor** — search: "syncfusion blazor components features 2026"
- Total component count
- DataGrid: every feature they advertise
- Charts: every chart type
- Unique selling points
- Recent additions (last 2 releases)
- Pricing

**Telerik UI for Blazor** — search: "telerik blazor components features 2026"
- Same breakdown

**DevExpress Blazor** — search: "devexpress blazor components features 2026"
- Same breakdown

### 2b. Tier 2 competitors (open source, indirect threat)
**MudBlazor** — search: "mudblazor components list", check GitHub stars, issues
- What do they have that we don't?
- What do users complain about in GitHub issues? (search: most-upvoted issues)

**Radzen Blazor** — search: "radzen blazor components"
- Same breakdown

**Ant Design Blazor** — search: "ant design blazor"
- Same breakdown

### 2c. Competitive gap matrix
Build this table (be honest — mark ◐ for partial implementations):

| Feature | Arcadia | Syncfusion | Telerik | DevExpress | MudBlazor | Radzen |
|---------|---------|------------|---------|------------|-----------|--------|

Categories to cover:
- DataGrid features (20+ rows)
- Chart types (15+ rows)
- Form components (10+ rows)
- Layout components (10+ rows)
- Navigation components (5+ rows)
- Data visualization beyond charts (5+ rows)
- Theming/styling capabilities (5+ rows)
- Accessibility features (5+ rows)
- Developer experience (localization, RTL, keyboard nav, etc.)

### 2d. Pricing comparison
| Vendor | Free Tier | Pro/Individual | Team | Enterprise |
|--------|-----------|---------------|------|------------|

## Phase 3: Market Demand Analysis

### 3a. Developer pain points (search the web for each)
Search for:
- "blazor datagrid" site:reddit.com — what do people complain about?
- "blazor component library" site:reddit.com — which libraries do people recommend and why?
- "switching from mudblazor" OR "switching from syncfusion" site:reddit.com — why do people switch?
- "blazor components missing" OR "blazor ecosystem gaps" — what's underserved?
- "blazor enterprise" site:reddit.com — what do enterprise teams need?
- GitHub: most-upvoted open issues on MudBlazor, Radzen repos

### 3b. Enterprise requirements
Search for:
- "blazor enterprise requirements 2025 2026"
- "blazor LOB application" (line of business)
- "blazor ERP components"
What do enterprise buyers specifically need that component libraries often lack?

### 3c. Emerging trends
Search for:
- "blazor AI components" — AI-assisted features (copilot, smart filters, anomaly detection)
- "blazor real-time" — real-time/SignalR integration
- "blazor PWA components" — offline-first capabilities
- "blazor .NET 10" — what's coming in the framework that enables new component patterns?
- "blazor MAUI hybrid" — cross-platform considerations

### 3d. What makes people PAY for components?
Search for:
- "why pay for blazor components" site:reddit.com
- "syncfusion vs mudblazor" site:reddit.com — what justifies the price?
- "blazor component library worth it"
This tells us which features CONVERT free users to paid.

## Phase 4: Rigorous Verification

For EVERY feature you're considering recommending:

### 4a. Codebase check (MANDATORY)
```bash
# Search for the feature keyword in ALL source files
grep -ri "<feature_keyword>" src/ --include="*.cs" --include="*.razor" --include="*.js" --include="*.css" | grep -v obj | grep -v bin
# Check parameter names
grep -ri "<ParameterName>" src/ --include="*.cs" --include="*.razor" | grep -v obj
# Check JS interop
grep -ri "<feature_keyword>" src/ --include="*.js" | grep -v obj
# Check CSS for visual features
grep -ri "<feature_keyword>" src/ --include="*.css" | grep -v obj
```

### 4b. Demo check
- Does a demo page already show this feature?
- Is there a test page for it?

### 4c. Classification
For each potential feature, classify as:
- **ALREADY EXISTS** — we have it, skip
- **PARTIALLY EXISTS** — we have a foundation but it's incomplete, note what's missing
- **DOES NOT EXIST** — confirmed gap, proceed to scoring

## Phase 5: Strategic Scoring

### 5a. Score each confirmed gap
| Criterion | Weight | Scale | Description |
|-----------|--------|-------|-------------|
| **Revenue Impact** | 3x | 1-5 | 1=nice-to-have, 3=helps close deals, 5=must-have for enterprise sales |
| **Competitive Parity** | 2x | 1-5 | 1=nobody has it, 3=some competitors have it, 5=everyone has it and we don't |
| **User Demand** | 2x | 1-5 | 1=niche request, 3=regularly requested, 5=top community request |
| **Differentiation** | 2x | 1-5 | 1=commodity feature, 3=better than average, 5=unique to us/best-in-class |
| **Effort** | -2x | 1-5 | 1=afternoon, 2=day, 3=week, 4=sprint, 5=quarter |
| **Risk** | -1x | 1-5 | 1=safe, 3=some unknowns, 5=major technical risk |

**Score** = (Revenue × 3) + (Parity × 2) + (Demand × 2) + (Differentiation × 2) - (Effort × 2) - (Risk × 1)

Max possible: 60. Min possible: -15.

### 5b. Categorize
- **Quick Wins** (effort ≤ 2, score ≥ 20): Ship this sprint
- **High-Value Investments** (score ≥ 25, any effort): Worth the investment
- **Table Stakes** (parity ≥ 4, demand ≥ 4): Must have to be taken seriously
- **Moonshots** (differentiation = 5, effort ≥ 4): Risky but could be game-changing
- **Skip** (score < 10): Not worth it right now

### 5c. Implementation dependencies
For each recommended feature, note:
- Does it depend on another feature being built first?
- Does it require new JS interop?
- Does it require new NuGet packages?
- Does it affect existing public API (breaking change risk)?

## Phase 6: Actionable Roadmap

### 6a. Sprint plan (next 2 weeks)
Pick 3-5 quick wins that can ship immediately. For each:
- Exact component/file to modify
- Parameters to add
- Estimated line count
- Test strategy

### 6b. Quarter plan
Pick 3-5 high-value investments. For each:
- Architecture sketch (how it fits with existing code)
- New files/components needed
- Integration points with existing features
- Documentation and demo requirements

### 6c. Backlog
Everything else, ordered by score. Brief rationale for each.

## Output Format

Structure the full report as:

```
═══════════════════════════════════════
  ARCADIA CONTROLS — FEATURE AUDIT
  Date: {today}
═══════════════════════════════════════

1. PRODUCT INVENTORY
   [Summary table + key stats]

2. COMPETITIVE LANDSCAPE
   [Gap matrix + pricing comparison]

3. MARKET DEMAND
   [Top pain points with sources]

4. RECOMMENDATIONS (top 15)
   [Scored table with rationale]

5. ROADMAP
   [Sprint + Quarter + Backlog]

6. FEATURES WE ALREADY HAVE (that competitors don't)
   [Our unique strengths — don't forget to sell what we've got]
═══════════════════════════════════════
```

**IMPORTANT**: Section 6 matters. Identify features where Arcadia is AHEAD of competitors. These should be highlighted in marketing, not ignored.

**IMPORTANT**: Include exact URLs for every competitive claim. "Syncfusion has X" must link to where you found it.

**IMPORTANT**: Every recommendation must include the grep command you ran to verify we don't already have it, and the output (or lack thereof) that confirms it.
