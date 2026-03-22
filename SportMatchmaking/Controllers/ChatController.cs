//vinh

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services;
using SportMatchmaking.Hubs;

namespace SportMatchmaking.Controllers
{
    [Route("MatchPost/Chat")]
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

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(long? threadId)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _chatThreadService.GetChatIndexDataAsync(currentUserId.Value, threadId);
            return View(model);
        }

        [HttpPost("create-post-thread")]
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

        [HttpPost("add-confirmed-participants")]
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

        [HttpPost("send")]
        public async Task<IActionResult> SendMessageFromUi(long threadId, string messageText)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                await _chatThreadService.SendMessageAsync(threadId, currentUserId.Value, messageText);

                await _hubContext.Clients.Group(threadId.ToString())
                    .SendAsync("ReceiveMessage", new
                    {
                        threadId = threadId
                    });

                return RedirectToAction("Index", new
                {
                    threadId = threadId
                });
            }
            catch (Exception ex)
            {
                TempData["ChatError"] = ex.Message;

                return RedirectToAction("Index", new
                {
                    threadId = threadId
                });
            }
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditMessageFromUi(long messageId, long threadId, string newText)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var result = await _chatThreadService.EditMessageAsync(messageId, currentUserId.Value, newText);

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
                threadId = threadId
            });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteMessageFromUi(long messageId, long threadId)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var result = await _chatThreadService.DeleteMessageAsync(messageId, currentUserId.Value);

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
                threadId = threadId
            });
        }
    }
}