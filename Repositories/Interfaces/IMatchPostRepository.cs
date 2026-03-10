using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface IMatchPostRepository
    {
        MatchPost? GetById(long postId);
        void Update(MatchPost post);
        void Save();
    }
}