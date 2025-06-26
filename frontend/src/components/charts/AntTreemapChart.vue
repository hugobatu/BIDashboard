<!-- src/components/charts/AntTreemapChart.vue -->
<template>
  <div ref="container" :style="{ height: '350px' }"></div>
</template>

<script setup>
import { ref, onMounted, watch, onUnmounted } from 'vue';
import { Treemap } from '@antv/g2plot';

const props = defineProps({
  data: Array,
  colorField: String,
  valueField: String,
  color: [Array, Function],
  // SỬA ĐỔI 1: Thêm prop để nhận tổng giá trị
  totalValue: {
    type: Number,
    required: true,
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
        
        // SỬA ĐỔI 2: Cập nhật label để hiển thị %
        label: {
            formatter: (d) => {
                // Tính toán tỷ lệ phần trăm
                const percentage = props.totalValue > 0 ? (d.value / props.totalValue * 100).toFixed(2) : 0;
                // Chỉ hiển thị nhãn cho các ô đủ lớn
                if (d.value / props.totalValue > 0.05) { // Ngưỡng 5%
                    return `${d.name}\n${percentage}%`;
                }
                return ''; // Ẩn nhãn cho các ô quá nhỏ
            },
            style: {
                fontSize: 14,
                fill: '#fff',
                textShadow: '1px 1px 2px black', // Thêm viền đen cho chữ dễ đọc
            },
        },

        // SỬA ĐỔI 3: Giữ tooltip hiển thị số lượng tuyệt đối
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

// Thêm totalValue vào watch để vẽ lại khi tổng thay đổi
watch(() => [props.data, props.totalValue], () => {
  renderChart();
}, { deep: true });

onUnmounted(() => {
  if (chart) {
    chart.destroy();
  }
});
</script>