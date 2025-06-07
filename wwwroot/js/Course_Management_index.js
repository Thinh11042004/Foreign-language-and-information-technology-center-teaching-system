// Course Management System JavaScript

// Modal Functions
function openCreateModal() {
    document.getElementById('createModal').style.display = 'block';
}

function openCategoryModal() {
    document.getElementById('categoryModal').style.display = 'block';
}

function openImportModal() {
    alert('Tính năng import sẽ được phát triển trong phiên bản tiếp theo!');
}

function openSettingsModal() {
    alert('Tính năng cài đặt sẽ được phát triển trong phiên bản tiếp theo!');
}

function closeModal(modalId) {
    document.getElementById(modalId).style.display = 'none';
}

// Close modal when clicking outside
window.onclick = function (event) {
    const modals = document.querySelectorAll('.modal');
    modals.forEach(modal => {
        if (event.target === modal) {
            modal.style.display = 'none';
        }
    });
}

// Search functionality
function initializeSearch() {
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            const searchTerm = this.value.toLowerCase();
            const courseCards = document.querySelectorAll('.course-card');

            courseCards.forEach(card => {
                const title = card.querySelector('h3').textContent.toLowerCase();
                const description = card.querySelector('.course-description').textContent.toLowerCase();
                const meta = card.querySelector('.course-meta').textContent.toLowerCase();

                if (title.includes(searchTerm) || description.includes(searchTerm) || meta.includes(searchTerm)) {
                    card.style.display = 'block';
                } else {
                    card.style.display = 'none';
                }
            });
        });
    }
}

// Filter functionality
function initializeFilters() {
    const filterSelects = document.querySelectorAll('.filter-select');
    filterSelects.forEach(select => {
        select.addEventListener('change', function () {
            // Filter logic would be implemented here
            console.log('Filter changed:', this.value);
            filterCourses();
        });
    });
}

// Course filtering logic
function filterCourses() {
    const statusFilter = document.querySelector('.filter-select:nth-child(2)').value;
    const categoryFilter = document.querySelector('.filter-select:nth-child(3)').value;
    const courseCards = document.querySelectorAll('.course-card');

    courseCards.forEach(card => {
        let shouldShow = true;

        // Status filter
        if (statusFilter !== 'Tất cả trạng thái') {
            const statusBadge = card.querySelector('.status-badge');
            if (statusBadge && !statusBadge.textContent.includes(statusFilter)) {
                shouldShow = false;
            }
        }

        // Category filter (simplified - would need more sophisticated logic in real app)
        if (categoryFilter !== 'Tất cả danh mục') {
            const courseMeta = card.querySelector('.course-meta').textContent;
            if (!courseMeta.includes(categoryFilter)) {
                shouldShow = false;
            }
        }

        card.style.display = shouldShow ? 'block' : 'none';
    });
}

// Course card interactions
function initializeCourseCards() {
    document.querySelectorAll('.course-card').forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-4px)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });
}

// Button loading states
function initializeButtonStates() {
    document.querySelectorAll('.btn').forEach(btn => {
        btn.addEventListener('click', function () {
            if (this.type === 'submit') {
                const originalText = this.innerHTML;
                this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
                this.disabled = true;

                // Simulate processing
                setTimeout(() => {
                    this.innerHTML = originalText;
                    this.disabled = false;
                }, 2000);
            }
        });
    });
}

// Form validation
function validateCreateCourseForm() {
    const form = document.querySelector('#createModal form');
    if (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            const courseName = form.querySelector('input[placeholder="Nhập tên khóa học"]');
            const category = form.querySelector('select:nth-child(1)');
            const level = form.querySelector('select:nth-child(2)');

            // Basic validation
            if (!courseName.value.trim()) {
                alert('Vui lòng nhập tên khóa học!');
                courseName.focus();
                return false;
            }

            if (courseName.value.trim().length < 3) {
                alert('Tên khóa học phải có ít nhất 3 ký tự!');
                courseName.focus();
                return false;
            }

            // Show success message
            alert('Tạo khóa học thành công!');
            closeModal('createModal');

            // Reset form
            form.reset();

            // In a real app, you would send data to server here
            console.log('Course created:', {
                name: courseName.value,
                category: category.value,
                level: level.value
            });
        });
    }
}

// Pagination functionality
function initializePagination() {
    const paginationBtns = document.querySelectorAll('.pagination-btn');
    paginationBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            // Remove active class from all buttons
            paginationBtns.forEach(b => b.classList.remove('active'));

            // Add active class to clicked button (if it's a number)
            if (!isNaN(this.textContent)) {
                this.classList.add('active');

                // Simulate page loading
                const coursesGrid = document.querySelector('.courses-grid');
                coursesGrid.style.opacity = '0.5';

                setTimeout(() => {
                    coursesGrid.style.opacity = '1';
                }, 300);

                console.log('Loading page:', this.textContent);
            }
        });
    });
}

// Statistics animation
function animateStats() {
    const statNumbers = document.querySelectorAll('.stat-number');

    statNumbers.forEach(statNumber => {
        const finalValue = parseInt(statNumber.textContent.replace(/[^\d]/g, ''));
        let currentValue = 0;
        const increment = finalValue / 50; // Animation steps

        const timer = setInterval(() => {
            currentValue += increment;
            if (currentValue >= finalValue) {
                statNumber.textContent = statNumber.textContent; // Keep original format
                clearInterval(timer);
            } else {
                statNumber.textContent = Math.floor(currentValue);
            }
        }, 30);
    });
}

// Quick actions functionality
function initializeQuickActions() {
    const actionItems = document.querySelectorAll('.action-item');
    actionItems.forEach(item => {
        item.addEventListener('click', function () {
            // Add click animation
            this.style.transform = 'scale(0.98)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 150);
        });
    });
}

// Category management
function initializeCategoryManagement() {
    const categoryModal = document.getElementById('categoryModal');
    if (categoryModal) {
        const addCategoryBtn = categoryModal.querySelector('.btn-primary');
        const categoryNameInput = categoryModal.querySelector('input[placeholder="Nhập tên danh mục"]');

        if (addCategoryBtn) {
            addCategoryBtn.addEventListener('click', function (e) {
                e.preventDefault();

                if (!categoryNameInput.value.trim()) {
                    alert('Vui lòng nhập tên danh mục!');
                    return;
                }

                // Simulate adding category
                alert(`Đã thêm danh mục: ${categoryNameInput.value}`);
                categoryNameInput.value = '';

                console.log('Category added:', categoryNameInput.value);
            });
        }
    }
}

// Course action handlers
function initializeCourseActions() {
    // Edit course buttons
    document.querySelectorAll('.btn:contains("Chỉnh sửa")').forEach(btn => {
        btn.addEventListener('click', function () {
            const courseCard = this.closest('.course-card');
            const courseName = courseCard.querySelector('h3').textContent;
            console.log('Editing course:', courseName);
            alert(`Chỉnh sửa khóa học: ${courseName}`);
        });
    });

    // View details buttons
    document.querySelectorAll('.btn:contains("Xem chi tiết")').forEach(btn => {
        btn.addEventListener('click', function