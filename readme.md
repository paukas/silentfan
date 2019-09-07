This is a relay based fan controller.

Arduino switches a relay via pin `PIN_CTL`. Relay inputs are connected to 5V (relay on) and 12V (relay off). Switching is based on temperature received from PC over Serail PORT. PC writes GPU temperature as string to serial port on regular intervals. If arduino fails to receive or parse temperature reading it goes to an error state and switches relay back to 12V.

Possible error state scenarios:
* Arduino failed to receive temperature readings `MAX_NOSIGNALCNT=5` or more times
* Arduino failed to parse temperature reading
* Temperature reading was out of range [`TEMP_MINPOSSIBLE=25`, `TEMP_MAXPOSSIBLE=80`]

Arduino switches relay on only when temperature is below `TEMP_LOW=50`. It switches relay off if temperature reches `TEMP_HIGH=55`.