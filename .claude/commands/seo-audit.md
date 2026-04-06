---
description: Comprehensive SEO audit — meta tags, structured data, sitemap, performance, content quality, internal linking, and Google Search Console verification.
---

# SEO Audit — Arcadia Controls

Run a full SEO audit of arcadiaui.com. Check EVERY item. Fix issues as you find them. Use automated checks — do not eyeball.

## Phase 1: Technical SEO (Codebase)

### 1.1 Meta Tags — Every Page Must Have These
```bash
# Check for missing titles
for f in $(find website/src/pages -name "*.astro" -o -name "*.mdx"); do
  grep -qL "title:" "$f" && grep -qL "<title" "$f" && echo "MISSING TITLE: $f"
done

# Check for missing descriptions
for f in $(find website/src/pages -name "*.astro" -o -name "*.mdx"); do
  grep -qL "description:" "$f" && echo "MISSING DESCRIPTION: $f"
done
```

Verify each layout file has:
- [ ] `<title>` — unique per page, under 60 chars, includes brand
- [ ] `<meta name="description">` — unique per page, 150-160 chars
- [ ] `<meta name="robots" content="index, follow">`
- [ ] `<link rel="canonical">` — absolute URL
- [ ] Open Graph: og:title, og:description, og:image, og:url, og:type, og:site_name
- [ ] Twitter: twitter:card, twitter:title, twitter:description, twitter:image
- [ ] Viewport: `<meta name="viewport" content="width=device-width, initial-scale=1.0">`

Files to check:
- `website/src/layouts/Layout.astro`
- `website/src/layouts/BlogLayout.astro`
- `website/src/layouts/DocsLayout.astro`

### 1.2 Structured Data (JSON-LD)
- [ ] **SoftwareApplication** schema on main layout — verify pricing matches actual tiers
- [ ] **BlogPosting** schema on blog layout — verify datePublished, author, headline
- [ ] **TechArticle** schema on docs layout — verify headline, publisher
- [ ] **BreadcrumbList** schema — should exist on docs pages (currently missing)
- [ ] **FAQPage** schema — add to homepage FAQ section if one exists
- [ ] Verify all schema at https://search.google.com/test/rich-results (paste page URL)

### 1.3 Sitemap
```bash
# Check sitemap integration
grep -n "sitemap" website/astro.config.mjs

# Check if build generates sitemap
ls website/dist/sitemap*.xml 2>/dev/null || echo "SITEMAP NOT BUILT — run npm build in website/"

# Verify robots.txt references sitemap
grep "Sitemap" website/public/robots.txt
```
- [ ] Astro sitemap integration is configured
- [ ] Sitemap generates all 135+ pages
- [ ] robots.txt Sitemap: URL matches actual location
- [ ] No orphan pages missing from sitemap

### 1.4 robots.txt
- [ ] Allows all legitimate crawlers
- [ ] AI crawlers welcomed (GPTBot, ClaudeBot, etc.)
- [ ] References sitemap
- [ ] No accidental disallows of important paths

### 1.5 llms.txt
- [ ] File exists at `website/public/llms.txt`
- [ ] Contains accurate product description, features, pricing
- [ ] Component counts match reality
- [ ] Test counts match reality

## Phase 2: Content Quality

### 2.1 Blog Posts
```bash
# Count posts
ls website/src/pages/blog/*.mdx | wc -l

# Check for keyword-rich titles
grep "title:" website/src/pages/blog/*.mdx | head -20

# Check for proper date formatting
grep "date:" website/src/pages/blog/*.mdx | head -20

# Check heading hierarchy (h1 should only appear once per page)
for f in website/src/pages/blog/*.mdx; do
  count=$(grep -c "^# " "$f")
  [ "$count" -gt 1 ] && echo "MULTIPLE H1: $f ($count)"
done
```

### 2.2 Doc Pages
```bash
# Count doc pages
find website/src/pages/docs -name "*.mdx" | wc -l

# Check all have descriptions
for f in $(find website/src/pages/docs -name "*.mdx"); do
  grep -q "description:" "$f" || echo "MISSING DESCRIPTION: $f"
done
```

### 2.3 Internal Linking
```bash
# Find broken internal links
grep -roh 'href="/docs/[^"]*"' website/src/ | sort -u | while read ref; do
  path=$(echo "$ref" | sed 's|href="|website/src/pages|;s|"$||')
  mdx="$path.mdx"; mdxIdx="$path/index.mdx"; astro="$path.astro"
  [ ! -f "$mdx" ] && [ ! -f "$mdxIdx" ] && [ ! -f "$astro" ] && echo "BROKEN: $ref"
done
```

