using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Proyecto_Cine.Pages
{
    [Authorize]
    public class TestAuthModel : PageModel
    {
        public void OnGet() { }
    }
}
