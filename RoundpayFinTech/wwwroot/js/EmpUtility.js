"use strict";
var Q;
(Q => {
    Q.loginInfo = () => {
        let cte = { LoginTypeID: 3 };
        $.ajax({
            type: 'POST',
            url: '/LoginInfo',
            dataType: 'json',
            contentType: 'application/json',
            data: JSON.stringify(cte),
            success: function (result) {
                if (result.name !== undefined) {
                    setInfo(result);
                    $('body').removeAttr('class');
                }
            },
            error: function (xhr, result) {
                an.title = "Oops! Error";
                an.content = xhr.status === 404 ? "Requested path not find" : (xhr.status === 0 ? "Internet is not connected" : "Server error");
                an.alert(an.type.failed);
                if (result === 'parsererror') {
                    reload();
                }
            }
        });
    };

    Q.ChangePass = (isMandate) => {
        preloader.load();
        $.post('/Employee/_ChangePassword', { IsMandate: isMandate })
            .done(result => {
                resultReload(result);
                mdlA.id = 'myalert';
                mdlA.content = result;
                mdlA.options.backdrop = 'static';
                mdlA.options.keyboard = !isMandate;
                mdlA.alert(mdlA.size.small);
                $('button.close span,#mdlCancel').click(() => mdlA.dispose());
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    };

    Q.GetComparision = () => {
        let UserID = $('#hfUserID').val();
        $.post('/Employee/ComparisionChart', { UserID: UserID })
            .done(result => {
                $('LMTDVsMTDGrid').empty().append('<table class="table table-hover">').find('table')
                    .append('<thead class="bg-tableth"><tr><th>Type</th><th>LM</th><th>LMTD</th><th>MTD</th><th>Growth</th></tr></thead>')
                    .append(result.map(e => `<tr><td>${e.type}</td><td>${e.lm}</td><td>${e.lmtd}</td><td>${e.mtd}</td><td>${e.growth}%</td></tr>`));
                let _Type = [], _lm = [], _lmtd = [], _mtd = [];
                _Type.push(result.map(e => e.type));
                _lm.push(result.map(e => e.lm));
                _lmtd.push(result.map(e => e.lmtd));
                _mtd.push(result.map(e => e.mtd));
                Q.ComparisionChart(_Type, _lm, _lmtd, _mtd);
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    };

    Q.TargetSegment = () => {
        $.post('/Employee/TargetSegment')
            .done(result => {
                $('#divTargetSegment').empty().append('<table class="table table-hover">').find('table')
                    .append('<thead class="bg-tableth"><tr><th>Type</th><th>Target</th><th>Achieve</th><th>Achieve(%)</th><th>Incentive</th></tr></thead>')
                    .append(result.map(e => `<tr><td>${e.type}</td><td>${e.target}</td><td>${e.achieve}</td><td>${Math.round(parseFloat(e.achievePercent))}</td><td>${e.strIncentive}</td></tr>`));
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    };

    Q.CommitmentSummary = () => {
        $.post('/Employee/GetCommitmentSummary')
            .done(result => {
                $('#divCommitmentSummary').empty().append(`<div class="text-monospace"><small><span><strong>Commitment : ${result.totalCommitment}/<strong></span> | <span><strong>Achieved : ${result.totalAchieved}<strong></span></small></div>`);
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    };

    Q.GetLastdayVsTodayData = () => {
        $.post('/Employee/LastdayVsTodayData')
            .done(result => Q.LastdayVsTodayChart(result))
            .fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    };

    Q.CommitmentSummarychart = () => {
        $.post('/Employee/CommitmentSummarychart').done(result => {
            let _data = [];
            let mlist = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

            let dt = new Date();
            let _todayDate = mlist[dt.getMonth()]
            for (let i = 0; i < result.length; i++) {
                let obj = {
                    name: result[i].service,
                    y: result[i].amount,
                    sliced: result[i].service === 'Acheived' ? true : false,
                    selected: result[i].service === 'Acheived' ? true : false
                }
                _data.push(obj)
            }
            Highcharts.chart('chartdiv', {
                chart: {
                    plotBackgroundColor: null,
                    plotBorderWidth: null,
                    plotShadow: false,
                    type: 'pie',
                    height: '300'
                },
                title: {
                    text: '' /*'Commitment Summary'   ,' + (dt.getDay()) + ' ' + _todayDate*/
                },
                tooltip: {
                    pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
                },
                accessibility: {
                    point: {
                        valueSuffix: '%'
                    }
                },
                plotOptions: {
                    pie: {
                        allowPointSelect: true,
                        cursor: 'pointer',
                        dataLabels: {
                            enabled: true,
                            format: '<b>{point.name}</b>: {point.percentage:.1f} %'
                        }
                    }
                },
                series: [{
                    name: 'Commitment Summery',
                    colorByPoint: true,
                    data: _data
                }]
            });
        }).fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        }).always(() => preloader.remove());
    };

    Q.ComparisionChart = (type, lm, lmtd, mtd) => {
        $('#divComparisionChart').highcharts({
            chart: {
                type: 'column',
                height: '300'
            },
            title: {
                text: null//'LMTD Vs MTD Report'
            },
            subtitle: {
                text: ''//'Source: WorldClimate.com'
            },
            xAxis: {
                categories: type[0],
                crosshair: true,
            },
            yAxis: {
                min: 0,
                title: {
                    text: 'type Wise'
                }
            },
            tooltip: {
                headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                    '<td style="padding:0"><b>{point.y:.1f} Rs</b></td></tr>',
                footerFormat: '</table>',
                shared: true,
                useHTML: true
            },
            plotOptions: {
                column: {
                    pointPadding: 0.2,
                    borderWidth: 0,
                }
            },
            series: [{
                name: 'LM',
                data: lm[0]
            },
            {
                name: 'LMTD',
                data: lmtd[0]
            },
            {
                name: 'MTD',
                data: mtd[0]
            }]
        });
    };

    Q.LastdayVsTodayChart = (result) => {
        let _type = [], lastdayData = [], todayData = [];
        _type = result.map(e => e.type);
        lastdayData = result.map(e => e.lastDay);
        todayData = result.map(e => e.today);

        $('#divLastdayVsTodayChart').highcharts({
            chart: {
                type: 'column',
                height: '300'
            },
            title: {
                text: null
            },
            subtitle: {
                text: null
            },
            xAxis: {
                categories: _type,
                crosshair: true
            },
            yAxis: {
                min: 0,
                title: {
                    text: 'type Wise'
                }
            },
            tooltip: {
                headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                    '<td style="padding:0"><b>{point.y:.1f} Rs</b></td></tr>',
                footerFormat: '</table>',
                shared: true,
                useHTML: true
            },
            plotOptions: {
                column: {
                    pointPadding: 0.1,
                    borderWidth: 0
                }
            },
            series: [{
                name: 'Lastday',
                data: lastdayData

            }, {
                name: 'Today',
                data: todayData
            }]
        });
    };

    Q.EmpDownlineUser = () => {
        $.post('/Employee/GetEmpDownlineUser')
            .done(result => {
                $('#divEmpDownlineUser').empty().append('<table class="table table-hover">').find('table')
                    .append('<thead class="bg-tableth"><tr><th>#</th><th>User</th><th>Mobile</th><th>Attandance</th></tr></thead>')
                    .append(result.map((e, i) => `<tr><td>${i + 1}</td><td>${e.userName}[${e.prefix + e.userID}]</td><td>${e.userMobile}</td><td ${e.attandance === true ? 'class="btn btn-sm btn-success"' : 'class="btn btn-sm btn-danger"'}>${e.attandance === true ? 'P' : 'A'}</td></tr>`));
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    };

    Q.LivePST = () => {
        $('toadyLivePST').append('<div class="secction-Overlay"><span><img src="/images/Curve-Loading.gif" style="width:325px"/></span></div>');
        $.post('/Employee/TodayLivePST')
            .done(result => {
                $('toadyLivePST').find('table').remove();
                $('toadyLivePST').append('<table class="table table-hover">').find('table')
                    .append('<thead class="bg-tableth"><tr><th>Type</th><th>TotalAmount</th><th>Total Transaction</th><th>Unique Transaction</th></tr></thead>')
                    .append(result.map(e => {
                        let _v = '';
                        switch (e.type) {
                            case 'Primary':
                                _v = '<a href="/FundReceiveStatement?IsSelf=0&RecBy=1" class="text-monospace btn btn-sm btn-outline-info float-right ml-1" target="_blank"><i class="fa fa-eye"></i> View</a><a class="btn btn-sm btn-outline-info float-right btnTodayActiveUser" data-type-Id="1"><i class="fas fa-users"></i></a>';
                                break;
                            case 'Secoundary':
                                _v = '<a href="/FundReceiveStatement?IsSelf=0" class="text-monospace btn btn-sm btn-outline-info float-right ml-1" target="_blank"><i class="fa fa-eye"></i> View</a><a class="btn btn-sm btn-outline-info float-right btnTodayActiveUser" data-type-Id="2"><i class="fas fa-users"></i></a>';
                                break;
                            case 'Tertiary':
                                _v = '<a href="/Home/RechargeReport" class="text-monospace btn btn-sm btn-outline-info float-right ml-1" target="_blank"><i class="fa fa-eye"></i> View</a><a class="btn btn-sm btn-outline-info float-right btnTodayActiveUser" data-type-Id="3"><i class="fas fa-users"></i></a>';
                                break;
                            case 'Package':
                                _v = '<a href="javascript:Q.TodaySellPackages()" class="text-monospace btn btn-sm btn-outline-info float-right"><i class="fa fa-eye"></i> View</a>';
                                break;
                            case 'Outlet':
                                _v = '<a href="javascript:Q.TodayOutletsListForEmp()" class="text-monospace btn btn-sm btn-outline-info float-right"><i class="fa fa-eye"></i> View</a>';
                                break;
                        }
                        return '<tr><td>' + e.type + '</td><td>' + e.totalAmount + '</td><td>' + e.totalUser + '</td><td>' + e.uniqueUser + ' <span class="ml-2">' + _v + '</span></td></tr>';
                    }));
                $('.secction-Overlay').remove();
                $('.btnTodayActiveUser').unbind().click(e => {
                    preloader.load();
                    let param = {
                        type: $(e.currentTarget).data().typeId
                    };
                    $.post('/TodayTransactors', param).done(result => {
                        mdlA.id = "todayTransactors";
                        mdlA.title = "Active Users(Today)";
                        mdlA.content = result;
                        mdlA.modal(mdlA.size.large);
                    }).always(() => preloader.remove())
                });
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    };

    Q.TodayOutletsListForEmp = () => {
        preloader.load();
        $.post('/Employee/TodayOutletsListForEmp')
            .done(result => {
                let _content = '<div class="col-sm-12"><h4 class="text-monospace text-danger">No Record Found</h4></div>';
                if (result.length > 0) {
                    let _table = '<table class="table table-hover"><thead class="bg-tableth"><tr><th>Name</th><th>OutletName</th><th>MobileNo</th><th>Role</th></tr></thead><tbody>';
                    let _tbody = result.map(e => {
                        return '<tr><td>' + e.name + '</td><td>' + e.outletName + '</td><td>' + e.mobileNo + '</td><td>' + e.role + '</td></tr>';
                    });
                    _content = _table + _tbody + '</tbody></table>';
                }
                mdlA.id = "userList";
                mdlA.title = "Today Joining";
                mdlA.content = _content;
                mdlA.modal(mdlA.size.large);
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    };

    Q.TodaySellPackages = () => {
        preloader.load();
        $.post('/Employee/TodaySellPackages')
            .done(result => {
                let _content = '<div class="col-sm-12"><h4 class="text-monospace text-danger">No Record Found</h4></div>';
                if (result.length > 0) {
                    let _table = '<table class="table table-hover"><thead class="bg-tableth"><tr><th>Name</th><th>OutletName</th><th>PackageName</th></tr></thead><tbody>';
                    let _tbody = result.map(e => `<tr><td> ${e.name}</td><td>$[e.outletNamet]</td><td>${e.packageName}</td></tr>`);
                    _content = _table + _tbody + '</tbody></table>';
                }
                mdlA.id = "userList";
                mdlA.title = "Today's Sold Packages";
                mdlA.content = _content;
                mdlA.modal(mdlA.size.large);
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    };

    Q.LastSevenDayPSTChart = () => {
        $.post('/Employee/GetLastSevenDayPST').done(result => {
            let _dates = result.map(r => r.transactionDate.substring(0, 7));
            //let _data = [
            //    {
            //        name: "Primary",
            //        data: result.map(r => r.primary)
            //    },

            //{
            //        name: "Tertiary",
            //        data: result.map(r => r.tertiary)
            //    },
            //        {
            //        name: "Tertiary",
            //        data: result.map(r => r.tertiary)
            //    }];
            //Highcharts.chart('LastSeveDayPST', {
            //    chart: {
            //        type: 'spline',
            //        height: '300'
            //        //type: 'area',
            //        //zoomType: 'x',
            //        //panning: true,
            //        //panKey: 'shift',
            //        //scrollablePlotArea: {
            //        //    minWidth: 600
            //        //}
            //    },
            //    title: {
            //        text: null
            //    },

            //    subtitle: {
            //        text: null
            //    },

            //    yAxis: {
            //        min: 0,
            //        max: 2500000,
            //        title: {
            //            text: 'Amount'
            //        }
            //    },

            //    xAxis: {
            //        categories: _dates
            //    },
            //    labels: {
            //        formatter: function () {
            //            return this.value + '';
            //        }
            //    },
            //    tooltip: {
            //        crosshairs: true,
            //        shared: true
            //    },
            //    legend: {
            //        layout: 'vertical',
            //        align: 'right',
            //        verticalAlign: 'middle'
            //    },

            //    plotOptions: {
            //        series: {
            //            label: {
            //                connectorAllowed: false
            //            },
            //            pointStart: 0
            //        }
            //    },

            //    series: _data,

            //    responsive: {
            //        rules: [{
            //            condition: {
            //                maxWidth: 500
            //            },
            //            chartOptions: {
            //                legend: {
            //                    layout: 'horizontal',
            //                    align: 'center',
            //                    verticalAlign: 'bottom'
            //                }
            //            }
            //        }]
            //    }
            //});
            let _dataPri = [
                {
                    name: "Primary",
                    data: result.map(r => r.primary)
                }];
            Highcharts.chart('LastSeveDayPrimary', {
                chart: {
                    type: 'spline',
                    height: '300'
                },
                title: {
                    text: null
                },

                subtitle: {
                    text: null
                },

                yAxis: {
                    min: 0,
                    max: 2500000,
                    title: {
                        text: 'Amount'
                    }
                },

                xAxis: {
                    categories: _dates
                },
                labels: {
                    formatter: function () {
                        return this.value + '';
                    }
                },
                tooltip: {
                    crosshairs: true,
                    shared: true
                },
                legend: {
                    layout: 'vertical',
                    align: 'right',
                    verticalAlign: 'middle'
                },

                plotOptions: {
                    series: {
                        label: {
                            connectorAllowed: false
                        },
                        pointStart: 0
                    }
                },

                series: _dataPri,

                responsive: {
                    rules: [{
                        condition: {
                            maxWidth: 500
                        },
                        chartOptions: {
                            legend: {
                                layout: 'horizontal',
                                align: 'center',
                                verticalAlign: 'bottom'
                            }
                        }
                    }]
                }
            });
            let _dataSec = [
                {
                    name: "Secondary",
                    data: result.map(r => r.secoundary)
                }];
            Highcharts.chart('LastSeveDaySecondary', {
                chart: {
                    type: 'spline',
                    height: '300'
                },
                title: {
                    text: null
                },

                subtitle: {
                    text: null
                },

                yAxis: {
                    min: 0,
                    max: 2500000,
                    title: {
                        text: 'Amount'
                    }
                },

                xAxis: {
                    categories: _dates
                },
                labels: {
                    formatter: function () {
                        return this.value + '';
                    }
                },
                tooltip: {
                    crosshairs: true,
                    shared: true
                },
                legend: {
                    layout: 'vertical',
                    align: 'right',
                    verticalAlign: 'middle'
                },

                plotOptions: {
                    series: {
                        label: {
                            connectorAllowed: false
                        },
                        pointStart: 0
                    }
                },

                series: _dataSec,

                responsive: {
                    rules: [{
                        condition: {
                            maxWidth: 500
                        },
                        chartOptions: {
                            legend: {
                                layout: 'horizontal',
                                align: 'center',
                                verticalAlign: 'bottom'
                            }
                        }
                    }]
                }
            });
            let _dataTer = [
                {
                    name: "Tertiary",
                    data: result.map(r => r.tertiary)
                }];
            Highcharts.chart('LastSeveDayTertiary', {
                chart: {
                    type: 'spline',
                    height: '300'
                },
                title: {
                    text: null
                },

                subtitle: {
                    text: null
                },

                yAxis: {
                    min: 0,
                    max: 2500000,
                    title: {
                        text: 'Amount'
                    }
                },

                xAxis: {
                    categories: _dates
                },
                labels: {
                    formatter: function () {
                        return this.value + '';
                    }
                },
                tooltip: {
                    crosshairs: true,
                    shared: true
                },
                legend: {
                    layout: 'vertical',
                    align: 'right',
                    verticalAlign: 'middle'
                },

                plotOptions: {
                    series: {
                        label: {
                            connectorAllowed: false
                        },
                        pointStart: 0
                    }
                },

                series: _dataTer,

                responsive: {
                    rules: [{
                        condition: {
                            maxWidth: 500
                        },
                        chartOptions: {
                            legend: {
                                layout: 'horizontal',
                                align: 'center',
                                verticalAlign: 'bottom'
                            }
                        }
                    }]
                }
            });
        }).fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        }).always(() => preloader.remove());
    };

    Q.followUpStats = () => {
        $.post('/Employee/LeadStats').done(result => {
            $('#linkCurrentTotal').attr('href', 'Employee/LeadDetail?status=All').find('h2').text(result.total);
            $('#linkCurrentJunk').attr('href', 'Employee/LeadDetail?status=junk').find('h2').text(result.junk);
            $('#linkCurrentFollowup').attr('href', 'Employee/LeadDetail?status=FollowUP').find('h2').text(result.followup);
            $('#linkCurrentMatured').attr('href', 'Employee/LeadDetail?status=Matured').find('h2').text(result.matured);
            $('#linkTodayFollowup').attr('href', 'Employee/LeadDetail?status=FollowUP&onlyTodayFollowup=true').find('h2').text(result.todayFollowup);
        }).fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        });
    };

})(Q || (Q = {}));



$('#changepass').unbind().click(() => Q.ChangePass(false));
$(document).ready(function () {
    Q.loginInfo();

    $('#btnCreateLeadWin').click(() => {
        $.post('/Employee/CreateLead').done(result => {
            mdlA.id = "createLeadWin";
            mdlA.title = "Create Lead";
            mdlA.content = result;
            mdlA.modal(mdlA.size.large);
        })
    })
});

var uploadImage = function () {
    alert('in');
    var Mobile = $('#txtmobileno').val();
    if (Mobile.length < 9) {
        an.title = 'Oops';
        an.content = 'Mobile Number field is mandatory';
        an.alert(-1);
    }
    else {
        preloader.load();
        var formData = new FormData();
        formData.append('file', $('#dimageUpload1')[0].files[0]);
        formData.append('Mobile', Mobile);
        // AJAX request
        $.ajax({
            url: '/Employee-upload-shopImage',
            type: 'post',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                //console.log(response);
                an.title = response.statuscode === 1 ? 'Success' : 'Oops';
                an.content = response.msg;
                an.alert(response.statuscode);
                preloader.remove();
            }
        });
    }
}