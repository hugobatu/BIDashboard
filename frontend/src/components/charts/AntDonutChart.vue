<!-- src/components/charts/AntDonutChart.vue -->
<template>
  <div ref="container" :style="{ height: '350px' }"></div>
</template>

<script setup>
import { ref, onMounted, watch, onUnmounted } from 'vue';
import { Pie } from '@antv/g2plot';

const props = defineProps({
  data: Array,
  angleField: String,
  colorField: String,
  // SỬA ĐỔI: Thêm prop 'color' để nhận bảng màu. 
  // Nó có thể là một mảng các mã màu hoặc một hàm.
  color: [Array, Function],
});

const container = ref(null);
let chart = null;

const renderChart = () => {
  if (chart) {
    chart.destroy();
  }
  if (!container.value) return;

  // Xây dựng đối tượng cấu hình một cách tường minh
  const config = {
    data: props.data,
    angleField: props.angleField,
    colorField: props.colorField,
    radius: 0.8,
    innerRadius: 0.6,
    label: {
      type: 'inner',
      offset: '-50%',
      content: '{value}',
      style: { textAlign: 'center', fontSize: 14 },
    },
    interactions: [{ type: 'element-selected' }, { type: 'element-active' }],
    statistic: {
      title: false,
      content: {
        style: { whiteSpace: 'pre-wrap', overflow: 'hidden', textOverflow: 'ellipsis' },
      },
    },
  };

  // SỬA ĐỔI: Nếu có prop 'color' được truyền vào, hãy sử dụng nó
  if (props.color) {
    config.color = props.color;
  }

  chart = new Pie(container.value, config);
  chart.render();
};

onMounted(() => {
  renderChart();
});

// Watch cả dữ liệu và bảng màu để vẽ lại khi cần
watch(() => [props.data, props.color], () => {
  renderChart();
}, { deep: true });

onUnmounted(() => {
  if (chart) {
    chart.destroy();
  }
});
</script>