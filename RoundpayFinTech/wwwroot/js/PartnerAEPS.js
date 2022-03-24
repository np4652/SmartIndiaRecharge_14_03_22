
class Bank {
    constructor() {
        console.log('in');
        $(document).mouseup(e => {
            var container = $('[name="BankList"]');
            if (!container.is(e.target) && container.has(e.target).length === 0) {
                $('#suggestedList').css('display', 'none');
            }
        });
        this.bindBank();
        this.IsExists();
        console.log('in1');
        console.log(this);
        $('[name="BankList"]').focus(() => $('#suggestedList').css('display', 'block'));
        $('[name="BankList"]').on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $('#suggestedList div').filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
            });
        });
    }
    bindBank(bankName='') {
        $.post('/bindAEPSBankList', { bankName: bankName }).done(result => {
            console.log(result);
            var _parent = $('[name="BankList"]').parent();
            _parent.find('[name=suggestedList]').index() === -1 ? `${_parent.append('<div name="suggestedList" id="suggestedList"></div>')}` : "";
            for (let i = 0; i < result.length; i++) {
                //if ($('#banks option:contains(' + result[i].bankName.toUpperCase() + ')').index() === -1) {
                //    $('#banks').append(`<option data-item-IIN="${result[i].iin}">${result[i].bankName}</option>`);
                //}
                if ($('[name=suggestedList]:contains(' + result[i].bankName.toUpperCase() + ')').index() === -1) {
                    $('[name=suggestedList]').append(`<div data-item-IIN="${result[i].iin}" class="schild">
                                                        <img src="/Image/Banklogo/${result[i].id}.png"/> 
                                                        <span>${result[i].bankName}</span>
                                                      </div>`);
                }
                $('.schild').unbind().click(e => {
                    $('[name = "BankList"]').val($(e.currentTarget).find('span').text()).attr('data-item-IIN', $(e.currentTarget).attr('data-item-IIN'));
                    $('#suggestedList').css('display', 'none');
                });
            }
        });
    }
    IsExists() {
        $('#txtBank').keyup(e => {
            var _text = $(e.currentTarget).val();
            if (_text.length > 2) {
                let _index = $('#banks option:contains(' + _text.toUpperCase() + ')').index();
                console.log(_index);
                if (_index === -1) {
                    this.bindBank(_text);
                }
            }
        });
    }
    
}
let bank = new Bank();