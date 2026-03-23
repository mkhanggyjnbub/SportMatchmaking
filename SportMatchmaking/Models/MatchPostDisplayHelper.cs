using BusinessObjects;
using BusinessObjects.Enums;

namespace SportMatchmaking.Models
{
    public static class MatchPostDisplayHelper
    {
        public static IReadOnlyList<(byte Value, string Label)> MatchTypes { get; } =
            new List<(byte Value, string Label)>
            {
                (1, "Solo"),
                (2, "Doubles"),
                (3, "Team"),
                (4, "Flexible")
            };

        public static IReadOnlyList<(byte Value, string Label)> Statuses { get; } =
            new List<(byte Value, string Label)>
            {
                ((byte)PostStatus.Open, "Dang mo"),
                ((byte)PostStatus.Full, "Da du nguoi"),
                ((byte)PostStatus.Confirmed, "Da chot tran"),
                ((byte)PostStatus.Completed, "Da hoan thanh"),
                ((byte)PostStatus.Cancelled, "Da huy"),
                ((byte)PostStatus.Expired, "Het han")
            };

        public static IReadOnlyList<(byte Value, string Label)> FilterStatuses { get; } =
            new List<(byte Value, string Label)>
            {
                ((byte)PostStatus.Open, "Dang mo"),
                ((byte)PostStatus.Full, "Da du nguoi"),
                ((byte)PostStatus.Completed, "Da hoan thanh"),
                ((byte)PostStatus.Cancelled, "Da huy"),
                ((byte)PostStatus.Expired, "Het han")
            };

        public static IReadOnlyList<(byte Value, string Label)> ReportReasons { get; } =
            new List<(byte Value, string Label)>
            {
                (1, "Spam hoặc quảng cáo"),
                (2, "Nội dung sai sự thật"),
                (3, "Lừa đảo hoặc thu phí bất thường"),
                (4, "Ngôn từ xúc phạm"),
                (5, "Lý do khác")
            };

        public static string GetMatchTypeText(byte matchType)
        {
            var item = MatchTypes.FirstOrDefault(x => x.Value == matchType);
            return string.IsNullOrWhiteSpace(item.Label) ? $"Type {matchType}" : item.Label;
        }

        public static string GetStatusText(byte status)
        {
            var item = Statuses.FirstOrDefault(x => x.Value == status);
            return string.IsNullOrWhiteSpace(item.Label) ? $"Status {status}" : item.Label;
        }

        public static string GetParticipantRoleText(byte role)
        {
            return role == PostParticipantRoles.Creator ? "Chủ kèo" : "Participant";
        }

        public static string GetParticipantStatusText(byte status)
        {
            return status switch
            {
                PostParticipantStatuses.Confirmed => "Đã chốt",
                PostParticipantStatuses.Left => "Đã rời",
                PostParticipantStatuses.Removed => "Bị xóa",
                PostParticipantStatuses.NoShow => "No-show",
                _ => $"Status {status}"
            };
        }

        public static string GetSkillText(byte? skillMin, byte? skillMax)
        {
            if (!skillMin.HasValue && !skillMax.HasValue)
            {
                return "Mọi trình độ";
            }

            if (skillMin.HasValue && skillMax.HasValue)
            {
                return $"{skillMin}-{skillMax}";
            }

            if (skillMin.HasValue)
            {
                return $"Từ {skillMin}";
            }

            return $"Đến {skillMax}";
        }
    }
}
