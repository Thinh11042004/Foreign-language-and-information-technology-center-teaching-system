// ===== Modal Functions =====
function openCreateModal() {
    const modal = document.getElementById('createModal');
    modal.classList.add('animate__animated', 'animate__zoomIn');
    modal.style.display = 'block';
}

function openCategoryModal() {
    const modal = document.getElementById('categoryModal');
    modal.classList.add('animate__animated', 'animate__zoomIn');
    modal.style.display = 'block';
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

// ===== Close modal when clicking outside =====
window.onclick = function (event) {
    const modals = document.querySelectorAll('.modal');
    modals.forEach(modal => {
        if (event.target === modal) {
            modal.style.display = 'none';
        }
    });
}

// ===== Search and Filter =====
function initializeSearchAndFilters() {
    const searchInput = document.querySelector('.search-input');
    const filterSelects = document.querySelectorAll('.filter-select');

    function applyFilters() {
        const searchTerm = searchInput.value.toLowerCase();
        const statusFilter = document.querySelector('.filter-select.status').value;
        const categoryFilter = document.querySelector('.filter-select.category').value;

        const courseCards = document.querySelectorAll('.course-card');

        courseCards.forEach(card => {
            const title = card.querySelector('h3').textContent.toLowerCase();
            const description = card.querySelector('.course-description').textContent.toLowerCase();
            const meta = card.querySelector('.course-meta').textContent.toLowerCase();
            const statusBadge = card.querySelector('.status-badge')?.textContent.trim();
            const category = card.getAttribute('data-category');

            const matchesSearch = title.includes(searchTerm) || description.includes(searchTerm) || meta.includes(searchTerm);
            const matchesStatus = (statusFilter === 'Tất cả trạng thái' || statusBadge === statusFilter);
            const matchesCategory = (categoryFilter === 'Tất cả danh mục' || category === categoryFilter);

            card.style.display = (matchesSearch && matchesStatus && matchesCategory) ? 'block' : 'none';
        });
    }

    if (searchInput) {
        searchInput.addEventListener('input', applyFilters);
    }

    filterSelects.forEach(select => {
        select.addEventListener('change', applyFilters);
    });
}

// ===== Ripple Effect for Buttons =====
document.addEventListener('DOMContentLoaded', function () {
    initializeSearchAndFilters();

    document.querySelectorAll('button, .btn-action').forEach(button => {
        button.addEventListener('click', function (e) {
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;

            ripple.style.cssText = `
                position: absolute;
                border-radius: 50%;
                background: rgba(255,255,255,0.5);
                transform: scale(0);
                animation: ripple 0.6s linear;
                left: ${x}px;
                top: ${y}px;
                width: ${size}px;
                height: ${size}px;
                z-index: 99;
            `;

            this.style.position = 'relative';
            this.style.overflow = 'hidden';
            this.appendChild(ripple);

            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });

    // Inject ripple animation CSS
    const style = document.createElement('style');
    style.textContent = `
        @keyframes ripple {
            to {
                transform: scale(4);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(style);
});