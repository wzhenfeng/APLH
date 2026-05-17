-- ============================================================
-- SQL Server version of webapplication_db
-- Run this in SQL Server Management Studio (SSMS) or Azure Data Studio
-- ============================================================

-- Create database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'webapplication_db')
    CREATE DATABASE webapplication_db;
GO

USE webapplication_db;
GO

-- Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='users' AND xtype='U')
CREATE TABLE users (
    id          INT PRIMARY KEY IDENTITY(1,1),
    name        VARCHAR(100) NOT NULL,
    email       VARCHAR(100) NOT NULL UNIQUE,
    password    VARCHAR(255) NOT NULL,
    role        VARCHAR(20)  NOT NULL DEFAULT 'member'
                    CONSTRAINT chk_users_role CHECK (role IN ('admin', 'member')),
    joined      DATE         NOT NULL,
    created_at  DATETIME     DEFAULT GETDATE()
);
GO

-- Courses table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='courses' AND xtype='U')
CREATE TABLE courses (
    id          INT PRIMARY KEY IDENTITY(1,1),
    title       VARCHAR(200)    NOT NULL,
    description TEXT            NOT NULL,
    category    VARCHAR(20)     NOT NULL
                    CONSTRAINT chk_courses_category CHECK (category IN ('Technology', 'Design', 'Business', 'Science')),
    level       VARCHAR(20)     NOT NULL
                    CONSTRAINT chk_courses_level CHECK (level IN ('Beginner', 'Intermediate', 'Advanced')),
    price       DECIMAL(10,2)   DEFAULT 0,
    duration    INT             NOT NULL,
    emoji       VARCHAR(10),
    enrolled    INT             DEFAULT 0,
    rating      DECIMAL(3,2)    DEFAULT 4.5,
    created_at  DATETIME        DEFAULT GETDATE()
);
GO

-- Enrollments table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='enrollments' AND xtype='U')
CREATE TABLE enrollments (
    id            INT PRIMARY KEY IDENTITY(1,1),
    user_id       INT  NOT NULL,
    course_id     INT  NOT NULL,
    enrolled_date DATE NOT NULL,
    progress      INT  DEFAULT 0,
    completed     BIT  DEFAULT 0,
    CONSTRAINT fk_enrollments_user   FOREIGN KEY (user_id)   REFERENCES users(id)   ON DELETE CASCADE,
    CONSTRAINT fk_enrollments_course FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE,
    CONSTRAINT uq_enrollment UNIQUE (user_id, course_id)
);
GO

-- Quiz questions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='quiz_questions' AND xtype='U')
CREATE TABLE quiz_questions (
    id             INT PRIMARY KEY IDENTITY(1,1),
    question       TEXT         NOT NULL,
    option_a       VARCHAR(500) NOT NULL,
    option_b       VARCHAR(500) NOT NULL,
    option_c       VARCHAR(500) NOT NULL,
    option_d       VARCHAR(500) NOT NULL,
    correct_answer INT          NOT NULL,  -- 0=A, 1=B, 2=C, 3=D
    course_id      INT          NULL,      -- NULL = general quiz, otherwise specific to course
    created_at     DATETIME     DEFAULT GETDATE(),
    CONSTRAINT fk_quiz_course FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE
);
GO

-- Quiz scores table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='quiz_scores' AND xtype='U')
CREATE TABLE quiz_scores (
    id              INT PRIMARY KEY IDENTITY(1,1),
    user_id         INT          NOT NULL,
    score           INT          NOT NULL,
    total_questions INT          NOT NULL,
    percentage      DECIMAL(5,2),
    quiz_date       DATE         NOT NULL,
    CONSTRAINT fk_quiz_scores_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

--Activity Log table
CREATE TABLE activity_logs (
    id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    activity NVARCHAR(255) NOT NULL,
    created_at DATETIME DEFAULT GETDATE()
);

-- ============================================================
-- Sample data
-- ============================================================

-- Note: Passwords are BCrypt hashed. Plain text equivalents:
-- admin@webapplication.com: admin123
-- alice@example.com: pass123  
-- bob@example.com: pass123
INSERT INTO users (name, email, password, role, joined) VALUES
('Admin User',    'admin@webapplication.com', '$2a$11$nxBbdeNcdmSrRctsLYBHOemCET45uPwi/TOKgPqTN5p0dfZNQzCWi', 'admin',  CAST(GETDATE() AS DATE)),
('Alice Johnson', 'alice@example.com',        '$2a$11$KBaIMqNWiRedbaWmym5y4OFRJhEjGYDGIpjxFvCWjx3k4s48KWINK',  'member', CAST(GETDATE() AS DATE)),
('Bob Smith',     'bob@example.com',          '$2a$11$KBaIMqNWiRedbaWmym5y4OFRJhEjGYDGIpjxFvCWjx3k4s48KWINK',  'member', CAST(GETDATE() AS DATE));
GO

INSERT INTO courses (title, description, category, level, price, duration, emoji, enrolled, rating) VALUES
('Python for Beginners',      'Start your coding journey with Python. Learn variables, loops, functions, and build real projects from day one.',             'Technology', 'Beginner',     0,  12, N'🐍', 892,  4.9),
('UI/UX Design Fundamentals', 'Master the principles of beautiful, user-centered design. From wireframes to high-fidelity prototypes.',                     'Design',     'Beginner',    49,  10, N'🎨', 634,  4.8),
('Machine Learning A-Z',      'A complete hands-on guide to machine learning algorithms, data preprocessing, and model deployment.',                        'Technology', 'Advanced',    79,  28, N'🤖', 1243, 4.9),
('Business Strategy 101',     'Learn how successful companies think and execute. Frameworks from Harvard Business School applied to real cases.',            'Business',   'Intermediate',59,   8, N'📊', 445,  4.7),
('Data Science with R',       'Explore data analysis, visualization, and statistical modeling using R programming language.',                               'Science',    'Intermediate', 0,  16, N'📈', 567,  4.6),
('Web Development Bootcamp',  'Build full-stack web apps with HTML, CSS, JavaScript, React, and Node.js. 60+ projects included.',                          'Technology', 'Beginner',    89,  40, N'💻', 2100, 4.9);
GO

-- Assign questions to specific courses (course_id)
-- 1=Python, 2=UI/UX, 3=ML, 4=Business, 5=Data Science, 6=Web Dev
INSERT INTO quiz_questions (question, option_a, option_b, option_c, option_d, correct_answer, course_id) VALUES
('Which data structure uses LIFO order?',                    'Queue',                   'Stack',                    'Array',                   'Linked List', 1, 1),
('What does CSS stand for?',                                 'Computer Style Sheet',    'Cascading Style Sheet',    'Creative Style System',   'Colorful Style Sheet', 1, 6),
('Which language is primarily used for Machine Learning?',   'Java',                    'C++',                      'Python',                  'PHP', 2, 1),
('What is the result of 2 ** 10 in Python?',                 '20',                      '100',                      '512',                     '1024', 3, 1),
('Which HTTP method is used to UPDATE a resource?',          'GET',                     'POST',                     'PUT',                     'DELETE', 2, 6),
('What does SQL stand for?',                                 'Structured Query Language','Simple Query Logic',       'Sequential Query Language','Standard Query List', 0, 5);
GO

INSERT INTO enrollments (user_id, course_id, enrolled_date, progress) VALUES
(2, 1, CAST(GETDATE() AS DATE), 45),
(2, 2, CAST(GETDATE() AS DATE), 30),
(3, 1, CAST(GETDATE() AS DATE), 60),
(3, 3, CAST(GETDATE() AS DATE), 15);
GO