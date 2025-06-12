class StudentDashboard {
    constructor() {
        this.socket = null;
        this.notificationCount = 0;
        this.upcomingAssignments = [];
        this.init();
    }

    init() {
        this.initializeSignalR();
        this.loadDashboardData();
        this.setupEventListeners();
        this.initializeWidgets();
        this.startPeriodicUpdates();
    }

    async initializeSignalR() {
        this.socket = new signalR.HubConnectionBuilder()
            .withUrl("/studentHub")
            .withAutomaticReconnect()
            .build();

        try {
            await this.socket.start();
            console.log("Student SignalR connected");

            // Listen for assignment notifications
            this.socket.on("AssignmentDue", (assignment) => {
                this.handleAssignmentDueNotification(assignment);
            });

            // Listen for grade updates
            this.socket.on("GradeUpdated", (grade) => {
                this.handleGradeUpdate(grade);
            });

            // Listen for class announcements
            this.socket.on("ClassAnnouncement", (announcement) => {
                this.handleClassAnnouncement(announcement);
            });

        } catch (err) {
            console.error("SignalR connection failed:", err);
            setTimeout(() => this.initializeSignalR(), 5000);
        }
    }

    async loadDashboardData() {
        try {
            const [overview, assignments, grades, schedule] = await Promise.all([
                fetch('/api/Student/Overview').then(r => r.json()),
                fetch('/api/Student/UpcomingAssignments').then(r => r.json()),
                fetch('/api/Student/RecentGrades').then(r => r.json()),
                fetch('/api/Student/TodaySchedule').then(r => r.json())
            ]);

            this.updateOverviewCards(overview);
            this.updateAssignmentsList(assignments);
            this.updateGradesList(grades);
            this.updateTodaySchedule(schedule);
            this.updateProgressCharts(overview.progress);

        } catch (error) {
            console.error("Error loading dashboard data:", error);
            this.showErrorMessage("Failed to load dashboard data");
        }
    }

    updateOverviewCards(overview) {
        // Update enrollment info
        $('#total-courses').text(overview.totalEnrollments);
        $('#active-assignments').text(overview.pendingAssignments);
        $('#completed-assignments').text(overview.completedAssignments);
        $('#overall-gpa').text(overview.overallGPA.toFixed(2));

        // Update attendance rate with animation
        const attendanceRate = overview.attendanceRate;
        $('#attendance-rate').text(`${attendanceRate}%`);
        $('.attendance-progress').css('width', `${attendanceRate}%`);

        // Update attendance color based on rate
        const progressBar = $('.attendance-progress');
        progressBar.removeClass('bg-success bg-warning bg-danger');
        if (attendanceRate >= 85) progressBar.addClass('bg-success');
        else if (attendanceRate >= 70) progressBar.addClass('bg-warning');
        else progressBar.addClass('bg-danger');
    }

    updateAssignmentsList(assignments) {
        const container = $('#upcoming-assignments');
        container.empty();

        if (assignments.length === 0) {
            container.append('<div class="alert alert-info">No upcoming assignments</div>');
            return;
        }

        assignments.forEach(assignment => {
            const dueDate = new Date(assignment.dueDate);
            const isOverdue = dueDate < new Date();
            const daysDiff = Math.ceil((dueDate - new Date()) / (1000 * 60 * 60 * 24));

            let urgencyClass = 'border-success';
            let urgencyIcon = 'fa-clock text-success';
            let urgencyText = `${daysDiff} days left`;

            if (isOverdue) {
                urgencyClass = 'border-danger';
                urgencyIcon = 'fa-exclamation-triangle text-danger';
                urgencyText = 'Overdue';
            } else if (daysDiff <= 1) {
                urgencyClass = 'border-danger';
                urgencyIcon = 'fa-fire text-danger';
                urgencyText = 'Due today';
            } else if (daysDiff <= 3) {
                urgencyClass = 'border-warning';
                urgencyIcon = 'fa-clock text-warning';
                urgencyText = `${daysDiff} days left`;
            }

            const assignmentHtml = `
                <div class="card mb-3 ${urgencyClass} assignment-card" data-assignment-id="${assignment.id}">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start">
                            <div class="flex-grow-1">
                                <h6 class="card-title mb-1">${assignment.title}</h6>
                                <p class="card-text text-muted mb-2">${assignment.courseName}</p>
                                <small class="text-muted">${assignment.description}</small>
                            </div>
                            <div class="text-end">
                                <div class="badge bg-light text-dark mb-2">
                                    <i class="fas ${urgencyIcon}"></i> ${urgencyText}
                                </div>
                                <br>
                                <button class="btn btn-primary btn-sm" onclick="openAssignment(${assignment.id})">
                                    <i class="fas fa-pencil-alt"></i> Work
                                </button>
                            </div>
                        </div>
                        <div class="progress mt-2" style="height: 5px;">
                            <div class="progress-bar bg-primary" style="width: ${assignment.progress || 0}%"></div>
                        </div>
                    </div>
                </div>
            `;

            container.append(assignmentHtml);
        });
    }

    updateGradesList(grades) {
        const container = $('#recent-grades');
        container.empty();

        if (grades.length === 0) {
            container.append('<div class="alert alert-info">No grades available</div>');
            return;
        }

        grades.forEach(grade => {
            const percentage = (grade.score / grade.maxScore * 100).toFixed(1);
            let gradeClass = 'text-success';
            let gradeIcon = 'fa-check-circle';

            if (percentage < 60) {
                gradeClass = 'text-danger';
                gradeIcon = 'fa-times-circle';
            } else if (percentage < 80) {
                gradeClass = 'text-warning';
                gradeIcon = 'fa-exclamation-circle';
            }

            const gradeHtml = `
                <div class="d-flex justify-content-between align-items-center py-2 border-bottom">
                    <div>
                        <h6 class="mb-1">${grade.assignmentTitle}</h6>
                        <small class="text-muted">${grade.courseName}</small>
                    </div>
                    <div class="text-end">
                        <span class="badge bg-light text-dark">
                            ${grade.score}/${grade.maxScore}
                        </span>
                        <div class="${gradeClass}">
                            <i class="fas ${gradeIcon}"></i> ${percentage}%
                        </div>
                    </div>
                </div>
            `;

            container.append(gradeHtml);
        });
    }

    updateTodaySchedule(schedule) {
        const container = $('#today-schedule');
        container.empty();

        if (schedule.length === 0) {
            container.append('<div class="alert alert-info">No classes today</div>');
            return;
        }

        schedule.forEach(classItem => {
            const startTime = new Date(classItem.startTime);
            const now = new Date();
            const isActive = startTime <= now && now <= new Date(classItem.endTime);
            const isPast = new Date(classItem.endTime) < now;

            let statusClass = 'bg-light';
            let statusText = 'Upcoming';

            if (isActive) {
                statusClass = 'bg-success';
                statusText = 'In Progress';
            } else if (isPast) {
                statusClass = 'bg-secondary';
                statusText = 'Completed';
            }

            const scheduleHtml = `
                <div class="card mb-2 ${isActive ? 'border-success' : ''}">
                    <div class="card-body py-3">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="mb-1">${classItem.courseName}</h6>
                                <small class="text-muted">
                                    <i class="fas fa-clock"></i> ${startTime.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} - 
                                    ${new Date(classItem.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                </small>
                                <br>
                                <small class="text-muted">
                                    <i class="fas fa-map-marker-alt"></i> ${classItem.roomName}
                                </small>
                            </div>
                            <div class="text-end">
                                <span class="badge ${statusClass} text-white">${statusText}</span>
                                ${isActive ? '<br><button class="btn btn-success btn-sm mt-1" onclick="joinVirtualClass(' + classItem.id + ')"><i class="fas fa-video"></i> Join</button>' : ''}
                            </div>
                        </div>
                    </div>
                </div>
            `;

            container.append(scheduleHtml);
        });
    }

    updateProgressCharts(progressData) {
        // Overall Progress Doughnut Chart
        const progressCtx = document.getElementById('progressChart');
        if (progressCtx) {
            new Chart(progressCtx, {
                type: 'doughnut',
                data: {
                    labels: ['Completed', 'In Progress', 'Not Started'],
                    datasets: [{
                        data: [
                            progressData.completed,
                            progressData.inProgress,
                            progressData.notStarted
                        ],
                        backgroundColor: ['#28a745', '#ffc107', '#dc3545'],
                        borderWidth: 0
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

        // Skills Progress Radar Chart
        const skillsCtx = document.getElementById('skillsChart');
        if (skillsCtx && progressData.skills) {
            new Chart(skillsCtx, {
                type: 'radar',
                data: {
                    labels: progressData.skills.map(s => s.name),
                    datasets: [{
                        label: 'Current Level',
                        data: progressData.skills.map(s => s.current),
                        backgroundColor: 'rgba(54, 162, 235, 0.2)',
                        borderColor: 'rgb(54, 162, 235)',
                        pointBackgroundColor: 'rgb(54, 162, 235)'
                    }, {
                        label: 'Target Level',
                        data: progressData.skills.map(s => s.target),
                        backgroundColor: 'rgba(255, 99, 132, 0.2)',
                        borderColor: 'rgb(255, 99, 132)',
                        pointBackgroundColor: 'rgb(255, 99, 132)'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        r: {
                            beginAtZero: true,
                            max: 10
                        }
                    }
                }
            });
        }
    }

    setupEventListeners() {
        // Quick assignment submission
        $(document).on('click', '.quick-submit-btn', (e) => {
            const assignmentId = $(e.target).data('assignment-id');
            this.openQuickSubmission(assignmentId);
        });

        // Notification mark as read
        $(document).on('click', '.notification-item', (e) => {
            const notificationId = $(e.target).closest('.notification-item').data('id');
            this.markNotificationAsRead(notificationId);
        });

        // Course progress tracking
        $(document).on('click', '.progress-detail-btn', (e) => {
            const courseId = $(e.target).data('course-id');
            this.showProgressDetail(courseId);
        });

        // Calendar event handlers
        $('#calendar').on('dateClick', (info) => {
            this.showDayDetail(info.date);
        });
    }

    initializeWidgets() {
        // Initialize calendar
        this.initializeCalendar();

        // Initialize assignment tracker
        this.initializeAssignmentTracker();

        // Initialize study timer
        this.initializeStudyTimer();
    }

    initializeCalendar() {
        const calendarEl = document.getElementById('student-calendar');
        if (!calendarEl) return;

        const calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: 'dayGridMonth',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay'
            },
            events: '/api/Student/CalendarEvents',
            eventClick: (info) => {