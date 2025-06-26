import { reactive, readonly } from 'vue';
import { fetchDashboardData } from '@/services/api.js';
import { transformApiDataToCharts } from '@/utils/dataTransformer.js';

const state = reactive({
  loading: false,
  kpi: { total: 0, highPriority: 0, latestTitle: '', latestValue: 0 },
  charts: {
    priorityDistribution: { data: [] },
    treemap: { data: [] },
    servicePriority: { data: [] },
    shiftPriority: { data: [] },
    trendByDaeo: { data: [] },
  },
  dimensions: {
    ASSIGNMENT_GROUPS: [],
  }
});

/**
 * Hàm chính để cập nhật toàn bộ dữ liệu dashboard
 * @param {object} filters - Các bộ lọc từ UI
 */
async function updateDashboardData(filters) {
  state.loading = true;
  try {
    const apiData = await fetchDashboardData(filters);

    if (apiData.servicePriorityData) {
        const allGroupNames = apiData.servicePriorityData.map(item => item.AssignmentGroup).filter(Boolean);
        const uniqueGroupNames = [...new Set(allGroupNames)];
        state.dimensions.ASSIGNMENT_GROUPS = uniqueGroupNames.sort();
    }
    
    state.kpi = apiData.kpiData;
    
    state.charts = transformApiDataToCharts(apiData);

  } catch (error) {
    console.error("Không thể tải dữ liệu dashboard:", error);
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
    updateDashboardData,
  }
}