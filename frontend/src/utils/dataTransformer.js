/**
 * Biến đổi dữ liệu từ API thành định dạng mà các biểu đồ có thể sử dụng.
 * @param {object} apiData - Dữ liệu trả về từ các API backend.
 * @returns {object} - Đối tượng chứa dữ liệu đã xử lý cho các biểu đồ.
 */
export function transformApiDataToCharts(apiData) {
  const { servicePriorityData, shiftDistributionData, trendByDaeoData, groupDistributionData = [] } = apiData;

  const cleanedServiceData = servicePriorityData.filter(item => item && item.Service && item.Priority && item.AssignmentGroup);
  const cleanedShiftData = shiftDistributionData.filter(item => item && item.shift && item.count);

  // --- Treemap Data ---
  const treemapData = groupDistributionData
  .filter(item => item && item.AssignmentGroup)
  .map(item => ({
    name: item.AssignmentGroup,
    value: item.Count
  }));

  // --- Priority Pie Chart Data ---
  const priorityCount = cleanedServiceData.reduce((acc, item) => {
    acc[item.Priority] = (acc[item.Priority] || 0) + item.Count;
    return acc;
  }, {});
  const priorityDistributionData = Object.entries(priorityCount).map(([type, value]) => ({ type, value }));

  // --- Service-Priority Bar Chart Data ---
  const rawServicePriorityData = cleanedServiceData.map(item => ({
    service: item.Service,
    priority: item.Priority,
    count: item.Count,
  }));
  const serviceTotals = rawServicePriorityData.reduce((acc, item) => {
    acc[item.service] = (acc[item.service] || 0) + item.count;
    return acc;
  }, {});
  const sortedServices = Object.keys(serviceTotals).sort((a, b) => serviceTotals[b] - serviceTotals[a]);
  const finalServicePriorityData = rawServicePriorityData.sort((a, b) => {
    return sortedServices.indexOf(a.service) - sortedServices.indexOf(b.service);
  });
  
  // --- Shift-Priority Bar Chart Data (Cập nhật) ---
  const finalShiftData = cleanedShiftData.map(item => ({
    type: item.shift,
    total_sales: item.count,
  }));
  
  // --- DAEO Trend Line Chart Data ---
  const trendByDaeo = trendByDaeoData
    .filter(item => item && item.Time != null && item.AssignmentGroup)
    .map(item => ({
      time: item.Time.toString(),
      group: item.AssignmentGroup,
      count: item.Count || 0,
    }))
    .sort((a,b) => a.time.localeCompare(b.time, undefined, {numeric: true}));

  return {
    priorityDistribution: { data: priorityDistributionData },
    treemap: { data: treemapData },
    servicePriority: { data: finalServicePriorityData },
    shiftPriority: { data: finalShiftData },
    trendByDaeo: { data: trendByDaeo },
  };
}