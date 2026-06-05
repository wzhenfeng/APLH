// Authentication check
function checkAuth() {
    $.ajax({
        url: '/api/api/auth/currentuser',
        type: 'GET',
        success: function(user) {
            if (user && user.id) {
                updateUIForLoggedInUser(user);
            } else {
                updateUIForLoggedOutUser();
            }
        }
    });
}

function updateUIForLoggedInUser(user) {
    $('.nav-right').html(`
        <span class="user-name">${user.name || user.Name || 'User'}</span>
        <a class="nav-link" href="/Profile">My Profile</a>
        <button class="btn btn-ghost btn-sm" onclick="logout()">Log Out</button>
    `);

    $('#nav-quiz').show();
    $('#nav-profile').show();

    if (user.role === 'admin' || user.Role === 'admin') {
        $('#nav-admin').show();
        $('#addCourseBtn').show();
    }
}

function updateUIForLoggedOutUser() {
    $('.nav-right').html(`
        <button class="btn btn-ghost btn-sm" onclick="openLoginModal()">Log In</button>
        <button class="btn btn-primary btn-sm" onclick="openRegisterModal()">Get Started</button>
    `);
    $('#nav-quiz').hide();
    $('#nav-profile').hide();
    $('#nav-admin').hide();
    $('#addCourseBtn').hide();
}

// Authentication functions
function handleLogin() {
    const email = $('#loginEmail').val().trim();
    const password = $('#loginPassword').val();
    
    $.ajax({
        url: '/api/api/auth/login',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ email: email, password: password }),
        success: function(response) {
            if (response.success) {
                closeModal('loginModal');
                showToast(`Welcome back, ${response.user.name}!`, 'success');
                if (response.user.role === 'admin' || response.user.Role === 'admin') {
                    location.href = '/Admin';
                } else {
                    location.reload();
                }
            } else {
                showAlert('loginAlert', 'error', response.message || 'Invalid email or password');
            }
        }
    });
}

function handleRegister() {
    const name = $('#regName').val().trim();
    const email = $('#regEmail').val().trim();
    const password = $('#regPassword').val();
    const confirm = $('#regConfirm').val();
    
    if (password !== confirm) {
        showAlert('regAlert', 'error', 'Passwords do not match');
        return;
    }
    
   $.ajax({
    url: '/api/api/auth/register',
    type: 'POST',
    contentType: 'application/json',
    data: JSON.stringify({
        name: name,
        email: email,
        password: password
    }),

    success: function(response) {
        if (response.success) {
            closeModal('registerModal');
            showToast(
                'You have successfully registered your account',
                'success'
            );
            setTimeout(() => {
                openLoginModal();
            }, 1500);
        } else {
            showAlert(
                'regAlert',
                'error',
                response.message || 'Registration failed'
            );
        }
    }
})
};


function logout() {
    $.ajax({
        url: '/api/api/auth/logout',
        type: 'POST',
        success: function() {
            location.reload();
        }
    });
}

// Course functions
let activeCategory = 'all';

function loadCourses() {
    const search = $('#courseSearch').val() || '';
    const category = activeCategory;
    
    $.ajax({
        url: '/api/api/courses',
        type: 'GET',
        data: { category: category, search: search },
        success: function(courses) {
            renderCourses(courses);
        }
    });
}

function renderCourses(courses) {
    const grid = $('#coursesGrid');
    if (!courses || courses.length === 0) {
        grid.html('<div class="empty-state"><div class="icon">🔍</div><p>No courses found.</p></div>');
        return;
    }
    
    grid.html(courses.map(c => `
        <div class="course-card" onclick="viewCourseDetail(${c.id})">
            <div class="course-thumb" style="background:${getCategoryBg(c.category)}">${c.emoji}</div>
            <div class="course-body">
                <div class="course-cat" style="color:${getCategoryColor(c.category)}">${c.category}</div>
                <div class="course-title">${c.title}</div>
                <div class="course-desc">${c.description.substring(0, 90)}...</div>
                <div class="course-meta">
                    <span>⏱ ${c.duration} minutes</span>
                    <span>📶 ${c.level}</span>
                    <span>👤 ${(c.enrolled || 0).toLocaleString()} enrolled</span>
                </div>
                <div class="course-footer">
                    <div class="course-price ${c.price === 0 ? 'free' : ''}">${c.price === 0 ? 'FREE' : 'RM ' + c.price}</div>
                </div>
            </div>
        </div>
    `).join(''));
}

function viewCourseDetail(courseId) {
    window.location.href = `/CoursesDetails?id=${courseId}`;
}

function filterCourses(category, btn) {
    activeCategory = category;
    $('.filter-btn').removeClass('active');
    $(btn).addClass('active');
    loadCourses();
}

