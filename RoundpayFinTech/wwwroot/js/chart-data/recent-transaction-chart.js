var recentDaysTransactionChart = (id, chartType, chartTheme, chartTitleText, dataObject, isAmountType) => {
    chartTitleText = chartTitleText || "Last 7 Days Transaction";
    dataObject = dataObject || { ServiceTypeID: 0 };
    recentDaysTransaction(dataObject, chartTitleText, id, chartType, chartTheme, isAmountType);
};

var recentDaysTransaction = (dataObject, chartTitleText, targetID, chartType, chartTheme, isAmountType) => {
    preloader.load();
    $.post('/recent-days-transactions', dataObject)
        .done(result => {
            if (result != "") {
                if (result != undefined && result != '') {
                    if (result.length > 0) {
                        $("#" + targetID).parent().find(".lmtdAmt").html("<i class='fa fa-rupee-sign'></i>" + result[0]._AmountLMTD);
                        $("#" + targetID).parent().find(".mtdAmt").html("<i class='fa fa-rupee-sign'></i>" + result[0]._AmountTillDate);
                        $("#" + targetID).parent().find(".todayAmt").html("<i class='fa fa-rupee-sign'></i>" + result[0]._AmountCurrentDay);

                        $("#" + targetID).parent().find(".lmtdCount").html(result[0].lmtdCount);
                        $("#" + targetID).parent().find(".mtdCount").html(result[0].tillDateCount);
                        $("#" + targetID).parent().find(".todayCount").html(result[0]._CurrentDayCount);
                    }

                    let arrData = new Array();
                    result.forEach((item, index) => {
                        arrData.push({
                            date: item.entryDate,
                            value: item.valueFloat
                        });
                    })

                    let amChart = new amChartUtility().amChartUtility;
                    amChart.renderChart(arrData, chartTitleText, targetID, chartTheme, chartType, isAmountType);
                }
            }
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status == 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        })
        .always(() => preloader.remove());
}

//API User
var recentDaysTransactionChart_APIUser = (id, chartType, chartTheme, chartTitleText, dataObject, isAmountType) => {
    chartTitleText = chartTitleText || "Last 7 Days Transaction";
    dataObject = dataObject || { ServiceTypeID: 0 };
    recentDaysTransaction_APIUser(dataObject, chartTitleText, id, chartType, chartTheme, isAmountType);
};

var recentDaysTransaction_APIUser = (dataObject, chartTitleText, targetID, chartType, chartTheme, isAmountType) => {
    preloader.load();
    $.post('/recent-days-transactions', dataObject)
        .done(result => {
            if (result != "") {
                if (result != undefined && result != '') {
                    if (result.length > 0) {
                        
                        $("#" + targetID).parent().find(".lmtdAmt").html("<i class='fa fa-rupee-sign'></i>" + result[0]._AmountLMTD);
                        $("#" + targetID).parent().find(".lastAmt").html("<i class='fa fa-rupee-sign'></i>" + result[0]._AmountLastDay);

                        let lmtdMtdDiff = (isNaN(result[0].lmtD_MTD_Diff) ? 0 : result[0].lmtD_MTD_Diff);
                        let lastDayTodayDiff = (isNaN(result[0].lastDay_Current_Diff) ? 0 : result[0].lastDay_Current_Diff);

                        if (lmtdMtdDiff > 0) {
                            $("#" + targetID).parent().find(".mtdAmt").html("<i class='fa fa-rupee-sign'></i>" + result[0]._AmountTillDate + " <i class='fa fa-caret-up text-success'></i>");
                        }
                        else if (lmtdMtdDiff < 0) {
                            $("#" + targetID).parent().find(".mtdAmt").html("<i class='fa fa-rupee-sign'></i>" + result[0]._AmountTillDate + " <i class='fa fa-caret-down text-danger'></i>");
                        }
                        else {
                            $("#" + targetID).parent().find(".mtdAmt").html("<i class='fa fa-rupee-sign'></i>" + result[0]._AmountTillDate);
                        }

                        if (lastDayTodayDiff > 0) {
                            $("#" + targetID).parent().find(".todayAmt").html("<i class='fa fa-rupee-sign'></i>" + result[0]._AmountCurrentDay + " <i class='fa fa-caret-up text-success'></i>");
                        }
                        else if (lastDayTodayDiff < 0) {
                            $("#" + targetID).parent().find(".todayAmt").html("<i class='fa fa-rupee-sign'></i>" + result[0]._AmountCurrentDay + " <i class='fa fa-caret-down text-danger'></i>");
                        }
                        else {
                            $("#" + targetID).parent().find(".todayAmt").html("<i class='fa fa-rupee-sign'></i>" + result[0]._AmountCurrentDay);
                        }

                        $("#" + targetID).parent().find(".lmtdCount").html(result[0].lmtdCount);
                        $("#" + targetID).parent().find(".mtdCount").html(result[0].tillDateCount);
                        $("#" + targetID).parent().find(".lastCount").html(result[0]._LastDayCount);
                        $("#" + targetID).parent().find(".todayCount").html(result[0]._CurrentDayCount);
                    }

                    let arrData = new Array();
                    result.forEach((item, index) => {
                        arrData.push({
                            date: item.entryDate,
                            value: item.valueFloat
                        });
                    })

                    let amChart = new amChartUtility().amChartUtility;
                    amChart.renderChart(arrData, chartTitleText, targetID, chartTheme, chartType, isAmountType);
                }
            }
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status == 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        })
        .always(() => preloader.remove());
}

