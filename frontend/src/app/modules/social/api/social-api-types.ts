export type BlogStatus = 'Draft' | 'Published' | 'Archived';

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
  lastModifiedAt: string | null;
  images: string[];
  status: BlogStatus;
  comments: CommentDto[];
}

export interface CreateBlogDto {
  title: string;
  description: string;
  images: string[] | null;
}

export interface EditBlogDto {
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
  createdAt: string;
}

export interface ReceivedFollowRequestDto {
  id: string;
  senderId: string;
  createdAt: string;
}

export interface FollowerDto {
  userId: string;
  state: FollowRelationshipState;
  pendingRequestId: string | null;
}

export interface FollowedUserDto {
  userId: string;
}
