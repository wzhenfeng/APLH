using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Linq;
using APLH.Services;
using APLH.Models;

namespace APLH.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly LearningService _service;
        private readonly EmailService _emailService;

        // In-memory OTP store: email -> (otp, expiry)
        private static readonly Dictionary<string, (string Otp, DateTime Expiry)> _otpStore = new();

        public ApiController(LearningService service, EmailService emailService)
        {
            _service = service;
            _emailService = emailService;
        }

        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _service.AuthenticateAsync(request.Email, request.Password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                await _service.CreateActivityLogAsync(user.Id, "Logged into the system");

                return Ok(new { success = true, user = new { user.Name, user.Email, user.Role } });
            }
            return Ok(new { success = false, message = "Invalid email or password" });
        }

        [HttpPost("auth/register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _service.RegisterUserAsync(request.Name, request.Email, request.Password);
                await _service.CreateActivityLogAsync(user.Id, "Registered a new account");

                // Send welcome email — don't let email failure block registration
                try { await _emailService.SendEmailAsync(request.Email, request.Name); } catch { }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Registration succeeded but email failed.", error = ex.Message });
            }
        }

        [HttpPost("auth/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true });
        }

        [HttpGet("auth/currentuser")]
        public IActionResult GetCurrentUser()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok(new
                {
                    Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Name = User.FindFirstValue(ClaimTypes.Name),
                    Email = User.FindFirstValue(ClaimTypes.Email),
                    Role = User.FindFirstValue(ClaimTypes.Role)
                });
            }
            return Ok(new { success = false });
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses(string category = "all", string search = "")
        {
            IEnumerable<Course> courses;
            
            if (!string.IsNullOrEmpty(search))
                courses = await _service.SearchCoursesAsync(search);
            else if (category != "all")
                courses = (await _service.GetAllCoursesAsync()).Where(c => c.Category == category);
            else
                courses = await _service.GetAllCoursesAsync();
            
            return Ok(courses);
        }

        [HttpGet("courses/{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            var course = await _service.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();
            return Ok(course);
        }

        [HttpPost("courses/enroll")]
        public async Task<IActionResult> EnrollCourse([FromBody] EnrollRequest request)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var isEnrolled = await _service.IsUserEnrolledAsync(userId, request.CourseId);
            
            if (!isEnrolled)
            {
                await _service.EnrollUserAsync(userId, request.CourseId);

                await _service.CreateActivityLogAsync(userId, $"Enrolled in course ID {request.CourseId}");
                
                return Ok(new { success = true });
            }
            
            return Ok(new { success = false, message = "Already enrolled" });
        }

        [HttpPost("courses/save")]
        public async Task<IActionResult> SaveCourse([FromBody] Course course)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (course.Id == 0)
            {
                await _service.CreateCourseAsync(course);

                await _service.CreateActivityLogAsync(userId,
                    $"Added course: {course.Title}");
            }
            else
            {
                await _service.UpdateCourseAsync(course);

                await _service.CreateActivityLogAsync(userId,
                    $"Updated course: {course.Title}");
            }

            return Ok(new { success = true });
        }

        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            await _service.DeleteCourseAsync(id);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            await _service.CreateActivityLogAsync(userId,
                $"Deleted course ID {id}");

            return Ok(new { success = true });
        }

        [HttpGet("quiz/questions")]
        public async Task<IActionResult> GetQuizQuestions(int? courseId)
        {
            var questions = await _service.GetAllQuizQuestionsAsync();

            if (courseId.HasValue)
            {
                questions = questions.Where(q => q.CourseId == courseId.Value);
            }

            return Ok(questions);
        }

        [HttpGet("quiz/questions/{courseId}")]
        public async Task<IActionResult> GetQuizQuestionsByCourse(int courseId)
        {
            var questions = await _service.GetQuizQuestionsByCourseAsync(courseId);
            return Ok(questions);
        }

        [HttpPost("quiz/save")]
        public async Task<IActionResult> SaveQuizScore([FromBody] QuizScoreRequest request)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            await _service.SaveQuizScoreAsync(userId, request.CourseId, request.Score, request.Total);
            return Ok(new { success = true });
        }



        [HttpGet("profile/enrollments")]
        public async Task<IActionResult> GetUserEnrollments()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var courses = await _service.GetUserEnrolledCoursesAsync(userId);
            return Ok(courses);
        }

        [HttpGet("profile/scores")]
        public async Task<IActionResult> GetUserScores()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var scores = await _service.GetUserQuizScoresAsync(userId);
            return Ok(scores);
        }
        
        [HttpPost("quiz/questions/save")]
            public async Task<IActionResult> SaveQuizQuestion([FromBody] QuizQuestion question)
            {
                await _service.CreateQuizQuestionAsync(question);

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                await _service.CreateActivityLogAsync(userId,
                    "Added/Updated quiz question");
                
                return Ok(new { success = true });
            }

        [HttpDelete("quiz/questions/{id}")]
            public async Task<IActionResult> DeleteQuizQuestion(int id)
            {
                await _service.DeleteQuizQuestionAsync(id);

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                await _service.CreateActivityLogAsync(userId,
                    $"Deleted quiz question ID {id}");
                
                return Ok(new { success = true });
            }   

        [HttpGet("auth/google-login")]
