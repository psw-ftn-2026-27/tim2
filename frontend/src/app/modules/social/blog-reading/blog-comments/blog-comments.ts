import { Component, input, output, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { CommentDto } from '../../api/social-api-types';

export interface CommentEdit {
  commentId: string;
  text: string;
}

@Component({
  imports: [FormField],
  selector: 'app-blog-comments',
  styleUrl: './blog-comments.scss',
  templateUrl: './blog-comments.html',
})
export class BlogComments {
  readonly comments = input.required<CommentDto[]>();
  readonly currentUserId = input<string | null>(null);
  readonly commentingAllowed = input.required<boolean>();

  readonly addComment = output<string>();
  readonly editComment = output<CommentEdit>();
  readonly deleteComment = output<string>();

  protected readonly form = form(signal({ text: '' }), (path) => {
    required(path.text, { message: 'A comment cannot be empty.' });
  });

  protected readonly editingCommentId = signal<string | null>(null);

  // The comment whose three-dot menu is open.
  protected readonly openMenuId = signal<string | null>(null);

  // The comment whose deletion waits for "Yes".
  protected readonly confirmingDeleteId = signal<string | null>(null);

  protected readonly editForm = form(signal({ text: '' }), (path) => {
    required(path.text, { message: 'A comment cannot be empty.' });
  });

  protected submitComment(event: Event): void {
    event.preventDefault();
    this.addComment.emit(this.form.text().value());
    this.form().reset({ text: '' });
  }

  protected formatDate(value: string): string {
    return new Date(value).toLocaleString('sr-RS');
  }

  protected toggleMenu(commentId: string): void {
    this.openMenuId.update((open) => (open === commentId ? null : commentId));
  }

  protected askDelete(commentId: string): void {
    this.openMenuId.set(null);
    this.confirmingDeleteId.set(commentId);
  }

  protected confirmDelete(commentId: string): void {
    this.confirmingDeleteId.set(null);
    this.deleteComment.emit(commentId);
  }

  protected startEdit(comment: CommentDto): void {
    this.openMenuId.set(null);
    this.editingCommentId.set(comment.id);
    this.editForm().reset({ text: comment.text });
  }

  protected saveEdit(event: Event, commentId: string): void {
    event.preventDefault();
    this.editComment.emit({ commentId, text: this.editForm.text().value() });
    this.editingCommentId.set(null);
  }
}
