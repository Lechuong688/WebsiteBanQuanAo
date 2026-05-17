
var dashboardStartDate;
var dashboardEndDate;
function updateChartModeOptions(days) {

    var html = '';

    if (days <= 7) {

        html += '<option value=\"day\">Ngày</option>';
        html += '<option value=\"week\">Tuần</option>';

    }

    else if (days <= 31) {

        html += '<option value=\"day\">Ngày</option>';
        html += '<option value=\"week\">Tuần</option>';
        html += '<option value=\"month\">Tháng</option>';

    }

    else if (days <= 90) {

        html += '<option value=\"day\">Ngày</option>';
        html += '<option value=\"week\">Tuần</option>';
        html += '<option value=\"month\">Tháng</option>';
        html += '<option value=\"quarter\">Quý</option>';

    }

    else {

        html += '<option value=\"day\">Ngày</option>';
        html += '<option value=\"week\">Tuần</option>';
        html += '<option value=\"month\">Tháng</option>';
        html += '<option value=\"quarter\">Quý</option>';
        html += '<option value=\"year\">Năm</option>';

    }

    $('#chart-mode').html(html);

}
function drawDoanhThuTheoThang() {

    var mode = $('#chart-mode').val();

    fetch('/Admin/Dashboard/GetRevenueChart?startDate='
        + dashboardStartDate
        + '&endDate='
        + dashboardEndDate
        + '&mode='
        + mode)

        .then(res => res.json())

        .then(data => {

            $('#revenue-chart').html('');

            console.log(data);

            var chartData = [];

            data.forEach(x => {

                chartData.push({
                    y: x.label,
                    item1: parseFloat(x.revenue)
                });

            });

            Morris.Area({
                element: 'revenue-chart',
                resize: true,
                data: chartData,
                xkey: 'y',
                ykeys: ['item1'],
                labels: ['Doanh thu'],
                lineColors: ['#1f3d99'],
                hideHover: 'auto',
                parseTime: false
            });

        });

}
function drawOrderStatusChart() {

    fetch('/Admin/Dashboard/GetOrderStatusChart?startDate='
        + dashboardStartDate
        + '&endDate='
        + dashboardEndDate)

        .then(res => res.json())

        .then(data => {

            var labels = data.map(x => x.label);

            var values = data.map(x => x.value);

            var type =
                $('#order-status-type').val();

            $('#order-status-chart').empty();

            if (type == 'bar') {

                Morris.Bar({

                    element: 'order-status-chart',

                    data: data.map(x => ({
                        label: x.label,
                        value: x.value
                    })),

                    xkey: 'label',

                    ykeys: ['value'],

                    labels: ['Số lượng'],

                    resize: true,

                    barColors: [

                        '#3c8dbc', //Đã xác nhận

                        '#f39c12', //Đang giao

                        '#00a65a', //Hoàn thành

                        '#dd4b39', //Đã hủy

                        '#605ca8' //Đơn mới

                    ],

                });

            }
            else {

                Morris.Donut({

                    element: 'order-status-chart',

                    data: data.map(x => ({
                        label: x.label,
                        value: x.value
                    })),

                    resize: true,

                    colors: [

                        '#3c8dbc', //Đã xác nhận

                        '#f39c12', //Đang giao

                        '#00a65a', //Hoàn thành

                        '#dd4b39', //Đã hủy

                        '#605ca8' //Đơn mới

                    ]

                });

            }

        });

}
function drawTopProducts() {

    var top = $('#top-product-limit').val();

    fetch('/Admin/Dashboard/GetTopProducts?startDate='
        + dashboardStartDate
        + '&endDate='
        + dashboardEndDate
        + '&top='
        + top)

        .then(res => res.json())

        .then(data => {

            var html = '';

            data.forEach(x => {

                html += `
                    <tr>

                        <td style="width:70px">

                            <img src="${x.imagePath}"
                                 style="
                                    width:50px;
                                    height:50px;
                                    object-fit:cover;
                                    border-radius:8px">

                        </td>

                        <td>${x.productName}</td>

                        <td>${x.quantitySold}</td>

                        <td>
                            ${parseFloat(x.revenue)
                                .toLocaleString()} đ
                        </td>

                    </tr >
    `;

            });

            $('#top-product-body').html(html);

        });

}

