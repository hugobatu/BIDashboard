<!-- src/components/KpiCard.vue -->
<template>
  <a-card :bordered="false" :body-style="{ padding: '20px 24px' }">
    <div class="flex items-center justify-between">
      <!-- Phần thống kê: Title và Value -->
      <div class="statistic">
        <div class="text-gray-500 text-sm">{{ title }}</div>
        <div 
          class="text-3xl font-bold mt-1" 
          :style="{ color: themeConfig.color }"
        >
          {{ value }}
        </div>
      </div>
      
      <!-- Phần icon trang trí -->
      <div 
        class="icon-wrapper flex items-center justify-center w-16 h-16 rounded-full"
        :style="{ backgroundColor: themeConfig.bgColor }"
      >
        <component 
          :is="themeConfig.icon" 
          :style="{ color: themeConfig.color, fontSize: '32px' }"
        />
      </div>
    </div>
  </a-card>
</template>

<script setup>
import { computed } from 'vue';
import { 
  BarChartOutlined, 
  WarningOutlined,
  ExclamationCircleOutlined, // <-- SỬA ĐỔI: Sử dụng icon này cho warning
} from '@ant-design/icons-vue';

const props = defineProps({
  title: String,
  value: [String, Number],
  // 'info', 'danger', 'warning'
  theme: {
    type: String,
    default: 'info',
  },
});

// Định nghĩa các bộ màu và icon cho từng theme
const themes = {
  info: {
    color: '#1677FF', // Xanh dương
    bgColor: '#E6F4FF',
    icon: BarChartOutlined,
  },
  danger: {
    color: '#FF4D4F', // Đỏ
    bgColor: '#FFF1F0',
    icon: WarningOutlined,
  },
  // SỬA ĐỔI: Thêm theme 'warning'
  warning: {
    color: '#FA8C16', // Cam
    bgColor: '#FFF7E6',
    icon: ExclamationCircleOutlined,
  },
};

// Lấy ra cấu hình của theme hiện tại
const themeConfig = computed(() => {
  return themes[props.theme] || themes.info;
});
</script>