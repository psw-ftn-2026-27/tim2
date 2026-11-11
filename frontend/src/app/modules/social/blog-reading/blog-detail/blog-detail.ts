import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Marked } from 'marked';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto, FollowRelationshipDto } from '../../api/social-api-types';
import { BlogComments, CommentEdit } from '../blog-comments/blog-comments';
import { BlogReading } from '../blog-reading';

// Turns markdown text into safe text: "<b>" is shown as "<b>", not run as HTML.
function escapeHtml(text: string): string {
  return text
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

// Our own markdown setup, separate from the global `marked`. It formats only headings,
// lists, bold, italic and links (rule P5). Everything else, including HTML the author
// typed, is shown as plain text exactly as written (rule P2).
// Returning false means "use marked's normal formatting".
const descriptionMarkdown = new Marked({
  renderer: {
    html: (token) => (token.block ? `<p>${escapeHtml(token.raw)}</p>` : escapeHtml(token.raw)),
    code: (token) => `<p>${escapeHtml(token.raw)}</p>`,
    codespan: (token) => escapeHtml(token.raw),
    table: (token) => `<p>${escapeHtml(token.raw)}</p>`,
    image: (token) => escapeHtml(token.raw),
    blockquote: (token) => `<p>${escapeHtml(token.raw)}</p>`,
    hr: (token) => `<p>${escapeHtml(token.raw)}</p>`,
    del: (token) => escapeHtml(token.raw),
    checkbox: (token) => escapeHtml(token.raw),
    // Only web and e-mail links are formatted; "javascript:" and others stay plain text.
    link: (token) => (/^(https?:|mailto:)/i.test(token.href) ? false : escapeHtml(token.raw)),
  },
});

@Component({
  imports: [BlogComments, RouterLink],
  selector: 'app-blog-detail',
  styleUrl: './blog-detail.scss',
  templateUrl: './blog-detail.html',
})
export class BlogDetail {
  private readonly blogReading = inject(BlogReading);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDto>(() => `/api/social/blogs/${this.id()}`);

  // The description as HTML, bound with [innerHTML] in the template.
  protected readonly descriptionHtml = computed(() => {
    const blog = this.detail.value();
    return blog ? descriptionMarkdown.parse(blog.description, { async: false }) : '';
  });

  protected readonly relationship = httpResource<FollowRelationshipDto>(() => {
    const blog = this.detail.value();
    const user = this.auth.user();
    if (!blog || !user || blog.authorId === user.id) {
      return undefined;
    }
    return `/api/social/follow-requests/relationship/${blog.authorId}`;
  });

  protected readonly error = signal<string | null>(null);
  protected readonly followError = signal<string | null>(null);

  protected async follow(authorId: string): Promise<void> {
    this.followError.set(null);
    try {
      await this.blogReading.sendFollowRequest({ receiverId: authorId });
      this.relationship.reload();
    } catch (failure) {
      this.followError.set(serverMessage(failure, 'Could not send the follow request.'));
    }
  }

  protected async cancelRequest(requestId: string): Promise<void> {
    this.followError.set(null);
    try {
      await this.blogReading.withdrawFollowRequest(requestId);
      this.relationship.reload();
    } catch (failure) {
      this.followError.set(serverMessage(failure, 'Could not cancel the follow request.'));
    }
  }

  protected formatDate(value: string): string {
    return new Date(value).toLocaleString('sr-RS');
  }

  protected async add(text: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.addComment(this.id(), { text });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the comment.'));
    }
  }

  protected async edit(edit: CommentEdit): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.updateComment(this.id(), edit.commentId, { text: edit.text });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not update the comment.'));
    }
  }

  protected async remove(commentId: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.deleteComment(this.id(), commentId);
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the comment.'));
    }
  }
}
