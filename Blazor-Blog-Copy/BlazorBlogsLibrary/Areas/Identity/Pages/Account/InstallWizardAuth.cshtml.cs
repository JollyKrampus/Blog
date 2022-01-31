using System.Threading.Tasks;
using BlazorBlogsLibrary.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlazorBlogsLibrary.Areas.Identity.Pages.Account
{
    public class InstallWizardAuthModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstallWizardAuthModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync(string paramUsername, string paramPassword, string ReturnUrl)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            await _signInManager.PasswordSignInAsync(paramUsername, paramPassword, false, false);
            return LocalRedirect(ReturnUrl);
        }
    }
}