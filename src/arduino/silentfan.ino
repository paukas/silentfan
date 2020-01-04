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
  const int CHANGES_PER_REV = 4;
  const int MILLISECONDS_PER_MINUTE = 60000;

  int changeCount = 0;
  unsigned long sampleStartTick = 0;
  unsigned long sampleEndTick = 0;

  int toRpm() {
    unsigned long sampleLengthMillis = sampleEndTick - sampleStartTick;
    float revCount = (float) changeCount / (float) CHANGES_PER_REV;
    return (int) ((float) MILLISECONDS_PER_MINUTE / (float) sampleLengthMillis * revCount);
  }
} MeasuredSample_t;

typedef struct {
  int changeCount = 0;
  unsigned long sampleStartTick = 0;

  RpmSample_t sample() {
    RpmSample_t sample = RpmSample_t();
    sample.changeCount = changeCount;
    sample.sampleStartTick = sampleStartTick;
    sample.sampleEndTick = millis();

    changeCount = 0;
    sampleStartTick = sample.sampleEndTick;
    return sample;
  }
} CurrentRpmSample_t;

static const unsigned int MILLISECONDS_PER_MINUTE = 60000;
typedef struct RevBuffer_t {
  static const int BUFFER_SIZE = 10;
  static const int TIMESTAMPS_PER_REV = 4;

  unsigned long timestamps[BUFFER_SIZE];
  unsigned int timestampPos = 0;

  void tick() {
    timestampPos++;
    unsigned int pos = timestampPos % BUFFER_SIZE;
    timestamps[pos] = millis();
  }

  int toRpm() {
    int currentPos = timestampPos % BUFFER_SIZE;
    int previousPos = (timestampPos - 1) % BUFFER_SIZE;

    unsigned long timestampDiff = timestamps[currentPos] - timestamps[previousPos];
    return (int) (MILLISECONDS_PER_MINUTE / (float) timestampDiff / (float) TIMESTAMPS_PER_REV);
  }

  int toRpmAvg() {
    static const int SAMPLE_SIZE = BUFFER_SIZE - 1;

    unsigned long diffSum = 0;
    for (int i = 0; i < SAMPLE_SIZE; i++) {
      int currentPos = (timestampPos - i) % BUFFER_SIZE;
      int previousPos = (timestampPos - i - 1) % BUFFER_SIZE;

      unsigned long timestampDiff = timestamps[currentPos] - timestamps[previousPos];
      diffSum += timestampDiff;
    }

    float averageDiff = diffSum / (float) SAMPLE_SIZE;
    static const float BASE = MILLISECONDS_PER_MINUTE / (float) TIMESTAMPS_PER_REV;
    return (int) (BASE / (float) averageDiff);
  }

  void dump() {
    for (int i = 0; i < BUFFER_SIZE; i++) {
      Serial.print(timestamps[i]);
      Serial.print(";");
    }

    Serial.print("[");
    Serial.print(timestampPos);
    Serial.print("]");
  }
} RevBuffer_t;

//CurrentRpmSample_t CurrentSample;

RevBuffer_t revBuffer;
int readRpmSample() {
  return revBuffer.toRpm();
  //return CurrentSample.sample().toRpm();
}

void onRevPinInterrupt() {
  revBuffer.tick();
  //CurrentSample.changeCount++;
}

void loop() {
  int rpm = revBuffer.toRpm();
  int rpmAvg = revBuffer.toRpmAvg();
  Serial.print(rpm);
  Serial.print(",");
  Serial.print(rpmAvg);
  if (rpmAvg == 0)
    revBuffer.dump();
  Serial.println();
  delay(200);
}