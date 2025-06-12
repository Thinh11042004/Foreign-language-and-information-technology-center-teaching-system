const connection = new signalR.HubConnectionBuilder().withUrl("/notificationHub").build();

connection.start()
    .then(() => {
        console.log("SignalR Connected.");
        loadNotifications();
    })
    .catch(err => console.error(err.toString()));

connection.on("ReceiveNotification", function (notification) {
    addNotificationToUI(notification);
    updateNotificationCount();
    showToast(notification.title, notification.message);
});

function loadNotifications() {
    $.get('/api/NotificationApi/GetNotifications')
        .done(updateNotificationUI)
        .fail(() => console.error('Failed to load notifications'));
}

function updateNotificationUI(notifications) {
    const notificationList = $('#notification-list');
    notificationList.empty();

    if (notifications.length === 0) {
        notificationList.append('<div class="dropdown-item text-center text-muted">No notifications</div>');
    } else {
        notifications.forEach(addNotificationToUI);
    }
    updateNotificationCount();
}

function addNotificationToUI(notification) {
    const item = `
        <div class="dropdown-item notification-item ${!notification.isRead ? 'unread' : ''}" data-id="${notification.id}">
            <div class="d-flex">
                <div class="flex-grow-1">
                    <h6 class="dropdown-header">${notification.title}</h6>
                    <p class="mb-1">${notification.message}</p>
                    <small class="text-muted">${formatDate(notification.createdAt)}</small>
                </div>
                ${!notification.isRead ? '<div class="unread-indicator"></div>' : ''}
            </div>
        </div>`;
    $('#notification-list').prepend(item);
}

function updateNotificationCount() {
    const unreadCount = $('.notification-item.unread').length;
    $('#notification-count').text(unreadCount).toggle(unreadCount > 0);
}

$(document).on('click', '.notification-item.unread', function () {
    const id = $(this).data('id');
    const item = $(this);
    $.post(`/api/NotificationApi/${id}/mark-read`, () => {
        item.removeClass('unread');
        updateNotificationCount();
    });
});

function showToast(title, message, type = 'info') {
    const toast = `
        <div class="toast align-items-center text-bg-${type} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}</strong><br>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>`;
    $('#toast-container').append(toast);
    $('.toast').last().toast('show');
}

function formatDate(dateStr) {
    const date = new Date(dateStr);
    const now = new Date();
    const diff = Math.abs(now - date);
    const days = Math.ceil(diff / (1000 * 60 * 60 * 24));

    if (days === 1) return 'Yesterday';
    else if (days < 7) return `${days} days ago`;
    else return date.toLocaleDateString();
}

// ===== UI Enhancements =====
$(document).ready(function () {
    $('.data-table').DataTable({
        responsive: true,
        pageLength: 25,
        order: [[0, 'desc']],
        language: {
            search: "Search:",
            lengthMenu: "Show _MENU_ entries",
            info: "Showing _START_ to _END_ of _TOTAL_ entries",
            paginate: {
                first: "First",
                last: "Last",
                next: "Next",
                previous: "Previous"
            }
        }
    });

    $('[data-bs-toggle="tooltip"]').tooltip();
    $('[data-bs-toggle="popover"]').popover();

    window.addEventListener('load', function () {
        const forms = document.getElementsByClassName('needs-validation');
        Array.prototype.forEach.call(forms, function (form) {
            form.addEventListener('submit', function (event) {
                if (!form.checkValidity()) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                form.classList.add('was-validated');
            }, false);
        });
    });

    setTimeout(() => $('.alert:not(.alert-permanent)').fadeOut('slow'), 5000);

    $('.btn-delete').on('click', function (e) {
        if (!confirm('Are you sure you want to delete this item?')) {
            e.preventDefault();
        }
    });

    $('.btn-loading').on('click', function () {
        const btn = $(this);
        const original = btn.html();
        btn.html('<i class="fas fa-spinner fa-spin"></i> Loading...').prop('disabled', true);
        setTimeout(() => btn.html(original).prop('disabled', false), 3000);
    });
});

// ===== Progress Bars & Counters =====
function animateProgressBars() {
    $('.progress-bar').each(function () {
        const percent = $(this).data('percentage');
        $(this).css('width', '0%').animate({ width: percent + '%' }, 1000);
    });
}

function animateCounters() {
    $('.counter').each(function () {
        const counter = $(this);
        const target = parseInt(counter.text());
        let current = 0;
        const increment = target / 100;
        const timer = setInterval(() => {
            current += increment;
            counter.text(Math.ceil(current));
            if (current >= target) {
                counter.text(target);
                clearInterval(timer);
            }
        }, 20);
    });
}

// ===== Utility: Realtime Search =====
function setupRealtimeSearch(searchInput, targetTable) {
    $(searchInput).on('keyup', function () {
        const value = $(this).val().toLowerCase();
        $(targetTable + " tbody tr").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });
}

// ===== File Upload Preview =====
function handleFileUpload(input, preview) {
    input.addEventListener('change', function (e) {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                preview.src = e.target.result;
                preview.style.display = 'block';
            };
            reader.readAsDataURL(file);
        }
    });
}

// ===== Cookie and Session Managers =====
const CookieManager = {
    set: function (name, value, days) {
        const d = new Date();
        d.setTime(d.getTime() + (days * 24 * 60 * 60 * 1000));
        const expires = "; expires=" + d.toUTCString();
        document.cookie = name + "=" + (value || "") + expires + "; path=/";
    },
    get: function (name) {
        const nameEQ = name + "=";
        const ca = document.cookie.split(';');
        for (let c of ca) {
            while (c.charAt(0) === ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    },
    erase: function (name) {
        document.cookie = name + "=; Max-Age=-99999999;";
    }
};

const SessionManager = {
    trackPageVisit: function (page) {
        $.post('/api/Analytics/TrackPageVisit', { page });
    },
    updateLastActivity: function () {
        sessionStorage.setItem('lastActivity', new Date().getTime());
    },
    checkSessionTimeout: function () {
        const lastActivity = sessionStorage.getItem('lastActivity');
        const now = new Date().getTime();
        const timeout = 30 * 60 * 1000;
        if (lastActivity && (now - lastActivity) > timeout) {
            alert('Your session has expired. Please login again.');
            window.location.href = '/Account/Login';
        }
    }
};

setInterval(() => {
    SessionManager.updateLastActivity();
    SessionManager.checkSessionTimeout();
}, 60000);

$(document).ready(function () {
    SessionManager.trackPageVisit(window.location.pathname);
    SessionManager.updateLastActivity();
});