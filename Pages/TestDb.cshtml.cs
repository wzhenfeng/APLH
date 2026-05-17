using Microsoft.AspNetCore.Mvc.RazorPages;
using APLH.Data;
using APLH.Models;

namespace APLH.Pages
{
    public class TestDbModel : PageModel
    {
        private readonly SqlRepository _repository;
        private readonly IConfiguration _configuration;

        public TestDbModel(SqlRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public bool IsConnected { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string ConnectionString { get; set; } = "";
        public int UserCount { get; set; }
        public IEnumerable<User>? Users { get; set; }

        public async Task OnGetAsync()
        {
            ConnectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Not found";
            
            try
            {
                Users = await _repository.GetAllUsersAsync();
                UserCount = Users.Count();
                IsConnected = true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                ErrorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    ErrorMessage += " | Inner: " + ex.InnerException.Message;
                }
            }
        }
    }
}
