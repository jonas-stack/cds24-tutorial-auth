using Service.Blog.Dto;

namespace Service.Blog;

public interface IBlogService
{
    PostDetail GetById(long id);
    IEnumerable<Post> Newest(PostsQuery query);
    Task<long> CreateComment(long postId, CommentFormData data);
}