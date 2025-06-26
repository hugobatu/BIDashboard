import { reactive, readonly } from 'vue';
import { fetchDashboardData } from '@/services/api.js';
import { transformKpis, transformCharts } from '@/utils/dataTransformer.js';

// State của toàn bộ dashboard được quản lý tại đây
const state = reactive({
  loading: false,
  kpi: { total: 0, highPriority: 0, latestTitle: '', latestValue: 0 },
  charts: {
    priorityDistribution: { data: [] },
    treemap: { data: [] },
    servicePriority: { data: [] },
    shiftPriority: { data: [] },
  }
});

/**
 * Hàm chính để cập nhật toàn bộ dữ liệu dashboard
 * @param {object} filters - Các bộ lọc từ UI
 */
async function updateDashboardData(filters) {
  state.loading = true;
  try {
    // 1. Gọi API để lấy dữ liệu đã được lọc
    const { filteredData, dimensions } = await fetchDashboardData(filters);

    if (filteredData.length === 0) {
      // Reset state nếu không có dữ liệu
      state.kpi = { total: 0, highPriority: 0, latestTitle: 'Không có dữ liệu', latestValue: 0 };
      Object.values(state.charts).forEach(chart => chart.data = []);
      return;
    }
    
    // 2. SỬA ĐỔI: Gọi các hàm từ utils để biến đổi dữ liệu
    const kpiData = transformKpis(filteredData, filters.mode);
    const chartsData = transformCharts(filteredData, dimensions);

    // 3. SỬA ĐỔI: Cập nhật state với dữ liệu đã được biến đổi
    state.kpi = kpiData;
    state.charts = chartsData;

  } catch (error) {
    console.error("Lỗi khi cập nhật dữ liệu dashboard:", error);
  } finally {
    state.loading = false;
  }
}

/**
 * Composable chính để sử dụng trong component
 */
export function useDashboard() {
  return {
    state: readonly(state),
    updateDashboardData
  }
}