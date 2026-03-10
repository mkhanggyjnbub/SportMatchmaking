using BusinessObjects;
using Repositories.Admin;

namespace Services.Admin
{
    public class AdminPostService : IAdminPostService
    {
        private readonly IAdminPostRepository _adminPostRepository;

        private const byte POST_STATUS_CANCELLED = (byte)PostStatus.Cancelled;

        public AdminPostService(IAdminPostRepository adminPostRepository)
        {
            _adminPostRepository = adminPostRepository;
        }

        public async Task<List<MatchPost>> GetPostsAsync(
            string? keyword = null,
            int? sportId = null,
            byte? status = null,
            string? city = null,
            string? district = null,
            int? creatorUserId = null)
        {
            return await _adminPostRepository.GetPostsAsync(keyword, sportId, status, city, district, creatorUserId);
        }

        public async Task<MatchPost?> GetPostByIdAsync(long postId)
        {
            return await _adminPostRepository.GetPostByIdAsync(postId);
        }

        public async Task<(bool Success, string Message)> CancelPostAsync(long postId)
        {
            if (postId <= 0)
            {
                return (false, "Post không hợp lệ.");
            }

            var post = await _adminPostRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                return (false, "Không tìm thấy bài đăng.");
            }

            if (post.Status == POST_STATUS_CANCELLED)
            {
                return (false, "Bài đăng đã ở trạng thái hủy trước đó.");
            }

            bool result = await _adminPostRepository.CancelPostAsync(postId);

            return result
                ? (true, "Hủy bài đăng thành công.")
                : (false, "Hủy bài đăng thất bại.");
        }

        public async Task<(bool Success, string Message)> UpdatePostStatusAsync(long postId, byte status)
        {
            if (postId <= 0)
            {
                return (false, "Post không hợp lệ.");
            }

            var post = await _adminPostRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                return (false, "Không tìm thấy bài đăng.");
            }

            bool isValidStatus = Enum.IsDefined(typeof(PostStatus), (PostStatus)status);
            if (!isValidStatus)
            {
                return (false, "Trạng thái bài đăng không hợp lệ.");
            }

            bool result = await _adminPostRepository.UpdatePostStatusAsync(postId, status);

            return result
                ? (true, "Cập nhật trạng thái bài đăng thành công.")
                : (false, "Cập nhật trạng thái bài đăng thất bại.");
        }

        public async Task<int> GetReportCountByPostIdAsync(long postId)
        {
            return await _adminPostRepository.GetReportCountByPostIdAsync(postId);
        }

        public async Task<List<MatchPost>> GetHighlyReportedPostsAsync(int minReportCount = 1)
        {
            if (minReportCount < 1)
            {
                minReportCount = 1;
            }

            return await _adminPostRepository.GetHighlyReportedPostsAsync(minReportCount);
        }

        public async Task<int> CountPostsAsync()
        {
            return await _adminPostRepository.CountPostsAsync();
        }

        public async Task<int> CountCompletedPostsAsync()
        {
            return await _adminPostRepository.CountCompletedPostsAsync();
        }
    }
}