#include "serialtools/printVar.cpp"

#include "structs/IRpmMonitor.h"
#include "structs/RpmMonitor.cpp"

#include "structs/IPwmController.h"
#include "structs/PwmController.cpp"

static const int RPM1_PIN = 11; // do not change. configured using registers
static const int RPM2_PIN = 12; // do not change. configured using registers

static const int PWM1_PIN = 9;  // do not change. configured using registers
static const int PWM2_PIN = 10; // do not change. configured using registers
static const int PWM_TOP = 80;  // top value for PWM counter (ICR1 register)

#define SERIAL_TIMEOUT 500        // how long till serial times out
#define CONNECTION_TIMEOUT 2000   // how long till fans reset to default pwm

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

RpmMonitor rpmMonitor1 = RpmMonitor();
RpmMonitor rpmMonitor2 = RpmMonitor();

using Fan1PwmController = PwmController<PWM_TOP, PWM1_PIN>;
using Fan2PwmController = PwmController<PWM_TOP, PWM2_PIN>;
Fan1PwmController pwmController1 = Fan1PwmController(FAN1_DEFAULT_PWM);
Fan2PwmController pwmController2 = Fan2PwmController(FAN2_DEFAULT_PWM);

static const int FAN_COUNT = 2;
Fan fans[FAN_COUNT] = {
  Fan(pwmController1, rpmMonitor1, 0),
  Fan(pwmController2, rpmMonitor2, 1)
};

void setup() {

  Serial.begin(9600);
  Serial.setTimeout(SERIAL_TIMEOUT);
  Serial.println("Starting SilentFAN");

  pinMode(PWM1_PIN, OUTPUT);
  pinMode(PWM2_PIN, OUTPUT);
  TCCR1A = _BV(COM1A1) | _BV(COM1B1) | _BV(WGM11);
  TCCR1B = _BV(WGM12) | _BV(WGM13) | _BV(CS11);
  ICR1 = PWM_TOP;
  
  // HACKY SETUP FOR FAN2_RPM_PIN
  cli();
  // pin change interrupt hack
  // resource: https://thewanderingengineer.com/2014/08/11/arduino-pin-change-interrupts/
  // PCICR |= 0b00000001;    // turn on port b
  // PCMSK0 |= 0b00001000;   // turn on on PCINT4
  PCICR = bit(PCIE0);   // enable interrupts on Port B
  PCMSK0 = bit(PCINT3) | bit(PCINT4); // enable interrupts on PB3 & PB4
  PORTB = bit(PORTB3) | bit(PORTB4);  // enable internal pullup resistor on PB3 & PB4
  PCIFR = bit(PCIF0);   // clear existing interrupts in Port B
  sei();
  // --

  for (int i = 0; i < FAN_COUNT; i++) {
    fans[i].resetDutyCycle();
    fans[i].printState(printToSerial);
  }
}

uint8_t PINB_STATE = 0;
ISR(PCINT0_vect)
{
  uint8_t pinb_state = PINB;
  uint8_t changes = pinb_state ^ PINB_STATE;
  if (changes & bit(PORTB3)) {
    rpmMonitor1.tick();
  }
  if (changes & bit(PORTB4)) {
    rpmMonitor2.tick();
  }

  PINB_STATE = pinb_state;
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

  return false;
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
  unsigned long lastSuccessfulHandle = millis();
  bool connectionLost = false;
  while (true) {
    String command = Serial.readStringUntil('\n');
    bool handled = tryHandleCommand(command);
    unsigned long now = millis();
    if (handled) {
      lastSuccessfulHandle = now;
      connectionLost = false;
    } else if (now - lastSuccessfulHandle > CONNECTION_TIMEOUT) {
      connectionLost = true;
    }

    for (int i = 0; i < FAN_COUNT; i++) {
      if (connectionLost)
        fans[i].resetDutyCycle();

      fans[i].printState(printToSerial);
    }
  }
}