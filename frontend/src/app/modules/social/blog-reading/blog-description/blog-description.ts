import { Component, computed, input } from '@angular/core';
import { Marked, Token, Tokens } from 'marked';

// The one markdown setup for blog descriptions. marked only splits the text into pieces;
// the functions below decide how each piece is shown. Headings, lists, bold, italic and
// web or e-mail links are formatted (rule P5). Every other piece, including HTML the
// author typed, is shown as text exactly as written, with its line breaks (rules P2, P5).
const markdown = new Marked();

// Turns text into safe HTML text: "<b>" is shown as "<b>", not run as HTML.
function escapeHtml(text: string): string {
  return text
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

// A piece that is not formatted, shown exactly as the author wrote it.
function asWritten(raw: string, plain: boolean): string {
  const text = raw.replace(/\n+$/, '');
  return plain ? text : escapeHtml(text).replaceAll('\n', '<br>');
}

// On the card, a piece that is not formatted loses its markers too: only its words stay.
function withoutMarkers(token: Token): string {
  switch (token.type) {
    case 'table': {
      const table = token as Tokens.Table;
      const cells = [...table.header, ...table.rows.flat()];
      return cells.map((cell) => renderInlines(cell.tokens, true)).join(' ');
    }
    case 'blockquote':
      return renderBlocks((token as Tokens.Blockquote).tokens, true);
    case 'image':
      return (token as Tokens.Image).text;
    case 'code':
      return (token as Tokens.Code).text;
    case 'codespan':
      return (token as Tokens.Codespan).text;
    case 'del':
      return renderInlines((token as Tokens.Del).tokens, true);
    case 'hr':
    case 'checkbox':
      return '';
    default:
      return token.raw;
  }
}

// Blocks are the pieces that stand on their own lines: headings, paragraphs, lists...
function renderBlocks(tokens: Token[], plain: boolean): string {
  return tokens.map((token) => renderBlock(token, plain)).join('');
}

function renderBlock(token: Token, plain: boolean): string {
  switch (token.type) {
    case 'heading': {
      const heading = token as Tokens.Heading;
      const text = renderInlines(heading.tokens, plain);
      return plain ? `${text} ` : `<h${heading.depth}>${text}</h${heading.depth}>`;
    }
    case 'paragraph': {
      const text = renderInlines((token as Tokens.Paragraph).tokens, plain);
      return plain ? `${text} ` : `<p>${text}</p>`;
    }
    case 'list': {
      const list = token as Tokens.List;
      const items = list.items.map((item) => {
        const text = renderBlocks(item.tokens, plain);
        return plain ? `${text} ` : `<li>${text}</li>`;
      });
      if (plain) {
        return items.join('');
      }
      const tag = list.ordered ? 'ol' : 'ul';
      const start = list.ordered && list.start !== '' && list.start !== 1 ? ` start="${list.start}"` : '';
      return `<${tag}${start}>${items.join('')}</${tag}>`;
    }
    case 'text': {
      // Text directly inside a list item, without its own paragraph.
      const text = token as Tokens.Text;
      return text.tokens ? renderInlines(text.tokens, plain) : asWritten(text.text, plain);
    }
    case 'checkbox':
      // On the page, "[ ]" of a task list item stays as written, on the same line as the item.
      return plain ? withoutMarkers(token) : asWritten(token.raw, plain);
    case 'space':
      return plain ? ' ' : '';
    default:
      return plain ? `${withoutMarkers(token)} ` : `<p>${asWritten(token.raw, plain)}</p>`;
  }
}

// Inlines are the pieces inside one line: plain text, bold, italic, links...
function renderInlines(tokens: Token[], plain: boolean): string {
  return tokens.map((token) => renderInline(token, plain)).join('');
}

function renderInline(token: Token, plain: boolean): string {
  switch (token.type) {
    case 'strong': {
      const text = renderInlines((token as Tokens.Strong).tokens, plain);
      return plain ? text : `<strong>${text}</strong>`;
    }
    case 'em': {
      const text = renderInlines((token as Tokens.Em).tokens, plain);
      return plain ? text : `<em>${text}</em>`;
    }
    case 'link': {
      const link = token as Tokens.Link;
      // On the page, "javascript:" and other kinds of links stay as written.
      if (!plain && !/^(https?:|mailto:)/i.test(link.href)) {
        return asWritten(link.raw, plain);
      }
      const text = renderInlines(link.tokens, plain);
      return plain ? text : `<a href="${escapeHtml(link.href)}">${text}</a>`;
    }
    case 'text': {
      const text = token as Tokens.Text;
      return text.tokens ? renderInlines(text.tokens, plain) : asWritten(text.text, plain);
    }
    case 'escape':
      return asWritten((token as Tokens.Escape).text, plain);
    case 'br':
      return plain ? ' ' : '<br>';
    default:
      return plain ? withoutMarkers(token) : asWritten(token.raw, plain);
  }
}

@Component({
  selector: 'app-blog-description',
  styleUrl: './blog-description.scss',
  templateUrl: './blog-description.html',
})
export class BlogDescription {
  readonly description = input.required<string>();
  // True on the blog card: only the words of the description, without any markers, in one line.
  readonly plain = input(false);

  protected readonly html = computed(() => renderBlocks(markdown.lexer(this.description()), false));

  protected readonly text = computed(() =>
    renderBlocks(markdown.lexer(this.description()), true).replace(/\s+/g, ' ').trim(),
  );
}
