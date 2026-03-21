using BusinessObjects;

namespace Services.Sport
{
    public interface ISportService
    {
        Task<List<BusinessObjects.Sport>> GetSportsAsync();
        Task<BusinessObjects.Sport?> GetSportByIdAsync(int sportId);
    }
}
