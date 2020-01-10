#ifndef PWMCONTROLLER_CPP
#define PWMCONTROLLER_CPP

#include "IPwmController.h"

template<int COUNTER_TOP_LIMIT, int PIN>
class PwmController : public IPwmController {
  private:
    static const double MULTIPLIER = COUNTER_TOP_LIMIT / 100.0;

    const int _defaultDutyCycle;
    unsigned int _dutyCycle;
    byte _pin;

  public:
    PwmController(unsigned int defaultDutyCycle) : _defaultDutyCycle(defaultDutyCycle) {}

    void changeDutyCycle(unsigned int dutyCycle) {
      if (dutyCycle > 100 || dutyCycle == _dutyCycle) {
        return;
      }
      
      _dutyCycle = dutyCycle;
      analogWrite(PIN, _dutyCycle * MULTIPLIER);
    }

    unsigned int getDutyCycle() {
      return _dutyCycle;
    }

    void resetDutyCycle() {
      changeDutyCycle(_defaultDutyCycle);
    }
};

#endif