<template>
  <div ref="container" style="max-height: 200px;"></div>
</template>

<script setup>
import { ref, onMounted, watch, onUnmounted } from 'vue';
import { Treemap } from '@antv/g2plot';

const props = defineProps({
  data: Array,
  colorField: String,
  valueField: String,
  color: [Array, Function],
  totalValue: {
    type: Number,
    required: false,
  }
});

const container = ref(null);
let chart = null;

const renderChart = () => {
    if (chart) {
        chart.destroy();
    }
    if (!container.value) return;

    chart = new Treemap(container.value, {
        data: {
            name: 'root',
            children: props.data,
        },
        colorField: props.colorField,
        valueField: props.valueField,
        color: props.color,
        
        label: {
            formatter: (d) => {
                if (d.value) {
                    return `${d.name}\n${d.value}`;
                }
                return '';
            },
            style: {
                fontSize: 14,
                fill: '#fff',
                textShadow: '1px 1px 2px black',
            },
        },

        tooltip: {
            formatter: (d) => ({ name: d.name, value: `Số lượng: ${d.value}` }),
        },
        interactions: [{ type: 'treemap-drill-down' }],
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