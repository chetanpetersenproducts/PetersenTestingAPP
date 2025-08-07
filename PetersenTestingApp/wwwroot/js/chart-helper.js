window.renderPressureChart = function (canvasId, chartData, chartOptions) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) {
        console.error(`Canvas with id '${canvasId}' not found.`);
        return;
    }

    // Destroy previous chart if it exists (optional but helpful)
    if (ctx.chartInstance) {
        ctx.chartInstance.destroy();
    }

    ctx.chartInstance = new Chart(ctx, {
        type: 'line',
        data: chartData,
        options: chartOptions
    });
};
