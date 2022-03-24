function NumberToWords() {

    var units = ["Zero", "One", "Two", "Three", "Four", "Five", "Six",
        "Seven", "Eight", "Nine", "Ten"];
    var teens = ["Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen",
        "Sixteen", "Seventeen", "Eighteen", "Nineteen", "Twenty"];
    var tens = ["", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty",
        "Seventy", "Eighty", "Ninety"];

    var othersIndian = ["Thousand", "Lakh", "Crore"];

    var othersIntl = ["Thousand", "Million", "Billion", "Trillion"];

    var INDIAN_MODE = "indian";
    var INTERNATIONAL_MODE = "international";
    var currentMode = INDIAN_MODE;

    var getBelowHundred = function (n) {
        if (n >= 100) {
            return "greater than or equal to 100";
        }
        if (n <= 10) {
            return units[n];
        }
        if (n <= 20) {
            return teens[n - 10 - 1];
        }
        var unit = Math.floor(n % 10);
        n /= 10;
        var ten = Math.floor(n % 10);
        var tenWord = ten > 0 ? tens[ten] + " " : '';
        var unitWord = unit > 0 ? units[unit] : '';
        return tenWord + unitWord;
    };

    var getBelowThousand = function (n) {
        if (n >= 1000) {
            return "greater than or equal to 1000";
        }
        var word = getBelowHundred(Math.floor(n % 100));

        n = Math.floor(n / 100);
        var hun = Math.floor(n % 10);
        word = (hun > 0 ? units[hun] + " Hundred " : '') + word;

        return word;
    };

    return {
        numberToWords: function (n) {
            if (isNaN(n)) {
                return "Not a number";
            }

            var word = '';
            var val;

            val = Math.floor(n % 1000);
            n = Math.floor(n / 1000);

            word = getBelowThousand(val);

            if (this.currentMode === INDIAN_MODE) {
                othersArr = othersIndian;
                divisor = 100;
                func = getBelowHundred;
            }
            else if (this.currentMode === INTERNATIONAL_MODE) {
                othersArr = othersIntl;
                divisor = 1000;
                func = getBelowThousand;
            }
            else {
                throw "Invalid mode - '" + this.currentMode
                + "'. Supported modes: " + INDIAN_MODE + "|"
                + INTERNATIONAL_MODE;
            }

            var i = 0;
            while (n > 0) {
                if (i === othersArr.length - 1) {
                    word = this.numberToWords(n) + " " + othersArr[i] + " "
                        + word;
                    break;
                };
                val = Math.floor(n % divisor);
                n = Math.floor(n / divisor);
                if (val !== 0) {
                    word = func(val) + " " + othersArr[i] + " " + word;
                }
                i++;
            }
            return word;
        },
        setMode: function (mode) {
            if (mode !== INDIAN_MODE && mode !== INTERNATIONAL_MODE) {
                throw "Invalid mode specified - '" + mode
                + "'. Supported modes: " + INDIAN_MODE + "|"
                + INTERNATIONAL_MODE;
            }
            this.currentMode = mode;
        }
    };
}



var toWords = function translate(_input) {
    if (isNaN(_input)) {
        an.title = "Warning";
        an.content = "No valid number";
        an.alert(-1);
        return false;
    }
    var num2words = new NumberToWords();
    num2words.setMode("indian");
    //UnComment below line to get international format.
    //num2words.setMode("international");
    return num2words.numberToWords(_input);
};

