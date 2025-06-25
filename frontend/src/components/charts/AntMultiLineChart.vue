<!-- src/components/charts/AntMultiLineChart.vue -->
<template>
  <div ref="container" :style="{ height: '350px' }"></div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { Line } from '@antv/g2plot';

const props = defineProps({
  data: Array,
  xField: String,
  yField: String,
  seriesField: String,
});

const container = ref(null);
let chart = null;

onMounted(() => {
  chart = new Line(container.value, {
    data: props.data,
    xField: props.xField,
    yField: props.yField,
    seriesField: props.seriesField,
    smooth: false, // <-- THAY ĐỔI QUAN TRỌNG: Chuyển sang đường thẳng
    legend: { position: 'top' },
    // (Tùy chọn) Thêm điểm đánh dấu để rõ ràng hơn
    point: {
      size: 3,
      shape: 'circle',
    },
  });
  chart.render();
});

// Watcher không thay đổi
watch(() => props.data, (newData) => {
  chart?.changeData(newData);
});
</script>