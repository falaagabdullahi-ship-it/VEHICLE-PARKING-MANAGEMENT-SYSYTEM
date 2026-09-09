const dashboardPalette = ['#3b82f6', '#22c55e', '#f59e0b', '#a78bfa', '#ef4444', '#2dd4bf', '#fb923c'];

async function fetchJson(url) {
    const res = await fetch(url);
    return res.json();
}

async function renderOperationalDashboardCharts() {
    const occupancyTrend = await fetchJson('/Dashboard/DailyVisitorsChart');
    new Chart(document.getElementById('occupancyTrendChart'), {
        type: 'line',
        data: {
            labels: occupancyTrend.labels,
            datasets: [{ label: 'Vehicles', data: occupancyTrend.data, borderColor: dashboardPalette[0], backgroundColor: 'rgba(59,130,246,0.12)', tension: 0.35, fill: true }]
        },
        options: { plugins: { legend: { display: false } } }
    });

    const vehicleTypes = await fetchJson('/Dashboard/VehicleTypeChart');
    const vtTotal = vehicleTypes.data.reduce((a, b) => a + b, 0) || 1;
    new Chart(document.getElementById('vehicleTypeChart'), {
        type: 'doughnut',
        data: { labels: vehicleTypes.labels, datasets: [{ data: vehicleTypes.data, backgroundColor: dashboardPalette }] },
        options: { plugins: { legend: { display: false } }, cutout: '65%' }
    });
    const legendEl = document.getElementById('vehicleTypeLegend');
    if (legendEl) {
        legendEl.innerHTML = vehicleTypes.labels.map((label, i) => {
            const pct = Math.round((vehicleTypes.data[i] / vtTotal) * 100);
            const color = dashboardPalette[i % dashboardPalette.length];
            return `<div class="d-flex align-items-center gap-2 mb-1"><span class="legend-dot" style="background:${color};"></span><span>${label} <strong>${pct}%</strong></span></div>`;
        }).join('');
    }

    const weeklyRevenue = await fetchJson('/Dashboard/WeeklyRevenueChart');
    new Chart(document.getElementById('weeklyRevenueChart'), {
        type: 'bar',
        data: { labels: weeklyRevenue.labels, datasets: [{ label: 'Revenue', data: weeklyRevenue.data, backgroundColor: dashboardPalette[1], borderRadius: 6 }] },
        options: { plugins: { legend: { display: false } } }
    });

    const monthlyRevenue = await fetchJson('/Dashboard/MonthlyRevenueChart');
    new Chart(document.getElementById('monthlyRevenueChart'), {
        type: 'bar',
        data: { labels: monthlyRevenue.labels, datasets: [{ label: 'Revenue', data: monthlyRevenue.data, backgroundColor: dashboardPalette[5], borderRadius: 6 }] },
        options: { plugins: { legend: { display: false } } }
    });

    const peakHours = await fetchJson('/Dashboard/PeakHoursChart');
    new Chart(document.getElementById('peakHoursChart'), {
        type: 'bar',
        data: { labels: peakHours.labels, datasets: [{ label: 'Check-ins', data: peakHours.data, backgroundColor: dashboardPalette[4], borderRadius: 6 }] },
        options: { plugins: { legend: { display: false } } }
    });

    setupLiveParkingMap();
}
