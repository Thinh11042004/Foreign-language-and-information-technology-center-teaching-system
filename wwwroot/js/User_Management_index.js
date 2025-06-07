// Sample user data
const users = [
    {
        id: 1,
        name: "Nguyễn Văn An",
        email: "admin@educenter.com",
        role: "admin",
        status: "active",
        joinDate: "15/01/2024",
        lastLogin: "Hôm nay, 14:30",
        avatar: "A"
    },
    {
        id: 2,
        name: "Trần Thị Bình",
        email: "binh.tran@educenter.com",
        role: "teacher",
        status: "active",
        joinDate: "20/02/2024",
        lastLogin: "Hôm qua, 16:45",
        avatar: "B"
    },
    {
        id: 3,
        name: "Lê Quang Cường",
        email: "cuong.le@student.com",
        role: "student",
        status: "active",
        joinDate: "03/03/2024",
        lastLogin: "2 ngày trước",
        avatar: "C"
    }
    // Add more users as needed
];

function renderUsers(userList = users) {
    const tbody = document.getElementById('userTableBody');

    if (userList.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="empty-state">
                    <i class="fas fa-users"></i>
                    <h3>Không tìm thấy người dùng nào</h3>
                    <p>Thử thay đổi bộ lọc hoặc tìm kiếm khác</p>
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = userList.map(user => `
        <tr>
            <td><input type="checkbox" value="${user.id}"></td>
            <td>
                <div class="user-info">
                    <div class="user-avatar">${user.avatar}</div>
                    <div class="user-details">
                        <h4>${user.name}</h4>
                        <p>${user.email}</p>
                    </div>
                </div>
            </td>
            <td><span class="role-badge role-${user.role}">${getRoleLabel(user.role)}</span></td>
            <td><span class="status-badge status-${user.status}">${getStatusLabel(user.status)}</span></td>
            <td>${user.joinDate}</td>
            <td>${user.lastLogin}</td>
            <td style="text-align: center;">
                <div class="action-buttons">
                    <button class="btn-action btn-view" title="Xem chi tiết"><i class="fas fa-eye"></i></button>
                    <button class="btn-action btn-edit" title="Chỉnh sửa"><i class="fas fa-edit"></i></button>
                    <button class="btn-action btn-delete" title="Xóa" onclick="deleteUser(${user.id})"><i class="fas fa-trash"></i></button>
                </div>
            </td>
        </tr>
    `).join('');
}

function getRoleLabel(role) {
    const labels = {
        'admin': 'Admin',
        'teacher': 'Giáo viên',
        'student': 'Học sinh'
    };
    return labels[role] || role;
}

function getStatusLabel(status) {
    const labels = {
        'active': 'Hoạt động',
        'inactive': 'Tạm khóa'
    };
    return labels[status] || status;
}

function filterUsers() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    const roleFilter = document.getElementById('roleFilter').value;
    const statusFilter = document.getElementById('statusFilter').value;

    const filtered = users.filter(user => {
        const matchesSearch = user.name.toLowerCase().includes(searchTerm) ||
            user.email.toLowerCase().includes(searchTerm) ||
            user.id.toString().includes(searchTerm);
        const matchesRole = !roleFilter || user.role === roleFilter;
        const matchesStatus = !statusFilter || user.status === statusFilter;

        return matchesSearch && matchesRole && matchesStatus;
    });

    renderUsers(filtered);
}

function toggleSelectAll() {
    const selectAll = document.getElementById('selectAll');
    const checkboxes = document.querySelectorAll('tbody input[type="checkbox"]');

    checkboxes.forEach(checkbox => {
        checkbox.checked = selectAll.checked;
    });
}

function deleteUser(userId) {
    if (confirm('Bạn có chắc chắn muốn xóa người dùng này không?')) {
        const index = users.findIndex(user => user.id === userId);
        if (index > -1) {
            users.splice(index, 1);
            renderUsers();
        }
    }
}

function openAddUserModal() {
    alert('Mở modal thêm người dùng mới');
}

function changePage(direction) {
    // Pagination logic here
    console.log('Change page:', direction);
}

document.getElementById('searchInput').addEventListener('input', filterUsers);
document.getElementById('roleFilter').addEventListener('change', filterUsers);
document.getElementById('statusFilter').addEventListener('change', filterUsers);

renderUsers();
