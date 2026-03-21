using BusinessObjects;

namespace Repositories.Sport
{
    public interface ISportRepository
    {
        Task<List<BusinessObjects.Sport>> GetSportsAsync();
        Task<BusinessObjects.Sport?> GetSportByIdAsync(int sportId);
    }
}
