#include <Adafruit_FT6206.h>
#include <SPI.h>
#include <ClockMe.h>

Adafruit_FT6206 ts = Adafruit_FT6206();
bool isIn = true;
bool isState1 = true;
String message;

unsigned long previousMillisForTime = 0;
unsigned long previousMillisToEraseMessage = 0;
unsigned long previousMillisToCheckServerStatus = 0;
bool messageNeedsToBeErase;
bool startTimer;

ClockMe _clockMe;

char *in = (char*)"in.bmp";
char *out = (char*)"out.bmp";
char *reg = (char*)"register.bmp";
char *back = (char*)"back.bmp";

void setup(void)
{
  Serial.begin(9600);

  _clockMe.Init();

  ts.begin(40);
  state1(isIn);
  //tone(8, 3500, 1000);
  delay(1000);
  //tone(8, 1000, 500);


  messageNeedsToBeErase = false;
  startTimer = false;
}

void loop()
{
  unsigned long currentMillis = millis();

  if (currentMillis - previousMillisForTime > 60000) {
    previousMillisForTime = currentMillis;
    String timeFromServer = _clockMe.GetTimeFromServer();
    if (isState1)
    {
      _clockMe.DisplayMessage(20, 150, timeFromServer);
    }
  }

  if (currentMillis - previousMillisToCheckServerStatus > 1000) {
    previousMillisToCheckServerStatus = currentMillis;
    Serial.println("test");
    String serverStatus = _clockMe.GetServerStatus();
    if(serverStatus=="0")
    {
      uint8_t id = serverStatus.toInt();
      _clockMe.DeleteFingerprint(id);
      Serial.println(id);
    }
  }

  uint8_t fingerprintResult = _clockMe.GetFingerprintId();
  if (isState1)
  {
    if (fingerprintResult != 255 && fingerprintResult != 254 && fingerprintResult != 253)
    {

      String response = _clockMe.SendDataToServer(isIn, fingerprintResult);
      if (isIn)
      {
        message = "Hi ";
      }
      else
      {
        message = "Bye ";
      }
      message = message + response;
      _clockMe.DisplayMessage(20, 200, message);

      messageNeedsToBeErase = startTimer = true;
    }
    else if (fingerprintResult == 255)
    {
      message = "Fingerprint scan failed";
      _clockMe.DisplayMessage(20, 200, message);

      messageNeedsToBeErase = startTimer = true;
    }
    else if (fingerprintResult == 253)
    {
      message = "Unknown fingerprint";
      _clockMe.DisplayMessage(20, 200, message);

      messageNeedsToBeErase = startTimer = true;
    }
  }


  if (messageNeedsToBeErase)
  {
    if (startTimer)
    {
      startTimer = false;
      previousMillisToEraseMessage = currentMillis;
    }

    if (currentMillis - previousMillisToEraseMessage > 2000) {
      previousMillisToEraseMessage = currentMillis;
      if (isState1)
      {
        if (isIn)
        {
          message = "Welcome!";
        }
        else
        {
          message = "Goodbye!";
        }
        _clockMe.DisplayMessage(20, 200, message);
        messageNeedsToBeErase = false;
      }
      else
      {
        _clockMe.DisplayMessage(20, 80, "Place another finger");
        uint8_t fingerprintResult = _clockMe.GetFingerprintEnroll();
        if (fingerprintResult == 254)
        {
          messageNeedsToBeErase = startTimer = true;
        }
        else
        {
          messageNeedsToBeErase = false;
        }
      }
    }
  }

  if (ts.touched())
  {
    TS_Point p = ts.getPoint();
    int x = 240 - p.x;
    int y = 320 - p.y;
    if (isState1)
    {
      if (x > 13 && x < 227 && y > 2 && y < 106)
      {
        if (isIn)
        {
          _clockMe.Draw(out, 13, 2);
          message = "Goodbye!";
          isIn = false;
        }
        else
        {
          _clockMe.Draw(in, 13, 2);
          message = "Welcome!";
          isIn = true;
        }

        _clockMe.DisplayMessage(20, 200, message);
      }
      else if (x > 18 && x < 222 && y > 250 && y < 314)
      {
        state2();
      }
    }
    else
    {
      if (x > 18 && x < 222 && y > 250 && y < 314)
      {
        state1(isIn);
      }
    }
  }
}

void state1(bool isIn)
{
  isState1 = true;
  _clockMe.ClearScreen();
  if (isIn)
  {
    _clockMe.Draw(in, 13, 2);
    message = "Welcome!";
  }
  else
  {
    _clockMe.Draw(out, 13, 2);
    message = "Goodbye!";
  }
  _clockMe.Draw(reg, 18, 250);
  _clockMe.DisplayMessage(20, 200, message);
  _clockMe.DisplayMessage(20, 150, _clockMe.GetTimeFromServer());
}

void state2()
{
  isState1 = false;
  _clockMe.ClearScreen();
  _clockMe.Draw(back, 18, 250);
  _clockMe.SetId();
  _clockMe.DisplayMessage(20, 80, "Place your finger");
  uint8_t fingerprintResult = _clockMe.GetFingerprintEnroll();
  if (fingerprintResult == 254)
  {
    messageNeedsToBeErase = startTimer = true;
  }
}
