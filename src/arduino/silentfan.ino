#include "serialtools/printVar.cpp"

#include "structs/IRpmMonitor.h"
#include "structs/RpmMonitor.cpp"

#include "structs/IPwmController.h"
#include "structs/PwmControllerOnTimer1.cpp"
#include "structs/PwmControllerOnTimer2.cpp"

#define SERIAL_TIMEOUT 500        // how long till serial times out
#define CONNECTION_TIMEOUT 2000   // how long till fans reset to default pwm

#define FAN1_RPM_PIN PD2
#define FAN2_RPM_PIN PD5
#define FAN1_DEFAULT_PWM 80
#define FAN2_DEFAULT_PWM 80


class Fan {
  private:
    IPwmController& _pwmController;
    IRpmMonitor& _rpmMonitor;
    
    const String _rpmValuePrefix;
    const String _pwmValuePrefix;

  public:
    Fan(IPwmController& pwmController, IRpmMonitor& rpmMonitor, byte fanNumber) : 
      _pwmController(pwmController), 
      _rpmMonitor(rpmMonitor),
      _rpmValuePrefix("fan[" + String(fanNumber) + "].rpm="),
      _pwmValuePrefix("fan[" + String(fanNumber) + "].pwm=")
    {
    };
    
    void resetDutyCycle() {
      _pwmController.resetDutyCycle();
    }

    void printState(void (* textWriter)(const String& text)) {
      unsigned int rpm = _rpmMonitor.readRpm();
      unsigned int pwm = _pwmController.getDutyCycle();
      
      static const String newline = String("\n");
      textWriter(_rpmValuePrefix + String(rpm) + newline);
      textWriter(_pwmValuePrefix + String(pwm) + newline);
    }

    bool handleCommand(String command) {
      if (command.startsWith(_pwmValuePrefix)) {
        String value = command.substring(_pwmValuePrefix.length());
        unsigned int dutyCycle = value.toInt();
        _pwmController.changeDutyCycle(dutyCycle);
        return true;
      }

      return false;
    }
};

PwmControllerOnTimer1 pwmOnTimer1 = PwmControllerOnTimer1(FAN1_DEFAULT_PWM);
PwmControllerOnTimer2 pwmOnTimer2 = PwmControllerOnTimer2(FAN2_DEFAULT_PWM);
RpmMonitor rpmMonitor1 = RpmMonitor();
void onRpmMonitor1Tick() { rpmMonitor1.tick(); };
RpmMonitor rpmMonitor2 = RpmMonitor();
void onRpmMonitor2Tick() { rpmMonitor2.tick(); };

static const int FAN_COUNT = 2;
Fan fans[FAN_COUNT] = {
  Fan(pwmOnTimer1, rpmMonitor1, 0),
  Fan(pwmOnTimer2, rpmMonitor2, 1)
};

void setup() {
  Serial.begin(9600);
  Serial.setTimeout(SERIAL_TIMEOUT);
  Serial.println("Starting SilentFAN");

  pwmOnTimer1.setup();
  pwmOnTimer2.setup();

  RpmMonitor::setup(FAN1_RPM_PIN, onRpmMonitor1Tick);
  RpmMonitor::setup(FAN2_RPM_PIN, onRpmMonitor2Tick);

  // pinMode(PIN_RPM, INPUT_PULLUP);
  // attachInterrupt(digitalPinToInterrupt(PIN_RPM), onRevPinInterrupt, CHANGE);


  // pinMode(PIN_PWM, OUTPUT);
  // TCCR2A = _BV(COM2A1) | _BV(COM2B1) | _BV(WGM21) | _BV(WGM20);
  // TCCR2B = _BV(WGM22) | _BV(CS21);
  // OCR2A = 80; // PD3
  //OCR2B = 80;// ?? // pin 3 / PD3?

  // PwmDriver.initialize(PD3, OCR2A);
  // PwmDriver.defaultDutyCycle = 80;
  // PwmDriver.resetDutyCycle();

  //printVar("fan[0].pwm.mult=", PwmDriver.multiplier);

  for (int i = 0; i < FAN_COUNT; i++) {
    fans[i].printState(printToSerial);
  }
}

bool tryHandleCommand(String command) {
  if (command != NULL && command != "" ) {
    for (int i = 0; i < FAN_COUNT; i++) {
      bool handled = fans[i].handleCommand(command);
      if (handled) {
        return true;
      }
    }
  }
}

void resetDutyCyclesOnAllFans() {
  for (int i = 0; i < FAN_COUNT; i++) {
    fans[i].resetDutyCycle();
  }
}

void printToSerial(const String& text) {
  Serial.print(text);
}

void loop() {
  unsigned int lastSuccessfulHandle = millis();
  while (true) {
    String command = Serial.readStringUntil('\n');
    bool handled = tryHandleCommand(command);
    bool connectionLost = !handled && (millis() - lastSuccessfulHandle) > CONNECTION_TIMEOUT;

    for (int i = 0; i < FAN_COUNT; i++) {
      if (connectionLost)
        fans[i].resetDutyCycle();

      fans[i].printState(printToSerial);
    }
  }
}