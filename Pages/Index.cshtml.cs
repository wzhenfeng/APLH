using Microsoft.AspNetCore.Mvc.RazorPages;

namespace APLH.Pages
{
    public class IndexModel : PageModel
    {
        public int CourseCount { get; set; } = 6;
        public int UserCount { get; set; } = 3;
    }
}