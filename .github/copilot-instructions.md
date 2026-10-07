# Project instructions

## Toolbox project

Toolbox is the main application project. When working on this solution, assume
that requested code changes should normally be made in Toolbox.

## Containers project

Containers is an existing project used by Toolbox.

Treat Containers as a black-box dependency.

The instructions in this file describe how Containers is intended to be used.
Use those instructions when writing code in Toolbox.

Do not read the containers project. Do not modify Containers unless I explicitly ask you to.

When working on Toolbox, do not inspect or rely on the internal implementation
of Containers when the documented usage instructions provide the required
information.

If the information needed to use Containers is not covered by these
instructions, ask me rather than assuming how its implementation works.

# Console UI Framework — A brief documentation on Containers project

## 1. `Container` — the core drawing surface

A `Container` represents a rectangular block of console output — think of it as a tile in a grid-based UI.

**Fields**
- `int Width`, `int Height` — the intended dimensions of the tile.
- `List<Text>[] Content` — an array sized to `Height`; each index is one row, holding a list of colored text segments for that row.

`Text` is an internal building block (`string Content`, `ConsoleColor Foreground`, `ConsoleColor Background`) — it's never constructed or edited directly. All content goes in through `Container`'s methods.

### Entering content — tagged strings

Content is typically written as a string with inline color tags:

```csharp
container.AlterContent(0, "<Red_White>Error: <Default>file not found");
```

- A tag is written `<...>` and colors everything from the end of that tag to the next tag (or end of string).
- `<Default>` — fixed global White foreground / Black background.
- `<White>` (foreground only) — foreground = White, background always defaults to **Black**.
- `<Red_White>` (foreground `_` background) — both explicit, using exact `ConsoleColor` enum names.
- **A string must start with a tag.** Anything before the first tag is discarded — no `Text` is built until a tag is encountered.

### Population methods

All operate on a single row:

| Method | Effect |
|---|---|
| `AlterContent(int row, string content)` | Parses tagged string, **replaces** row |
| `AlterContent(int row, List<Text> content)` | Replaces row with raw content directly (e.g. copying a row from another container: `a.AlterContent(0, b.Content[0]);`) |
| `AppendContent(int row, string content)` | Parses tagged string, **appends** to row |
| `AppendContent(int row, List<Text> content)` | Appends raw content to row |

**Width enforcement**: Not checked at insertion — you can insert content longer than `Width`. Overflow is only truncated at print time.

### Printing

- `Print(int row)` — writes a row's content with correct colors, no trailing newline.
- `PrintLine(int row)` — same, with a trailing newline.
- Both are just `Console.Write`/`Console.WriteLine` under the hood, with color switching per segment.

**No built-in layout engine.** Positioning multiple containers relative to each other (side-by-side or stacked) is entirely manual:

