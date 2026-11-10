export type BlogStatus = 'Draft' | 'Published' | 'Closed';

export interface CommentDto {
  id: string;
  userId: string;
  text: string;
  createdAt: string;
}

export interface BlogDto {
  id: string;
  authorId: string;
  title: string;
  description: string;
  createdAt: string;
  images: string[];
  status: BlogStatus;
  comments: CommentDto[];
}

export interface CreateBlogDto {
  title: string;
  description: string;
  images: string[] | null;
}

export interface CreateCommentDto {
  text: string;
}

export interface UpdateCommentDto {
  text: string;
}

export type FollowRelationshipState = 'Follow' | 'Pending' | 'Following';

export type FollowRequestStatus = 'Pending' | 'Accepted' | 'Rejected' | 'Removed' | 'Unfollowed';

export interface FollowRelationshipDto {
  state: FollowRelationshipState;
  pendingRequestId: string | null;
}

export interface SendFollowRequestDto {
  receiverId: string;
}

export interface SentFollowRequestDto {
  id: string;
  receiverId: string;
  status: FollowRequestStatus;
}
