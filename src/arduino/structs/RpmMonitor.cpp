#ifndef RPMMONITOR_CPP
#define RPMMONITOR_CPP

#include "IRpmMonitor.h"

typedef struct RpmSample_t {
  static const unsigned int MILLISECONDS_PER_MINUTE = 60000;
  const int CHANGES_PER_REV = 4;

  int changeCount = 0;
  unsigned long startMillis = 0;

  RpmSample_t() {
    reset();
  }

  void tick() {
    changeCount++;
  }

  void reset() {
    changeCount = 0;
    startMillis = millis();
  }

  int toRpm(unsigned long nowMillis) {
    unsigned long sampleLengthMillis = nowMillis - startMillis;
    float revCount = (float) changeCount / (float) CHANGES_PER_REV;
    return (unsigned int) ((float) MILLISECONDS_PER_MINUTE / (float) sampleLengthMillis * revCount);
  }
} RpmSample_t;

typedef class RpmMontitor : public IRpmMonitor {
  private:
    RpmSample_t _sample;

  public:
    unsigned int readRpm() {
      RpmSample_t prevSample = RpmSample_t(_sample);
      _sample.reset();

      return prevSample.toRpm(_sample.startMillis);
    }

    void tick() {
      _sample.tick();
    }

    static void setup(int pinNumber, void (*onInterrupt)()) {
      pinMode(pinNumber, INPUT_PULLUP);
      attachInterrupt(digitalPinToInterrupt(pinNumber), onInterrupt, CHANGE);
    }
} RpmMonitor;

#endif