<template>
  <a-layout style="min-height: 100vh; background-color: #f0f2f5;">
    <a-layout-header style="background: #fff; padding: 0 24px; display: flex; align-items: center; justify-content: space-between;">
      <h1 style="font-size: 20px; font-weight: 600;">Tổng quan Hiệu suất Hệ thống - Phân tích Incident</h1>
      <a-space :size="12">
        <a-select
          v-model:value="filters.groups"
          mode="multiple"
          style="width: 250px"
          placeholder="Lọc theo phòng ban..."
          allow-clear
          :options="assignmentGroupOptions"
        >
        </a-select>
        <a-radio-group v-model:value="filters.mode">
          <a-radio-button value="month">Theo Tháng</a-radio-button>
          <a-radio-button value="year">Theo Năm</a-radio-button>
        </a-radio-group>
        <a-date-picker 
          v-model:value="filters.date"
          :picker="filters.mode"
          :allow-clear="false"
        />
      </a-space>
    </a-layout-header>

    <a-layout-content style="padding: 24px;">
      <a-spin :spinning="state.loading" size="large" tip="Đang tải dữ liệu...">
        <!-- Hàng 1: KPIs -->
        <a-row :gutter="[24, 24]">
          <a-col :xs="24" :sm="12" :lg="8">
            <KpiCard title="Tổng số Incident" :value="state.kpi.total" theme="info" />
          </a-col>
          <a-col :xs="24" :sm="12" :lg="8">
            <KpiCard title="Incident có Độ ưu tiên Cao/Nghiêm trọng" :value="state.kpi.highPriority" theme="danger" />
          </a-col>
          <a-col :xs="24" :sm="12" :lg="8">
            <KpiCard :title="state.kpi.latestTitle" :value="state.kpi.latestValue" theme="warning" />
          </a-col>
        </a-row>

        <a-row :gutter="[24, 24]" style="margin-top: 24px;">
          <a-col :xs="24" :lg="12">
            <a-card title="Số lượng Incident theo Mức độ Ưu tiên trong các Service" :bordered="false">
              <AntStackedBarChart :data="state.charts.servicePriority.data" x-field="service" y-field="count" series-field="priority" :color="getBarPriorityColor" />
            </a-card>
          </a-col>
          <a-col :xs="24" :lg="12">
            <a-card title="Tỷ trọng Incident theo Mức độ Ưu tiên" :bordered="false">
              <AntPieChart :data="state.charts.priorityDistribution.data" angle-field="value" color-field="type" :color="getPiePriorityColor" />
            </a-card>
          </a-col>
        </a-row>
        
        <a-row :gutter="[24, 24]" style="margin-top: 24px;">
          <a-col :span="24">
            <a-card title="Xu hướng Incident theo Nhóm DAEO và NON-DAEO" :bordered="false">
              <AntMultiLineChart
                :data="state.charts.trendByDaeo.data"
                x-field="time"
                y-field="count"
                series-field="group"
              />
            </a-card>
          </a-col>
        </a-row>

        <a-row :gutter="[24, 24]" style="margin-top: 24px;">
          <a-col :xs="24" :lg="12">
            <a-card title="Phân bổ Incident theo Phòng ban" :bordered="false">
              <AntTreemapChart :data="state.charts.treemap.data" color-field="name" value-field="value" :color="groupColors" :total-value="state.kpi.total" />
            </a-card>
          </a-col>
          <a-col :xs="24" :lg="12">
            <a-card title="Tổng số Incident theo Mức độ Ưu tiên trong các Ca" :bordered="false">
              <AntGroupedBarChart :data="state.charts.shiftPriority.data" x-field="shift" y-field="count" group-field="priority" :color="getBarPriorityColor" />
            </a-card>
          </a-col>
        </a-row>
      </a-spin>
    </a-layout-content>
  </a-layout>
</template>

<script setup>
import { reactive, onMounted, watch, computed } from 'vue';
import dayjs from 'dayjs';
import KpiCard from './components/KpiCard.vue';
import AntPieChart from './components/charts/AntPieChart.vue';
import AntTreemapChart from './components/charts/AntTreemapChart.vue';
import AntGroupedBarChart from './components/charts/AntGroupedBarChart.vue'; 
import AntStackedBarChart from './components/charts/AntStackedBarChart.vue'; 
import AntMultiLineChart from './components/charts/AntMultiLineChart.vue';
import { useDashboard } from './composables/useDashboard.js';

const { state, updateDashboardData } = useDashboard();

// ---- State cho các bộ lọc trên UI ----
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

// ---- Bảng màu ----
const groupColors = [
  '#5B8FF9', '#61DDAA', '#65789B', '#F6BD16', '#7262FD', 
  '#78D3F8', '#9661BC', '#F6903D', '#008685', '#F08BB4'
];

const priorityColorMap = {
  'Critical': '#FF4D4F',
  'High': '#FA8C16',
  'Moderate': '#FADB14',
  'Low': '#52C41A',
};
const getPiePriorityColor = ({ type }) => priorityColorMap[type] || '#E8E8E8';
const getBarPriorityColor = ({ priority }) => priorityColorMap[priority] || '#E8E8E8';

watch(filters, (newFilters) => {
  updateDashboardData(newFilters);
}, { deep: true });

onMounted(() => {
  updateDashboardData(filters); 
});
</script>