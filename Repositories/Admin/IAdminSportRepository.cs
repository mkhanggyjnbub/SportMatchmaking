using BusinessObjects;

namespace Repositories.Admin
{
    public interface IAdminSportRepository
    {
        Task<List<Sport>> GetSportsAsync(string? keyword = null);
        Task<Sport?> GetSportByIdAsync(int sportId);
        Task<Sport> AddSportAsync(Sport sport);
        Task<bool> UpdateSportAsync(Sport updatedSport);
        Task<bool> DeleteSportAsync(int sportId);
        Task<bool> IsSportInUseAsync(int sportId);
        Task<bool> SportNameExistsAsync(string sportName, int? excludeSportId = null);

        Task<List<SportImage>> GetSportImagesAsync();
        Task<SportImage?> GetSportImageByIdAsync(int imageId);
        Task<SportImage> AddSportImageAsync(SportImage sportImage);
        Task<bool> UpdateSportImageAsync(SportImage updatedImage);
        Task<bool> DeleteSportImageAsync(int imageId);
    }
}