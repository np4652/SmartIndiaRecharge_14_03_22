$(document).ready(function () {
    
    BindShoppingSearch('');
})







function BindShoppingSearch(obj) {
    var objData;

    if (obj.length >= 3 || obj.length == 0) {
        $.ajax({
            type: 'Post',
            url: "/ShoppingWebsite/GetSearchKeyWordList/",
            data: { searchkey: obj },
            datatype: 'json',
            success: function (data) {
             
                let arrSearch = [];
                $("#ddlsearchinput").empty();
                for (var i = 0; i < data.list.length; i++) {
                    /*$("#ddlsearchinput").append($(`<option></option>`).val(data.list[i].destinationID).html(data.list[i].Keyword));*/
                    $("#ddlsearchinput").append('<option value="' + data.list[i].keywordId + '">' + data.list[i].keywordId + '</option>');
                    arrSearch.push({
                        keywordId: data.list[i].keywordId,
                        keyword: data.list[i].keyword,
                        subcategoryId: data.list[i].subcategoryId,
                        image: data.list[i].productImage
                    });
                    //if (objData) {
                    //    let data = $('#ddlsearchinput').find('option[value="' + objData.id + '"]').text();
                    //    $('#myInput').attr('data-id', obj.id);
                    //    $('#myInput').attr('data-subcId', obj.subcategoryId);
                    //    $('#myInput').val(data);
                    //}
                }
                autocomplete(document.getElementById("myInput"), arrSearch);
                function autocomplete(inp, arr) {
                    var currentFocus;
                    inp.addEventListener("input", function (e) {
                        var a, b, i,c, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
                        currentFocus = -1;
                        a = document.createElement("DIV");
                        a.setAttribute("id", this.id + "autocomplete-list");
                        a.setAttribute("class", "autocomplete-items");
                        this.parentNode.appendChild(a);
                        for (i = 0; i < arr.length; i++) {
                            if (arr[i].keyword.substr(0, val.length).toUpperCase() == val.toUpperCase()) {
                                var prd = "ProductList/S/" + arr[i].subcategoryId;
                           
                                b = document.createElement("a");
                                b.setAttribute("href", "ProductList/S/" + arr[i].subcategoryId);
                                if (arr[i].image != "" && arr[i].image != null) {
                                    b.innerHTML += "<img src=" + arr[i].image + "></img>";
                                }
                                else {
                                    b.innerHTML += "<img src='/ShoppingWebsite/images/icons/filterprod.svg'></img>";
                                }
                                b.innerHTML += arr[i].keyword;
                                b.innerHTML += "<input type='hidden' value='" + arr[i].keyword + "' data-id='" + arr[i].keywordId + "' data-subcId='" + arr[i].subcategoryId + "'>";
                               
                                b.addEventListener("click", function (e) {
                                    inp.value = this.getElementsByTagName("input")[0].value;
                                    inp.setAttribute("data-id", this.getElementsByTagName("input")[0].getAttribute("data-id"));
                                    inp.setAttribute("data-subcId", this.getElementsByTagName("input")[0].getAttribute("data-subcId"));
                                    closeAllLists();
                                });
                                a.appendChild(b);
                            }
                        }
                    });

                    inp.addEventListener("keydown", function (e) {
                        var x = document.getElementById(this.id + "autocomplete-list");
                        if (x) x = x.getElementsByTagName("div");
                        if (e.keyCode == 40) {
                            currentFocus++;
                            addActive(x);
                        } else if (e.keyCode == 38) {
                            currentFocus--;
                            addActive(x);
                        } else if (e.keyCode == 13) {
                            e.preventDefault();
                            if (currentFocus > -1) {
                                if (x) x[currentFocus].click();
                            }
                        }
                    });
                    function addActive(x) {
                        if (!x) return false;
                        removeActive(x);
                        if (currentFocus >= x.length) currentFocus = 0;
                        if (currentFocus < 0) currentFocus = (x.length - 1);
                        x[currentFocus].classList.add("autocomplete-active");
                    }
                    function removeActive(x) {
                        for (var i = 0; i < x.length; i++) {
                            x[i].classList.remove("autocomplete-active");
                        }
                    }
                    function closeAllLists(elmnt) {
                        var x = document.getElementsByClassName("autocomplete-items");
                        for (var i = 0; i < x.length; i++) {
                            if (elmnt != x[i] && elmnt != inp) {
                                x[i].parentNode.removeChild(x[i]);
                            }
                        }
                    }
                    document.addEventListener("click", function (e) {
                        closeAllLists(e.target);
                    });
                }

            },
            error: function (i, d) {
                console.log(i);
                console.log(d);
            }
        })
    }
}
