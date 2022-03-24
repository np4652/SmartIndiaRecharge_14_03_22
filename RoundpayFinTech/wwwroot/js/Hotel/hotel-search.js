$(document).ready(function () {
    BindCityDropdown('');
    $(".roomsGuests").on("click", ".incNumber", function () {
        let _this = $(this);
        let inp = _this.parents(".input-group").find("input");
        inp.val((parseInt(inp.val()) || 0) + 1);
        if (inp.hasClass("childInput")) {
            if ((parseInt(inp.val())) > 4) {
                inp.val(4);
                return;
            }

            //work on child section
            $(this).parents(".addRoomRow").find(".divChild ul").empty();
            let _childCount = inp.val();
            if (_childCount > 0) {
                $(this).parents(".addRoomRow").find(".divChild").show();
                $(this).parents(".addRoomRow").find(".divChild ul").html(generateChildAge(_childCount));
            }
            else {
                $(this).parents(".addRoomRow").find(".divChild").hide();
            }
        }
        else {
            if (inp.val() > 12) {
                inp.val(12);
                return;
            }
        }
    })
    $(".roomsGuests").on("click", ".decNumber", function () {
        let _this = $(this);
        let inp = _this.parents(".input-group").find("input");
        if (inp.val() > 0)
            inp.val((parseInt(inp.val()) || 0) - 1);

        if (inp.hasClass("childInput")) {
            //work on child section
            $(this).parents(".addRoomRow").find(".divChild ul").empty();

            let _childCount = inp.val();
            if (_childCount > 0) {
                $(this).parents(".addRoomRow").find(".divChild").show();
                $(this).parents(".addRoomRow").find(".divChild ul").html(generateChildAge(_childCount));
            }
            else {
                $(this).parents(".addRoomRow").find(".divChild").hide();
            }
        }
        else {
            if (inp.val() < 1) {
                inp.val(1);
                return;
            }
        }
    })


    function generateChildAge(childCounter) {
        let li = "";
        for (var i = 0; i < childCounter; i++) {
            li += `<li>
                                 <span class="latoBold font12 grayText appendBottom10 ageName">CHILD ${(i + 1)} AGE</span><label class="lblAge" for="0">
                                    <select id="0" class="ageSelectBox">
                                        <option>Select</option>
                                        <option value="1">1</option>
                                        <option value="2">2</option>
                                        <option value="3">3</option>
                                        <option value="4">4</option>
                                        <option value="5">5</option>
                                        <option value="6">6</option>
                                        <option value="7">7</option>
                                        <option value="8">8</option>
                                        <option value="9">9</option>
                                        <option value="10">10</option>
                                        <option value="11">11</option>
                                        <option value="12">12</option>
                                    </select>
                                </label>
                            </li>`;
        }

        return li;
    }

    $(".roomsGuests").on("click", ".btnAddRoom", function () {
        let row = $(".roomsGuests").find(".addRoomRow").eq(0).clone();
        let rowContainer = $(".roomsGuestsTop");
        $(".roomsGuests").find(".addRoomRow").removeClass("active");
        row.addClass("active");
        rowContainer.append(row);
        refreshAllRooms();
    })

    $(".roomsGuests").on("click", ".btnedit", function () {
        let _this = $(this);
        let row = _this.parents(".addRoomRow");
        $(".roomsGuests").find(".addRoomRow").removeClass("active");
        row.addClass("active");
        refreshAllRooms();
    })

    $(".roomsGuests").on("click", ".btnremove", function () {
        let _this = $(this);
        let row = _this.parents(".addRoomRow");
        setTimeout(function () {
            row.remove();
            refreshAllRooms();
        }, 300)
    })

    function refreshAllRooms() {
        $(".roomsGuests").find(".addRoomRow").each(function (i) {
            let _this = $(this);
            let addRoomLeft = _this.find(".addRoomLeft");
            let addRoomRight = _this.find(".addRoomRight");

            if (_this.hasClass('active')) {
                addRoomLeft.find("p").text("ROOM " + (i + 1));
                addRoomLeft.find("small").hide();
                addRoomRight.show();
            } else {
                addRoomLeft.find("p").text("ROOM " + (i + 1));
                let noOfAdult = addRoomRight.find(".adultCounter").find("input").val();
                let noOfChild = addRoomRight.find(".childCounter").find("input").val() || 0;

                let roomInfo = (noOfAdult > 1 ? noOfAdult + " Adults" : noOfAdult + " Adult") + ", " + (noOfChild > 1 ? noOfChild + " Children" : noOfChild + " Child");
                let editButton = `<button type="button" class="btn btn-link btn-sm btnedit">edit</button>`;
                let removeButton = `<button type="button" class="btn btn-link btn-sm btnremove">remove</button>`;

                addRoomLeft.find("small").html("(" + roomInfo + ")" + editButton + removeButton);
                addRoomLeft.find("small").show();
                addRoomRight.hide();
            }
        })
    }

    $(".roomsGuests").on("click", ".btnApply", function () {
        let noOfAdult = 0, noOfChild = 0;
        $(".roomsGuests").find(".addRoomRow").each(function (i) {
            let _this = $(this);
            let addRoomRight = _this.find(".addRoomRight");
            noOfAdult += parseInt(addRoomRight.find(".adultCounter").find("input").val());
            noOfChild += parseInt((addRoomRight.find(".childCounter").find("input").val() || 0));
        })

        let adultInfo = (noOfAdult > 1 ? noOfAdult + " Adults" : noOfAdult + " Adult");
        let childInfo = (noOfChild > 1 ? noOfChild + " Children" : noOfChild + " Child");
        $(".btnDropdownToggle").find("a.dropdown-btn").find(".adult").text(adultInfo);
        $(".btnDropdownToggle").find("a.dropdown-btn").find(".children").text(childInfo);

        $(".btnDropdownToggle").trigger("click");
    })
})




