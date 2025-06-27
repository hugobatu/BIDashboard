<template>
  <div ref="container" style="max-height: 200px;"></div>
</template>

<script setup>
import { ref, onMounted, watch, onUnmounted } from 'vue';
import { Line } from '@antv/g2plot';

const props = defineProps({
  data: Array,
  xField: String,
  yField: String,
  seriesField: String,
  color: Array,
});

const container = ref(null);
let chart = null;

const renderChart = () => {
    if (chart) {
        chart.destroy();
    }
    if (!container.value) return;

    chart = new Line(container.value, {
        data: props.data,
        padding: 'auto',
        xField: props.xField,
        yField: props.yField,
        seriesField: props.seriesField,
        color: props.color,
        smooth: false,
        legend: { 
            position: 'top-right' 
        },
        yAxis: {
            label: {
                formatter: (v) => `${v}`.replace(/\d{1,3}(?=(\d{3})+$)/g, (s) => `${s},`),
            },
        },
        tooltip: {
            showMarkers: true,
            enterable: true,
            domStyles: {
                'g2-tooltip': {
                    boxShadow: '0px 0px 10px rgba(0, 0, 0, 0.1)',
                    borderRadius: '4px',
                },
            },
        },
        point: {
            size: 3,
            shape: 'circle',
        },
    });
    chart.render();
}

onMounted(() => {
    renderChart();
});

watch(() => props.data, () => {
    renderChart();
}, { deep: true });

onUnmounted(() => {
    if (chart) {
        chart.destroy();
    }
});
</script>