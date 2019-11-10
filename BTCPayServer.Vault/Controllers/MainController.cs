using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Vault.Services;
using BTCPayServer.Vault.ViewModels;
using Microsoft.AspNetCore.Mvc;


namespace BTCPayServer.Vault.Controllers
{
    public class MainController : Controller
    {
        private readonly PermissionsService _permissionsService;

        public MainController(PermissionsService permissionsService)
        {
            _permissionsService = permissionsService;
        }
        [Route("")]
        public async Task<IActionResult> Home()
        {
            return View(await _permissionsService.GetPermissions());
        }

        [HttpGet]
        [Route("permissions/revoke")]
        public IActionResult RevokePermission(string origin)
        {
            return View("Confirm", new ConfirmModel()
            {
                Action = "Revoke",
                Title = "Revoke permission",
                Description = $"Access to your hardware wallets from the website {origin} will need an explicit grant."
            });
        }
        [HttpPost]
        [Route("permissions/revoke")]
        public async Task<IActionResult> RevokePermissionPost(string origin)
        {
            await _permissionsService.Revoke(origin);
            TempData[WellKnownTempData.SuccessMessage] = $"Permissions to access your hardware wallet for {origin} got successfully revoked";
            return RedirectToAction(nameof(Home));
        }
    }
}
