//vinh

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services;
using Services.Models.Chat;
using SportMatchmaking.Hubs;

namespace SportMatchmaking.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatThreadService _chatThreadService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(
            IChatThreadService chatThreadService,
            IHubContext<ChatHub> hubContext)
        {
            _chatThreadService = chatThreadService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int currentUserId, long? threadId)
        {
            var model = await _chatThreadService.GetChatIndexDataAsync(currentUserId, threadId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePostThread(long postId)
        {
            try
            {
                var thread = await _chatThreadService.EnsurePostGroupThreadCreatedAsync(postId);

                return Ok(new
                {
                    success = true,
                    message = "Post group thread is ready.",
                    threadId = thread.ThreadId,
                    postId = thread.PostId,
                    threadType = thread.ThreadType,
                    createdAt = thread.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddConfirmedParticipants(long postId)
        {
            try
            {
                var addedCount = await _chatThreadService.AddConfirmedParticipantsToThreadAsync(postId);

                return Ok(new
                {
                    success = true,
                    message = "Confirmed participants have been added to the thread.",
                    postId = postId,
                    addedCount = addedCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMessageFromUi(long threadId, int currentUserId, string messageText)
        {
            try
            {
                await _chatThreadService.SendMessageAsync(threadId, currentUserId, messageText);

                await _hubContext.Clients.Group(threadId.ToString())
                    .SendAsync("ReceiveMessage", new
                    {
                        threadId = threadId
                    });

                return RedirectToAction("Index", new
                {
                    currentUserId = currentUserId,
                    threadId = threadId
                });
            }
            catch (Exception ex)
            {
                TempData["ChatError"] = ex.Message;

                return RedirectToAction("Index", new
                {
                    currentUserId = currentUserId,
                    threadId = threadId
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditMessageFromUi(long messageId, long currentUserId, long threadId, string newText)
        {
            var result = await _chatThreadService.EditMessageAsync(messageId, currentUserId, newText);

            if (result.success)
            {
                await _hubContext.Clients.Group(threadId.ToString())
                    .SendAsync("MessageEdited", new
                    {
                        messageId = messageId,
                        threadId = threadId
                    });
            }

            TempData["ChatMessage"] = result.message;
            TempData["ChatMessageType"] = result.success ? "success" : "error";

            return RedirectToAction("Index", new
            {
                currentUserId = currentUserId,
                threadId = threadId
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessageFromUi(long messageId, long currentUserId, long threadId)
        {
            var result = await _chatThreadService.DeleteMessageAsync(messageId, currentUserId);

            if (result.success)
            {
                await _hubContext.Clients.Group(threadId.ToString())
                    .SendAsync("MessageDeleted", new
                    {
                        messageId = messageId,
                        threadId = threadId
                    });
            }

            TempData["ChatMessage"] = result.message;
            TempData["ChatMessageType"] = result.success ? "success" : "error";

            return RedirectToAction("Index", new
            {
                currentUserId = currentUserId,
                threadId = threadId
            });
        }
    }
}