public IActionResult GoogleLogin()
{
    var redirectUrl = Url.Action("GoogleResponse", "Api");
    var properties = new AuthenticationProperties
    {
        RedirectUri = redirectUrl
    };

    return Challenge(properties, "Google");
}

[HttpGet("auth/google-response")]
public async Task<IActionResult> GoogleResponse()
{
    var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    if (!result.Succeeded)
    {
        return Redirect("/");
    }

    var email = result.Principal.FindFirstValue(ClaimTypes.Email);
    var name = result.Principal.FindFirstValue(ClaimTypes.Name);

    if (string.IsNullOrEmpty(email))
    {
        return Redirect("/");
    }

    var user = await _service.GetUserByEmailAsync(email);

    if (user == null)
    {
        user = await _service.RegisterGoogleUserAsync(name ?? "Google User", email);
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Redirect("/");
}

[HttpPost("materials/save")]
public async Task<IActionResult> SaveCourseMaterial([FromBody] CourseMaterial material)
{
    if (material.Id == 0)
    {
        await _service.CreateCourseMaterialAsync(material);
    }
    else
    {
        await _service.UpdateCourseMaterialAsync(material);
    }

    return Ok(new { success = true });
}
[HttpGet("materials/{courseId}")]
public async Task<IActionResult> GetCourseMaterials(int courseId)
{
    var materials = await _service.GetCourseMaterialsAsync(courseId);

    return Ok(materials);
}

[HttpGet("chapters/{courseId}")]
public async Task<IActionResult> GetCourseChapters(int courseId)
{
    var chapters = await _service.GetCourseChaptersAsync(courseId);
    return Ok(chapters);
}

[HttpPost("chapters/save")]
public async Task<IActionResult> SaveCourseChapter([FromBody] CourseChapter chapter)
{
    var existing = await _service.GetCourseChaptersAsync(chapter.CourseId);
    if (existing.Any(c => c.ChapterOrder == chapter.ChapterOrder))
    {
        return Conflict(new { success = false, message = $"Chapter {chapter.ChapterOrder} already exists for this course." });
    }

    var created = await _service.CreateCourseChapterAsync(chapter);

    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    await _service.CreateActivityLogAsync(userId,
        $"Added chapter '{chapter.ChapterTitle}' to course ID {chapter.CourseId}");

    return Ok(new { success = true, chapter = created });
}

[HttpDelete("chapters/{id}")]
public async Task<IActionResult> DeleteCourseChapter(int id)
{
    await _service.DeleteCourseChapterAsync(id);
    return Ok(new { success = true });
}

[HttpDelete("users/{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
    await _service.DeleteUserAsync(id);
    return Ok(new { success = true });
}
        // ── Forgot Password ──────────────────────────────────────────────────────

        [HttpPost("auth/forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _service.GetUserByEmailAsync(request.Email);
            if (user == null)
                return Ok(new { success = false, message = "No account found with that email address." });

            var otp = new Random().Next(100000, 999999).ToString();
            _otpStore[request.Email.ToLower()] = (otp, DateTime.UtcNow.AddMinutes(10));

            string emailError = "";
            try
            {
                await _emailService.SendOtpEmailAsync(request.Email, otp);
            }
            catch (Exception ex)
            {
                emailError = ex.Message;
            }

            // Always return success=true so OTP flow continues
            // emailError will be shown in UI if email failed
            return Ok(new { success = true, emailError, otp = emailError != "" ? otp : "" });
        }

        [HttpPost("auth/verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var key = request.Email.ToLower();
            if (!_otpStore.TryGetValue(key, out var entry))
                return Ok(new { success = false, message = "OTP not found. Please request a new one." });

            if (DateTime.UtcNow > entry.Expiry)
            {
                _otpStore.Remove(key);
                return Ok(new { success = false, message = "OTP has expired. Please request a new one." });
            }

            if (entry.Otp != request.Otp)
                return Ok(new { success = false, message = "Invalid OTP. Please try again." });

            return Ok(new { success = true });
        }

        [HttpPost("auth/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var key = request.Email.ToLower();
            if (!_otpStore.TryGetValue(key, out var entry))
                return Ok(new { success = false, message = "Session expired. Please restart the process." });

            if (DateTime.UtcNow > entry.Expiry || entry.Otp != request.Otp)
                return Ok(new { success = false, message = "Invalid or expired OTP." });

            await _service.UpdatePasswordAsync(request.Email, request.NewPassword);
            _otpStore.Remove(key);
            return Ok(new { success = true });
        }
    }

    public class LoginRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class RegisterRequest { public string Name { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class EnrollRequest { public int CourseId { get; set; } }
    public class QuizScoreRequest { public int Score { get; set; } public int Total { get; set; } public int CourseId { get; set; } }
    public class ForgotPasswordRequest { public string Email { get; set; } = string.Empty; }
    public class VerifyOtpRequest { public string Email { get; set; } = string.Empty; public string Otp { get; set; } = string.Empty; }
    public class ResetPasswordRequest { public string Email { get; set; } = string.Empty; public string Otp { get; set; } = string.Empty; public string NewPassword { get; set; } = string.Empty; }
}