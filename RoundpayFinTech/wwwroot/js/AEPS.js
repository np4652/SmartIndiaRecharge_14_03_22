var AllButtonDetail = { GButtonText: $('#btnCheckBalance').html(), GButton: $('#btnCheckBalance'), PAYButton: $('#btnPay'), GettingBalanceText: 'Getting Balance...', WithdrawingText: 'Withdrawing Cash...', DepositingText: 'Depositing Cash...', MinstatementText: 'Getting MiniStatement...', AadharPayText: 'Paying with Aadhar...', 1: 'CASH WITHDRAWL', 2: 'CASH DEPOSIT', 3: 'MINI STATEMENT', 4: 'AADHAR PAY' };
$(document).ready(function () {
    $('#ddl_Device').change(function () {
        localStorage.setItem('did', $('#ddl_Device').val());
        $('#imgDID').attr('src', $$host + '/Image/FPDevice/' + $('#ddl_Device').val() + '.png');
    });
    var did = 0;
    if (localStorage.did === undefined || localStorage.did === 'null') {
        localStorage.setItem('did', 1);
    }
    did = localStorage.did === '0' ? 1 : localStorage.did;
    $('#ddl_Device').val(did);
    $('#ddl_Device').change();

    ResetRecentBank();

    $('#recentBanks').removeAttr('data-recentbank');

    $('#btnAmount button').click(function () {
        $('#txtAmount').val($(this).val());
        $('#txtAmount').attr("step", $(this).val());
    });
    $('input[name="rdoGroupOp"]').change(function () {
        $('[for="txtAadhar"]').html("Aadhar");
        $('#txtAadhar').attr("placeholder", "Enter Aadhar");
        $('#txtAmount').val('');
        $('#btnAmount').parent().removeClass('d-none');
        $('#btnCheckBalance').removeClass('d-none');
        $('#imgDID').removeClass('d-none');
        $('#ddl_Device').removeClass('d-none');
        $('#btnCheckBalance').removeClass('d-none');
        let radioindex = $(this).attr('id').split('_')[1];
        var opDetail = $(this).data().op;
        if (opDetail !== undefined) {
            $('#txtAmount').attr('step', parseInt(opDetail.min));
            $('#txtAmount').attr('min', parseInt(opDetail.min));
            $('#txtAmount').attr('max', parseInt(opDetail.max));
            $('#txtAmount').attr('maxlength', (opDetail.max+'').length);
        }
        if (radioindex === '1') {
            $('#btnPay').html(AllButtonDetail[1]);
            $('#btnPay').val(1);
        }
        else if (radioindex === '2') {
            $('#btnPay').html(AllButtonDetail[2]);
            $('#btnPay').val(2);
            $('#txtAadhar').attr("placeholder", "Enter Account No");
            $('[for="txtAadhar"]').html("Bank Account Number");
            $('#imgDID').addClass('d-none');
            $('#ddl_Device').addClass('d-none');
            $('#btnCheckBalance').addClass('d-none');
        }
        else if (radioindex === '3') {
            $('#btnPay').html(AllButtonDetail[3]);
            $('#btnPay').val(3);
            $('#btnAmount').parent().addClass('d-none');
            $('#btnCheckBalance').addClass('d-none');
        }
        else if (radioindex === '4') {
            $('#btnPay').html(AllButtonDetail[4]);
            $('#btnPay').val(4);
        }
    });
});
var compareAndGetIndexOfObjectInList = function (l, o) {
    for (_o in l) {
        if (JSON.stringify(l[_o]) === JSON.stringify(o)) {
            return _o;
        }
    }
    return -1;
}
var ResetRecentBank = function () {
    let rbanks = $('#recentBanks').data().recentbank;
    if (localStorage.recentBanks === undefined || localStorage.recentBanks === 'null' || localStorage.recentBanks === "") {
        localStorage.setItem('recentBanks', JSON.stringify($('#recentBanks').data().recentbank));
    } else {
        rbanks = JSON.parse(localStorage.recentBanks);
        if (rbanks.length === 0) {
            localStorage.setItem('recentBanks', JSON.stringify($('#recentBanks').data().recentbank));
            rbanks = $('#recentBanks').data().recentbank;
        }
    }
    rbanks = JSON.parse(localStorage.recentBanks);
    let isSetAgain = false;
    while (rbanks.length > 10) {
        rbanks.pop();
        isSetAgain = true;
    }
    if (isSetAgain === true) {
        localStorage.setItem('recentBanks', JSON.stringify(rbanks));
    }
    let recentbankList = rbanks;
    var x;
    $('#recentBanks').empty();
    for (x in recentbankList) {
        var imgE = document.createElement("img");
        imgE.setAttribute('src', $$host + '/Image/BankLogo/' + recentbankList[x].id + '.png');
        imgE.setAttribute('class', 'rounded mx-auto p-2');
        imgE.setAttribute('data-bankdata', JSON.stringify(recentbankList[x]));
        imgE.setAttribute('alt', recentbankList[x].bankName);
        imgE.setAttribute('data-toggle', 'tooltip');
        imgE.setAttribute('data-html', 'true');
        imgE.setAttribute('title', '<span class="text-capitalize">' + recentbankList[x].bankName + '</span>');
        $('#recentBanks').append(imgE);
    }
    $('[data-toggle="tooltip"]').tooltip();
    $('#recentBanks img').click(function () {
        let cBank = $(this).data().bankdata;
        $('[name = "BankList"]').val(cBank.bankName.toUpperCase()).attr('data-item-IIN', cBank.iin);
        $('#recentBanks img.active').removeClass('active');
        $(this).addClass('active');
    });
}
class Bank {
    constructor() {
        $(document).mouseup(e => {
            var container = $('[name="BankList"]');
            if (!container.is(e.target) && !$('#suggestedList').is(e.target) && container.has(e.target).length === 0) {
                $('#suggestedList').css('display', 'none');
            }
        });
        this.bindBank();
        this.IsExists();
        $('[name="BankList"]').focus(() => $('#suggestedList').css('display', 'block'));
        $('[name="BankList"]').on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $('#suggestedList div').filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
            });
        });
    }
    bindBank(bankName = '') {
        $.post('/bindAEPSBanks', { bankName: bankName }).done(result => {
            var _parent = $('[name="BankList"]').parent();
            _parent.find('[name=suggestedList]').index() === -1 ? `${_parent.append('<div name="suggestedList" id="suggestedList"></div>')}` : "";
            for (let i = 0; i < result.length; i++) {
                if ($('[name=suggestedList]:contains(' + result[i].bankName.toUpperCase() + ')').index() === -1) {
                    $('[name=suggestedList]').append(`<div data-item-IIN="${result[i].iin}" class="schild" data-selectedbank='${JSON.stringify(result[i])}'>
                                                        <img src="/Image/Banklogo/${result[i].id}.png"/> 
                                                        <span>${result[i].bankName}</span>
                                                      </div>`);

                }
                $('.schild').unbind().click(e => {
                    $('[name = "BankList"]').val($(e.currentTarget).find('span').text()).attr('data-item-IIN', $(e.currentTarget).attr('data-item-IIN'));
                    $('#suggestedList').css('display', 'none');
                    let selectedbank = $(e.currentTarget).data().selectedbank;
                    if (localStorage.recentBanks !== undefined && localStorage.recentBanks !== 'null' && localStorage.recentBanks !== "") {
                        let rban = JSON.parse(localStorage.recentBanks);
                        let _index = compareAndGetIndexOfObjectInList(rban, selectedbank);
                        if (_index > -1) {
                            rban.splice(_index, 1);
                        }
                        rban.unshift(selectedbank);
                        localStorage.setItem('recentBanks', JSON.stringify(rban));
                        ResetRecentBank();
                    }
                });
            }
        });
    }
    IsExists() {
        $('#txtBank').keyup(e => {
            var _text = $(e.currentTarget).val();
            if (_text.length > 0) {
                let _index = $('[name=suggestedList]:contains(' + _text.toUpperCase() + ')').index();
                if (_index === -1) {
                    this.bindBank(_text);
                }
                $('[name="BankList"]').focus(() => $('#suggestedList').css('display', 'block'));
            }
        });
    }
}
let bank = new Bank();