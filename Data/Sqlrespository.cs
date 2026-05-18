using System.Data;
using Npgsql;
using Dapper;
using APLH.Models;

namespace APLH.Data
{
    public class SqlRepository
    {
        private readonly string _connectionString;

        public SqlRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        // ── User operations ─────────────────────────────────────────────────────

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM users WHERE email = @Email", new { Email = email });
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM users WHERE id = @Id", new { Id = id });
        }

        public async Task<User> CreateUserAsync(User user)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO users (name, email, password, role, joined)
                        VALUES (@Name, @Email, @Password, @Role, @Joined)
                        RETURNING id;";

            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                user.Name,
                user.Email,
                user.Password,
                user.Role,
                Joined = DateTime.UtcNow.AddHours(8)
            });

            user.Id = id;
            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<User>("SELECT * FROM users ORDER BY id");
        }

        public async Task DeleteUserAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync("DELETE FROM users WHERE id = @Id", new { Id = id });
        }

        // ── Course operations ────────────────────────────────────────────────────

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<Course>("SELECT * FROM courses ORDER BY id");
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Course>(
                "SELECT * FROM courses WHERE id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(string category)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<Course>(
                "SELECT * FROM courses WHERE category = @Category", new { Category = category });
        }

        public async Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT * FROM courses
                        WHERE title LIKE @Search OR description LIKE @Search";
            var searchPattern = $"%{searchTerm}%";
            return await connection.QueryAsync<Course>(sql, new { Search = searchPattern });
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO courses (title, description, category, level, price, duration, emoji)
                        VALUES (@Title, @Description, @Category, @Level, @Price, @Duration, @Emoji)
                        RETURNING id;";

            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                course.Title,
                course.Description,
                course.Category,
                course.Level,
                course.Price,
                course.Duration,
                course.Emoji
            });

            course.Id = id;
            return course;
        }

        public async Task UpdateCourseAsync(Course course)
        {
            using var connection = CreateConnection();
            var sql = @"UPDATE courses SET
                        title       = @Title,
                        description = @Description,
                        category    = @Category,
                        level       = @Level,
                        price       = @Price,
                        duration    = @Duration,
                        emoji       = @Emoji
                        WHERE id = @Id";

            await connection.ExecuteAsync(sql, course);
        }

        public async Task DeleteCourseAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync("DELETE FROM courses WHERE id = @Id", new { Id = id });
        }

        public async Task IncrementCourseEnrollmentAsync(int courseId)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE courses SET enrolled = enrolled + 1 WHERE id = @Id",
                new { Id = courseId });
        }

        // ── Enrollment operations ────────────────────────────────────────────────

        public async Task<bool> IsUserEnrolledAsync(int userId, int courseId)
        {
            using var connection = CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM enrollments WHERE user_id = @UserId AND course_id = @CourseId",
                new { UserId = userId, CourseId = courseId });
            return count > 0;
        }

        public async Task EnrollUserAsync(int userId, int courseId)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO enrollments (user_id, course_id, enrolled_date, progress)
                        VALUES (@UserId, @CourseId, @EnrolledDate, 0)";

            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledDate = DateTime.UtcNow.AddHours(8)
            });

            await IncrementCourseEnrollmentAsync(courseId);
        }

        public async Task<IEnumerable<Course>> GetUserEnrolledCoursesAsync(int userId)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT c.* FROM courses c
                        INNER JOIN enrollments e ON c.id = e.course_id
                        WHERE e.user_id = @UserId
                        ORDER BY e.enrolled_date DESC";

            return await connection.QueryAsync<Course>(sql, new { UserId = userId });
        }

        // ── Quiz operations ──────────────────────────────────────────────────────

        public async Task<IEnumerable<QuizQuestion>> GetAllQuizQuestionsAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<QuizQuestion>("SELECT * FROM quiz_questions ORDER BY id");
        }

        public async Task<IEnumerable<QuizQuestion>> GetQuizQuestionsByCourseAsync(int courseId)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<QuizQuestion>(
                "SELECT * FROM quiz_questions WHERE course_id = @CourseId ORDER BY id",
                new { CourseId = courseId });
        }

        public async Task<QuizQuestion?> GetQuizQuestionByIdAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<QuizQuestion>(
                "SELECT * FROM quiz_questions WHERE id = @Id", new { Id = id });
        }

        public async Task<QuizQuestion> CreateQuizQuestionAsync(QuizQuestion question)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO quiz_questions (question, option_a, option_b, option_c, option_d, correct_answer, course_id)
            VALUES (@Question, @OptionA, @OptionB, @OptionC, @OptionD, @CorrectAnswer, @CourseId)
            RETURNING id;";

            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                question.Question,
                OptionA = question.OptionA,
                OptionB = question.OptionB,
                OptionC = question.OptionC,
                OptionD = question.OptionD,
                question.CorrectAnswer,
                question.CourseId
            });

            question.Id = id;
            return question;
        }

        public async Task DeleteQuizQuestionAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync("DELETE FROM quiz_questions WHERE id = @Id", new { Id = id });
        }

        // ── Quiz scores ──────────────────────────────────────────────────────────

        public async Task SaveQuizScoreAsync(QuizScore score)
        {
            using var connection = CreateConnection();
            var percentage = (decimal)score.Score / score.TotalQuestions * 100;
            var sql = @"INSERT INTO quiz_scores (user_id, score, total_questions, percentage, quiz_date)
                        VALUES (@UserId, @Score, @TotalQuestions, @Percentage, @QuizDate)";

            await connection.ExecuteAsync(sql, new
            {
                score.UserId,
                score.Score,
                score.TotalQuestions,
                Percentage = percentage,
                QuizDate = DateTime.UtcNow.AddHours(8)
            });
        }

        public async Task<IEnumerable<QuizScore>> GetUserQuizScoresAsync(int userId)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<QuizScore>(
                "SELECT * FROM quiz_scores WHERE user_id = @UserId ORDER BY quiz_date DESC",
                new { UserId = userId });
        }

        //Email
        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection = CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM users WHERE email = @Email", new { Email = email });
            return count > 0;
        }

        //Activity Log
        public async Task CreateActivityLogAsync(int userId, string activity)
        {
            using var connection = CreateConnection();

            var sql = @"INSERT INTO activity_logs (user_id, activity, created_at)
                        VALUES (@UserId, @Activity, NOW())";

            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                Activity = activity
            });
        }

        public async Task<IEnumerable<ActivityLog>> GetAllActivityLogsAsync()
        {
            using var connection = CreateConnection();

            return await connection.QueryAsync<ActivityLog>(
                @"SELECT 
                    id AS Id,
                    user_id AS UserId,
                    activity AS Activity,
                    created_at AS CreatedAt
                FROM activity_logs
                ORDER BY created_at DESC"
            );
        }

        //Course Materials
        public async Task CreateCourseMaterialAsync(CourseMaterial material)
        {
            using var connection = CreateConnection();

            var sql = @"
                INSERT INTO course_materials
                (course_id, title, content)
                VALUES
                (@CourseId, @Title, @Content)";

            await connection.ExecuteAsync(sql, new
            {
                material.CourseId,
                material.Title,
                material.Content
            });
        }

        public async Task<IEnumerable<CourseMaterial>> GetCourseMaterialsAsync(int courseId)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    id AS Id,
                    course_id AS CourseId,
                    title AS Title,
                    content AS Content,
                    created_at AS CreatedAt
                FROM course_materials
                WHERE course_id = @CourseId
                ORDER BY created_at ASC";

            return await connection.QueryAsync<CourseMaterial>(sql, new
            {
                CourseId = courseId
            });
        }
    }
}