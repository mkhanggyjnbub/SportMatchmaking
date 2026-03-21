using BusinessObjects;
using DataAccessObjects;

namespace Repositories.Post
{
    public class PostRepository : IPostRepository
    {
        private readonly PostDAO _postDAO;

        public PostRepository(PostDAO postDAO)
        {
            _postDAO = postDAO;
        }

        public async Task<List<MatchPost>> GetPostsAsync(string? keyword = null, int? sportId = null)
        {
            return await _postDAO.GetPostsAsync(keyword, sportId);
        }

        public async Task<MatchPost?> GetPostByIdAsync(long postId)
        {
            return await _postDAO.GetPostByIdAsync(postId);
        }

        public async Task<MatchPost> AddPostAsync(MatchPost post)
        {
            return await _postDAO.AddPostAsync(post);
        }

        public async Task UpdatePostAsync(MatchPost post)
        {
            await _postDAO.UpdatePostAsync(post);
        }
    }
}
