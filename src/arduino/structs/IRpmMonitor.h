#ifndef IRPMMONITOR_H
#define IRPMMONITOR_H

class IRpmMonitor {
  public:
    virtual unsigned int readRpm();
};

#endif