using BusinessObjects;
using Repositories.Admin;

namespace Services.Admin
{
    public class AdminSportService : IAdminSportService
    {
        private readonly IAdminSportRepository _adminSportRepository;

        public AdminSportService(IAdminSportRepository adminSportRepository)
        {
            _adminSportRepository = adminSportRepository;
        }

        public async Task<List<Sport>> GetSportsAsync(string? keyword = null)
        {
            return await _adminSportRepository.GetSportsAsync(keyword);
        }

        public async Task<Sport?> GetSportByIdAsync(int sportId)
        {
            return await _adminSportRepository.GetSportByIdAsync(sportId);
        }

        public async Task<(bool Success, string Message, Sport? Sport)> AddSportAsync(Sport sport)
        {
            if (sport == null)
            {
                return (false, "Dữ liệu sport không hợp lệ.", null);
            }

            if (string.IsNullOrWhiteSpace(sport.Name))
            {
                return (false, "Tên môn thể thao không được để trống.", null);
            }

            bool exists = await _adminSportRepository.SportNameExistsAsync(sport.Name.Trim());
            if (exists)
            {
                return (false, "Tên môn thể thao đã tồn tại.", null);
            }

            if (sport.TeamMin <= 0 || sport.TeamMax <= 0 || sport.TeamMin > sport.TeamMax)
            {
                return (false, "Số lượng người chơi không hợp lệ.", null);
            }

            sport.Name = sport.Name.Trim();

            var createdSport = await _adminSportRepository.AddSportAsync(sport);
            return (true, "Thêm môn thể thao thành công.", createdSport);
        }

        public async Task<(bool Success, string Message)> UpdateSportAsync(Sport updatedSport)
        {
            if (updatedSport == null || updatedSport.SportId <= 0)
            {
                return (false, "Dữ liệu sport không hợp lệ.");
            }

            var existingSport = await _adminSportRepository.GetSportByIdAsync(updatedSport.SportId);
            if (existingSport == null)
            {
                return (false, "Không tìm thấy môn thể thao.");
            }

            if (string.IsNullOrWhiteSpace(updatedSport.Name))
            {
                return (false, "Tên môn thể thao không được để trống.");
            }

            bool exists = await _adminSportRepository.SportNameExistsAsync(updatedSport.Name.Trim(), updatedSport.SportId);
            if (exists)
            {
                return (false, "Tên môn thể thao đã tồn tại.");
            }

            if (updatedSport.TeamMin <= 0 || updatedSport.TeamMax <= 0 || updatedSport.TeamMin > updatedSport.TeamMax)
            {
                return (false, "Số lượng người chơi không hợp lệ.");
            }

            updatedSport.Name = updatedSport.Name.Trim();

            bool result = await _adminSportRepository.UpdateSportAsync(updatedSport);

            return result
                ? (true, "Cập nhật môn thể thao thành công.")
                : (false, "Cập nhật môn thể thao thất bại.");
        }

        public async Task<(bool Success, string Message)> DeleteSportAsync(int sportId)
        {
            if (sportId <= 0)
            {
                return (false, "Sport không hợp lệ.");
            }

            var sport = await _adminSportRepository.GetSportByIdAsync(sportId);
            if (sport == null)
            {
                return (false, "Không tìm thấy môn thể thao.");
            }

            bool isInUse = await _adminSportRepository.IsSportInUseAsync(sportId);
            if (isInUse)
            {
                return (false, "Không thể xóa vì môn thể thao đang được sử dụng trong bài đăng.");
            }

            bool result = await _adminSportRepository.DeleteSportAsync(sportId);

            return result
                ? (true, "Xóa môn thể thao thành công.")
                : (false, "Xóa môn thể thao thất bại.");
        }

        public async Task<bool> IsSportInUseAsync(int sportId)
        {
            return await _adminSportRepository.IsSportInUseAsync(sportId);
        }

        public async Task<List<SportImage>> GetSportImagesAsync()
        {
            return await _adminSportRepository.GetSportImagesAsync();
        }

        public async Task<SportImage?> GetSportImageByIdAsync(int imageId)
        {
            return await _adminSportRepository.GetSportImageByIdAsync(imageId);
        }

        public async Task<(bool Success, string Message, SportImage? SportImage)> AddSportImageAsync(SportImage sportImage)
        {
            if (sportImage == null)
            {
                return (false, "Dữ liệu ảnh không hợp lệ.", null);
            }

            if (string.IsNullOrWhiteSpace(sportImage.ImageUrl))
            {
                return (false, "Đường dẫn ảnh không được để trống.", null);
            }

            sportImage.ImageUrl = sportImage.ImageUrl.Trim();

            var createdImage = await _adminSportRepository.AddSportImageAsync(sportImage);
            return (true, "Thêm ảnh môn thể thao thành công.", createdImage);
        }

        public async Task<(bool Success, string Message)> UpdateSportImageAsync(SportImage updatedImage)
        {
            if (updatedImage == null || updatedImage.ImageId <= 0)
            {
                return (false, "Dữ liệu ảnh không hợp lệ.");
            }

            var existingImage = await _adminSportRepository.GetSportImageByIdAsync(updatedImage.ImageId);
            if (existingImage == null)
            {
                return (false, "Không tìm thấy ảnh môn thể thao.");
            }

            if (string.IsNullOrWhiteSpace(updatedImage.ImageUrl))
            {
                return (false, "Đường dẫn ảnh không được để trống.");
            }

            updatedImage.ImageUrl = updatedImage.ImageUrl.Trim();

            bool result = await _adminSportRepository.UpdateSportImageAsync(updatedImage);

            return result
                ? (true, "Cập nhật ảnh môn thể thao thành công.")
                : (false, "Cập nhật ảnh môn thể thao thất bại.");
        }

        public async Task<(bool Success, string Message)> DeleteSportImageAsync(int imageId)
        {
            if (imageId <= 0)
            {
                return (false, "Image không hợp lệ.");
            }

            var existingImage = await _adminSportRepository.GetSportImageByIdAsync(imageId);
            if (existingImage == null)
            {
                return (false, "Không tìm thấy ảnh môn thể thao.");
            }

            bool result = await _adminSportRepository.DeleteSportImageAsync(imageId);

            return result
                ? (true, "Xóa ảnh môn thể thao thành công.")
                : (false, "Không thể xóa ảnh vì ảnh đang được sử dụng hoặc thao tác thất bại.");
        }
    }
}