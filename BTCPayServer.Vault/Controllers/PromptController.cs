using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BTCPayServer.Vault.Services;
using BTCPayServer.Vault.Views.Prompt;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Vault.Controllers
{
    public class PromptController : Controller
    {
        private readonly Prompts _prompts;
        private readonly PermissionsService _permissionsService;

        public PromptController(Prompts prompts, PermissionsService permissionsService)
        {
            _prompts = prompts;
            _permissionsService = permissionsService;
        }

        [Route("authorize")]
        public IActionResult Authorize(uint id)
        {
            if (!_prompts.TryGetPrompt(id, out var prompt))
                return NotFound();
            return View(new AuthorizeViewModel() { Origin = prompt.Origin });
        }
        [Route("authorize")]
        [HttpPost]
        public async Task<IActionResult> Authorize(uint id, string command)
        {
            var confirm = command == "confirm";
            if (!_prompts.TryGetPrompt(id, out var prompt) ||
                !_prompts.TrySetResult(id, confirm))
                return NotFound();
            if (confirm)
            {
                await _permissionsService.Grant(prompt.Origin);
                TempData[WellKnownTempData.SuccessMessage] = $"Authorization to {prompt.Origin} granted";
            }
            else
            {
                TempData[WellKnownTempData.ErrorMessage] = $"Authorization to {prompt.Origin} denied";
            }
            return RedirectToAction(nameof(MainController.Home), "Main");
        }
    }
}
