<template>
  <div ref="container"  style="max-height: 200px;"></div>
</template>

<script setup>
import { ref, onMounted, watch, onUnmounted } from 'vue';
import { Pie } from '@antv/g2plot';

const props = defineProps({
  data: Array,
  angleField: String,
  colorField: String,
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
    angleField: props.angleField,
    colorField: props.colorField,
    radius: 0.75,
    innerRadius: 0,
    
    label: {
      type: 'outer',
      content: (data) => `${data.type}: ${(data.percent * 100).toFixed(2)}%`,
      layout: {
        type: 'pie-spider'
      },
      style: {
        fontSize: 12,
      },
    },
    legend: {
      position: 'top',
    },
    statistic: null,
    interactions: [{ type: 'element-selected' }, { type: 'element-active' }],
  };

  if (props.color) {
    config.color = props.color;
  }

  chart = new Pie(container.value, config);
  chart.render();
};

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