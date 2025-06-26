import dayjs from 'dayjs';

/**
 * Hàm trợ giúp cục bộ để lấy tên từ ID.
 * @param {string} id - ID cần tìm.
 * @param {Array<object>} list - Mảng dimension (SERVICES, PRIORITIES, etc.).
 * @returns {string} - Tên tương ứng.
 */
function getName(id, list) {
    return list.find(item => item.id === id)?.name || 'Unknown';
}

/**
 * Biến đổi dữ liệu thô thành dữ liệu cho các thẻ KPI.
 * @param {Array<object>} data - Mảng incident đã được lọc.
 * @param {string} filterMode - 'month' hoặc 'year'.
 * @returns {object} - Đối tượng KPI.
 */
export function transformKpis(data, filterMode) {
    const total = data.length;
    const highPriority = data.filter(inc => ['p1', 'p2'].includes(inc.priorityId)).length;
    
    let latestTitle = '';
    let latestValue = 0;

    if (filterMode === 'month') {
        latestTitle = 'Incident Mới (Ngày gần nhất)';
        const latestDateStr = data.reduce((max, p) => p.date > max ? p.date : max, data[0].date);
        latestValue = data.filter(inc => inc.date === latestDateStr).length;
    } else {
        latestTitle = 'Incident Mới (Tháng gần nhất)';
        const latestMonth = data.reduce((max, p) => dayjs(p.date).month() > dayjs(max.date).month() ? p : max, data[0]);
        latestValue = data.filter(inc => dayjs(inc.date).isSame(dayjs(latestMonth.date), 'month')).length;
    }
    
    return { total, highPriority, latestTitle, latestValue };
}

/**
 * Biến đổi dữ liệu thô thành dữ liệu cho tất cả các biểu đồ.
 * @param {Array<object>} data - Mảng incident đã được lọc.
 * @param {object} dimensions - Đối tượng chứa các mảng dimension.
 * @returns {object} - Đối tượng chứa dữ liệu đã xử lý cho các biểu đồ.
 */
export function transformCharts(data, dimensions) {
    const { SERVICES, PRIORITIES, SHIFTS } = dimensions;

    // Dữ liệu cho Pie Chart
    const priorityCount = data.reduce((acc, inc) => { acc[inc.priorityId] = (acc[inc.priorityId] || 0) + 1; return acc; }, {});
    const priorityDistributionData = Object.entries(priorityCount).map(([id, value]) => ({ type: getName(id, PRIORITIES), value }));

    // Dữ liệu cho Treemap
    const treemapCount = data.reduce((acc, inc) => { const serviceName = getName(inc.serviceId, SERVICES); acc[serviceName] = (acc[serviceName] || 0) + 1; return acc; }, {});
    const treemapData = Object.entries(treemapCount).map(([name, value]) => ({ name, value }));

    // Dữ liệu cho Bar Charts
    const servicePriorityCount = data.reduce((acc, inc) => { const key = `${inc.serviceId}_${inc.priorityId}`; acc[key] = (acc[key] || { serviceId: inc.serviceId, priorityId: inc.priorityId, count: 0 }); acc[key].count++; return acc; }, {});
    const servicePriorityData = Object.values(servicePriorityCount).map(item => ({ service: getName(item.serviceId, SERVICES), priority: getName(item.priorityId, PRIORITIES), count: item.count }));

    const shiftPriorityCount = data.reduce((acc, inc) => { const key = `${inc.shiftId}_${inc.priorityId}`; acc[key] = (acc[key] || { shiftId: inc.shiftId, priorityId: inc.priorityId, count: 0 }); acc[key].count++; return acc; }, {});
    const shiftPriorityData = Object.values(shiftPriorityCount).map(item => ({ shift: getName(item.shiftId, SHIFTS), priority: getName(item.priorityId, PRIORITIES), count: item.count }));

    return {
        priorityDistribution: { data: priorityDistributionData },
        treemap: { data: treemapData },
        servicePriority: { data: servicePriorityData },
        shiftPriority: { data: shiftPriorityData },
    };
}