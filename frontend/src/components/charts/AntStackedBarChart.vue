<template>
  <div ref="container" style="max-height: 200px;"></div>
</template>

<script setup>
import { ref, onMounted, watch, onUnmounted } from 'vue';
import { Bar } from '@antv/g2plot';

const props = defineProps({
  data: Array,
  xField: String,
  yField: String,
  seriesField: String,
  color: [Array, Function],
});

const container = ref(null);
let chart = null;

const renderChart = () => {
    if (chart) {
        chart.destroy();
    }
    if (!container.value) return;

    chart = new Bar(container.value, {
        data: props.data,
        isStack: true,
        xField: props.yField,
        yField: props.xField,
        seriesField: props.seriesField,
        color: props.color,
        label: {
          position: 'middle',
          layout: [{ type: 'interval-adjust-position' }],
          style: {
            fill: '#fff',
            fontSize: 10,
            fontStyle: 400,
            textShadow: '1px 1px 2px black',
          },
          
        },
        legend: {
          position: 'right-top',
        },
    });
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