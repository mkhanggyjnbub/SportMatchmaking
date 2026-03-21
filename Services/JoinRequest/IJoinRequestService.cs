using Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.JoinRequest
{
    public interface IJoinRequestService
    {
        void Create(CreateJoinRequestDTO dto);
        List<PostJoinRequestItemDTO> GetRequestsOfPost(long postId, int currentUserId);
        List<MyJoinRequestItemDTO> GetMyRequests(int currentUserId);
        void CancelRequest(long requestId, int currentUserId);
        void AcceptRequest(long requestId, int currentUserId);
        void RejectRequest(long requestId, int currentUserId);
    }
}
