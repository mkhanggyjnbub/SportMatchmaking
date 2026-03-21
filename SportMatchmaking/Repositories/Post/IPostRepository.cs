using BusinessObjects;

namespace Repositories.Post
{
    public interface IPostRepository
    {
        Task<List<MatchPost>> GetPostsAsync(string? keyword = null, int? sportId = null);
        Task<MatchPost?> GetPostByIdAsync(long postId);
        Task<MatchPost> AddPostAsync(MatchPost post);
        Task UpdatePostAsync(MatchPost post);
    }
}
