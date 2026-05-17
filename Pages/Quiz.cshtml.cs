using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace APLH.Pages
{
    [Authorize]
    public class QuizModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}