#ifndef PWMCONTROLLERONTIMER1_CPP
#define PWMCONTROLLERONTIMER1_CPP

#include "PwmControllerBase.cpp"

typedef class PwmControllerOnTimer1 : public PwmControllerBase {
  private:
    static const unsigned int COUNTER_TOP_LIMIT = 80;
    static const double MULTIPLIER = COUNTER_TOP_LIMIT / 100.0;

  protected:
    void setupInternal() {
      pinMode(PB2, OUTPUT);
      TCCR1A = _BV(COM1A1) | _BV(COM1B1) | _BV(WGM11) | _BV(WGM10);
      TCCR1B = _BV(WGM12) | _BV(CS11);
      OCR1A = COUNTER_TOP_LIMIT;
    }

    void changeDutyCycleInternal(unsigned int dutyCycle) {
      OCR1B = dutyCycle * MULTIPLIER;
    }

  public:
    PwmControllerOnTimer1(unsigned int defaultDutyCycle) : PwmControllerBase(defaultDutyCycle) {}

} PwmControllerOnTimer1;

#endif