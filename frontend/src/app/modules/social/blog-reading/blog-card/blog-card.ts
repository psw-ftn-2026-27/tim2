import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Marked } from 'marked';
import { BlogDto } from '../../api/social-api-types';

// Our own markdown setup, separate from the global `marked`, used only to read the text.
const plainMarkdown = new Marked();

// Pieces of markdown that start a new block, so their words get a space before them.
const blockTypes = ['paragraph', 'heading', 'list_item', 'blockquote', 'code', 'table', 'space'];

@Component({
  imports: [RouterLink],
  selector: 'app-blog-card',
  styleUrl: './blog-card.scss',
  templateUrl: './blog-card.html',
})
export class BlogCard {
  readonly blog = input.required<BlogDto>();

  // The description without markdown markers: "# Hello **world**" becomes "Hello world".
  protected readonly plainDescription = computed(() => {
    const parts: string[] = [];
    const tokens = plainMarkdown.lexer(this.blog().description);
    plainMarkdown.walkTokens(tokens, (token) => {
      if (blockTypes.includes(token.type)) {
        parts.push(' ');
      }
      switch (token.type) {
        case 'text':
          // Text with inner pieces (bold, links...) is read through those pieces instead.
          if (!token.tokens) {
            parts.push(token.text);
          }
          break;
        case 'codespan':
        case 'code':
        case 'escape':
        case 'html':
          parts.push(token.text);
          break;
      }
    });
    return parts.join('').replace(/\s+/g, ' ').trim();
  });
}
