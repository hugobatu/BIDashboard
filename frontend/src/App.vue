<template>
  <a-layout style="height: 100vh; overflow: hidden;">
    <a-layout-header 
      :style="{ 
        position: 'relative',
        zIndex: 10, 
        width: '100%', 
        background: '#fff', 
        padding: '0 24px',
        borderBottom: '1px solid #f0f0f0' 
      }"
    >
      <div 
        class="header-content" 
        style="display: flex; align-items: center; justify-content: space-between; max-width: 1600px; margin: 0 auto;"
      >
        <h1 style="font-size: 20px; font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; margin: 0;">
          Tổng quan Hiệu suất Hệ thống
        </h1>
        <a-space :size="12">
          <a-select v-model:value="filters.groups" mode="multiple" style="width: 200px" placeholder="Lọc theo phòng ban..." allow-clear :options="assignmentGroupOptions" />
          <a-radio-group v-model:value="filters.mode">
            <a-radio-button value="month">Tháng</a-radio-button>
            <a-radio-button value="year">Năm</a-radio-button>
          </a-radio-group>
          <a-date-picker v-model:value="filters.date" :picker="mode" :allow-clear="false" />
        </a-space>
      </div>
    </a-layout-header>

    <a-layout-content 
      :style="{ 
        padding: '24px', 
        background: '#f0f2f5',
        overflow: 'hidden'
      }"
    >
      <div style="max-width: 1600px; margin: 0 auto; display: flex; flex-direction: column; height: 100%;">
        <a-spin :spinning="state.loading" size="large" tip="Đang tải dữ liệu..." style="display: flex; flex-direction: column; height: 100%;">

          <a-row :gutter="[24, 24]" style="flex: 0 0 auto;">
            <a-col :xs="24" :sm="24" :md="8"><KpiCard title="Tổng số Incident" :value="state.kpi.total" theme="info" /></a-col>
            <a-col :xs="24" :sm="12" :md="8"><KpiCard title="Số Incident Nghiêm trọng" :value="state.kpi.highPriority" theme="danger" /></a-col>
            <a-col :xs="24" :sm="12" :md="8"><KpiCard :title="state.kpi.latestTitle" :value="state.kpi.latestValue" theme="warning" /></a-col>
          </a-row>

          <div style="flex: 1 1 auto; display: flex; flex-direction: column; justify-content: center; margin-top: 24px; min-height: 0;">

            <a-row :gutter="[24, 24]">
              <a-col :xs="24" :lg="12">
                <a-card title="Số lượng Incident theo Mức độ Ưu tiên trong các Service" :bordered="false" class="limited-height-card">
                  <AntStackedBarChart :data="sortedServicePriorityData" x-field="service" y-field="count" series-field="priority" :color="getBarPriorityColor" />
                </a-card>
              </a-col>
              <a-col :xs="24" :lg="12">
                <a-card title="Xu hướng Incident theo Nhóm DAEO và NON-DAEO" :bordered="false" class="limited-height-card">
                  <AntMultiLineChart :data="state.charts.trendByDaeo.data" x-field="time" y-field="count" series-field="group" :color="groupColors"/>
                </a-card>
              </a-col>
            </a-row>

            <a-row :gutter="[24, 24]" style="margin-top: 24px;">
              <a-col :xs="24" :lg="8">
                <a-card title="Tỷ trọng Incident theo Mức độ Ưu tiên" :bordered="false" class="limited-height-card">
                  <AntPieChart :data="state.charts.priorityDistribution.data" angle-field="value" color-field="type" :color="getPiePriorityColor" />
                </a-card>
              </a-col>
              <a-col :xs="24" :lg="8">
                <a-card title="Phân bổ Incident theo Phòng ban" :bordered="false" class="limited-height-card">
                  <AntTreemapChart :data="state.charts.treemap.data" color-field="name" value-field="value" :color="groupColors" :total-value="state.kpi.total" />
                </a-card>
              </a-col>
              <a-col :xs="24" :lg="8">
                <a-card title="Tổng số Incident theo Ca" :bordered="false" class="limited-height-card">
                  <AntDrillBarChart :data="state.charts.shiftPriority.data" :filters="filters" :color="groupColors" />
                </a-card>
              </a-col>
            </a-row>
            
          </div>
        </a-spin>
      </div>
    </a-layout-content>
  </a-layout>
</template>

<script setup>
import { reactive, onMounted, watch, computed } from 'vue';
import dayjs from 'dayjs';
import KpiCard from './components/KpiCard.vue';
import AntPieChart from './components/charts/AntPieChart.vue';
import AntTreemapChart from './components/charts/AntTreemapChart.vue';
import AntDrillBarChart from './components/charts/AntDrillBarChart.vue';
import AntStackedBarChart from './components/charts/AntStackedBarChart.vue'; 
import AntMultiLineChart from './components/charts/AntMultiLineChart.vue';
import { useDashboard } from './composables/useDashboard.js';

const { state, updateDashboardData } = useDashboard();

const filters = reactive({
  mode: 'month',
  date: dayjs('2025-05-29'),
  groups: [],
});

const assignmentGroupOptions = computed(() => {
    return state.dimensions.ASSIGNMENT_GROUPS.map(name => ({
        label: name,
        value: name
    }));
});

const groupColors = [
  '#0072B2', // Xanh dương đậm
  '#E69F00', // Vàng cam
  '#56B4E9', // Xanh da trời
  '#009E73', // Xanh lá cây (ngả lam)
  '#CC79A7', // Hồng tím
  '#D55E00', // Cam đỏ (Vermilion)
  '#F0E442', // Vàng chanh
  '#882255', // Đỏ rượu vang (Maroon)
  '#44AA99', // Xanh bạc hà (Teal)
  '#888888', // Xám trung tính
];

const priorityColorMap = {
  'Critical': '#D55E00',
  'High': '#C19A00',
  'Moderate': '#332288',
  'Low': '#56B4E9',
};
const getPiePriorityColor = ({ type }) => priorityColorMap[type] || '#E8E8E8';
const getBarPriorityColor = ({ priority }) => priorityColorMap[priority] || '#E8E8E8';

const sortedServicePriorityData = computed(() => {
  const priorityOrder = ['Critical', 'High', 'Moderate', 'Low'];

  if (!state.charts.servicePriority.data) {
    return [];
  }
  
  return [...state.charts.servicePriority.data].sort((a, b) => {
    const indexA = priorityOrder.indexOf(a.priority);
    const indexB = priorityOrder.indexOf(b.priority);
    return indexA - indexB;
  });
});

watch(filters, (newFilters) => {
  updateDashboardData(newFilters);
}, { deep: true });

onMounted(() => {
  updateDashboardData(filters); 
});
</script>

<style scoped>
.limited-height-card {
  max-height: 300px; 
  
  display: flex;
  flex-direction: column;
}

.limited-height-card :deep(.ant-card-body) {
  flex: 1 1 auto;
  display: flex;
  flex-direction: column;
}

.limited-height-card :deep(.ant-card-body > div) {
    flex: 1;
    min-height: 0;
}
</style>