export class geoLocation {
    constructor() {
        alert('1')
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(geoSuccess);
        }
    }
    geoSuccess(position) {
        geoLoactionDetail.Latitude = position.coords.latitude;
        geoLoactionDetail.Longitute = position.coords.longitude;
    }
}