var monthlyWiseTransactionChart = (dataObject, chartTitleText, targetID, chartType, chartTheme, isAmountType) => {
    preloader.load();
    $.post('/month-week-day-transactions', dataObject)
        .done(result => {
            if (result != "") {
                if (result != undefined && result != '') {
                    let arrData = new Array();
                    result.forEach((item, index) => {
                        arrData.push({
                            date: item.entryDate,
                            value: item.valueFloat
                        });
                    })

                    let amChart = new amChartUtility().amChartUtility;
                    amChart.renderChart(arrData, chartTitleText, targetID, chartTheme, chartType, isAmountType);
                }
            }
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status == 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        })
        .always(() => preloader.remove());
}

var todaySplitTransactionChart = (dataObject, chartTitleText, targetID, chartType, chartTheme, isAmountType) => {
    preloader.load();
    $.post('/today-transactions-status', dataObject)
        .done(result => {
            if (result != "") {
                if (result != undefined && result != '') {
                    let arrData = new Array();
                    result.forEach((item, index) => {
                        arrData.push({
                            date: item.status,
                            value: item.valueFloat,
                            color: ""
                        });
                    })

                    let amChart = new amChartUtility().amChartUtility;
                    amChart.renderChart(arrData, chartTitleText, targetID, chartTheme, chartType, isAmountType);
                }
            }
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status == 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        })
        .always(() => preloader.remove());
}

var serviceWiseTransactionStatusChart = (dataObject, chartTitleText, targetID, chartType, chartTheme, isAmountType) => {
    preloader.load();
    $.post('/date-optype-transactions-status', dataObject)
        .done(result => {
            if (result.length == 0) {
                result.push({ status: "No Record", valueFloat: 0.0 });
            }

            if (result != "") {
                if (result != undefined && result != '') {
                    let arrData = new Array();
                    result.forEach((item, index) => {
                        arrData.push({
                            date: item.status,
                            value: item.valueFloat,
                            color: ""
                        });
                    })
                    
                    let amChart = new amChartUtility().amChartUtility;
                    amChart.renderChart(arrData, chartTitleText, targetID, chartTheme, chartType, isAmountType);
                }
            }
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status == 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        })
        .always(() => preloader.remove());
}
//ENd API User

var recentDaysPriSecTerTransaction = (id, chartType, chartTheme, chartTitleText, dataObject, isAmountType) => {
    chartTitleText = chartTitleText || "Last 7 Days ";
    dataObject = dataObject || { PriSecTerType: 0 };
    recentDaysPriSecTer(dataObject, chartTitleText, id, chartType, chartTheme, isAmountType);
};

var recentDaysPriSecTer = (dataObject, chartTitleText, targetID, chartType, chartTheme, isAmountType) => {
    preloader.load();
    $.post('/recent-days-prisecter', dataObject)
        .done(result => {
            if (result != "") {
                if (result != undefined && result != '') {
                    let arrData = new Array();
                    result.forEach((item, index) => {
                        arrData.push({
                            date: item.entryDate,
                            value: item.valueFloat
                        });
                    })
                    let amChart = new amChartUtility().amChartUtility;
                    amChart.renderChart(arrData, chartTitleText, targetID, chartTheme, chartType, isAmountType);
                }
            }
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status == 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        })
        .always(() => preloader.remove());
}