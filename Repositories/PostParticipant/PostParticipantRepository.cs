using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.PostParticipant
{
    public class PostParticipantRepository : IPostParticipantRepository
    {
        private readonly PostParticipantDAO _postParticipantDAO;

        public PostParticipantRepository(PostParticipantDAO postParticipantDAO)
        {
            _postParticipantDAO = postParticipantDAO;
        }
        BusinessObjects.PostParticipant? IPostParticipantRepository.GetByPostAndUser(long postId, int userId)
        {
            return _postParticipantDAO.GetByPostAndUser(postId, userId);
        }

        public void Add(BusinessObjects.PostParticipant entity)
        {
            _postParticipantDAO.Add(entity);
        }

        public void Update(BusinessObjects.PostParticipant entity)
        {
            _postParticipantDAO.Update(entity);
        }

        public Task SaveAsync()
        {
            return _postParticipantDAO.SaveChangesAsync();
        }

        public Task<List<BusinessObjects.PostParticipant>> GetByPostIdAsync(long postId)
        {
            return _postParticipantDAO.GetByPostIdAsync(postId);
        }

        public Task<int> GetConfirmedParticipantSlotsAsync(long postId)
        {
            return _postParticipantDAO.GetConfirmedParticipantSlotsAsync(postId);
        }
    }
}
