<!-- src/components/charts/AntGroupedBarChart.vue -->
<template>
  <div ref="container" :style="{ height: '350px' }"></div>
</template>

<script setup>
import { ref, onMounted, watch, onUnmounted } from 'vue';
import { Bar } from '@antv/g2plot';

const props = defineProps({
  data: Array,
  xField: String,
  yField: String,
  groupField: String,
  // SỬA ĐỔI: Thêm prop 'color' để nhận bảng màu tùy chỉnh
  color: [Array, Function],
});

const container = ref(null);
let chart = null;

const renderChart = () => {
    if (chart) {
        chart.destroy();
    }
    if (!container.value) return;

    const config = {
        data: props.data,
        isGroup: true,
        xField: props.yField, // Swap x và y để thành biểu đồ cột ngang (dễ đọc hơn)
        yField: props.xField,
        seriesField: props.groupField,
        marginRatio: 0.1,
        label: {
          position: 'middle',
          layout: [{ type: 'interval-adjust-position' }],
        },
    };

    // SỬA ĐỔI: Áp dụng màu sắc tùy chỉnh nếu được cung cấp
    if (props.color) {
        config.color = props.color;
    }

    chart = new Bar(container.value, config);
    chart.render();
}

onMounted(() => {
    renderChart();
});

watch(() => [props.data, props.color], () => {
    renderChart();
}, { deep: true });

onUnmounted(() => {
    if (chart) {
        chart.destroy();
    }
});
</script>