function BindCityDropdown(obj) {
    let searchdata = $('#searchJson').val();
    console.log(searchdata)
    var objData;
    if (searchdata && searchdata != "") {
        objData = jQuery.parseJSON(searchdata);
        $('#CheckInDate1').val(objData.CheckInDate);
        $('#CheckOutDate1').val(objData.CheckOutDate);
    }
    if (obj.length >= 3 || obj.length == 0) {
        $.ajax({
            type: 'Post',
            url: "/Hotel/bindcity/",
            data: { searchkey: obj },
            datatype: 'json',
            success: function (data) {
                console.log(data)
                let arrSearch = [];
                $("#ddlhotelsearch").empty();
                for (var i = 0; i < data.list.length; i++) {
                    /*$("#ddlhotelsearch").append($(`<option></option>`).val(data.list[i].destinationID).html(data.list[i].cityName));*/
                    $("#ddlhotelsearch").append('<option value="' + data.list[i].destinationID + '">' + data.list[i].cityName + '</option>');
                    arrSearch.push({
                        destinationID: data.list[i].destinationID,
                        cityName: data.list[i].cityName,
                        countryCode: data.list[i].countryCode
                    });
                    if (objData) {
                        let data = $('#ddlhotelsearch').find('option[value="' + objData.CityId + '"]').text();
                        $('#txthotel').attr('data-cityid', obj.CityId);
                        $('#txthotel').attr('data-countrycode', obj.CountryCode);
                        $('#txthotel').val(data);
                    }
                }
                autocomplete(document.getElementById("txthotel"), arrSearch);
                function autocomplete(inp, arr) {
                    var currentFocus;
                    inp.addEventListener("input", function (e) {
                        var a, b, i, val = this.value;
                        closeAllLists();
                        if (!val) { return false; }
                        currentFocus = -1;
                        a = document.createElement("DIV");
                        a.setAttribute("id", this.id + "autocomplete-list");
                        a.setAttribute("class", "autocomplete-items");
                        this.parentNode.appendChild(a);
                        for (i = 0; i < arr.length; i++) {
                            if (arr[i].cityName.substr(0, val.length).toUpperCase() == val.toUpperCase()) {
                                b = document.createElement("DIV");
                                b.innerHTML = "<strong>" + arr[i].cityName.substr(0, val.length) + "</strong>";
                                b.innerHTML += arr[i].cityName.substr(val.length);
                                b.innerHTML += "<input type='hidden' value='" + arr[i].cityName + "' data-cityId='" + arr[i].destinationID + "' data-countrycode='" + arr[i].countryCode + "'>";
                                b.addEventListener("click", function (e) {
                                    inp.value = this.getElementsByTagName("input")[0].value;
                                    inp.setAttribute("data-cityId", this.getElementsByTagName("input")[0].getAttribute("data-cityId"));
                                    inp.setAttribute("data-countrycode", this.getElementsByTagName("input")[0].getAttribute("data-countrycode"));
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



function HotelLoaderShow(data1 = '', data2 = '', data3 = '') {
    /* const d1 = data1 == '' ? d1 : data1 , d2 = data2==''?d2:data2, d3 = data3==''?d3:data3;*/

    let parentDiv = $('#HotelLoader')
    parentDiv.append(`<div class="Hotel-Loader" style="height: 70%; display: grid; width: 58%; margin: 6% auto 0; background-color:white; box-shadow: 2px 3px 18px -1px;">
        <div class="carousel slide carousel-fade" data-ride="carousel" data-interval="3000" id="carousel-1">
        <div class="carousel-inner" role="listbox">
            <div class="carousel-item active"><img class="img-fluid w-100 d-block" src="../Image/Hotel/LoaderBanner/b.jpg" alt="Slide Image">
                </div>
            <div class="carousel-item"><img class="img-fluid w-100 d-block" src="../Image/Hotel/LoaderBanner/b1.jpg" alt="Slide Image">
                
                   </div>
            <div class="carousel-item"><img class="img-fluid w-100 d-block" src="../Image/Hotel/LoaderBanner/b2.jpg" alt="Slide Image">
                </div>
            
        </div>
        
        <div>
            <a class="carousel-control-prev" href="#carousel-1" role="button" data-slide="prev">
                <span class="carousel-control-prev-icon"><i class="la la-cutlery"></i></span>
                
            </a>
            <a class="carousel-control-next" href="#carousel-1" role="button" data-slide="next">
                <span class="carousel-control-next-icon"><i class="la la-cutlery"></i></span>
                
            </a>
        </div>
        
    </div>
        <div class="Search-text" style="background:white">
            <table class="table">
                <tbody><tr>
                    <td style="font-weight:bold">Destination</td>
                    <td style="font-weight:bold">:</td>
                    <td>${data1}</td>
                    <td style="font-weight:bold">Checkin</td>
                    <td style="font-weight:bold">:</td>
                    <td>${data2}</td>
                    <td style="font-weight:bold">CheckOut</td>
                    <td style="font-weight:bold">:</td>
                    <td>${data3}</td>
                </tr>
                <tr>
                    <td colspan="10">
                        <div class="progress" style="margin-bottom:-20px !important">
                      <div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100" style="width: 100%">
                Processing
                         </div>

                       </div>
                        
                    </td>
                </tr>
            </tbody></table>

        </div>

    </div>`)
    $('#HotelLoader').show();
}



$('#btnSearch').on('click', function () {
    HotelLoaderShow($('#txthotel').val(), $('#CheckInDate1').val(), $('#CheckOutDate1').val());
    let CityId = $('#txthotel').attr("data-cityid"),
        CountryCode = $('#txthotel').attr("data-countrycode"),
        NoOfRooms = 0,
        RoomGuests = [],
        NoOfNights = 1,
        CheckInDate = $('#CheckInDate1').val(),
        CheckOutDate = $('#CheckOutDate1').val(),
        GuestNationality = 'IN';

    $(".roomsGuests").find(".addRoomRow").each(function (i) {
        let _this = $(this);
        let addRoomRight = _this.find(".addRoomRight");
        let noOfAdult = parseInt(addRoomRight.find(".adultCounter").find("input").val());
        let noOfChild = parseInt(addRoomRight.find(".childCounter").find("input").val());
        let childAge = [];
        if (noOfChild > 0) {
            _this.find(".divChild ul li").each(function () {
                let age = isNaN($(this).find(".ageSelectBox").val()) ? 0 : $(this).find(".ageSelectBox").val();
                age > 0 && childAge.push(age);
            })
        }
        RoomGuests.push({
            NoOfAdults: noOfAdult,
            NoOfChild: noOfChild,
            ChildAge: childAge
        });

        NoOfRooms++;
    })

    let jsonObject = {
        CheckInDate: CheckInDate,
        CheckOutDate: CheckOutDate,
        NoOfNights: NoOfNights,
        CountryCode: CountryCode,
        CityId: CityId,
        PreferredCurrency: "INR",
        GuestNationality: GuestNationality,
        NoOfRooms: NoOfRooms,
        RoomGuests: RoomGuests
    };
    var json = JSON.stringify(jsonObject);
    $("#searchJsonlist").val(JSON.stringify(jsonObject));
    $('#frmSearch').submit();
})