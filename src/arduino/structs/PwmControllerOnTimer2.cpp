#ifndef PWMCONTROLLERONTIMER2_CPP
#define PWMCONTROLLERONTIMER2_CPP

#include "PwmControllerBase.cpp"

typedef class PwmControllerOnTimer2 : public PwmControllerBase {
  private:
    static const unsigned int COUNTER_TOP_LIMIT = 80;
    static const double MULTIPLIER = COUNTER_TOP_LIMIT / 100.0;

  protected:
    void setupInternal() {
      pinMode(PD3, OUTPUT);
      TCCR2A = _BV(COM2A1) | _BV(COM2B1) | _BV(WGM21) | _BV(WGM20);
      TCCR2B = _BV(WGM22) | _BV(CS21);
      OCR2A = COUNTER_TOP_LIMIT;
    }

    void changeDutyCycleInternal(unsigned int dutyCycle) {
      OCR2B = dutyCycle * MULTIPLIER;
    }

  public:
    PwmControllerOnTimer2(unsigned int defaultDutyCycle) : PwmControllerBase(defaultDutyCycle) {}

} PwmControllerOnTimer2;

#endif