function drawRecentOrders() {

    var top = $('#recent-order-limit').val();

    fetch('/Admin/Dashboard/GetRecentOrders?startDate='
        + dashboardStartDate
        + '&endDate='
        + dashboardEndDate
        + '&top='
        + top)

        .then(res => res.json())

        .then(data => {

            var html = '';

            data.forEach(x => {

                let color = '#605ca8';

                if (x.status == 'Đơn mới')
                    color = '#605ca8';

                else if (x.status == 'Đã xác nhận')
                    color = '#3c8dbc';

                else if (x.status == 'Đang giao')
                    color = '#f39c12';

                else if (x.status == 'Hoàn thành')
                    color = '#00a65a';

                else if (x.status == 'Đã huỷ')
                    color = '#dd4b39';

                html += `
                    <tr>

                        <td>${x.transactionCode}</td>

                        <td>${x.customerName}</td>

                        <td>
                            ${parseFloat(x.totalAmount)
                        .toLocaleString()} đ
                        </td>

                        <td>

                            <span class="label"
                                  style="
                                    background:${color};
                                    color:white;
                                  ">

                                ${x.status}

                            </span>

                        </td>

                    </tr>
                `;

            });

            $('#recent-order-body').html(html);

        });

}

function drawLowStockProducts() {

    var quantity =
        $('#low-stock-limit').val();

    fetch('/Admin/Dashboard/GetLowStockProducts?quantity='
        + quantity)

        .then(res => res.json())

        .then(data => {

            var html = '';

            data.forEach(x => {

                html += `
                    <tr>

                        <td style="width:70px">

                            <img src="${x.imagePath}"
                                 style="
                                    width:50px;
                                    height:50px;
                                    object-fit:cover;
                                    border-radius:8px">

                        </td>

                        <td>${x.productName}</td>

                        <td>

                            <span class="label label-danger">
                                ${x.quantity}
                            </span>

                        </td>

                    </tr>
                `;

            });

            $('#low-stock-body').html(html);

        });

}
$('#top-product-limit').change(function () {

    drawTopProducts();

});

$('#recent-order-limit').change(function () {

    drawRecentOrders();

});

$('#low-stock-limit').change(function () {

    drawLowStockProducts();

});

$('#order-status-type').change(function () {

    drawOrderStatusChart();

});
function reloadDashboard() {

    drawDoanhThuTheoThang();

    drawOrderStatusChart();

    drawTopProducts();

    drawRecentOrders();

    drawLowStockProducts();

}

$(document).ready(function () {

  'use strict';

  // Make the dashboard widgets sortable Using jquery UI
  $('.connectedSortable').sortable({
    placeholder         : 'sort-highlight',
    connectWith         : '.connectedSortable',
    handle              : '.box-header, .nav-tabs',
    forcePlaceholderSize: true,
    zIndex              : 999999
  });
  $('.connectedSortable .box-header, .connectedSortable .nav-tabs-custom').css('cursor', 'move');

  // jQuery UI sortable for the todo list
  $('.todo-list').sortable({
    placeholder         : 'sort-highlight',
    handle              : '.handle',
    forcePlaceholderSize: true,
    zIndex              : 999999
  });

  // bootstrap WYSIHTML5 - text editor
    $('.textarea').wysihtml5();

    dashboardStartDate = moment()
        .startOf('year')
        .format('YYYY-MM-DD');

    dashboardEndDate = moment()
        .endOf('year')
        .format('YYYY-MM-DD');

    updateChartModeOptions(365);

    $('#chart-mode').val('month');

    reloadDashboard();

    $('#chart-mode').change(function () {
        drawDoanhThuTheoThang();
    });
    //Lọc tổng
    $('#dashboard-date-range').daterangepicker({
        ranges: {
            'Today': [moment(), moment()],
            'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
            'Last 7 Days': [moment().subtract(6, 'days'), moment()],
            'Last 30 Days': [moment().subtract(29, 'days'), moment()],
            'This Month': [moment().startOf('month'), moment().endOf('month')],
            'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
            'This Year': [moment().startOf('year'), moment().endOf('year')],
            'Last Year': [moment().subtract(1, 'year').startOf('year'), moment().subtract(1, 'year').endOf('year')]
        },
        startDate: moment().startOf('year'),
        endDate: moment().endOf('year')
    }, function (start, end) {
        dashboardStartDate = start.format('YYYY-MM-DD');
        dashboardEndDate = end.format('YYYY-MM-DD');
        var days = end.diff(start, 'days') + 1;

        updateChartModeOptions(days);

        reloadDashboard();
        //drawDoanhThuTheoThang()
        //window.alert('You chose: ' + start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
    });

});
