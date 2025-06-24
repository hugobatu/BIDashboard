<!-- src/components/AntBarChart.vue -->
<template>
  <div ref="container"></div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { Bar } from '@antv/g2plot';

// Định nghĩa props để nhận dữ liệu từ component cha
const props = defineProps({
  data: {
    type: Array,
    required: true,
  },
  xField: {
    type: String,
    required: true,
  },
  yField: {
    type: String,
    required: true,
  },
  // Các options tùy chỉnh khác
  options: {
    type: Object,
    default: () => ({}),
  },
});

const container = ref(null);
let chart = null;

onMounted(() => {
  if (container.value) {
    chart = new Bar(container.value, {
      data: props.data,
      xField: props.xField,
      yField: props.yField,
      // Các cấu hình mặc định cho biểu đồ
      xAxis: {
        label: {
          autoHide: true,
          autoRotate: false,
        },
      },
      tooltip: {
        // Tùy chỉnh nội dung tooltip nếu cần
      },
      ...props.options, // Ghi đè bằng các options tùy chỉnh
    });
    chart.render();
  }
});

// Cập nhật biểu đồ khi dữ liệu thay đổi
watch(() => props.data, (newData) => {
  if (chart) {
    chart.changeData(newData);
  }
});
</script>