function enrollCourse(courseId) {
    $.ajax({
        url: '/api/api/courses/enroll',
        type: 'POST',
        data: JSON.stringify({ courseId: courseId }),
        contentType: 'application/json',
        success: function(response) {
            if (response.success) {
                showToast('Successfully enrolled! 🎉', 'success');
                setTimeout(() => location.reload(), 1500);
            } else if (response.message === 'Already enrolled') {
                showToast('You are already enrolled in this course!', 'info');
            }
        },
        error: function() {
            openLoginModal();
        }
    });
}

// Helper functions
function getCategoryColor(cat) {
    const colors = { Technology: '#6bcbff', Design: '#a78bfa', Business: '#ffd93d', Science: '#4ade80' };
    return colors[cat] || '#6bcbff';
}

function getCategoryBg(cat) {
    const bg = { 
        Technology: 'rgba(107,203,255,0.12)', 
        Design: 'rgba(167,139,250,0.12)', 
        Business: 'rgba(255,217,61,0.12)', 
        Science: 'rgba(74,222,128,0.12)' 
    };
    return bg[cat] || 'rgba(107,203,255,0.12)';
}

function openModal(id) {
    $(`#${id}`).addClass('open');
}

function closeModal(id) {
    $(`#${id}`).removeClass('open');
}

function showToast(msg, type = 'success') {
    const toast = $('#toast');
    toast.text(msg);
    toast.removeClass('success error info').addClass(type);
    toast.addClass('show');
    setTimeout(() => toast.removeClass('show'), 3000);
}

function showAlert(id, type, msg) {
    const alert = $(`#${id}`);
    alert.removeClass('alert-success alert-error').addClass(`alert-${type}`);
    alert.text(msg);
    alert.addClass('show');
    setTimeout(() => alert.removeClass('show'), 5000);
}



function openLoginModal() {
    openModal('loginModal');
}

function openRegisterModal() {
    openModal('registerModal');
}

function showCoursesLoginMessage() {
    showToast(
        'Please login or register to view courses 🔒',
        'error'
    );

    setTimeout(() => {
        openLoginModal();
    }, 1000);
}

//Save course button(Admin)
function saveCourse() {

        const course = {
        id: $('#courseEditId').val(),
        title: $('#courseTitle').val(),
        description: $('#courseDesc').val(),
        category: $('#courseCat').val(),
        level: $('#courseLevel').val(),
        price: parseFloat($('#coursePrice').val()),
        duration: parseInt($('#courseDuration').val()),
        emoji: $('#courseEmoji').val()
    };

    $.ajax({
        url: '/api/api/courses/save',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(course),

        success: function(response) {

            if (response.success) {

                closeModal('courseModal');

                showToast(
                    'Course saved successfully 🎉',
                    'success'
                );

                setTimeout(() => {
                    location.reload();
                }, 1000);

            } else {

                showToast(
                    response.message || 'Save failed',
                    'error'
                );

            }
        },

        error: function() {
            showToast('Server error', 'error');
        }
    });
}

function saveQuizQuestion() {

    const question = {

        question: $('#quizQuestion').val(),

        optionA: $('#optionA').val(),

        optionB: $('#optionB').val(),

        optionC: $('#optionC').val(),

        optionD: $('#optionD').val(),

        correctAnswer: parseInt(
            $('#correctAnswer').val()
        )
    };

    $.ajax({

        url: '/api/api/quiz/questions/save',

        type: 'POST',

        contentType: 'application/json',

        data: JSON.stringify(question),

        success: function(response) {

            if (response.success) {

                showToast(
                    'Quiz question saved successfully 🎉',
                    'success'
                );

                setTimeout(() => {
                    location.reload();
                }, 1000);
            }
        },

        error: function() {

            showToast(
                'Failed to save question',
                'error'
            );

        }
    });
}

//Click on Blank area to close modals
$(document).ready(function () {

    $('.modal-overlay').on('click', function (e) {

        // Only close when clicking the overlay itself
        if ($(e.target).hasClass('modal-overlay')) {
            $(this).removeClass('open');
        }

    });

});

// Initialize
$(document).ready(function() {
    checkAuth();
    if ($('#coursesGrid').length) loadCourses();
});

//Funtional for Keyboard Enter key
$(document).on('keypress', function(e) {

    if (e.key === 'Enter') {
        if (
            $('#loginModal').hasClass('open') ||
            $('#loginModal').hasClass('active')
        ) {
            handleLogin();
            return;
        }
        if (
            $('#registerModal').hasClass('open') ||
            $('#registerModal').hasClass('active')
        ) {
            handleRegister();
            return;
        }
        if (
            $('#courseModal').hasClass('open') ||
            $('#courseModal').hasClass('active')
        ) {
            saveCourse();
            return;
        }
    }
});