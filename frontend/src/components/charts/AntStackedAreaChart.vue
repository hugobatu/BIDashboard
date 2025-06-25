<!-- src/components/charts/AntStackedAreaChart.vue -->
<template>
  <div ref="container" :style="{ height: '350px' }"></div>
</template>

<script setup>
import { ref, onMounted, watch, onUnmounted } from 'vue';
import { Area } from '@antv/g2plot'; // Sử dụng Area thay vì Line

const props = defineProps({
  data: Array,
  xField: String,
  yField: String,
  seriesField: String,
});

const container = ref(null);
let chart = null;

const renderChart = () => {
    if (chart) {
        chart.destroy();
    }
    if (!container.value) return;

    chart = new Area(container.value, {
        data: props.data,
        xField: props.xField,
        yField: props.yField,
        seriesField: props.seriesField,
        isStack: true,      // <-- SỬA ĐỔI QUAN TRỌNG: Bật tính năng chồng (stack)
        smooth: true,       // Đường miền thường đẹp hơn khi làm mượt
        legend: { position: 'top' },
        // Thêm màu sắc để dễ phân biệt hơn
        color: ['#1979C9', '#D62A0D', '#FAA219', '#30BF78', '#5D4037', '#7E57C2'],
    });
    chart.render();
}

onMounted(() => {
  renderChart();
});

// Watch để cập nhật biểu đồ khi dữ liệu thay đổi
watch(() => props.data, () => {
  renderChart();
}, { deep: true });

// Dọn dẹp để tránh rò rỉ bộ nhớ
onUnmounted(() => {
  if (chart) {
    chart.destroy();
  }
});
</script>