## Phase 3: Performance Signals

### 3.1 Core Web Vitals Proxies
- [ ] Preconnect for external origins (fonts.googleapis.com, fonts.gstatic.com)
- [ ] Static site generation (output: 'static' in astro.config.mjs)
- [ ] Gzip enabled in nginx.conf
- [ ] Cache-Control headers for static assets (1 year immutable)
- [ ] CSS purging / tree-shaking enabled

### 3.2 Images
```bash
# Find images without dimensions
grep -rn "<img" website/src/ --include="*.astro" --include="*.mdx" | grep -v "width\|height" | head -10

# Check image sizes
find website/public -name "*.png" -o -name "*.jpg" | while read f; do
  size=$(stat -f%z "$f" 2>/dev/null || stat --printf="%s" "$f" 2>/dev/null)
  [ "$size" -gt 200000 ] && echo "LARGE: $f ($(($size/1024))KB)"
done
```

### 3.3 Security Headers (affect trust signals)
```bash
grep -E "Content-Security-Policy|Strict-Transport|X-Frame|Referrer-Policy|Permissions-Policy" website/nginx.conf
```

## Phase 4: Google Search Console Checks

### 4.1 Indexing Status
Use the DataForSEO API or web search to check:
- [ ] Search `site:arcadiaui.com` — how many pages indexed?
- [ ] Search `site:arcadiaui.com/docs` — are doc pages indexed?
- [ ] Search `site:arcadiaui.com/blog` — are blog posts indexed?
- [ ] Search for "arcadia controls blazor" — does the site appear?
- [ ] Search for "blazor chart library" — where does the site rank?

### 4.2 SERP Appearance
For each of these searches, check:
- [ ] Title tag renders correctly in SERP
- [ ] Meta description renders (not truncated, not auto-generated)
- [ ] Rich snippets show (FAQ, pricing, star ratings if applicable)
- [ ] Canonical URL is correct (no www/non-www duplication)
- [ ] No "blocked by robots.txt" or "noindex" issues

### 4.3 Keyword Rankings (use DataForSEO if available)
Check rankings for target keywords:
- "blazor chart library"
- "blazor component library"
- "blazor datagrid"
- "free blazor charts"
- "blazor dashboard components"
- "blazor UI components .NET"
- "arcadia controls"
- "blazor svg charts"
- "blazor form builder"
- "blazor drag and drop dashboard"

For each keyword:
- Current rank (position)
- SERP features present (featured snippet, PAA, knowledge panel)
- Competitors in top 10

### 4.4 Backlink Profile (use DataForSEO if available)
- [ ] Total backlinks count
- [ ] Referring domains count
- [ ] Domain authority / rank
- [ ] Top referring pages
- [ ] Anchor text distribution

### 4.5 Page Speed (use Lighthouse via DataForSEO if available)
Run Lighthouse on:
- [ ] https://arcadiaui.com (homepage)
- [ ] https://arcadiaui.com/docs (docs index)
- [ ] https://arcadiaui.com/blog (blog index)

Check scores for:
- Performance (target: 90+)
- Accessibility (target: 95+)
- Best Practices (target: 90+)
- SEO (target: 95+)

## Phase 5: Competitor SEO Comparison

Search for and compare:
- "blazor chart library" — who ranks, what content do they have?
- "blazor component library comparison" — are we mentioned?
- Check if competitors (Syncfusion, Telerik, DevExpress, MudBlazor, Radzen) have better structured data, more backlinks, or stronger content

## Phase 6: Fix & Report

### Immediate Fixes (do in this session)
1. Fix any missing meta tags
2. Fix any stale counts in structured data
3. Add missing og:image to BlogLayout
4. Verify sitemap generates correctly

### Report Format
```
═══════════════════════════════════════
  ARCADIA CONTROLS — SEO AUDIT REPORT
  Date: {today}
═══════════════════════════════════════

1. TECHNICAL HEALTH
   Score: X/10
   Issues: [list]

2. CONTENT
   Score: X/10
   Blog posts: N
   Doc pages: N
   Keyword coverage: [gaps]

3. SEARCH VISIBILITY
   Indexed pages: N
   Top keyword rankings: [table]
   Featured snippets: [list]

4. BACKLINKS
   Referring domains: N
   Domain rank: X

5. PERFORMANCE
   Lighthouse scores: [table]

6. ACTION ITEMS
   [Priority-ordered list]
═══════════════════════════════════════
```

Report EVERY issue found. Fix what you can. Prioritize what will have the most impact on search visibility.
