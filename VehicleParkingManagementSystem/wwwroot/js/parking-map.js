function setupLiveParkingMap() {
    const toggle = document.getElementById('liveUpdateToggle');
    const grid = document.getElementById('parkingMapGrid');
    if (!toggle || !grid) return;

    let timer = null;

    const maxCarIcons = 6;

    function carSvg(statusClass) {
        return `<svg class="car-icon ${statusClass}" viewBox="0 0 24 42" width="20" height="35" xmlns="http://www.w3.org/2000/svg">
            <rect x="1" y="9" width="4" height="8" rx="1.5" fill="#475569" />
            <rect x="19" y="9" width="4" height="8" rx="1.5" fill="#475569" />
            <rect x="1" y="25" width="4" height="8" rx="1.5" fill="#475569" />
            <rect x="19" y="25" width="4" height="8" rx="1.5" fill="#475569" />
            <rect x="3" y="1" width="18" height="40" rx="7" fill="currentColor" />
            <rect x="6" y="5" width="12" height="9" rx="2" fill="#0f172a" opacity="0.45" />
            <rect x="6" y="26" width="12" height="8" rx="2" fill="#0f172a" opacity="0.3" />
        </svg>`;
    }

    function carsMarkup(activeCount) {
        if (activeCount === 0) {
            return carSvg('available');
        }
        const shown = Math.min(activeCount, maxCarIcons);
        let html = carSvg('occupied').repeat(shown);
        if (activeCount > maxCarIcons) {
            html += `<span class="car-overflow">+${activeCount - maxCarIcons}</span>`;
        }
        return html;
    }

    async function refresh() {
        const res = await fetch('/Dashboard/ParkingMap');
        const areas = await res.json();
        grid.innerHTML = areas.map(a => {
            const tag = a.areaId != null ? 'a' : 'div';
            const href = a.areaId != null ? `href="/ParkingAreas/Details/${a.areaId}"` : '';
            return `<${tag} ${href} class="map-area-tile ${a.activeCount > 0 ? 'occupied' : ''} ${a.isVip ? 'vip' : ''}" data-area-id="${a.areaId ?? ''}">
                <div class="area-name">${a.areaName} ${a.isVip ? '<span class="badge bg-warning text-dark">VIP</span>' : ''}</div>
                <div class="area-status">${a.activeCount > 0 ? 'Occupied' : 'Available'}</div>
                <div class="area-cars">${carsMarkup(a.activeCount)}</div>
                <div class="area-count">${a.activeCount} <small>parked</small></div>
            </${tag}>`;
        }).join('');
    }

    function start() {
        timer = setInterval(refresh, 15000);
    }

    function stop() {
        if (timer) clearInterval(timer);
        timer = null;
    }

    toggle.addEventListener('change', function () {
        if (this.checked) start(); else stop();
    });

    if (toggle.checked) start();
}
