#define PIN_CTL PD2
#define SERIAL_TIMEOUT 500
#define MAX_NOSIGNALCNT 5
#define SILENTMODE_SWITCHBACK_DELAY 10000

#define TEMP_LOW 43
#define TEMP_HIGH 45
#define TEMP_MINPOSSIBLE 25
#define TEMP_MAXPOSSIBLE 80


void setup() {
  Serial.begin(9600);
  Serial.setTimeout(SERIAL_TIMEOUT);

  pinMode(PIN_CTL, OUTPUT);

  Serial.println("Starting SilentFAN");
}

bool _silentModeEnabeld = false;
unsigned long _silentModeSwitchTime = 0;
short _noSignalCnt = 0;
bool _ledState = HIGH;

bool switchDelayInEffect(unsigned long now) {
  if (_silentModeEnabeld)
    return false;
  else
    return !switchDelayExpired(now, SILENTMODE_SWITCHBACK_DELAY);
}

bool switchDelayExpired(unsigned long now, unsigned long delay) {
  const unsigned long ULONG_MAX = ((unsigned long)0) - 1;

  unsigned long timeDiff = 0;
  if (now < _silentModeSwitchTime) {
    timeDiff = ULONG_MAX - _silentModeSwitchTime + now;
  } else {
    timeDiff = now - _silentModeSwitchTime;
  }

  if (timeDiff > delay) {
    return true;
  }

  return false;
}

void loop() {
  unsigned long now = millis();
  String temperatureString = Serial.readStringUntil('\n');
  if (switchDelayInEffect(now)) {
    Serial.println("Switch delay in effect");
    return;
  }

  bool silentModeEnabled = _silentModeEnabeld;
  short temperature = 0;
  if (parseTemperatureString(temperatureString, &temperature)) {
    if (temperature < TEMP_HIGH) {
      silentModeEnabled = true;
    } else if (temperature > TEMP_LOW) {
      silentModeEnabled = false;
    } else if (temperature < TEMP_MINPOSSIBLE || temperature > TEMP_MAXPOSSIBLE) {
      silentModeEnabled = false;
    }

    _noSignalCnt = 0;
  } else {
    Serial.print("Failed to parse serial input: ");
    Serial.print(temperatureString);
    Serial.println();

    if (_noSignalCnt < MAX_NOSIGNALCNT)
    {
      _noSignalCnt++;
    }
  }

  if (_noSignalCnt > 0) {
    toggleBuiltInLed();
  } else {
    turnOffBuildInLed();
  }

  if (_noSignalCnt >= MAX_NOSIGNALCNT) {
    silentModeEnabled = false;
  }

  if (_silentModeEnabeld != silentModeEnabled) {
    if (silentModeEnabled) {
      digitalWrite(PIN_CTL, HIGH);
    } else {
      digitalWrite(PIN_CTL, LOW);
    }

    _silentModeEnabeld = silentModeEnabled;
    _silentModeSwitchTime = now;
    Serial.print("Silent mode: ");
    Serial.print(_silentModeEnabeld);
    Serial.println();
  }
}

void toggleBuiltInLed() {
  if (_ledState == LOW)
    _ledState = HIGH;
  else
    _ledState = LOW;

  digitalWrite(LED_BUILTIN, _ledState);
}

void turnOffBuildInLed() {
  _ledState = LOW;
  digitalWrite(LED_BUILTIN, _ledState);
}

bool parseTemperatureString(String temperatureString, short* temperature) {
  if (temperatureString == NULL)
    return false;
  if (temperatureString.length() != 2)
    return false;

  char char0 = temperatureString[0];
  char char1 = temperatureString[1];
//  char char3 = temperatureString[2];

  // if (char3 != '\n')
  //   return false;

  short num0 = 0;
  short num1 = 0;
  if (parseNumber(char0, &num0) && parseNumber(char1, &num1)) {
    *temperature = (short) (num0 * 10 + num1);
    return true;
  }

  return false;
}

bool parseNumber(char character, short* number) {
  if (character >= 48 && character <= 57) {
    *number = character - 48;
    return true;
  }
  return false;
}