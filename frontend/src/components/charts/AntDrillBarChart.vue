<template>
    <div ref="mainContainer" style="position: relative; width: 100%; height: 100%;">
        <div ref="chartContainer" style="width: 100%; height: 100%; max-height: 200px;"></div>

        <div 
            v-if="tooltip.visible" 
            :style="tooltip.style"
            class="custom-tooltip"
        >
            <h4>{{ tooltip.title }} - Tổng: {{ tooltip.total }}</h4>
            <div :id="pieContainerId" style="height: 180px; width: 100%;">
                Đang tải...
            </div>
        </div>
    </div>
</template>

<script setup>
import { onMounted, onUnmounted, ref, watch, reactive, nextTick } from 'vue';
import { Column, Pie } from '@antv/g2plot';
import { fetchShiftPriorityDetails } from '@/services/api.js';

const priorityColorMap = {
    'Critical': '#FF4D4F',
    'High': '#FA8C16',
    'Moderate': '#FADB14',
    'Low': '#52C41A',
};

const props = defineProps({
    data: { type: Array, required: true },
    filters: { type: Object, required: true },
});

const tooltip = reactive({
    visible: false,
    title: '',
    total: 0,
    style: {},
});
const pieContainerId = 'pie-tooltip-container';

const mainContainer = ref(null);
const chartContainer = ref(null);
let columnPlot = null;
let piePlot = null;
const breakdownCache = ref({});

const showTooltip = (event) => {
    const { data } = event.data;
    if (!data) return;

    const { x, y } = event;
    const tooltipWidth = 260;
    const offset = 15;
    
    let leftPosition = x - tooltipWidth - offset;

    if (leftPosition < 0) {
        leftPosition = x + offset;
    }

    tooltip.visible = true;
    tooltip.title = data.type;
    tooltip.total = data.total_sales;
    tooltip.style = {
        position: 'absolute',
        top: `${y - 130}px`,
        left: `${leftPosition}px`,
        pointerEvents: 'none',
    };

    nextTick(() => {
        loadAndRenderPie(data.type);
    });
};

const hideTooltip = () => {
    tooltip.visible = false;
    if (piePlot) {
        piePlot.destroy();
        piePlot = null;
    }
};

const loadAndRenderPie = async (shiftName) => {
    const pieContainer = document.getElementById(pieContainerId);
    if (!pieContainer) return;
    let pieData = breakdownCache.value[shiftName];
    if (!pieData) {
        try {
            const details = await fetchShiftPriorityDetails(props.filters, shiftName);
            pieData = details.map(item => ({ name: item.priority, value: item.count }));
            breakdownCache.value[shiftName] = pieData;
        } catch (error) { console.error(`Lỗi khi lấy chi tiết cho ca ${shiftName}:`, error); pieContainer.innerText = 'Lỗi tải dữ liệu.'; return; }
    }
    if (!pieData || pieData.length === 0) { pieContainer.innerText = 'Không có dữ liệu chi tiết.'; return; }
    pieContainer.innerText = '';
    piePlot = new Pie(pieContainer, {
        data: pieData, appendPadding: 0, angleField: 'value', colorField: 'name', color: ({ name }) => priorityColorMap[name] || '#E8E8E8', radius: 0.8,
        legend: { position: 'right', layout: 'vertical', itemSpacing: 8, itemName: { style: { fontSize: 12 } }, itemValue: { formatter: () => '' } },
        label: { type: 'inner', offset: '-30%', content: ({ percent }) => `${(percent * 100).toFixed(0)}%`, style: { fontSize: 12, textAlign: 'center', fill: '#fff' } },
        tooltip: { formatter: (datum) => ({ name: datum.name, value: datum.value }) }, animation: false,
    });
    piePlot.render();
};

const renderChart = () => {
    if (columnPlot) columnPlot.destroy();
    if (!chartContainer.value || !props.data || props.data.length === 0) return;

    columnPlot = new Column(chartContainer.value, {
        data: props.data,
        xField: 'type',
        yField: 'total_sales',
        seriesField: 'type',
        legend: { position: 'top-left' },
        columnWidthRatio: 0.5,
        columnStyle: {
            radius: [4, 4, 0, 0],
        },
        tooltip: false,
    });

    columnPlot.on('element:mouseenter', showTooltip);
    columnPlot.on('element:mouseleave', hideTooltip);

    columnPlot.render();
};

onMounted(renderChart);

watch(() => props.data, () => {
    breakdownCache.value = {};
    hideTooltip();
    renderChart();
}, { deep: true });

onUnmounted(() => {
    if (columnPlot) {
        columnPlot.off('element:mouseenter', showTooltip);
        columnPlot.off('element:mouseleave', hideTooltip);
        columnPlot.destroy();
    }
    hideTooltip();
});
</script>

<style scoped>
.custom-tooltip {
    width: 260px;
    background-color: white;
    border-radius: 4px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    padding: 10px;
    transition: opacity 0.2s;
    opacity: 1;
}

.custom-tooltip h4 {
    margin: 0 0 10px 0;
    text-align: center;
    font-size: 14px;
    font-weight: 600;
}
</style>