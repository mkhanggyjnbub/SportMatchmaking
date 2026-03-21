using Repositories.Sport;

namespace Services.Sport
{
    public class SportService : ISportService
    {
        private readonly ISportRepository _sportRepository;

        public SportService(ISportRepository sportRepository)
        {
            _sportRepository = sportRepository;
        }

        public async Task<List<BusinessObjects.Sport>> GetSportsAsync()
        {
            return await _sportRepository.GetSportsAsync();
        }

        public async Task<BusinessObjects.Sport?> GetSportByIdAsync(int sportId)
        {
            return await _sportRepository.GetSportByIdAsync(sportId);
        }
    }
}
