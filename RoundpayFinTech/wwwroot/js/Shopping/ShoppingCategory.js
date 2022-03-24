
(() => {
    let url = 'select2.min.css';
    if ($('head').find(url).index() === -1) {
        $('head').append('<link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-beta.1/dist/css/select2.min.css" rel="stylesheet" />');
    }
})();

var bindCategoryLevel1 = (e, _id) => {
    $.post('/List-Category', { cid: $(e.currentTarget).val() })
        .done(result => {
            import('https://cdn.jsdelivr.net/npm/select2@4.1.0-beta.1/dist/js/select2.min.js')
                .then(obj => {
                    $('#' + _id)
                        .empty()
                        .append(`<option value="0">:: Choose Category ::</option>`)
                        .append(result.map(m => `<option value="${m.subCategoryId}">${m.name}</option>`))
                        .select2();
                   // console.log(result);
                })
                .catch(() => { })
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        }).always(() => preloader.remove());
};

var bindBrand = (e, _id) => {
    $.post('/List-Brand', { cid: $(e.currentTarget).val() })
        .done(result => {
            import('https://cdn.jsdelivr.net/npm/select2@4.1.0-beta.1/dist/js/select2.min.js')
                .then(obj => $('#' + _id).empty().append(`<option value="0">:: Choose Brand ::</option>`).append(result.map(m => `<option value="${m.brandId}">${m.brandName}</option>`)))
                .catch(() => { })
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        }).always(() => preloader.remove());
};

var bindCategoryLevel2 = (e, _id) => {
    preloader.load();
    $.post('/List-SubCategoryNew', { sid: $(e.currentTarget).val() })
        .done(result => {
            import('https://cdn.jsdelivr.net/npm/select2@4.1.0-beta.1/dist/js/select2.min.js')
                .then(obj => {
                    $('#' + _id)
                        .empty()
                        .append(`<option value="0">:: Choose Subcategory ::</option>`)
                        .append(result.map(m => `<option value="${m.subCategoryId}" data-Min-Comm="${m.commission}" data-Comm-Type=${m.commissionType}>${m.name}</option>`))
                        .select2();
                   // console.log(result);
                })
                .catch(() => { })
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        }).always(() => preloader.remove());
};