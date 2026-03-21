using BusinessObjects;
using DataAccessObjects;

namespace Repositories.Admin
{
    public class AdminSportRepository : IAdminSportRepository
    {
        private readonly AdminSportDAO _adminSportDAO;

        public AdminSportRepository(AdminSportDAO adminSportDAO)
        {
            _adminSportDAO = adminSportDAO;
        }

        public async Task<List<Sport>> GetSportsAsync(string? keyword = null)
        {
            return await _adminSportDAO.GetSportsAsync(keyword);
        }

        public async Task<Sport?> GetSportByIdAsync(int sportId)
        {
            return await _adminSportDAO.GetSportByIdAsync(sportId);
        }

        public async Task<Sport> AddSportAsync(Sport sport)
        {
            return await _adminSportDAO.AddSportAsync(sport);
        }

        public async Task<bool> UpdateSportAsync(Sport updatedSport)
        {
            return await _adminSportDAO.UpdateSportAsync(updatedSport);
        }

        public async Task<bool> DeleteSportAsync(int sportId)
        {
            return await _adminSportDAO.DeleteSportAsync(sportId);
        }

        public async Task<bool> IsSportInUseAsync(int sportId)
        {
            return await _adminSportDAO.IsSportInUseAsync(sportId);
        }

        public async Task<bool> SportNameExistsAsync(string sportName, int? excludeSportId = null)
        {
            return await _adminSportDAO.SportNameExistsAsync(sportName, excludeSportId);
        }

        public async Task<List<SportImage>> GetSportImagesAsync()
        {
            return await _adminSportDAO.GetSportImagesAsync();
        }

        public async Task<SportImage?> GetSportImageByIdAsync(int imageId)
        {
            return await _adminSportDAO.GetSportImageByIdAsync(imageId);
        }

        public async Task<SportImage> AddSportImageAsync(SportImage sportImage)
        {
            return await _adminSportDAO.AddSportImageAsync(sportImage);
        }

        public async Task<bool> UpdateSportImageAsync(SportImage updatedImage)
        {
            return await _adminSportDAO.UpdateSportImageAsync(updatedImage);
        }

        public async Task<bool> DeleteSportImageAsync(int imageId)
        {
            return await _adminSportDAO.DeleteSportImageAsync(imageId);
        }
    }
}