- **Side-by-side**: either interleave `Print` calls row-by-row (since `Print` doesn't newline), or use `Console.SetCursorPosition()` before each row.
- **Stacked**: print all rows of one container, then all rows of the next.
- **Known trap**: side-by-side containers with different `Height`s — printing in naive lockstep causes the taller one's overflow rows to wrap back to column 0 instead of staying in their column. Fix with one of: a filler container to occupy the gap, padding the shorter container with a blank row, or explicit `Console.SetCursorPosition()` for the overflow rows.

---

## 2. `Characters` — glyph sets for visuals

A static class providing character sets for building borders, bars, and indicators, meant to be embedded into the tagged strings fed into `Container`.

- **`BoxStyle`** (record struct): `Horizontal`, `Vertical`, `TopLeft/Right`, `BottomLeft/Right`, `TeeRight/Left/Down/Up`, `Cross`. Presets: `Single`, `Double`, `Heavy`, `Ascii`.
- **`BlockStyle`** (record struct): `Full`, `Dark`, `Medium`, `Light`. Presets: `Solid`, `Ascii`.
- **`ArrowStyle`** (sealed record): `Left`, `Right`, `Up`, `Down`, `Bullet`. Presets: `Unicode`, `Ascii`.
- **`StatusStyle`** (sealed record): `Ok`, `Error`, `Warning`, `Info`. Presets: `Unicode`, `Ascii`.
- **`Scale`** — a single standalone char (`'□'`).

Each family has an ASCII fallback variant for environments without extended character support.

---

## 3. `Button` / `ButtonsLayout` — arrow-key navigation

`Button` is a simple struct: `sbyte ID`, `bool Selected`, `bool Unactive`.

`ButtonsLayout` is an **abstract base class** — a pure state/logic manager for keyboard navigation. It does **no rendering** of its own. To use it, you create a subclass per screen (e.g. a menu), implement `Setup()` to populate the button grid, and the base class handles moving a "selector" around in response to input.

**Fields**
- `Button[][] ButtonsArray` — jagged grid of buttons for this screen.
- `int LastRow`, `int LastCol` — position of the currently selected button.

**Methods**

| Member | Purpose |
|---|---|
| `abstract Setup()` | Subclass-specific; populates `ButtonsArray` for that screen's layout |
| `virtual ProcessInput(ConsoleKeyInfo input)` | Reads arrow-key input, checks whether moving from `(LastRow, LastCol)` in that direction lands on a valid button (exists, not `Unactive`); if so, deselects the current button and selects the new one, updating `LastRow`/`LastCol` |
| `virtual MoveSelectorToCoords(int newRow, int newCol)` | Jumps selection to explicit coordinates, same select/deselect logic |
| `virtual MoveSelectorToID(int buttonID)` | Jumps selection to the button with the given `ID` |
| `virtual ResetAwaitingResponse()` | Resets an "awaiting confirmation" state after a button action has been processed |
| `virtual GetButtonFontCode(int buttonID, ButtonStatesFonts font)` | Returns the color/font code appropriate to a button's current state (e.g. selected vs. normal vs. unactive), for use when rendering that button |

**Key point**: navigation state changes (selection moving, etc.) don't automatically redraw anything. The developer must manually re-render the affected UI (typically via `Container.AlterContent`/`AppendContent` with tagged strings), using `GetButtonFontCode` to determine the correct colors per button's current state.

---

## 4. `Font` / `UIFonts<TKey>` — centralized theming

- **`Font`** — a readonly record struct: `ConsoleColor f` (foreground), `ConsoleColor b` (background). The atomic color pairing used throughout.
- **`UIFonts<TKey>`** (generic, `TKey : Enum`) — holds a `Dictionary<TKey, Font>` mapping semantic UI roles (e.g. "Header", "Line") to a `Font`. `ReturnFontCodeColorFromElement(TKey element)` looks up the role's `Font` and converts it into a tag-string color code (via `ColorsManager.BuildTextCode`), ready to prepend to content passed into `AlterContent`/`AppendContent`.
- **`DefaultUIElements`** — a general-purpose enum of common roles: `Text`, `Header`, `Value`, `HeavyLine`, `DoubleLine`, `Line`, `Background`. Custom enums can be used instead per project, since `UIFonts<TKey>` is generic.

**Purpose**: centralizing colors in one `UIFonts` instance means a single dictionary edit restyles every element using that role, consistently, across the whole screen. Code should always resolve colors through `ReturnFontCodeColorFromElement` on a configured instance, never hardcode tag strings.

---

## 5. Typical screen-building convention

Screens are generally built with a consistent three-method pattern operating on private static state:

- `Rescale()` — determines/creates the `Container`(s) and their sizes.
- `FillContainers()` — orchestrator; calls smaller per-element fill methods (e.g. `FillButtons()`), all of which populate content via `AlterContent`/`AppendContent`.
- `Render()` — the actual `Print`/`PrintLine` calls that output everything to the console.

This keeps sizing, content-building, and drawing as clearly separated, repeatable steps.




# UI Spec Format — Reading Guide

The following instructions define the notation used in the screen specs that follow in later messages. It assumes you already have the project's console UI framework
documentation (`Container`, `Characters`, `ButtonsLayout`, `UIFonts`, and the
`Rescale()` / `FillContainers()` / `Render()` screen convention and so on). This guide
only explains the *shorthand* used to describe a screen's layout — it is not
the framework itself.

Each UI instruction will contain one or more spec blocks in this notation,
describing one screen at a time. Treat each block as the complete, literal
requirement for that screen. **Do not invent or assume any value that isn't
present in the spec — if something looks missing, flag it instead of
guessing.**

Each block type below (`CONTAINER`, `BUTTON GRID`) supports one additional,
optional field: `notes:`. It exists purely as an escape hatch for genuine
edge cases that don't fit any structured field defined in this guide — not
as a substitute for a structured field that already exists. If there's
nothing extra to say, **leave it out of the block entirely** (or leave it
blank) — an omitted/empty `notes:` means "no additional requirements," not
"unspecified, use your judgment." When it *is* present, its content is just
as authoritative as any structured field — implement it literally, and ask
rather than guess if it's ambiguous.

---

## 1. Container block

```
CONTAINER <Name>
  anchor: <anchor keywords>
  width: <value>        height: <value>
  border: <BoxStyle preset | none>
  colors:
    border = <color spec>
    fill   = <color spec>
  content: static | dynamic
  notes: <optional free text — omit if none>
  notes: <optional free text — omit the line entirely if there's nothing to add>
```

`<Name>` is the identifier to use for the backing `Container` field/variable.

### Anchor keywords

| Keyword | Meaning |
|---|---|
| `top` / `bottom` / `left` / `right` | Pins that edge to the corresponding edge of its parent (or the console, if top-level). |
| `below X` / `above X` / `right-of X` / `left-of X` | Pins that edge adjacent to another named container, `X`. |
| `stretch-x` / `stretch-y` | Size along that axis is **not fixed** — recompute it in `Rescale()` from the parent's current dimensions every time. |
| `fill-remaining` | After fixed-size siblings are subtracted, this container takes whatever space is left along that axis. |

### Sizing rules

- A value written as `N (fixed)` is a literal constant. Never recompute it, never treat it as a default that can shift.
- A value with no `(fixed)` tag, or the words `stretch`/`fill-remaining`, is **computed** — it must be derived from `Console.WindowWidth`/`Console.WindowHeight` and sibling sizes inside `Rescale()`, and recalculated on every resize. Never hardcode a number here.

---

## 2. Colors

Three forms, always written explicitly as one of:

- `role:<TKey>` — resolve **live** via `UIFonts<TKey>.ReturnFontCodeColorFromElement(TKey.<TKey>)`. Never bake this down into a fixed tag string — the whole point is that editing the `UIFonts` dictionary later restyles this element automatically. Used for **plain containers** (border/fill).
- `fg:<ConsoleColor> bg:<ConsoleColor>` — a literal one-off `Font`, converted once via `ColorsManager.BuildTextCode` and used as a constant. Also for **plain containers**, when a role isn't worth defining.
- **`button-state` (buttons only)** — never assign a `role:` or `fg:/bg:` color directly to a button. The project already has per-button, per-part colors preconfigured via `ButtonsLayout.GetButtonFontCode(int buttonID, ButtonStatesFonts font)`. Overriding that with a hardcoded role or literal color makes the preconfigured system pointless. `font` selects **which visual part** of the button to color (e.g. a `borderFont` value for the border, a `labelFont` value for the label text) — the button's actual state (normal/selected/unactive) is resolved internally by the method itself and is never passed in. So every button needs **two calls**, one per part: `GetButtonFontCode(buttonID, borderFont)` and `GetButtonFontCode(buttonID, labelFont)`. The exact `buttonID` values and which `ButtonStatesFonts` members map to `borderFont`/`labelFont` are supplied per screen when we configure that screen's button layout — never written as a `colors:` block inside a grid spec.

A plain container can have **multiple independent named color zones** (e.g. `border`, `fill`). Each name applies only to that specific part — never reuse one zone's color for another part not named.

---

## 3. Button grids

```
BUTTON GRID
  anchor: <same anchor keywords as containers, relative to the parent container's inner area>
  width: <value>   height: <value>
  rows: <int | [array]>    columns: <int>
  sizing: fixed-button | fill-container
  xMargin: <n>   yMargin: <n>
  xGridMargin: <n>   yGridMargin: <n>
  xPadding: <n>   yPadding: <n>
  button border: <BoxStyle preset | none>
  id order: row-major | column-major
  button colors:
    border = GetButtonFontCode(buttonID, borderFont)
    label  = GetButtonFontCode(buttonID, labelFont)
  labels: [in id-order]
  notes: <optional free text — omit if none>
  notes: <optional free text — omit the line entirely if there's nothing to add>
```

**Anchor/size:** a grid is **not** assumed to fill its parent container. It gets its own box within the container's inner area, positioned with the exact same `anchor`/`width`/`height` vocabulary defined in §1 (`top`/`left`/`stretch-x`/`fill-remaining`/etc., or relative to another named element inside the same container). This lets a grid occupy only part of a container — e.g. the bottom half, or the space left after a label above it.

**Row/column form:**
- `rows: 4  columns: 1` — strict rectangular grid.
- `rows: [2,2,1]` — ragged grid: each array entry is the button count for that row. A row shorter than the widest row is horizontally centered unless the spec says otherwise.

**Sizing modes — exactly one is ever the input, the other is always derived. Never treat both button size and grid size as independent given values; they're mathematically linked and will conflict on resize if both are fixed.**
- `fixed-button`: button `Width`/`Height` are literal. The grid's total footprint falls out of the math (margins + N×button size + (N-1)×gutters).
- `fill-container`: the grid's outer size is fixed or stretched from its parent container. Button `Width`/`Height` = `(available space − margins − gutters) / count`, divided evenly.

**Box model (outside → in), matches the framework's spacing terms exactly:**

```
[ container's inner content area ]
   ↳ grid's own anchor + width/height → the grid's box (may not fill the container)
      [ grid box edge ]
         xMargin / yMargin              → gap: grid box edge → first button
         [ buttons ]
            xGridMargin / yGridMargin   → gap: BETWEEN buttons (gutter, not outer edge)
            [ button ]
               xPadding / yPadding      → gap: button border → label content
               [ label ]
```

**id order** determines how buttons are numbered into `Button.ID` and indexed into `ButtonsArray` — this matters directly for `ButtonsLayout.ProcessInput`'s directional (up/down/left/right) math, so implement exactly the order specified, not whichever is more convenient.

**Overflow (applies under `fixed-button` sizing only — `fill-container` derives button size from available space, so it always fits by construction):**

- **Horizontal overflow** — a row's buttons don't fit within the grid's declared width: snap/wrap that row and **grow the grid by adding as many additional rows as needed.** The `rows:` value in the spec is a minimum, not a hard cap, once wrapping is triggered.
- **Vertical overflow** — the resulting rows (including any added by horizontal wrapping) don't fit within the grid's declared height: **cut the remaining content that doesn't fit.** Do not shrink button size, do not compress margins/gutters to force a fit, and do not grow the grid's own `height` past what's declared — rows past the bottom edge are simply not rendered.

Any other behavior will be stated explicitly in the spec — never assume it silently.

---

## 4. What NOT to do

- Don't invent a size, color, margin, or default that isn't in the spec — ask/flag instead.
- Don't collapse a `role:` color into a hardcoded tag string; keep the `UIFonts` lookup live.
- Don't treat `(fixed)` values as approximate — they're exact.
- Don't assume a rectangular grid from a bare `rows: N` — check whether it's the `[array]` (ragged) form first.
- Don't reuse one named color zone (e.g. `border`) for a part it wasn't assigned to.
- Don't assign a `role:` or `fg:/bg:` color directly to a button — always resolve it through `GetButtonFontCode(buttonID, borderFont)` / `GetButtonFontCode(buttonID, labelFont)` instead.
- Don't cap a grid's row count at its declared `rows:` value when horizontal wrapping is triggered — let it grow. Only *vertical* overflow clips content; horizontal overflow never does.
- Don't use `notes:` as a substitute for a structured field that already exists (e.g. don't write `notes: make it red` instead of a proper `colors:` entry) — `notes:` is only for things this guide has no dedicated field for yet.

