function toggleUserStatus(userId, isActive) {
    const action = isActive ? 'kích hoạt' : 'vô hiệu hóa';

    if (confirm(`Bạn có chắc chắn muốn ${action} người dùng này?`)) {
        $.ajax({
            url: '@Url.Action("ToggleStatus", "User")',
            type: 'POST',
            data: {
                id: userId,
                isActive: isActive,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    showNotification(`${action.charAt(0).toUpperCase() + action.slice(1)} người dùng thành công!`, 'success');
                    location.reload();
                } else {
                    showNotification(result.message || 'Có lỗi xảy ra!', 'error');
                }
            },
            error: function () {
                showNotification('Có lỗi xảy ra khi xử lý yêu cầu!', 'error');
            }
        });
    }
}

// Delete user
function deleteUser(userId, userName) {
    if (confirm(`Bạn có chắc chắn muốn xóa người dùng "${userName}"?\n\nLưu ý: Hành động này không thể hoàn tác!`)) {
        $.ajax({
            url: '@Url.Action("Delete", "User")',
            type: 'POST',
            data: {
                id: userId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    showNotification('Xóa người dùng thành công!', 'success');
                    location.reload();
                } else {
                    showNotification(result.message || 'Có lỗi xảy ra!', 'error');
                }
            },
            error: function () {
                showNotification('Có lỗi xảy ra khi xử lý yêu cầu!', 'error');
            }
        });
    }
}

// Export functions
function exportToExcel() {
    window.location.href = '@Url.Action("ExportExcel", "User")' + window.location.search;
}

function exportToPDF() {
    window.location.href = '@Url.Action("ExportPDF", "User")' + window.location.search;
}

// Show import modal
function showImportModal() {
    // Implementation for import modal
    alert('Chức năng nhập dữ liệu đang được phát triển!');
}

// Notification function
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'success' ? 'success' : type === 'error' ? 'danger' : 'info'} alert-dismissible fade show position-fixed`;
    notification.style.top = '20px';
    notification.style.right = '20px';
    notification.style.zIndex = '9999';
    notification.style.minWidth = '300px';

    notification.innerHTML = `
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;

    document.body.appendChild(notification);

    // Auto remove after 3 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 3000);
}

// Auto-submit form on filter change
document.getElementById('roleFilter').addEventListener('change', function () {
    this.form.submit();
});

document.getElementById('statusFilter').addEventListener('change', function () {
    this.form.submit();
});

// Initialize tooltips
document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[title]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

function applyStatusStyle(select, status) {
    select.className = 'form-select form-select-sm status-select'; // Reset class
    switch (status) {
        case 'Active':
            select.classList.add('bg-success', 'text-white');
            break;
        case 'Inactive':
            select.classList.add('bg-secondary', 'text-white');
            break;
        case 'Graduated':
            select.classList.add('bg-primary', 'text-white');
            break;
        case 'Suspended':
            select.classList.add('bg-danger', 'text-white');
            break;
        case 'Transferred':
            select.classList.add('bg-warning', 'text-dark');
            break;
    }
}

function updateUserStatus(select) {
    const userId = select.dataset.userId;
    const newStatus = select.value;

    // Apply màu cho select
    applyStatusStyle(select, newStatus);

    fetch('/UserManagement/UpdateStatus', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ userId, newStatus })
    })
        .then(response => {
            if (!response.ok) {
                alert("Cập nhật trạng thái thất bại.");
            }
        });
}

// Gọi khi trang load để gán màu
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".status-select").forEach(select => {
        applyStatusStyle(select, select.value);
    });
});
