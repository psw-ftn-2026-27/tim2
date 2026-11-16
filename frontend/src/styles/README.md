# Styling guide

Which global class to use when, for every team. The classes live in the four partials
next to this file; `frontend/AGENTS.md` (Styling) holds the rules on where a new style goes.

## Palette

The "Yacht club" palette, defined once in `_tokens.scss`. Component SCSS refers to the
custom properties, never to a hex value.

| Token | Value | Use it for | Contrast on white |
|---|---|---|---|
| `--color-bg` | `#f2f0ef` | page background | — |
| `--color-surface` | `#ffffff` | cards, forms, tables | — |
| `--color-border` | `#bbbdbc` | decorative borders: cards, table rows | 1.9:1, never for text |
| `--color-field-border` | `#7d8382` | borders of inputs and other controls | 3.9:1 |
| `--color-text` | `#1b2427` | text | 15.8:1 |
| `--color-text-muted` | `#4f5856` | secondary text: labels, dates, hints | 7.3:1 |
| `--color-accent` | `#245f73` | buttons, links, the active item, Published | 7.1:1 |
| `--color-accent-hover` | `#1a4554` | hover of the above | — |
| `--color-danger` | `#733e24` | delete, archive, cancel, errors | 8.6:1 |
| `--color-highlight` | `#733e24` | money (EC) and points (XP) | 8.6:1 |

`--color-danger` and `--color-highlight` share a value but not a meaning: pick the token
by what the element means, so either can change later without touching the other.

## Page structure

Every page follows the same skeleton, top to bottom. Leave out the parts a page does not
need, but keep the order.

```html
<div class="stack">
  <p><a routerLink="/social">Back to blogs</a></p>        <!-- optional: back link -->

  <header class="page-header">                             <!-- title, main action -->
    <h1>My blogs</h1>
    <a class="button button--primary" routerLink="/social/create">Create blog</a>
  </header>

  <nav class="row row--wide">                              <!-- optional: sub-pages -->
    <a routerLink="/social/clubs">Clubs</a>
    <a routerLink="/social/connections">Connections</a>
  </nav>

  @if (error()) {
    <p class="error">{{ error() }}</p>
  }

  @if (blogs.isLoading()) {
    <p>Loading your blogs...</p>
  } @else if (blogs.error()) {
    <p class="error">Could not load your blogs. Log in and try again.</p>
  } @else {
    <table class="table">...</table>                       <!-- or a .grid of .card -->
    <p class="paging">...</p>
  }
</div>
```

A page whose content is one form is a centred card instead:

```html
<div class="card stack form-page">
  <h1>Create club</h1>
  <form class="stack" (submit)="submit($event)">
    ...
    <div class="row">
      <button class="button button--primary" type="submit">Save</button>
      <a class="button" routerLink="/social/clubs/mine">Cancel</a>
    </div>
  </form>
</div>
```

## Classes

### Layout (`_layout.scss`)

| Class | When to use it |
|---|---|
| `.container` | Set once in `app.html`; pages never use it. |
| `.stack` | Vertical space between the children of a page, card or form. |
| `.grid` | Cards side by side that wrap to new rows (tour list, club list). |
| `.page-header` | The page title with the page's main buttons on the right. |
| `.row` | Elements side by side: buttons in a table cell, Save and Cancel. In a table, put it on a `<div>` inside the `<td>`, never on the `<td>` itself. |
| `.row--wide` | Add to `.row` for more room, for links to sub-pages. |
| `.form-page` | A page that is one form: a centred card 32rem wide. Add `.form-page--sm` (24rem) for a short form like login, `.form-page--lg` (40rem) for a long one like a quiz. |

### Components (`_components.scss`)

| Class | When to use it |
|---|---|
| `.button` | Any secondary action: Edit, Cancel, Previous. Works on `<button>` and on `<a>`. |
| `.button--primary` | The one main action of a page or form: Save, Publish, Create. At most one per form. |
| `.button--danger` | An action that deletes, archives, removes or cancels something, and its Confirm button. |
| `.card` | A white box around one item or one form. |
| `.field` | A wrapper around one `<label>` and its `<input>`, `<select>` or `<textarea>`, plus its `.error` messages. |
| `.checkbox-group` | Add to a `<fieldset class="field">` of checkboxes or radios with a `<legend>`. |
| `.table` | Every data table. Use `<th scope="col">` for headers. On a phone the table scrolls sideways by itself. |
| `.badge` | A status written as a word: Published, Active. |
| `.badge--muted` | Add to `.badge` for a status that is not active: Draft, Archived, Retired, Cancelled, Pending. |
| `.amount` | Money and points: `<span class="amount">{{ price }} EC</span>`, `{{ xp }} XP`. |
| `.paging` | Previous / Page N / Next under a paged list. |
| `.meta` | Small secondary text: dates, hints, "none". |
| `.error` | An error message, from the server or from a form validator. |

## Rules

1. Use a global class before writing your own. A local class in the component's `.scss`
   is for layout that only this page has.
2. Never copy a global class into a component's `.scss` (no local `.paging`, `.actions`,
   `.links`). If you need a variant, ask the platform team.
3. No colour, font or spacing value in component SCSS. Use a token: `var(--space-2)`,
   `var(--color-text-muted)`.
4. A status is never colour alone: a `.badge` always has a word in it.
5. Every text the user sees is in English, including button labels, error messages and
   the fallback text passed to `serverMessage`. Format dates with `'en-GB'`.
6. A class used by more than one module goes global only through the platform team
   (`docs/protocols/zahtevi-platformskom-timu.md`).

## Review checklist

Copy this into the Frontend part of your review on every PR that changes the frontend in
this sprint. Each item that does not hold is a line comment.

```markdown
- [ ] The page follows the page structure (page-header, states, content, paging)
- [ ] Delete, archive, remove and cancel actions use `button--danger`
- [ ] Each form has at most one `button--primary`
- [ ] Statuses are shown with `badge` (`badge--muted` for inactive states)
- [ ] Money and XP use `amount`
- [ ] Button groups use `row`; no local `.actions`, `.paging` or `.links` in the component SCSS
- [ ] No hex colours or hard-coded spacing in the component SCSS
- [ ] All visible text is in English
```

Two checks you can run from `frontend/` before you review or open a PR:

```bash
grep -rnE "#[0-9a-fA-F]{3,6}\b" src/app --include=*.scss
```

```bash
grep -rnE "[čćžšđČĆŽŠĐ]" src/app --include=*.html --include=*.ts
```

Both should print nothing (place names in `core/map-demo` are the one exception).
