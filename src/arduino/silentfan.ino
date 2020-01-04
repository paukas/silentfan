#define SERIAL_TIMEOUT 500

#define PIN_PWM PD3
#define PIN_RPM PD2 // input pullup (or pulldown?)

#include "structs/PwmDriver.cpp";
#include "structs/RpmMonitor.cpp"
#include "serialtools/printVar.cpp"

PwmDriver_t PwmDriver;
RpmMonitor_t RpmMonitor;

void setup() {
  Serial.begin(9600);
  Serial.setTimeout(SERIAL_TIMEOUT);

  pinMode(PIN_RPM, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(PIN_RPM), onRevPinInterrupt, CHANGE);

  Serial.println("Starting SilentFAN");

  pinMode(PIN_PWM, OUTPUT);
  TCCR2A = _BV(COM2A1) | _BV(COM2B1) | _BV(WGM21) | _BV(WGM20);
  TCCR2B = _BV(WGM22) | _BV(CS21);
  OCR2A = 80;

  PwmDriver.initialize(PD3, OCR2A);
  PwmDriver.defaultDutyCycle = 80;
  PwmDriver.resetDutyCycle();

  printVar("fan[0].pwm.mult=", PwmDriver.multiplier);
}

void onRevPinInterrupt() {
  RpmMonitor.tick();
}

void printState() {
  unsigned int rpm = RpmMonitor.readRpm();
  printVar("fan[0].rpm=", rpm);
  printVar("fan[0].pwm=", PwmDriver.dutyCycle);
}

typedef struct CommandHandler_t {
  unsigned long lastHandleTime = 0;

  void handle(String command) {
    bool handled = tryHandleDutyCycleCommand(command);
    if (handled) {
      lastHandleTime = millis();
    }
  }

  unsigned int getMillisSinceLastHandle() {
    return millis() - lastHandleTime;
  }

  bool tryHandleDutyCycleCommand(String command) {
    static const String CMD_PWM_SET = "fan[0].pwm=";
    if (command.startsWith(CMD_PWM_SET)) {
      String value = command.substring(CMD_PWM_SET.length());
      int dutyCycle = value.toInt();
      PwmDriver.setDutyCycle(dutyCycle);
      return true;
    }
    return false;
  }
} CommandHandler_t;

CommandHandler_t CommandHandler;
void loop() {
  String command = Serial.readStringUntil('\n');
  CommandHandler.handle(command);
  
  if (CommandHandler.getMillisSinceLastHandle() > 2000) {
    PwmDriver.resetDutyCycle();
  }

  printState();
}