<!-- src/App.vue -->
<template>
  <a-layout style="min-height: 100vh; background-color: #f0f2f5;">
    <!-- Header của Dashboard -->
    <a-layout-header style="background: #fff; padding: 0 24px;">
      <h1 style="font-size: 20px; font-weight: bold; color: #333;">
        Hệ thống Báo cáo Incident
      </h1>
    </a-layout-header>

    <!-- Nội dung chính -->
    <a-layout-content style="padding: 24px;">
      <!-- Bộ lọc -->
      <a-row :gutter="16" style="margin-bottom: 24px;">
        <a-col>
          <a-date-picker placeholder="Chọn tháng" picker="month" />
        </a-col>
        <a-col>
          <a-select
            placeholder="Chọn Service"
            style="width: 200px"
            mode="multiple"
            allow-clear
          >
            <a-select-option value="service1">Thanh toán Online</a-select-option>
            <a-select-option value="service2">Hệ thống CRM</a-select-option>
          </a-select>
        </a-col>
      </a-row>

      <!-- Dòng chứa các biểu đồ -->
      <a-row :gutter="[24, 24]">
        <!-- Biểu đồ 1: Incident theo Service -->
        <a-col :xs="24" :lg="12">
          <a-card title="Tổng số Incident theo Service" :bordered="false">
            <AntBarChart
              :data="serviceIncidentData"
              x-field="service"
              y-field="count"
              :style="{ height: '400px' }"
            />
          </a-card>
        </a-col>

        <!-- Biểu đồ 2: Incident theo Mức độ ưu tiên -->
        <a-col :xs="24" :lg="12">
          <a-card title="Phân bổ Incident theo Mức độ Ưu tiên" :bordered="false">
             <!-- Giả sử bạn có một AntPieChart component -->
             <!-- <AntPieChart :data="priorityData" ... /> -->
             <div style="height: 400px; display: flex; align-items: center; justify-content: center; color: #999;">
                Pie Chart Placeholder
             </div>
          </a-card>
        </a-col>

        <!-- Biểu đồ 3: Xu hướng Incident -->
        <a-col :span="24">
          <a-card title="Xu hướng Incident hàng tháng" :bordered="false">
            <!-- Giả sử bạn có một AntLineChart component -->
            <div style="height: 400px; display: flex; align-items: center; justify-content: center; color: #999;">
                Line Chart Placeholder
             </div>
          </a-card>
        </a-col>
      </a-row>
    </a-layout-content>
  </a-layout>
</template>

<script setup>
import { ref } from 'vue';
// Import các component biểu đồ bạn đã tạo
import AntBarChart from './components/AntBarChart.vue';
// import AntPieChart from './components/AntPieChart.vue'; // Ví dụ
// import AntLineChart from './components/AntLineChart.vue'; // Ví dụ

// Dữ liệu giả, trong thực tế bạn sẽ lấy từ API
const serviceIncidentData = ref([
  { service: 'Thanh toán Online', count: 120 },
  { service: 'Hệ thống CRM', count: 95 },
  { service: 'Quản lý Kho', count: 70 },
  { service: 'Báo cáo Nội bộ', count: 55 },
  { service: 'Hỗ trợ Khách hàng', count: 40 },
]);

// Bạn có thể định nghĩa thêm dữ liệu cho các biểu đồ khác ở đây
// const priorityData = ref([...]);
// const trendData = ref([...]);
</script>

<style>
/* Bạn có thể thêm các style tùy chỉnh ở đây nếu cần */
.ant-card {
  border-radius: 8px !important;
}
</style>