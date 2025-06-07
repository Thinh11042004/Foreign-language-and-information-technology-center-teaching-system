// Lấy biến CSS
function getCssVar(name, fallback = '') {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim() || fallback;
}

// Biến màu dùng chung
const colors = {
    primary: getCssVar('--color-3', '#A6B1E1'),
    secondary: getCssVar('--color-2', '#DCD6F7'),
    tertiary: getCssVar('--color-4', '#424874'),
    light: getCssVar('--color-1', '#F4EEFF'),
    success: '#10b981',
    warning: '#f59e0b',
    danger: '#ef4444',
    purple: '#8b5cf6'
};

// Cấu hình mặc định Chart.js
Chart.defaults.font.family = "'Inter', 'Segoe UI', sans-serif";
Chart.defaults.color = colors.tertiary;

// Biến toàn cục chart
let revenueChart, registrationChart, courseDistributionChart, revenueByCourseChart, performanceRadarChart;

// Dữ liệu mẫu
const chartData = {
    revenue: {
        daily: {
            labels: ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'],
            data: [120000000, 150000000, 180000000, 210000000, 190000000, 250000000, 280000000]
        },
        weekly: {
            labels: ['Tuần 1', 'Tuần 2', 'Tuần 3', 'Tuần 4'],
            data: [850000000, 920000000, 1100000000, 1300000000]
        },
        monthly: {
            labels: ['T1', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'T8', 'T9', 'T10', 'T11', 'T12'],
            data: [2100000000, 2400000000, 2800000000, 2650000000, 3100000000, 2900000000, 3200000000, 3500000000, 3800000000, 3600000000, 4100000000, 4500000000]
        }
    },
    registration: {
        labels: ['T1', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'T8', 'T9', 'T10', 'T11', 'T12'],
        data: [45, 52, 61, 58, 72, 68, 85, 92, 98, 105, 112, 125]
    },
    courseDistribution: {
        labels: ['Lập trình', 'Toán học', 'Tiếng Anh', 'Khoa học', 'Kỹ thuật', 'Khác'],
        data: [35, 25, 15, 12, 8, 5]
    },
    revenueByCourse: {
        labels: ['Python', 'Tiếng Anh', 'Toán học', 'Hóa học', 'Sinh học'],
        data: [472.5, 200.4, 290.0, 176.4, 167.2]
    },
    performance: {
        current: [85, 92, 78, 88, 76, 82],
        target: [90, 95, 85, 90, 85, 88]
    }
};

// Khởi tạo Dashboard
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(animateCounters, 300);
    setTimeout(initializeCharts, 600);
    setupEventListeners();
    setTimeout(animateProgressBars, 1000);
    setTimeout(() => showToast('Dashboard đã sẵn sàng!'), 2000);
});

// Counter
function animateCounters() {
    document.querySelectorAll('.stat-number').forEach(counter => {
        const target = parseFloat(counter.dataset.target);
        const isMoney = counter.textContent.includes('₫');
        const isPercent = counter.textContent.includes('%');
        let count = 0;
        const increment = target / 80;

        const timer = setInterval(() => {
            count += increment;
            if (count >= target) {
                clearInterval(timer);
                counter.textContent = isMoney ? '₫' + formatNumber(target)
                    : isPercent ? target + '%' : formatNumber(target);
            } else {
                counter.textContent = isMoney ? '₫' + formatNumber(Math.floor(count))
                    : isPercent ? (Math.floor(count * 10) / 10) + '%' : formatNumber(Math.floor(count));
            }
        }, 25);
    });
}

// Format số đẹp
function formatNumber(num) {
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
    return num.toLocaleString();
}

// Tạo biểu đồ
function initializeCharts() {
    try {
        createRevenueChart();
        createRegistrationChart();
        createCourseDistributionChart();
        createRevenueByCourseChart();
        createPerformanceRadarChart();
    } catch (e) {
        showToast('Lỗi tải biểu đồ', 'error');
        setTimeout(initializeCharts, 2000);
    }
}

// Biểu đồ revenue (line)
function createRevenueChart() {
    const ctx = document.getElementById('revenueChart');
    revenueChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: chartData.revenue.daily.labels,
            datasets: [{
                label: 'Doanh thu',
                data: chartData.revenue.daily.data,
                borderColor: colors.primary,
                backgroundColor: colors.primary + '20',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: colors.primary
            }]
        },
        options: commonLineOptions()
    });
}

// Biểu đồ đăng ký (bar)
function createRegistrationChart() {
    const ctx = document.getElementById('registrationChart');
    registrationChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: chartData.registration.labels,
            datasets: [{
                label: 'Học sinh',
                data: chartData.registration.data,
                backgroundColor: chartData.registration.data.map((_, i) =>
                    [colors.primary, colors.secondary, colors.success, colors.warning, colors.danger, colors.purple][i % 6] + '80'
                ),
                borderRadius: 10,
                borderWidth: 1
            }]
        },
        options: commonBarOptions()
    });
}

// Biểu đồ phân bố khóa học (doughnut)
function createCourseDistributionChart() {
    const ctx = document.getElementById('courseDistributionChart');
    courseDistributionChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: chartData.courseDistribution.labels,
            datasets: [{
                data: chartData.courseDistribution.data,
                backgroundColor: [colors.primary, colors.secondary, colors.tertiary, colors.success, colors.warning, colors.danger],
                cutout: '70%'
            }]
        },
        options: commonDoughnutOptions('%')
    });
}

