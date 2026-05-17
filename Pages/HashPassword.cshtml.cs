using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace APLH.Pages
{
    public class HashPasswordModel : PageModel
    {
        public string HashedPassword { get; set; } = "";

        public void OnPost(string password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            }
        }
    }
}
