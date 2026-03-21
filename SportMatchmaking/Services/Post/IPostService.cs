using BusinessObjects;
using Services.DTOs;

namespace Services.Post
{
    public interface IPostService
    {
        Task<List<MatchPost>> GetPostsAsync(string? keyword = null, int? sportId = null);
        Task<MatchPost?> GetPostByIdAsync(long postId);
        Task<(bool Success, string Message, long? PostId)> CreatePostAsync(CreatePostDTO dto, int creatorUserId);
    }
}
