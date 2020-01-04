#ifndef PWMDRIVER_CPP
#define PWMDRIVER_CPP

typedef struct PwmDriver_t {
  unsigned int defaultDutyCycle = 50;
  unsigned int pinPwm = 0;
  unsigned int dutyCycle = 50;
  double multiplier = 1.0;

  void initialize(int pwmPin, int maxHardwareDutyCycle) {
    pinPwm = pwmPin;
    multiplier = maxHardwareDutyCycle / 100.0;
  }

  void setDutyCycle(unsigned int newDutyCycle) {
    if (newDutyCycle > 100) {
      return;
    }

    if (newDutyCycle == dutyCycle) {
      return;
    }

    dutyCycle = newDutyCycle;
    analogWrite(pinPwm, dutyCycle * multiplier);
  }

  void resetDutyCycle() {
    setDutyCycle(defaultDutyCycle);
  }
} PwmDriver_t;

#endif