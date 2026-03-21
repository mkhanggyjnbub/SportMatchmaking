using DataAccessObjects;

namespace Repositories.Sport
{
    public class SportRepository : ISportRepository
    {
        private readonly SportDAO _sportDAO;

        public SportRepository(SportDAO sportDAO)
        {
            _sportDAO = sportDAO;
        }

        public async Task<List<BusinessObjects.Sport>> GetSportsAsync()
        {
            return await _sportDAO.GetSportsAsync();
        }

        public async Task<BusinessObjects.Sport?> GetSportByIdAsync(int sportId)
        {
            return await _sportDAO.GetSportByIdAsync(sportId);
        }
    }
}
