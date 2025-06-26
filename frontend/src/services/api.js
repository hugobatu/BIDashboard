import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'https://localhost:7034',
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Hàm tổng hợp, gọi tất cả các API cần thiết từ backend.
 * @param {object} filters - { mode, date, groups }
 * @returns {Promise<object>} - Một object lớn chứa tất cả dữ liệu từ backend
 */
export async function fetchDashboardData(filters) {
  const { mode, date, groups } = filters;
  
  const paramsWithGroup = {
    year: date.year(),
    ...(mode === 'month' && { month: date.month() + 1 }), 
    assignmentGroups: groups,
  };
  
  const paramsWithoutGroup = {
    year: date.year(),
    ...(mode === 'month' && { month: date.month() + 1 }),
  };

  try {
    const [kpisRes, servicePriorityRes, shiftPriorityRes, trendByDaeoRes, groupDistributionRes] = await Promise.all([
      apiClient.get('/kpis', { params: paramsWithGroup }),
      apiClient.get('/service-priority-distribution', { params: paramsWithGroup }),
      apiClient.get('/shift-priority-distribution', { params: paramsWithGroup }),
      apiClient.get('/trend-by-daeo-group', { params: paramsWithoutGroup }),
      apiClient.get('/assignment-group-distribution', { params: paramsWithoutGroup }),
    ]);

    return {
      kpiData: kpisRes.data,
      servicePriorityData: servicePriorityRes.data,
      shiftPriorityData: shiftPriorityRes.data,
      trendByDaeoData: trendByDaeoRes.data,
      groupDistributionData: groupDistributionRes.data,
    };
  } catch (error) {
    console.error("Lỗi khi gọi API backend:", error);
    throw error;
  }
}