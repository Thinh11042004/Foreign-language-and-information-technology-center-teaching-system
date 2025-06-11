// Additional JavaScript - dashboard.js

// Dashboard-specific JavaScript functionality
class DashboardManager {
    constructor() {
        this.charts = {};
        this.refreshInterval = 300000; // 5 minutes
        this.init();
    }

    init() {
        this.initCharts();
        this.setupEventListeners();
        this.startAutoRefresh();
        this.loadRealtimeData();
    }

    initCharts() {
        this.initRevenueChart();
        this.initEnrollmentChart();
        this.initCourseDistributionChart();
    }

    initRevenueChart() {
        const ctx = document.getElementById('revenueChart');
        if (!ctx) return;

        this.charts.revenue = new Chart(ctx, {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Revenue',
                    data: [],
                    borderColor: '#4e73df',
                    backgroundColor: 'rgba(78, 115, 223, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                }
            }
        });
    }

    initEnrollmentChart() {
        const ctx = document.getElementById('enrollmentChart');
        if (!ctx) return;

        this.charts.enrollment = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: [],
                datasets: [{
                    label: 'Enrollments',
                    data: [],
                    backgroundColor: '#1cc88a'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }

    initCourseDistributionChart() {
        const ctx = document.getElementById('courseChart');
        if (!ctx) return;

        this.charts.course = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Language Courses', 'IT Courses'],
                datasets: [{
                    data: [0, 0],
                    backgroundColor: ['#4e73df', '#1cc88a'],
                    hoverBackgroundColor: ['#2e59d9', '#17a673'],
                    hoverBorderColor: "rgba(234, 236, 244, 1)",
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }

    setupEventListeners() {
        // e.g., refresh button
        const refreshBtn = document.getElementById('btn-refresh-dashboard');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => this.loadRealtimeData());
        }
    }

    startAutoRefresh() {
        setInterval(() => {
            this.loadRealtimeData();
        }, this.refreshInterval);
    }

    loadRealtimeData() {
        fetch('/api/Dashboard/GetRevenueChart')
            .then(res => res.json())
            .then(data => this.updateRevenueChart(data));

        fetch('/api/Dashboard/GetEnrollmentChart')
            .then(res => res.json())
            .then(data => this.updateEnrollmentChart(data));

        fetch('/api/Dashboard/GetCourseDistribution')
            .then(res => res.json())
            .then(data => this.updateCourseChart(data));
    }

    updateRevenueChart(data) {
        const chart = this.charts.revenue;
        if (!chart) return;

        chart.data.labels = data.labels;
        chart.data.datasets[0].data = data.values;
        chart.update();
    }

    updateEnrollmentChart(data) {
        const chart = this.charts.enrollment;
        if (!chart) return;

        chart.data.labels = data.labels;
        chart.data.datasets[0].data = data.values;
        chart.update();
    }

    updateCourseChart(data) {
        const chart = this.charts.course;
        if (!chart) return;

        chart.data.datasets[0].data = [data.language, data.it];
        chart.update();
    }
}

// Initialize dashboard on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    new DashboardManager();
});
