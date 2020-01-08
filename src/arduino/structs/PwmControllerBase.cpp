#ifndef PWMCONTROLLERBASE_CPP
#define PWMCONTROLLERBASE_CPP

#include "IPwmController.h"

typedef class PwmControllerBase : public IPwmController {
  private:
    const int _defaultDutyCycle;
    unsigned int _dutyCycle;

  protected:
    virtual void changeDutyCycleInternal(unsigned int dutyCycle);
    virtual void setupInternal();

  public:
    PwmControllerBase(unsigned int defaultDutyCycle) : _defaultDutyCycle(defaultDutyCycle) {}

    void setup() {
      setupInternal();
      changeDutyCycle(_defaultDutyCycle);
    }

    void changeDutyCycle(unsigned int dutyCycle) {
      if (dutyCycle > 100 || dutyCycle == _dutyCycle) {
        return;
      }
      
      _dutyCycle = dutyCycle;
      changeDutyCycleInternal(dutyCycle);
    }

    unsigned int getDutyCycle() {
      return _dutyCycle;
    }

    void resetDutyCycle() {
      changeDutyCycle(_defaultDutyCycle);
    }
} PwmControllerBase;

#endif