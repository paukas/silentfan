#ifndef IPWMCONTROLLER_H
#define IPWMCONTROLLER_H

class IPwmController {
  public:
    virtual void changeDutyCycle(unsigned int dutyCycle) = 0;
    virtual unsigned int getDutyCycle() = 0;
    virtual void resetDutyCycle() = 0;
};

#endif