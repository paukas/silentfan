#ifndef IPWMCONTROLLER_H
#define IPWMCONTROLLER_H

typedef class IPwmController {
  public:
    virtual void changeDutyCycle(unsigned int dutyCycle);
    virtual unsigned int getDutyCycle();
    virtual void resetDutyCycle();
} IPwmController;

#endif