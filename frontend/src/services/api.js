import { rawIncidents, SERVICES, PRIORITIES, SHIFTS, ASSIGNMENT_GROUPS } from '@/data/mockData.js';
import dayjs from 'dayjs';

/**
 * Giả lập việc gọi API để lấy dữ liệu dashboard đã được lọc.
 * Sau này, bạn chỉ cần thay thế phần logic lọc bằng một lệnh fetch/axios thật.
 * @param {object} filters - Các bộ lọc từ UI, ví dụ: { mode: 'month', date: dayjs_object, groups: ['grp01'] }
 * @returns {Promise<object>} - Một promise trả về dữ liệu đã được xử lý.
 */
export function fetchDashboardData(filters) {
  console.log("Đang gọi API giả lập với bộ lọc:", filters);

  return new Promise(resolve => {
    // Giả lập độ trễ mạng
    setTimeout(() => {
      // 1. Lọc theo thời gian
      let incidentsByDate;
      if (filters.mode === 'month') {
        incidentsByDate = rawIncidents.filter(inc => dayjs(inc.date).isSame(filters.date, 'month'));
      } else {
        incidentsByDate = rawIncidents.filter(inc => dayjs(inc.date).isSame(filters.date, 'year'));
      }

      // 2. Lọc tiếp theo phòng ban
      let finalFilteredIncidents = incidentsByDate;
      if (filters.groups && filters.groups.length > 0) {
        finalFilteredIncidents = incidentsByDate.filter(inc => 
          filters.groups.includes(inc.assignmentGroupId)
        );
      }

      // Trả về dữ liệu thô đã được lọc, và các dimension để xử lý ở tầng logic
      resolve({
        filteredData: finalFilteredIncidents,
        dimensions: { SERVICES, PRIORITIES, SHIFTS, ASSIGNMENT_GROUPS }
      });

    }, 500); // 500ms delay
  });
}