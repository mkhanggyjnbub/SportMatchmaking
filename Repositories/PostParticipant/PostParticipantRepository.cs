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

        public IQueryable<BusinessObjects.PostParticipant> GetQueryable()
        {
            return _postParticipantDAO.GetQueryable();
        }

        public void Add(BusinessObjects.PostParticipant entity)
        {
            _postParticipantDAO.Add(entity);
        }

        public void Update(BusinessObjects.PostParticipant entity)
        {
            _postParticipantDAO.Update(entity);
        }
    }
}
