<template>
  <a-layout style="min-height: 100vh; background-color: #f0f2f5;">
    <a-layout-header style="background: #fff; padding: 0 24px; display: flex; align-items: center; justify-content: space-between;">
      <h1 style="font-size: 20px; font-weight: 600;">Tổng quan Hiệu suất Hệ thống - Phân tích Incident</h1>
      <a-space :size="12">
        <a-select v-model:value="filters.groups" mode="multiple" style="width: 250px" placeholder="Lọc theo phòng ban..." allow-clear>
          <a-select-option v-for="group in ASSIGNMENT_GROUPS" :key="group.id" :value="group.id">{{ group.name }}</a-select-option>
        </a-select>
        <a-radio-group v-model:value="filters.mode">
          <a-radio-button value="month">Theo Tháng</a-radio-button>
          <a-radio-button value="year">Theo Năm</a-radio-button>
        </a-radio-group>
        <a-date-picker v-model:value="filters.date" :picker="filters.mode" :allow-clear="false" />
      </a-space>
    </a-layout-header>

    <a-layout-content style="padding: 24px;">
      <a-spin :spinning="state.loading" size="large">
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
            <a-card title="Phân bổ Incident theo Service" :bordered="false">
              <AntTreemapChart :data="state.charts.treemap.data" color-field="name" value-field="value" :color="serviceColors" :total-value="state.kpi.total" />
            </a-card>
          </a-col>
          <a-col :xs="24" :lg="12">
            <a-card title="Tỷ trọng Incident theo Mức độ Ưu tiên" :bordered="false">
              <AntPieChart :data="state.charts.priorityDistribution.data" angle-field="value" color-field="type" :color="getPiePriorityColor" />
            </a-card>
          </a-col>
        </a-row>

        <a-row :gutter="[24, 24]" style="margin-top: 24px;">
          <a-col :xs="24" :lg="12">
            <a-card title="Số lượng Incident theo Mức độ Ưu tiên trong các Service" :bordered="false">
              <AntGroupedBarChart :data="state.charts.servicePriority.data" x-field="service" y-field="count" group-field="priority" :color="getBarPriorityColor" />
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
import { reactive, onMounted, watch } from 'vue';
import dayjs from 'dayjs';

import KpiCard from './components/KpiCard.vue';
import AntPieChart from './components/charts/AntPieChart.vue';
import AntTreemapChart from './components/charts/AntTreemapChart.vue';
import AntGroupedBarChart from './components/charts/AntGroupedBarChart.vue';
import { ASSIGNMENT_GROUPS } from './data/mockData.js';

import { useDashboard } from './composables/useDashboard.js';


const { state, updateDashboardData } = useDashboard();

const filters = reactive({
  mode: 'month',
  date: dayjs('2023-11-01'),
  groups: [],
});


const serviceColors = ['#1677FF', '#722ED1', '#13C2C2', '#EB2F96', '#FAAD14'];
const priorityColorMap = {
  'Critical': '#FF4D4F',
  'High': '#FA8C16',
  'Medium': '#FADB14',
  'Low': '#52C41A',
};
const getPiePriorityColor = ({ type }) => priorityColorMap[type] || '#E8E8E8';
const getBarPriorityColor = ({ priority }) => priorityColorMap[priority] || '#E8E8E8';


watch(
  filters,
  (newFilters) => {
    if (newFilters.mode === 'month' && !dayjs(newFilters.date).isSame(dayjs(newFilters.date), 'month')) {
      filters.date = dayjs(newFilters.date).startOf('year');
    }
    updateDashboardData(newFilters);
  },
  { deep: true }
);

onMounted(() => {
  updateDashboardData(filters);
});
</script>