// Biểu đồ doanh thu theo khóa học
function createRevenueByCourseChart() {
    const ctx = document.getElementById('revenueByCourseChart');
    revenueByCourseChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: chartData.revenueByCourse.labels,
            datasets: [{
                data: chartData.revenueByCourse.data,
                backgroundColor: [colors.primary, colors.secondary, colors.tertiary, colors.success, colors.purple],
                borderColor: '#fff',
                borderWidth: 2,
                cutout: '60%'
            }]
        },
        options: commonDoughnutOptions('M', '₫')
    });
}

// Biểu đồ radar
function createPerformanceRadarChart() {
    const ctx = document.getElementById('performanceRadarChart');
    performanceRadarChart = new Chart(ctx, {
        type: 'radar',
        data: {
            labels: ['Doanh thu', 'Học sinh', 'Khóa học', 'Đánh giá', 'Hoàn thành', 'Tương tác'],
            datasets: [
                {
                    label: 'Hiện tại',
                    data: chartData.performance.current,
                    backgroundColor: colors.primary + '30',
                    borderColor: colors.primary,
                    pointBackgroundColor: colors.primary
                },
                {
                    label: 'Mục tiêu',
                    data: chartData.performance.target,
                    backgroundColor: colors.tertiary + '20',
                    borderColor: colors.tertiary,
                    pointBackgroundColor: colors.tertiary
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                r: {
                    max: 100,
                    beginAtZero: true,
                    grid: { color: colors.light },
                    angleLines: { color: colors.light }
                }
            },
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { usePointStyle: true }
                }
            }
        }
    });
}

// Options chung cho bar chart
function commonBarOptions() {
    return {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: {
            x: { grid: { display: false }, border: { display: false } },
            y: {
                beginAtZero: true,
                grid: { color: colors.light },
                border: { display: false }
            }
        }
    };
}

// Options chung cho line chart
function commonLineOptions() {
    return {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: {
            x: { grid: { display: false }, border: { display: false } },
            y: {
                beginAtZero: true,
                grid: { color: colors.light },
                border: { display: false },
                ticks: {
                    callback: value => '₫' + formatNumber(value)
                }
            }
        },
        interaction: { intersect: false, mode: 'index' }
    };
}

// Options chung cho doughnut
function commonDoughnutOptions(suffix = '', prefix = '') {
    return {
        responsive: true,
        plugins: {
            legend: { position: 'bottom', labels: { usePointStyle: true } },
            tooltip: {
                callbacks: {
                    label: context => `${context.label}: ${prefix}${context.parsed}${suffix}`
                }
            }
        }
    };
}

// Xử lý nút
function setupEventListeners() {
    document.querySelectorAll('.date-btn').forEach(btn => {
        btn.onclick = () => {
            document.querySelectorAll('.date-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            updateChartsForDateRange(btn.dataset.range);
        };
    });

    document.querySelectorAll('.chart-btn[data-period]').forEach(btn => {
        btn.onclick = () => {
            btn.closest('.chart-card')
                .querySelectorAll('.chart-btn[data-period]')
                .forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            updateRevenueChart(btn.dataset.period);
        };
    });

    document.addEventListener('keydown', handleKeyboardShortcuts);
}

function updateRevenueChart(period) {
    if (!revenueChart) return;
    showChartLoading();
    setTimeout(() => {
        const data = chartData.revenue[period];
        revenueChart.data.labels = data.labels;
        revenueChart.data.datasets[0].data = data.data;
        revenueChart.update();
        hideChartLoading();
        showToast(`Cập nhật biểu đồ: ${period}`);
    }, 500);
}

function updateChartsForDateRange(range) {
    showChartLoading();
    setTimeout(() => {
        hideChartLoading();
        showToast(`Hiển thị dữ liệu ${range} ngày gần đây`);
        animateCounters();
    }, 1200);
}

// Toast thông báo
function showToast(msg, type = 'success') {
    const icon = {
        success: 'check-circle',
        error: 'exclamation-circle',
        warning: 'exclamation-triangle',
        info: 'info-circle'
    }[type];
    const div = document.createElement('div');
    div.className = `toast-notification toast-${type}`;
    div.innerHTML = `<div class="toast-content"><i class="fas fa-${icon}"></i> ${msg}</div>`;
    document.body.appendChild(div);
    setTimeout(() => div.style.transform = 'translateX(0)', 100);
    setTimeout(() => {
        div.style.transform = 'translateX(100%)';
        setTimeout(() => div.remove(), 300);
    }, 3000);
}

// Phím tắt
function handleKeyboardShortcuts(e) {
    if (e.ctrlKey && e.key === 'r') {
        e.preventDefault();
        refreshData();
    }
    if (e.ctrlKey && e.key === 'e') {
        e.preventDefault();
        showToast('Tính năng xuất sẽ có ở bản sau!', 'info');
    }
    if (e.ctrlKey && e.key === 'p') {
        e.preventDefault();
        window.print();
    }
}

// Làm mới dữ liệu
function refreshData() {
    showToast('Đang làm mới dữ liệu...', 'info');
    showChartLoading();
    setTimeout(() => {
        hideChartLoading();
        animateCounters();
        animateProgressBars();
        showToast('Dữ liệu đã được cập nhật');
    }, 1500);
}

// Progress animation
function animateProgressBars() {
    document.querySelectorAll('.progress-fill').forEach(bar => {
        const width = bar.style.width;
        bar.style.width = '0%';
        setTimeout(() => {
            bar.style.width = width;
        }, 100);
    });
}

// Hiệu ứng loading
function showChartLoading() {
    document.querySelectorAll('.chart-loading').forEach(e => e.style.display = 'flex');
    document.querySelectorAll('canvas').forEach(c => c.style.opacity = '0.3');
}
function hideChartLoading() {
    document.querySelectorAll('.chart-loading').forEach(e => e.style.display = 'none');
    document.querySelectorAll('canvas').forEach(c => c.style.opacity = '1');
}
