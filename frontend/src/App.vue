<!-- src/App.vue -->
<template>
  <a-layout style="min-height: 100vh; background-color: #f0f2f5;">
    <!-- Header -->
    <a-layout-header style="background: #fff; padding: 0 24px; display: flex; align-items: center; justify-content: space-between;">
      <h1 style="font-size: 20px; font-weight: 600;">Tổng quan Hiệu suất Hệ thống - Phân tích Incident</h1>
      <a-space :size="12">
        <a-select
          v-model:value="selectedGroups"
          mode="multiple"
          style="width: 250px"
          placeholder="Lọc theo phòng ban..."
          allow-clear
          @change="processData"
        >
          <a-select-option v-for="group in ASSIGNMENT_GROUPS" :key="group.id" :value="group.id">
            {{ group.name }}
          </a-select-option>
        </a-select>

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
          <KpiCard 
            :title="kpi.latestTitle" 
            :value="kpi.latestValue"
            theme="warning" 
          />
        </a-col>
      </a-row>

      <!-- Hàng 2: SỬA ĐỔI HOÀN TOÀN - Treemap và Pie Chart -->
      <a-row :gutter="[24, 24]" style="margin-top: 24px;">
        <a-col :xs="24" :lg="12">
          <a-card title="Phân bổ Incident theo Service" :bordered="false">
             <!-- Đưa Treemap lên đây -->
            <AntTreemapChart 
              :data="charts.treemap.data"
              color-field="name"
              value-field="value"
              :color="serviceColors"
              :total-value="kpi.total"
            />
          </a-card>
        </a-col>
        <a-col :xs="24" :lg="12">
          <a-card title="Tỷ trọng Incident theo Mức độ Ưu tiên" :bordered="false">
            <!-- Giữ lại Pie Chart này -->
            <AntPieChart
              :data="charts.priorityDistribution.data"
              angle-field="value"
              color-field="type"
              :color="getPiePriorityColor"
            />
          </a-card>
        </a-col>
      </a-row>
      
      <!-- Hàng 3 đã được xóa, bây giờ là Hàng 4 -->
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
import AntPieChart from './components/charts/AntPieChart.vue';
import AntTreemapChart from './components/charts/AntTreemapChart.vue';
import AntGroupedBarChart from './components/charts/AntGroupedBarChart.vue';
import { rawIncidents, SERVICES, PRIORITIES, SHIFTS, ASSIGNMENT_GROUPS } from './data/mockData.js';

// ---- State cho bộ lọc ----
const filterMode = ref('month');
const selectedDate = ref(dayjs('2023-11-01'));
const selectedGroups = ref([]);

// ---- Bảng màu tùy chỉnh ----
const serviceColors = ['#1677FF', '#722ED1', '#13C2C2', '#EB2F96', '#FAAD14'];
const priorityColorMap = {
  'Critical': '#FF4D4F',
  'High': '#FA8C16',
  'Medium': '#FADB14',
  'Low': '#52C41A',
};
const getPiePriorityColor = ({ type }) => priorityColorMap[type] || '#E8E8E8';
const getBarPriorityColor = ({ priority }) => priorityColorMap[priority] || '#E8E8E8';

// ---- State cho dữ liệu hiển thị ----
const kpi = reactive({ total: 0, highPriority: 0, latestTitle: '', latestValue: 0 });
const charts = reactive({
  // SỬA ĐỔI: Xóa serviceDistribution
  priorityDistribution: { data: [] },
  treemap: { data: [] },
  servicePriority: { data: [] },
  shiftPriority: { data: [] },
});

// ---- Logic xử lý dữ liệu chính ----
const processData = () => {
  if (!selectedDate.value) return;

  const period = selectedDate.value;
  let incidentsByDate;
  if (filterMode.value === 'month') {
    incidentsByDate = rawIncidents.filter(inc => dayjs(inc.date).isSame(period, 'month'));
  } else {
    incidentsByDate = rawIncidents.filter(inc => dayjs(inc.date).isSame(period, 'year'));
  }

  let finalFilteredIncidents = incidentsByDate;
  if (selectedGroups.value && selectedGroups.value.length > 0) {
    finalFilteredIncidents = incidentsByDate.filter(inc => selectedGroups.value.includes(inc.assignmentGroupId));
  }
  
  if (finalFilteredIncidents.length === 0) {
    Object.assign(kpi, { total: 0, highPriority: 0, latestTitle: 'Không có dữ liệu', latestValue: 0 });
    Object.values(charts).forEach(chart => chart.data = []);
    return;
  }

  kpi.total = finalFilteredIncidents.length;
  kpi.highPriority = finalFilteredIncidents.filter(inc => ['p1', 'p2'].includes(inc.priorityId)).length;
  if (filterMode.value === 'month') {
    kpi.latestTitle = 'Incident Mới (Ngày gần nhất)';
    const latestDateStr = finalFilteredIncidents.reduce((max, p) => p.date > max ? p.date : max, finalFilteredIncidents[0].date);
    kpi.latestValue = finalFilteredIncidents.filter(inc => inc.date === latestDateStr).length;
  } else {
    kpi.latestTitle = 'Incident Mới (Tháng gần nhất)';
    const latestMonth = finalFilteredIncidents.reduce((max, p) => dayjs(p.date).month() > dayjs(max.date).month() ? p : max, finalFilteredIncidents[0]);
    kpi.latestValue = finalFilteredIncidents.filter(inc => dayjs(inc.date).isSame(dayjs(latestMonth.date), 'month')).length;
  }

  const getName = (id, list) => list.find(item => item.id === id)?.name || 'Unknown';
  
  // SỬA ĐỔI: Chỉ cần tính dữ liệu cho priority pie chart
  const priorityCount = finalFilteredIncidents.reduce((acc, inc) => { acc[inc.priorityId] = (acc[inc.priorityId] || 0) + 1; return acc; }, {});
  charts.priorityDistribution.data = Object.entries(priorityCount).map(([id, value]) => ({ type: getName(id, PRIORITIES), value }));

  // Treemap
  const treemapCount = finalFilteredIncidents.reduce((acc, inc) => {
    const serviceName = getName(inc.serviceId, SERVICES);
    acc[serviceName] = (acc[serviceName] || 0) + 1;
    return acc;
  }, {});
  charts.treemap.data = Object.entries(treemapCount).map(([name, value]) => ({ name, value }));
  
  // Grouped Bar Charts
  const servicePriorityCount = finalFilteredIncidents.reduce((acc, inc) => { const key = `${inc.serviceId}_${inc.priorityId}`; acc[key] = (acc[key] || { serviceId: inc.serviceId, priorityId: inc.priorityId, count: 0 }); acc[key].count++; return acc; }, {});
  charts.servicePriority.data = Object.values(servicePriorityCount).map(item => ({ service: getName(item.serviceId, SERVICES), priority: getName(item.priorityId, PRIORITIES), count: item.count }));
  const shiftPriorityCount = finalFilteredIncidents.reduce((acc, inc) => { const key = `${inc.shiftId}_${inc.priorityId}`; acc[key] = (acc[key] || { shiftId: inc.shiftId, priorityId: inc.priorityId, count: 0 }); acc[key].count++; return acc; }, {});
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