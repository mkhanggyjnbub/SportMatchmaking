using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Models.Chat;

namespace SportMatchmaking.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatThreadService _chatThreadService;

        public ChatController(IChatThreadService chatThreadService)
        {
            _chatThreadService = chatThreadService;
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
    }
}