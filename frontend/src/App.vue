<!-- src/App.vue -->
<template>
  <a-layout style="min-height: 100vh; background-color: #f0f2f5;">
    <!-- Header -->
    <a-layout-header style="background: #fff; padding: 0 24px; display: flex; align-items: center; justify-content: space-between;">
      <h1 style="font-size: 20px; font-weight: 600;">Tổng quan Hiệu suất Hệ thống - Phân tích Incident</h1>
      <a-space :size="12">
        <a-radio-group v-model:value="filterMode" @change="onFilterModeChange">
          <a-radio-button value="month">Theo Tháng</a-radio-button>
          <a-radio-button value="year">Theo Năm</a-radio-button>
        </a-radio-group>
        <a-date-picker 
          v-model:value="selectedDate"
          :picker="filterMode"
          @change="processData"
          :allow-clear="false"
        />
      </a-space>
    </a-layout-header>

    <!-- Content -->
    <a-layout-content style="padding: 24px;">
      <!-- Hàng 1: KPIs -->
      <a-row :gutter="[24, 24]">
        <a-col :xs="24" :sm="12" :lg="8">
          <KpiCard 
            title="Tổng số Incident" 
            :value="kpi.total" 
            theme="info" 
          />
        </a-col>
        <a-col :xs="24" :sm="12" :lg="8">
          <KpiCard 
            title="Incident có Độ ưu tiên Cao/Nghiêm trọng" 
            :value="kpi.highPriority" 
            theme="danger" 
          />
        </a-col>
        <a-col :xs="24" :sm="12" :lg="8">
          <!-- SỬA ĐỔI CHÍNH Ở ĐÂY: Chuyển theme từ 'success' thành 'warning' -->
          <KpiCard 
            :title="kpi.latestTitle" 
            :value="kpi.latestValue"
            theme="warning" 
          />
        </a-col>
      </a-row>

      <!-- Hàng 2: Phân bổ -->
      <a-row :gutter="[24, 24]" style="margin-top: 24px;">
        <a-col :xs="24" :lg="12">
          <a-card title="Tỷ trọng Incident theo Service" :bordered="false">
            <AntDonutChart
              :data="charts.serviceDistribution.data"
              angle-field="value"
              color-field="type"
              :color="serviceColors"
            />
          </a-card>
        </a-col>
        <a-col :xs="24" :lg="12">
          <a-card title="Tỷ trọng Incident theo Mức độ Ưu tiên" :bordered="false">
            <AntDonutChart
              :data="charts.priorityDistribution.data"
              angle-field="value"
              color-field="type"
              :color="getDonutPriorityColor"
            />
          </a-card>
        </a-col>
      </a-row>
      
      <!-- Hàng 3: Xu hướng -->
      <a-row style="margin-top: 24px;">
        <a-col :span="24">
           <a-card title="Xu hướng & Tỷ trọng Incident theo Thời gian" :bordered="false">
            <AntStackedAreaChart 
              :data="charts.trend.data"
              :x-field="charts.trend.xField"
              y-field="count"
              series-field="service"
            />
          </a-card>
        </a-col>
      </a-row>

      <!-- Hàng 4: Phân tích sâu -->
      <a-row :gutter="[24, 24]" style="margin-top: 24px;">
        <a-col :xs="24" :lg="12">
          <a-card title="Số lượng Incident theo Mức độ Ưu tiên trong các Service" :bordered="false">
            <AntGroupedBarChart 
              :data="charts.servicePriority.data" 
              x-field="service" 
              y-field="count" 
              group-field="priority" 
              :color="getBarPriorityColor"
            />
          </a-card>
        </a-col>
        <a-col :xs="24" :lg="12">
          <a-card title="Tổng số Incident theo Mức độ Ưu tiên trong các Ca" :bordered="false">
            <AntGroupedBarChart 
              :data="charts.shiftPriority.data" 
              x-field="shift" 
              y-field="count" 
              group-field="priority"
              :color="getBarPriorityColor"
            />
          </a-card>
        </a-col>
      </a-row>
    </a-layout-content>
  </a-layout>
</template>

<script setup>
import { ref, onMounted, reactive } from 'vue';
import dayjs from 'dayjs';
import KpiCard from './components/KpiCard.vue';
import AntDonutChart from './components/charts/AntDonutChart.vue';
import AntStackedAreaChart from './components/charts/AntStackedAreaChart.vue';
import AntGroupedBarChart from './components/charts/AntGroupedBarChart.vue';
import { rawIncidents, SERVICES, PRIORITIES, SHIFTS } from './data/mockData.js';

// ---- State cho bộ lọc ----
const filterMode = ref('month');
const selectedDate = ref(dayjs('2023-11-01'));

// ---- Bảng màu tùy chỉnh ----
const serviceColors = ['#1677FF', '#722ED1', '#13C2C2', '#EB2F96', '#FAAD14'];
const priorityColorMap = {
  'Critical': '#FF4D4F', // Đỏ
  'High': '#FA8C16',     // Cam
  'Medium': '#FADB14',   // Vàng
  'Low': '#52C41A',      // Xanh lá
};
const getDonutPriorityColor = ({ type }) => {
    return priorityColorMap[type] || '#E8E8E8';
};
const getBarPriorityColor = ({ priority }) => {
    return priorityColorMap[priority] || '#E8E8E8';
};

