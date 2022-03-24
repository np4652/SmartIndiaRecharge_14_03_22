function amChartUtility() {
    this.amChartUtility = {
        chartType: { line: "line-chart", bar: "bar-chart", zoomableAxis: "zoomable_axis_chart", lineWithPoint: "line-point", pieChartSlice: "pie-chart-slice",pieChartSliceLegend:"pie-chart-slice-legend" },
        chartTheme: { none: "", dataviz: am4themes_dataviz, frozen: am4themes_frozen, moonrisekingdom: am4themes_moonrisekingdom },
        renderChart: (arrData, titleText, id, chartTheme, chartType, isAmountType) => {
            isAmountType = isAmountType || false;
            switch (chartType) {
                case this.amChartUtility.chartType.line:
                    renderLineChart(arrData, titleText, id, chartTheme, isAmountType);
                    break;
                case this.amChartUtility.chartType.bar:
                    renderBarChart(arrData, titleText, id, chartTheme, isAmountType);
                    break;
                case this.amChartUtility.chartType.zoomableAxis:
                    renderZoomableAxisChart(arrData, titleText, id, chartTheme, isAmountType);
                    break;
                case this.amChartUtility.chartType.lineWithPoint:
                    renderLineWithPointChart(arrData, titleText, id, chartTheme, isAmountType);
                    break;
                case this.amChartUtility.chartType.pieChartSlice:
                    renderPieWithSliceChart(arrData, titleText, id, chartTheme, isAmountType);
                    break;
                case this.amChartUtility.chartType.pieChartSliceLegend:
                    renderPieWithSliceLegendChart(arrData, titleText, id, chartTheme, isAmountType);
                    break;
                default:
                    renderLineChart(arrData, titleText, id, chartTheme, isAmountType);
            }
        }
    }

    function renderLineChart(arrData, titleText, id, chartTheme, isAmountType) {
        //am4core.options.onlyShowOnViewport = true;
        am4core.ready(function () {
            am4core.unuseAllThemes(chartTheme);
            if (chartTheme != "") {
                am4core.useTheme(chartTheme); //am4themes_dataviz
            }
            var chart = am4core.create(id, am4charts.XYChart);
            if (isAmountType) {
                chart.numberFormatter.numberFormat = "'₹'#.00";
            }
            chart.data = arrData;
            var title = chart.titles.create();
            title.text = titleText;
            var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            // Create series
            var series = chart.series.push(new am4charts.LineSeries());
            series.dataFields.valueY = "value";
            series.dataFields.dateX = "date";
            series.tooltipText = "{value}"
            series.strokeWidth = 2;
            series.minBulletDistance = 15;
            series.tooltip.background.cornerRadius = 20;
            series.tooltip.background.strokeOpacity = 0;
            series.tooltip.pointerOrientation = "vertical";
            series.tooltip.label.minWidth = 40;
            series.tooltip.label.minHeight = 40;
            series.tooltip.label.textAlign = "middle";
            series.tooltip.label.textValign = "middle";
            // Make bullets grow on hover
            var bullet = series.bullets.push(new am4charts.CircleBullet());
            bullet.circle.strokeWidth = 2;
            bullet.circle.radius = 4;
            bullet.circle.fill = am4core.color("#fff");
            var bullethover = bullet.states.create("hover");
            bullethover.properties.scale = 1.3;

            // Make a panning cursor
            chart.cursor = new am4charts.XYCursor();
            chart.cursor.behavior = "panXY";
            chart.cursor.snapToSeries = series;
        });
    }
    function renderBarChart(arrData, titleText, id, chartTheme, isAmountType) {
        //am4core.options.onlyShowOnViewport = true;
        am4core.ready(function () {
            am4core.unuseAllThemes(chartTheme);
            if (chartTheme != "") {
                am4core.useTheme(chartTheme); //am4themes_dataviz
            }
            // Create chart instance
            var chart = am4core.create(id, am4charts.XYChart);
            var title = chart.titles.create();
            title.text = titleText;
            if (isAmountType) {
                chart.numberFormatter.numberFormat = "'₹'#.00";
            }
            chart.data = arrData;
            // Create axes
            var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
            categoryAxis.dataFields.category = "date";
            categoryAxis.renderer.grid.template.location = 0;
            categoryAxis.renderer.minGridDistance = 30;
            categoryAxis.renderer.labels.template.horizontalCenter = "right";
            categoryAxis.renderer.labels.template.verticalCenter = "middle";
            categoryAxis.renderer.labels.template.rotation = 270;
            categoryAxis.tooltip.disabled = true;
            categoryAxis.renderer.minHeight = 0;

            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.renderer.minWidth = 50;

            // Create series
            var series = chart.series.push(new am4charts.ColumnSeries());
            series.sequencedInterpolation = true;
            series.dataFields.valueY = "value";
            series.dataFields.categoryX = "date";
            series.tooltipText = "[{categoryX}: bold]{valueY}[/]";
            series.columns.template.strokeWidth = 0;

            series.tooltip.pointerOrientation = "vertical";

            series.columns.template.column.cornerRadiusTopLeft = 10;
            series.columns.template.column.cornerRadiusTopRight = 10;
            series.columns.template.column.fillOpacity = 0.8;

            // on hover, make corner radiuses bigger
            var hoverState = series.columns.template.column.states.create("hover");
            hoverState.properties.cornerRadiusTopLeft = 0;
            hoverState.properties.cornerRadiusTopRight = 0;
            hoverState.properties.fillOpacity = 1;

            series.columns.template.adapter.add("fill", function (fill, target) {
                return chart.colors.getIndex(target.dataItem.index);
            });
            chart.cursor = new am4charts.XYCursor();
        });
    }
    function renderZoomableAxisChart(arrData, titleText, id, chartTheme, isAmountType) {
        //am4core.options.onlyShowOnViewport = true;
        am4core.createDeferred(function (div) {
            am4core.unuseAllThemes(chartTheme);
            if (chartTheme != "") {
                am4core.useTheme(chartTheme); //am4themes_dataviz
            }
            //am4core.useTheme(am4themes_animated);
            // Create chart instance
            var chart = am4core.create(id, am4charts.XYChart);
            var title = chart.titles.create();
            title.text = titleText;
            if (isAmountType) {
                chart.numberFormatter.numberFormat = "'₹'#.00";
            }
            chart.data = arrData

            // Create axes
            var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            dateAxis.renderer.grid.template.location = 0;
            dateAxis.renderer.minGridDistance = 50;

            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            var series = chart.series.push(new am4charts.LineSeries());
            series.dataFields.valueY = "value";
            series.dataFields.dateX = "date";
            series.strokeWidth = 3;
            series.fillOpacity = 0.5;

            // Add vertical scrollbar
            //chart.scrollbarY = new am4core.Scrollbar();
            //chart.scrollbarY.marginLeft = 0;

            // Add cursor
            chart.cursor = new am4charts.XYCursor();
            chart.cursor.behavior = "zoomY";
            chart.cursor.lineX.disabled = true;
            return chart;
        }, arrData, titleText, id, chartTheme)
    }
    function renderLineWithPointChart(arrData, titleText, id, chartTheme, isAmountType) {
        debugger
        //am4core.options.onlyShowOnViewport = true;
        am4core.createDeferred(function (div) {
            am4core.unuseAllThemes(chartTheme);
            if (chartTheme != "") {
                am4core.useTheme(chartTheme); //am4themes_dataviz
            }
            //am4core.useTheme(am4themes_animated);
            // Create chart instance
            var chart = am4core.create(id, am4charts.XYChart);
            var title = chart.titles.create();
            title.text = titleText;

            if (isAmountType) {
                chart.numberFormatter.numberFormat = "'₹'#.00";
            }

            chart.data = arrData;
            var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
            categoryAxis.renderer.grid.template.location = 0;
            categoryAxis.renderer.ticks.template.disabled = true;
            categoryAxis.renderer.line.opacity = 0;
            categoryAxis.renderer.grid.template.disabled = true;
            //categoryAxis.renderer.minGridDistance = 40;
            categoryAxis.dataFields.category = "date";
            categoryAxis.startLocation = 0.4;
            categoryAxis.endLocation = 0.6;


            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.tooltip.disabled = true;
            valueAxis.renderer.line.opacity = 0;
            valueAxis.renderer.ticks.template.disabled = true;
            valueAxis.min = 0;

            var lineSeries = chart.series.push(new am4charts.LineSeries());
            lineSeries.dataFields.categoryX = "date";
            lineSeries.dataFields.valueY = "value";
            lineSeries.tooltipText = "{valueY.value}";
            lineSeries.fillOpacity = 0.5;
            lineSeries.strokeWidth = 3;
            lineSeries.propertyFields.stroke = "lineColor";
            lineSeries.propertyFields.fill = "lineColor";

            var bullet = lineSeries.bullets.push(new am4charts.CircleBullet());
            bullet.circle.radius = 6;
            bullet.circle.fill = am4core.color("#fff");
            bullet.circle.strokeWidth = 3;

            chart.cursor = new am4charts.XYCursor();
            chart.cursor.behavior = "panX";
            chart.cursor.lineX.opacity = 0;
            chart.cursor.lineY.opacity = 0;

            chart.scrollbarX = new am4core.Scrollbar();
            chart.scrollbarX.parent = chart.bottomAxesContainer;

            return chart;
        }, arrData, titleText, id, chartTheme)
    }
    function renderPieWithSliceChart(arrData, titleText, id, chartTheme, isAmountType) {
        am4core.ready(function () {
            am4core.unuseAllThemes(chartTheme);
            if (chartTheme != "") {
                am4core.useTheme(chartTheme); //am4themes_dataviz
            }

            // Create chart instance
            var chart = am4core.create(id, am4charts.PieChart);
            let isColorFound = false;
            if (arrData.length > 0) {
                for (var i = 0; i < arrData.length; i++) {
                    if (arrData[i].color != undefined) {
                        if (arrData[i].date.toString().toLowerCase() == "success") {
                            arrData[i].color = am4core.color("#10ac84");
                        }
                        else if (arrData[i].date.toString().toLowerCase() == "failed") {
                            arrData[i].color = am4core.color("#ee5253");
                        }
                        else if (arrData[i].date.toString().toLowerCase() == "pending") {
                            arrData[i].color = am4core.color("#feca57");
                        }

                        isColorFound = true;
                    }
                }
            }

            if (isAmountType) {
                chart.numberFormatter.numberFormat = "'₹'#.00";
            }
            // Add data
            chart.data = arrData;

            // Add and configure Series
            var pieSeries = chart.series.push(new am4charts.PieSeries());
            pieSeries.dataFields.value = "value";
            pieSeries.dataFields.category = "date";
            pieSeries.slices.template.stroke = am4core.color("#fff");
            pieSeries.slices.template.strokeWidth = 2;
            pieSeries.slices.template.strokeOpacity = 1;
            if (isColorFound) {
                pieSeries.slices.template.propertyFields.fill = "color";
            }

            // This creates initial animation
            pieSeries.hiddenState.properties.opacity = 1;
            pieSeries.hiddenState.properties.endAngle = -90;
            pieSeries.hiddenState.properties.startAngle = -90;

        });
    }
    function renderPieWithSliceLegendChart(arrData, titleText, id, chartTheme, isAmountType) {
        am4core.ready(function () {
            am4core.unuseAllThemes(chartTheme);
            if (chartTheme != "") {
                am4core.useTheme(chartTheme); //am4themes_dataviz
            }

            // Create chart instance
            var chart = am4core.create(id, am4charts.PieChart);

            chart.legend = new am4charts.Legend();
            chart.legend.position = "right";

            if (isAmountType) {
                chart.numberFormatter.numberFormat = "'₹'#.00";
            }
            // Add data
            chart.data = arrData;

            // Add and configure Series
            var pieSeries = chart.series.push(new am4charts.PieSeries());
            pieSeries.dataFields.value = "value";
            pieSeries.dataFields.category = "date";
            pieSeries.slices.template.stroke = am4core.color("#fff");
            pieSeries.slices.template.strokeWidth = 2;
            pieSeries.slices.template.strokeOpacity = 1;
            pieSeries.slices.template
                // change the cursor on hover to make it apparent the object can be interacted with
                .cursorOverStyle = [
                    {
                        "property": "cursor",
                        "value": "pointer"
                    }
                ];

            // This creates initial animation
            pieSeries.hiddenState.properties.opacity = 1;
            pieSeries.hiddenState.properties.endAngle = -90;
            pieSeries.hiddenState.properties.startAngle = -90;
        });
    }
};