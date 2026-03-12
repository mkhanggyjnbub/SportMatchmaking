using BusinessObjects;

namespace Services.Admin
{
    public interface IAdminSportService
    {
        Task<List<Sport>> GetSportsAsync(string? keyword = null);
        Task<Sport?> GetSportByIdAsync(int sportId);
        Task<(bool Success, string Message, Sport? Sport)> AddSportAsync(Sport sport);
        Task<(bool Success, string Message)> UpdateSportAsync(Sport updatedSport);
        Task<(bool Success, string Message)> DeleteSportAsync(int sportId);
        Task<bool> IsSportInUseAsync(int sportId);

        Task<List<SportImage>> GetSportImagesAsync();
        Task<SportImage?> GetSportImageByIdAsync(int imageId);
        Task<(bool Success, string Message, SportImage? SportImage)> AddSportImageAsync(SportImage sportImage);
        Task<(bool Success, string Message)> UpdateSportImageAsync(SportImage updatedImage);
        Task<(bool Success, string Message)> DeleteSportImageAsync(int imageId);
    }
}