using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostParticipantt = BusinessObjects.PostParticipant;

namespace Repositories.PostParticipant
{
    public interface IPostParticipantRepository
    {
        PostParticipantt? GetByPostAndUser(long postId, int userId);
        void Add(PostParticipantt entity);
        void Update(PostParticipantt entity);
        Task SaveAsync();
        Task<List<PostParticipantt>> GetByPostIdAsync(long postId);
        Task<int> GetConfirmedParticipantSlotsAsync(long postId);
    }
}
