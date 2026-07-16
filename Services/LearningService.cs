using APLH.Data;
using APLH.Models;

namespace APLH.Services
{
    public class LearningService
    {
        private readonly SqlRepository _repository;

        public LearningService(SqlRepository repository)
        {
            _repository = repository;
        }

        // User operations
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _repository.GetUserByEmailAsync(email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                return user;
            return null;
        }

        public async Task<User> RegisterUserAsync(string name, string email, string password)
{
    // Check for duplicate email (like signup.php does)
    if (await _repository.EmailExistsAsync(email))
        throw new Exception("An account with this email already exists.");

    if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
        throw new Exception("Name must be at least 2 characters.");

    if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        throw new Exception("Password must be at least 6 characters.");

    var user = new User
    {
        Name = name,
        Email = email,
        Password = BCrypt.Net.BCrypt.HashPassword(password), //change the password to hash for security
        Role = "member",
        Joined = DateTime.Now
    };
    return await _repository.CreateUserAsync(user);
}

        public async Task<User?> GetUserByIdAsync(int id) => await _repository.GetUserByIdAsync(id);
        public async Task<IEnumerable<User>> GetAllUsersAsync() => await _repository.GetAllUsersAsync();
        public async Task DeleteUserAsync(int id) => await _repository.DeleteUserAsync(id);

        // Course operations
        public async Task<IEnumerable<Course>> GetAllCoursesAsync() => await _repository.GetAllCoursesAsync();
        public async Task<Course?> GetCourseByIdAsync(int id) => await _repository.GetCourseByIdAsync(id);
        public async Task<Course> CreateCourseAsync(Course course) => await _repository.CreateCourseAsync(course);
        public async Task UpdateCourseAsync(Course course) => await _repository.UpdateCourseAsync(course);
        public async Task DeleteCourseAsync(int id) => await _repository.DeleteCourseAsync(id);
        public async Task<IEnumerable<Course>> SearchCoursesAsync(string search) => await _repository.SearchCoursesAsync(search);

        // Enrollment operations
        public async Task<bool> IsUserEnrolledAsync(int userId, int courseId)
            => await _repository.IsUserEnrolledAsync(userId, courseId);

        public async Task EnrollUserAsync(int userId, int courseId)
            => await _repository.EnrollUserAsync(userId, courseId);

        public async Task<IEnumerable<Course>> GetUserEnrolledCoursesAsync(int userId)
            => await _repository.GetUserEnrolledCoursesAsync(userId);

        // Quiz operations
        public async Task<IEnumerable<QuizQuestion>> GetAllQuizQuestionsAsync()
            => await _repository.GetAllQuizQuestionsAsync();

        public async Task<IEnumerable<QuizQuestion>> GetQuizQuestionsByCourseAsync(int courseId)
            => await _repository.GetQuizQuestionsByCourseAsync(courseId);

        public async Task<QuizQuestion> CreateQuizQuestionAsync(QuizQuestion question)
            => await _repository.CreateQuizQuestionAsync(question);

        public async Task DeleteQuizQuestionAsync(int id)
            => await _repository.DeleteQuizQuestionAsync(id);

        public async Task SaveQuizScoreAsync(int userId, int courseId, int score, int total)
        {
            var quizScore = new QuizScore
            {
                UserId = userId,
                CourseId = courseId,
                Score = score,
                TotalQuestions = total
            };
            await _repository.SaveQuizScoreAsync(quizScore);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _repository.GetUserByEmailAsync(email);
        }

        public async Task<User> RegisterGoogleUserAsync(string name, string email)
        {
            var user = new User
            {
                Name = name,
                Email = email,
                Password = "",
                Role = "member",
                Joined = DateTime.Now
            };

            return await _repository.CreateUserAsync(user);
        }

        public async Task CreateActivityLogAsync(int userId, string activity)
        {
            await _repository.CreateActivityLogAsync(userId, activity);
        }

        public async Task<IEnumerable<ActivityLog>> GetAllActivityLogsAsync()
        {
            return await _repository.GetAllActivityLogsAsync();
        }


        public async Task CreateCourseMaterialAsync(CourseMaterial material)
        {
            await _repository.CreateCourseMaterialAsync(material);
        }

        public async Task UpdateCourseMaterialAsync(CourseMaterial material)
        {
            await _repository.UpdateCourseMaterialAsync(material);
        }

        public async Task<IEnumerable<CourseMaterial>> GetCourseMaterialsAsync(int courseId)
        {
            return await _repository.GetCourseMaterialsAsync(courseId);
        }

        // Chapter operations
        public async Task<CourseChapter> CreateCourseChapterAsync(CourseChapter chapter)
            => await _repository.CreateCourseChapterAsync(chapter);

        public async Task<IEnumerable<CourseChapter>> GetCourseChaptersAsync(int courseId)
            => await _repository.GetCourseChaptersAsync(courseId);

        public async Task DeleteCourseChapterAsync(int id)
            => await _repository.DeleteCourseChapterAsync(id);

        public async Task CreateChatMessageAsync(ChatMessage chatMessage)
        {
            await _repository.CreateChatMessageAsync(chatMessage);
        }

        public async Task<List<ChatMessage>> GetChatMessagesByUserIdAsync(int userId)
        {
            return await _repository.GetChatMessagesByUserIdAsync(userId);
        }

        public async Task<List<ChatMessage>> GetAllChatMessagesAsync()
        {
            return await _repository.GetAllChatMessagesAsync();
        }
        
        public async Task<IEnumerable<QuizScore>> GetUserQuizScoresAsync(int userId)
            => await _repository.GetUserQuizScoresAsync(userId);

        public async Task UpdatePasswordAsync(string email, string newPassword)
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _repository.UpdatePasswordAsync(email, hashed);
        }
    }
}