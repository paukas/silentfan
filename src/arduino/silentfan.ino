#define SERIAL_TIMEOUT 500

#define PIN_RPM PD2 // input pullup (or pulldown?)

void setup() {
  Serial.begin(9600);
  Serial.setTimeout(SERIAL_TIMEOUT);

  pinMode(PIN_RPM, INPUT_PULLUP);

  Serial.println("Starting SilentFAN");

  attachInterrupt(digitalPinToInterrupt(PIN_RPM), onRevPinInterrupt, CHANGE);
}

struct RpmSample_t {
  static const unsigned int MILLISECONDS_PER_MINUTE = 60000;
  const int CHANGES_PER_REV = 4;

  int changeCount = 0;
  unsigned long sampleStartTick = 0;
  unsigned long sampleEndTick = 0;

  unsigned int toRpm() {
    unsigned long sampleLengthMillis = sampleEndTick - sampleStartTick;
    float revCount = (float) changeCount / (float) CHANGES_PER_REV;
    return (unsigned int) ((float) MILLISECONDS_PER_MINUTE / (float) sampleLengthMillis * revCount);
  }
} MeasuredSample_t;

typedef struct {
  int changeCount = 0;
  unsigned long startTick = 0;

  RpmSample_t sample() {
    RpmSample_t sample = RpmSample_t();
    sample.changeCount = changeCount;
    sample.sampleStartTick = startTick;
    sample.sampleEndTick = millis();

    changeCount = 0;
    startTick = sample.sampleEndTick;
    return sample;
  }
} RpmMonitor_t;

RpmMonitor_t RpmMonitor;
void onRevPinInterrupt() {
  RpmMonitor.changeCount++;
}

void printVar(const char * name, const unsigned int value) {
  Serial.print(name);
  Serial.print(value);
  Serial.println();
}

void loop() {
  unsigned int rpm = RpmMonitor.sample().toRpm();
  printVar("fan[0].rpm=", rpm);
  delay(200);
}