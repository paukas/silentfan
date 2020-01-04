void printVar(const char * name, const unsigned int value) {
  Serial.print(name);
  Serial.print(value);
  Serial.println();
}

void printVar(const char * name, const double value) {
  Serial.print(name);
  Serial.print(value);
  Serial.println();
}