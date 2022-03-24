
// Credit: Mateusz Rybczonec


var createElement = (_time = 15, selector = 'jsTimer') => {
    const timerStyle = `<style>
      

        .base-timer {
            position: relative;
            width: 50px;
            height: 50px;
        }

        .base-timer__svg {
            transform: scaleX(-1);
        }

        .base-timer__circle {
            fill: none;
            stroke: none;
        }

        .base-timer__path-elapsed {
            stroke-width: 7px;
            stroke: green;
        }

        .base-timer__path-remaining {
            stroke-width: 7px;
            stroke-linecap: round;
            transform: rotate(90deg);
            transform-origin: center;
            transition: 1s linear all;
            fill-rule: nonzero;
            stroke: #dcf8c6!important;
        }

            .base-timer__path-remaining.green {
            color: white;
            }

            .base-timer__path-remaining.orange {
            color: orange;
            }

            .base-timer__path-remaining.red {
            color: red;
            }

        .base-timer__label {
            position: absolute;
            width: 47px;
            height: 47px;
            top: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 11px;
    font-weight: 700;
color:white;
        }
    </style>`
    const FULL_DASH_ARRAY = 283;
    const WARNING_THRESHOLD = 10;
    const ALERT_THRESHOLD = 5;

    const COLOR_CODES = {
        info: {
            color: "white"
        },
        warning: {
            color: "orange",
            threshold: WARNING_THRESHOLD
        },
        alert: {
            color: "red",
            threshold: ALERT_THRESHOLD
        }
    };
    var TIME_LIMIT = 86400;
    let timerInterval = null;
    let remainingPathColor = COLOR_CODES.info.color;
    function setRemainingPathColor(timeLeft) {
        const { alert, warning, info } = COLOR_CODES;
        if (timeLeft <= alert.threshold) {
            document
                .getElementById("base-timer-path-remaining")
                .classList.remove(warning.color);
            document
                .getElementById("base-timer-path-remaining")
                .classList.add(alert.color);
        } else if (timeLeft <= warning.threshold) {
            document
                .getElementById("base-timer-path-remaining")
                .classList.remove(info.color);
            document
                .getElementById("base-timer-path-remaining")
                .classList.add(warning.color);
        }
    }


    function calculateTimeFractions(timer) {
        const rawTimeFraction = timer / TIME_LIMIT;
        return rawTimeFraction - (1 / TIME_LIMIT) * (1 - rawTimeFraction);
    }



    function setCircleDasharrays(timer) {
        const circleDasharray = `${(
            calculateTimeFractions(timer) * FULL_DASH_ARRAY
        ).toFixed(0)} 283`;
        document
            .getElementById("base-timer-path-remaining")
            .setAttribute("stroke-dasharray", circleDasharray);
    }
    function Timer(duration) {
        var timer = duration, hours, minutes, seconds;
 
        var intervalid = setInterval(function () {
            clearInterval(intervalid);
            hours = parseInt((timer / 3600) % 24, 10)
            minutes = parseInt((timer / 60) % 60, 10)
            seconds = parseInt(timer % 60, 10);
          //  for hourd -minuts-seconds
          //  document.getElementById("base-timer-label").innerHTML = hours + ":" + minutes + ":" + seconds;

              //  for hourd -minuts Only
            document.getElementById("base-timer-label").innerHTML = hours + ":" + minutes;

            setCircleDasharrays(timer);
            setRemainingPathColor(timer);
            --timer;
            if (timer < 0)
                clearInterval(intervalid);
        }, 1000);

    }
        $('head').append(timerStyle);
    document.getElementById(selector).innerHTML = `
<div class="base-timer">
        <svg class="base-timer__svg" viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">
            <g class="base-timer__circle">
                <circle class="base-timer__path-elapsed" cx="50" cy="50" r="45"></circle>
                <path
                    id="base-timer-path-remaining"
                    stroke-dasharray="283"
                    class="base-timer__path-remaining ${remainingPathColor}"
                    d="
          M 50, 50
          m -45, 0
          a 45,45 0 1,0 90,0
          a 45,45 0 1,0 -90,0
        "
                ></path>
            </g>
        </svg>
        <span id="base-timer-label" class="base-timer__label">
        </span>
    </div>
`;
    Timer(_time);
}

