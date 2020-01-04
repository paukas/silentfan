#ifndef RPMMONITOR_CPP
#define RPMMONITOR_CPP

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

typedef struct {
  RpmSample_t sample;

  void tick() {
    sample.tick();
  }

  int readRpm() {
    RpmSample_t prevSample = RpmSample_t(sample);
    sample.reset();

    return prevSample.toRpm(sample.startMillis);
  }
} RpmMonitor_t;

#endif