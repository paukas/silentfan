#define PIN_PWM PD3
#define PIN_RPM PD2

void setup() {
    Serial.begin(9600);

    pinMode(PIN_PWM, OUTPUT);
    pinMode(PIN_RPM, INPUT);

    TCCR2A = _BV(COM2A1) | _BV(COM2B1) | _BV(WGM21) | _BV(WGM20);
    TCCR2B = _BV(WGM22) | _BV(CS21);
    OCR2A = 80;

    attachInterrupt(digitalPinToInterrupt(PIN_RPM), onRpmSignalReceived, FALLING);

    Serial.println("Starting SilentFAN");
}

short rpmSignalCount = 0;
void onRpmSignalReceived() {
    rpmSignalCount++;
}

void loop() {
    const short delayMs = 1000;

    long resetTime = millis();
    while (true) { 
        long elapsed = millis() - resetTime;
        resetTime = millis();

        short rpm = captureRpm(elapsed);
        short pwm = adjustPwm(rpm);


        Serial.print("PWM: ");
        Serial.print(pwm);
        Serial.print("; RPM: ");
        Serial.print(rpm);
        Serial.println();


        delay(delayMs);
    }
}

const short maxPwm = 80;
const short pwmStep = maxPwm * 0.02;
short pwm = maxPwm / 2;
short adjustPwm(short rpm) {
    const short rpmTarget = 800;
    
    short newPwm;
    if (rpm > rpmTarget) {
        newPwm = pwm - pwmStep;
    } else if (rpm > 0) {
        newPwm = pwm + pwmStep;
    }

    if (rpm > 0 && newPwm > 0 && newPwm < maxPwm) {
        analogWrite(PIN_PWM, newPwm);
        pwm = newPwm;
    }

    return pwm;
}

short captureRpm(short elapsed) {
    short currentRpmSignalCount = resetRpmSignalCount();
    short rpm = calculateRpm(currentRpmSignalCount, elapsed);
    return rpm;
}

short resetRpmSignalCount() {
    short currentRpmSignalCount = rpmSignalCount;
    rpmSignalCount = 0;
    return currentRpmSignalCount;
}

short calculateRpm(short rpmSignalCount, short timespanMs) {
    const double signalsPerRev = 2.0;
    const double oneSecondMs = 1000.0;

    return (rpmSignalCount * 60.0 / signalsPerRev) * (timespanMs / oneSecondMs);
}
 