// ---- State cho dữ liệu hiển thị ----
const kpi = reactive({ total: 0, highPriority: 0, latestTitle: '', latestValue: 0 });
const charts = reactive({
  serviceDistribution: { data: [] },
  priorityDistribution: { data: [] },
  trend: { data: [], xField: 'date' },
  servicePriority: { data: [] },
  shiftPriority: { data: [] },
});

// ---- Logic xử lý dữ liệu chính ----
const processData = () => {
  if (!selectedDate.value) return;
  const period = selectedDate.value;
  let filteredIncidents;
  if (filterMode.value === 'month') {
    filteredIncidents = rawIncidents.filter(inc => dayjs(inc.date).isSame(period, 'month'));
  } else {
    filteredIncidents = rawIncidents.filter(inc => dayjs(inc.date).isSame(period, 'year'));
  }

  if (filteredIncidents.length === 0) {
    Object.assign(kpi, { total: 0, highPriority: 0, latestTitle: 'Không có dữ liệu', latestValue: 0 });
    Object.values(charts).forEach(chart => chart.data = []);
    return;
  }

  // KPIs
  kpi.total = filteredIncidents.length;
  kpi.highPriority = filteredIncidents.filter(inc => ['p1', 'p2'].includes(inc.priorityId)).length;
  if (filterMode.value === 'month') {
    kpi.latestTitle = 'Incident Mới (Ngày gần nhất)';
    const latestDateStr = filteredIncidents.reduce((max, p) => p.date > max ? p.date : max, filteredIncidents[0].date);
    kpi.latestValue = filteredIncidents.filter(inc => inc.date === latestDateStr).length;
  } else {
    kpi.latestTitle = 'Incident Mới (Tháng gần nhất)';
    const latestMonth = filteredIncidents.reduce((max, p) => dayjs(p.date).month() > dayjs(max.date).month() ? p : max, filteredIncidents[0]);
    kpi.latestValue = filteredIncidents.filter(inc => dayjs(inc.date).isSame(dayjs(latestMonth.date), 'month')).length;
  }

  const getName = (id, list) => list.find(item => item.id === id)?.name || 'Unknown';
  
  // Donut Charts
  const serviceCount = filteredIncidents.reduce((acc, inc) => { acc[inc.serviceId] = (acc[inc.serviceId] || 0) + 1; return acc; }, {});
  charts.serviceDistribution.data = Object.entries(serviceCount).map(([id, value]) => ({ type: getName(id, SERVICES), value }));
  const priorityCount = filteredIncidents.reduce((acc, inc) => { acc[inc.priorityId] = (acc[inc.priorityId] || 0) + 1; return acc; }, {});
  charts.priorityDistribution.data = Object.entries(priorityCount).map(([id, value]) => ({ type: getName(id, PRIORITIES), value }));

  // Stacked Area Chart
  if (filterMode.value === 'month') {
    charts.trend.xField = 'date';
    const trendCount = filteredIncidents.reduce((acc, inc) => {
      const key = `${inc.date}_${inc.serviceId}`;
      acc[key] = (acc[key] || { date: inc.date, serviceId: inc.serviceId, count: 0 });
      acc[key].count++;
      return acc;
    }, {});
    charts.trend.data = Object.values(trendCount)
        .map(item => ({ ...item, service: getName(item.serviceId, SERVICES) }))
        .sort((a, b) => a.date.localeCompare(b.date));
  } else {
    charts.trend.xField = 'month';
    const trendCount = filteredIncidents.reduce((acc, inc) => {
      const month = dayjs(inc.date).format('YYYY-MM');
      const key = `${month}_${inc.serviceId}`;
      acc[key] = (acc[key] || { month: month, serviceId: inc.serviceId, count: 0 });
      acc[key].count++;
      return acc;
    }, {});
    charts.trend.data = Object.values(trendCount)
        .map(item => ({ ...item, service: getName(item.serviceId, SERVICES) }))
        .sort((a, b) => a.month.localeCompare(b.month));
  }

  // Grouped Bar Charts
  const servicePriorityCount = filteredIncidents.reduce((acc, inc) => { const key = `${inc.serviceId}_${inc.priorityId}`; acc[key] = (acc[key] || { serviceId: inc.serviceId, priorityId: inc.priorityId, count: 0 }); acc[key].count++; return acc; }, {});
  charts.servicePriority.data = Object.values(servicePriorityCount).map(item => ({ service: getName(item.serviceId, SERVICES), priority: getName(item.priorityId, PRIORITIES), count: item.count }));
  const shiftPriorityCount = filteredIncidents.reduce((acc, inc) => { const key = `${inc.shiftId}_${inc.priorityId}`; acc[key] = (acc[key] || { shiftId: inc.shiftId, priorityId: inc.priorityId, count: 0 }); acc[key].count++; return acc; }, {});
  charts.shiftPriority.data = Object.values(shiftPriorityCount).map(item => ({ shift: getName(item.shiftId, SHIFTS), priority: getName(item.priorityId, PRIORITIES), count: item.count }));
};

const onFilterModeChange = () => {
  if (filterMode.value === 'month') {
    selectedDate.value = dayjs('2023-11-01');
  } else {
    selectedDate.value = dayjs('2023-01-01');
  }
  processData();
};

onMounted(() => {
  processData();
});
</script>