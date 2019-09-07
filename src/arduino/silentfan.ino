#define PIN_CTL A2
#define SERIAL_TIMEOUT 500
#define MAX_NOSIGNALCNT 5

#define TEMP_LOW 50
#define TEMP_HIGH 55
#define TEMP_MINPOSSIBLE 25
#define TEMP_MAXPOSSIBLE 80


void setup() {
  Serial.begin(9600);
  Serial.setTimeout(SERIAL_TIMEOUT);

  pinMode(PIN_CTL, OUTPUT);

  Serial.println("Starting SilentFAN");
}

bool _silentModeEnabeld = false;
short _noSignalCnt = 0;
bool _ledState = HIGH;

void loop() {
  String temperatureString = Serial.readStringUntil('\n');
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