---

## 5. Worked example

```
CONTAINER HeaderBar
  anchor: top, stretch-x
  height: 3 (fixed)
  border: Double
  colors:
    border = role:Header
    fill   = role:Header

CONTAINER NavPanel
  anchor: left, below HeaderBar, stretch-y
  width: 20 (fixed)
  border: Single
  colors:
    border = role:Line
    fill   = role:Background

  BUTTON GRID
    anchor: fill-remaining (fills NavPanel's inner area after its own margins)
    width: fill-remaining   height: fill-remaining
    rows: 4    columns: 1
    sizing: fill-container
    xMargin: 1   yMargin: 1
    xGridMargin: 0   yGridMargin: 1
    xPadding: 2   yPadding: 0
    button border: Single
    id order: row-major
    button colors:
      border = GetButtonFontCode(buttonID, borderFont)
      label  = GetButtonFontCode(buttonID, labelFont)
    labels: [Start, Load, Settings, Exit]

CONTAINER MainView
  anchor: right-of NavPanel, below HeaderBar, stretch-x stretch-y
  width: fill-remaining
  border: Single
  colors:
    border = role:Line
    fill   = role:Text
  content: dynamic
```

Reading this: `HeaderBar` spans the full console width and never changes height.
`NavPanel` is a fixed 20-wide column stretching to fill remaining vertical
space. Its button grid is separately anchored to fill NavPanel's own inner
area (it isn't assumed automatically), and its button height is derived
(not fixed) from however tall that grid box ends up being. Each button's
color comes from two separate `GetButtonFontCode` calls — one for its
border, one for its label — not a spec'd role or literal. The project's
existing state-color config drives that, not this file.
`MainView` takes whatever horizontal space is left after `NavPanel`, and
resizes on both axes.

---

## 6. Workflow reminder

Every following message contains one or more spec blocks in this exact
notation, one screen at a time. Each is the full and final requirement for
that screen — implement it through the project's existing `Rescale()` /
`FillContainers()` / `Render()` convention, using the framework's own methods
(`AlterContent`, `AppendContent`, `Print`, `PrintLine`, `ReturnFontCodeColorFromElement`,
etc.) rather than reimplementing equivalent logic.