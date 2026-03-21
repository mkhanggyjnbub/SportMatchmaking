using BusinessObjects;
using Repositories.Post;
using Repositories.PostParticipant;
using Services.Sport;
using Services.DTOs;

namespace Services.Post
{
    public class PostService : IPostService
    {
        private const byte POST_STATUS_OPEN = 1;

        private readonly IPostRepository _postRepository;
        private readonly ISportService _sportService;
        private readonly IPostParticipantRepository _postParticipantRepository;

        public PostService(
            IPostRepository postRepository,
            ISportService sportService,
            IPostParticipantRepository postParticipantRepository)
        {
            _postRepository = postRepository;
            _sportService = sportService;
            _postParticipantRepository = postParticipantRepository;
        }

        public async Task<List<MatchPost>> GetPostsAsync(string? keyword = null, int? sportId = null)
        {
            return await _postRepository.GetPostsAsync(keyword, sportId);
        }

        public async Task<MatchPost?> GetPostByIdAsync(long postId)
        {
            return await _postRepository.GetPostByIdAsync(postId);
        }

        public async Task<(bool Success, string Message, long? PostId)> CreatePostAsync(CreatePostDTO model, int creatorUserId)
        {
            if (creatorUserId <= 0)
            {
                return (false, "B?n ch?a ??ng nh?p.", null);
            }

            if (model == null)
            {
                return (false, "D? li?u không h?p l?.", null);
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return (false, "Tiêu ?? không ???c ?? tr?ng.", null);
            }

            var sport = await _sportService.GetSportByIdAsync(model.SportId);
            if (sport == null)
            {
                return (false, "Môn th? thao không t?n t?i.", null);
            }

            if (model.SlotsNeeded <= 0)
            {
                return (false, "S? l??ng slot c?n tìm ph?i l?n h?n 0.", null);
            }

            if (model.StartTime <= DateTime.Now)
            {
                return (false, "Th?i gian b?t ??u ph?i ? t??ng lai.", null);
            }

            if (model.EndTime.HasValue && model.EndTime.Value <= model.StartTime)
            {
                return (false, "Th?i gian k?t thúc ph?i l?n h?n th?i gian b?t ??u.", null);
            }

            if (model.SkillMin.HasValue && (model.SkillMin.Value < 1 || model.SkillMin.Value > 10))
            {
                return (false, "SkillMin không h?p l?.", null);
            }

            if (model.SkillMax.HasValue && (model.SkillMax.Value < 1 || model.SkillMax.Value > 10))
            {
                return (false, "SkillMax không h?p l?.", null);
            }

            if (model.SkillMin.HasValue && model.SkillMax.HasValue && model.SkillMin.Value > model.SkillMax.Value)
            {
                return (false, "SkillMin không ???c l?n h?n SkillMax.", null);
            }

            var entity = new MatchPost
            {
                CreatorUserId = creatorUserId,
                SportId = model.SportId,
                Title = model.Title.Trim(),
                MatchType = model.MatchType,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                LocationText = string.IsNullOrWhiteSpace(model.LocationText) ? null : model.LocationText.Trim(),
                GoogleMapsUrl = string.IsNullOrWhiteSpace(model.GoogleMapsUrl) ? null : model.GoogleMapsUrl.Trim(),
                City = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim(),
                District = string.IsNullOrWhiteSpace(model.District) ? null : model.District.Trim(),
                SkillMin = model.SkillMin,
                SkillMax = model.SkillMax,
                SlotsNeeded = model.SlotsNeeded,
                FeePerPerson = model.FeePerPerson,
                IsUrgent = model.IsUrgent,
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                Status = POST_STATUS_OPEN,
                ExpiresAt = model.ExpiresAt,
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };

            var created = await _postRepository.AddPostAsync(entity);

            var host = _postParticipantRepository.GetByPostAndUser(created.PostId, creatorUserId);
            if (host == null)
            {
                _postParticipantRepository.Add(new BusinessObjects.PostParticipant
                {
                    PostId = created.PostId,
                    UserId = creatorUserId,
                    Role = (byte)ParticipantRole.Host,
                    Status = (byte)ParticipantStatus.Confirmed,
                    PartySize = 1,
                    JoinedAt = DateTime.Now,
                    LeftAt = null
                });

                await _postParticipantRepository.SaveAsync();
            }

            return (true, "T?o bài ??ng thành công.", created.PostId);
        }
    }
}
