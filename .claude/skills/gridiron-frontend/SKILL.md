---
name: gridiron-frontend
description: Sports broadcast and sportsbook-inspired frontend design for the Gridiron NFL management game. Use this skill when building any UI component, page, or interface for Gridiron. Produces dark, data-dense, sports-media aesthetics reminiscent of ESPN, Yahoo Sports, and modern sportsbooks.
---

# Gridiron Frontend Design

This skill guides frontend development for Gridiron with a sports broadcast/sportsbook aesthetic. All UI should feel like a professional sports media platform—data-rich, dark-themed, and immediately legible for stat-heavy content.

## Aesthetic Direction

**Tone**: Sports broadcast control room meets Vegas sportsbook. Dark, serious, data-forward. Not playful, not minimal—information-dense and confident.

**Theme**: Dark mode by default. Light backgrounds are exceptions, not the rule.

## Color System

Use CSS variables. Define in a central theme file.

```css
:root {
  /* Base */
  --bg-primary: #0a0a0f;
  --bg-secondary: #12121a;
  --bg-tertiary: #1a1a24;
  --bg-card: #1e1e2a;
  
  /* Borders */
  --border-subtle: #2a2a3a;
  --border-emphasis: #3a3a4a;
  
  /* Text */
  --text-primary: #ffffff;
  --text-secondary: #a0a0b0;
  --text-muted: #606070;
  
  /* Accent - Team/League colors can override */
  --accent-primary: #00d4aa;    /* Teal/cyan for primary actions */
  --accent-win: #22c55e;        /* Green for wins/positive */
  --accent-loss: #ef4444;       /* Red for losses/negative */
  --accent-warning: #f59e0b;    /* Amber for warnings/neutral */
  
  /* Live/Active states */
  --live-pulse: #ef4444;
  --active-highlight: rgba(0, 212, 170, 0.1);
}
```

Accent colors should pop against the dark background. Use --accent-win and --accent-loss consistently for positive/negative values (point differentials, cap space, win/loss records).

## Typography

### Font Choices

**Headlines/Stats**: Use condensed, bold sans-serifs. Suggestions:
- Oswald
- Barlow Condensed  
- Anton
- Bebas Neue
- Industry (if available)

**Body/UI**: Clean, legible sans-serif with tabular figures for numbers:
- IBM Plex Sans
- Source Sans Pro
- Fira Sans

**Monospace for raw data**: When displaying play-by-play logs, box scores, or code:
- JetBrains Mono
- Fira Code

### Type Scale

Headlines should be large and bold. Stats should be scannable. Use tabular figures (`font-variant-numeric: tabular-nums`) for any columnar numbers.

```css
.stat-value {
  font-family: 'Oswald', sans-serif;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
}
```

## Layout Patterns

### Data Density

Sports sites pack information. Embrace density:
- Sidebar standings/scores visible at all times where appropriate
- Multi-column layouts for stats
- Tables are acceptable and expected—style them well
- Card grids for players, teams, matchups

### Common Components

**Scoreboard/Ticker**: Horizontal scrolling or fixed bar showing live scores, upcoming games.

**Stat Tables**: Striped rows, sticky headers, sortable columns. Highlight leaders.

```
| Player         | POS | YDS  | TD | AVG  |
|----------------|-----|------|----|------|
| J. Williams    | RB  | 1,247| 12 | 5.2  | ← highlight row (leader)
| M. Harrison    | WR  | 1,102| 9  | 14.8 |
```

**Matchup Cards**: Two teams facing off. Logos, records, key stats, game time.

**Player Cards**: Photo/avatar, position, key attributes, contract status. Badge overlays for status (injured, free agent, franchise tag).

**Standings Widget**: Division/conference groupings, W-L-T, PCT, PF, PA, DIFF, streak.

**Box Score**: Quarter-by-quarter scoring, team stats, individual leaders.

### Sidebar Usage

Left or right sidebar for persistent context:
- League standings
- Recent results
- Upcoming schedule
- Cap space summary
- Trade deadline countdown

### Responsive Approach

Mobile: Stack into single column. Scoreboard becomes swipeable. Tables become horizontally scrollable. Cards collapse to essential info.

## Visual Details

### Borders and Dividers

Use subtle borders to separate sections. 1px solid var(--border-subtle). Occasionally use accent-colored left borders on highlighted items.

### Shadows and Depth

Minimal. Dark themes don't need heavy shadows. Use subtle elevation via background color stepping (--bg-secondary on --bg-primary).

### Team Colors

When displaying team-specific content, incorporate team colors as accents. Apply to:
- Left border on team cards
- Header background tint
- Logo backdrops

### Status Indicators

- **Live**: Red pulsing dot with "LIVE" badge
- **Final**: Muted text "FINAL"
- **Upcoming**: Countdown or date/time

### Numbers and Stats

Large, bold, prominent. Stats are the content. Make them scannable.

Positive numbers: --accent-win  
Negative numbers: --accent-loss  
Neutral: --text-primary

### Micro-interactions

- Hover states on table rows (subtle background shift)
- Smooth transitions on tab switches
- Loading skeletons for data fetching
- Subtle scale on card hover (transform: scale(1.01))

## Component Archetypes

When building these, follow patterns from ESPN, Yahoo Sports, DraftKings, FanDuel:

1. **Standings Table** — Sortable, division groupings, playoff line indicator
2. **Schedule Grid** — Week-by-week, bye weeks marked, results with scores
3. **Roster Table** — Position filters, sortable columns, contract status badges
4. **Player Profile** — Stats by season, career totals, contract details, transaction history
5. **Game Center** — Play-by-play feed, box score tabs, scoring summary
6. **Draft Board** — Round-by-round, pick trading visualization, prospect profiles
7. **Trade Block** — Available players, asset comparison tool
8. **League Dashboard** — Activity feed, standings snapshot, upcoming games, cap standings

## Anti-Patterns

Avoid:
- Light/white backgrounds as defaults
- Excessive whitespace (embrace density)
- Generic card layouts without sports context
- Pastel colors
- Rounded corners larger than 8px
- Sans-serif fonts without character (Inter, Arial, Roboto for headlines)
- Burying stats behind clicks—surface key numbers immediately

## Implementation Notes

- Use TailwindCSS classes aligned with the color system above
- Define custom colors in tailwind.config.js extending the theme
- Prefer CSS Grid for standings/stat tables, Flexbox for cards
- Use Recharts or similar for visualizations (player progression charts, cap graphs)
- Test mobile layouts early—sports fans are often on mobile
