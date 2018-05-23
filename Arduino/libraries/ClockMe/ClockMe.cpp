#include <ClockMe.h>
#include <Arduino.h>
#include <Fonts\FreeSans9pt7b.h>

ClockMe::ClockMe() {
}

void ClockMe::Init()
{
	WiFi.setPins(8,7,5);
	IPAddress server(192, 168, 0, 164);
	
	_finger.begin(57600);
	_tft.begin();
	WiFi.begin("BUB", "Acasa12345");
	
	_client.connect(server, 81);
	
	_tft.setTextColor(0x0000);
	_tft.setFont(&FreeSans9pt7b);
}

void ClockMe::DisplayMessage(int16_t x, int16_t y, String message)
{
	_tft.fillRect(0, y - 20, 240, 30, 0xffff);
	_tft.setCursor(x, y);
	_tft.print(message);
}

void ClockMe::Draw(char *filename, int16_t x, int16_t y)
{
	_tft.bmpDraw(filename, x, y);
}
void ClockMe::ClearScreen()
{
	_tft.fillScreen(0xffff);
}

void ClockMe::SetId()
{
	_finger.getTemplateCount();
	_id = _finger.templateCount + 1;
}

String ClockMe::GetServerStatus() {
  _client.println("GET /Arduino/Status HTTP/1.1");
  _client.println("Host: localhost");
  _client.println();
  return ProcessIncomingMessages(false);
}

String ClockMe::GetTimeFromServer() {
  _client.println("GET /Arduino/Test HTTP/1.1");
  _client.println("Host: localhost");
  _client.println();
  return ProcessIncomingMessages(false);
}

String ClockMe::SendDataToServer(uint8_t requestType, uint8_t fingerId) {
  String request = "GET /Arduino/ArduinoRequest";
  request = request + "?p1=" + requestType + "&p2=" + fingerId + " HTTP/1.1";
  Serial.println(request);
  _client.println(request);
  _client.println("Host: localhost");
  _client.println();
  return ProcessIncomingMessages(true);
}

String ClockMe::ProcessIncomingMessages(bool ignore)
{
  int cnt = 0;
  String message;
  while (!_client.available()) {};
  while (_client.available()) {
    while (cnt < 10)
    {
      char c = _client.read();
      if (c == '\n')
      {
        cnt++;
      }
    }
    char c = _client.read();
    message += c;
  }
  Serial.println(message);
  if(!ignore)
  {
	return message;
  }
  return "";
}

uint8_t ClockMe::DeleteFingerprint(uint8_t id) {
  uint8_t p = -1;
  
  p = _finger.deleteModel(id);

  if (p == FINGERPRINT_OK) {
    Serial.println("Deleted!");
  } else if (p == FINGERPRINT_PACKETRECIEVEERR) {
    Serial.println("Communication error");
    return p;
  } else if (p == FINGERPRINT_BADLOCATION) {
    Serial.println("Could not delete in that location");
    return p;
  } else if (p == FINGERPRINT_FLASHERR) {
    Serial.println("Error writing to flash");
    return p;
  } else {
    Serial.print("Unknown error: 0x"); Serial.println(p, HEX);
    return p;
  }   
}

uint8_t ClockMe::GetFingerprintId() {
  uint8_t p = _finger.getImage();
  switch (p) {
    case FINGERPRINT_OK:
      Serial.println("Image taken");
      break;
    case FINGERPRINT_NOFINGER:
      //Serial.println("No finger detected");
      return -2;
    case FINGERPRINT_PACKETRECIEVEERR:
      Serial.println("Communication error");
      return -1;
    case FINGERPRINT_IMAGEFAIL:
      Serial.println("Imaging error");
      return -1;
    default:
      Serial.println("Unknown error");
      return -1;
  }

  // OK success!

  p = _finger.image2Tz();
  switch (p) {
    case FINGERPRINT_OK:
      Serial.println("Image converted");
      break;
    case FINGERPRINT_IMAGEMESS:
      Serial.println("Image too messy");
      return -1;
    case FINGERPRINT_PACKETRECIEVEERR:
      Serial.println("Communication error");
      return -1;
    case FINGERPRINT_FEATUREFAIL:
      Serial.println("Could not find fingerprint features");
      return -1;
    case FINGERPRINT_INVALIDIMAGE:
      Serial.println("Could not find fingerprint features");
      return -1;
    default:
      Serial.println("Unknown error");
      return -1;
  }
  
  // OK converted!
  p = _finger.fingerFastSearch();
  if (p == FINGERPRINT_OK) {
    Serial.println("Found a print match!");
  } else if (p == FINGERPRINT_PACKETRECIEVEERR) {
    Serial.println("Communication error");
    return -1;
  } else if (p == FINGERPRINT_NOTFOUND) {
    Serial.println("Not found");
    return -3;
  } else {
    Serial.println("Unknown error");
    return -1;
  }   
  
  // found a match!
  return _finger.fingerID;
}

uint8_t ClockMe::GetFingerprintEnroll() {

  int p = -1;
  while (p != FINGERPRINT_OK) {
    p = _finger.getImage();
    switch (p) {
    case FINGERPRINT_OK:
      Serial.println("Image taken");
      break;
    case FINGERPRINT_NOFINGER:
      //Serial.println(".");
      break;
    case FINGERPRINT_PACKETRECIEVEERR:
      Serial.println("Communication error");
      break;
    case FINGERPRINT_IMAGEFAIL:
      Serial.println("Imaging error");
      break;
    default:
      Serial.println("Unknown error");
      break;
    }
  }

  // OK success!

  p = _finger.image2Tz(1);
  switch (p) {
    case FINGERPRINT_OK:
      Serial.println("Image converted");
      break;
    case FINGERPRINT_IMAGEMESS:
      Serial.println("Image too messy");
      return p;
    case FINGERPRINT_PACKETRECIEVEERR:
      Serial.println("Communication error");
      return p;
    case FINGERPRINT_FEATUREFAIL:
      Serial.println("Could not find fingerprint features");
      return p;
    case FINGERPRINT_INVALIDIMAGE:
      Serial.println("Could not find fingerprint features");
      return p;
    default:
      Serial.println("Unknown error");
      return p;
  }
  
  p = _finger.fingerFastSearch();
  if (p == FINGERPRINT_OK) {
	DisplayMessage(20,80,"Fingerprint already exists");
	return -2;
  } else{
  
  DisplayMessage(20,80,"Remove finger");
  Serial.println("Remove finger");
  delay(2000);
  p = 0;
  while (p != FINGERPRINT_NOFINGER) {
    p = _finger.getImage();
  }
  Serial.print("ID "); Serial.println(_id);
  p = -1;
  DisplayMessage(20,80,"Place same finger again");
  Serial.println("Place same finger again");
  while (p != FINGERPRINT_OK) {
    p = _finger.getImage();
    switch (p) {
    case FINGERPRINT_OK:
      Serial.println("Image taken");
      break;
    case FINGERPRINT_NOFINGER:
      break;
    case FINGERPRINT_PACKETRECIEVEERR:
      Serial.println("Communication error");
      break;
    case FINGERPRINT_IMAGEFAIL:
      Serial.println("Imaging error");
      break;
    default:
      Serial.println("Unknown error");
      break;
    }
  }

  // OK success!

  p = _finger.image2Tz(2);
  switch (p) {
    case FINGERPRINT_OK:
      Serial.println("Image converted");
      break;
    case FINGERPRINT_IMAGEMESS:
      Serial.println("Image too messy");
      return p;
    case FINGERPRINT_PACKETRECIEVEERR:
      Serial.println("Communication error");
      return p;
    case FINGERPRINT_FEATUREFAIL:
      Serial.println("Could not find fingerprint features");
      return p;
    case FINGERPRINT_INVALIDIMAGE:
      Serial.println("Could not find fingerprint features");
      return p;
    default:
      Serial.println("Unknown error");
      return p;
  }
  
  // OK converted!
  Serial.print("Creating model for #");  Serial.println(_id);
  
  p = _finger.createModel();
  if (p == FINGERPRINT_OK) {
    Serial.println("Prints matched!");
  } else if (p == FINGERPRINT_PACKETRECIEVEERR) {
    Serial.println("Communication error");
    return p;
  } else if (p == FINGERPRINT_ENROLLMISMATCH) {
    Serial.println("Fingerprints did not match");
    return p;
  } else {
    Serial.println("Unknown error");
    return p;
  }   
  
  Serial.print("ID "); Serial.println(_id);
  p = _finger.storeModel(_id);
  if (p == FINGERPRINT_OK) {
    Serial.println("Stored!");
  } else if (p == FINGERPRINT_PACKETRECIEVEERR) {
    Serial.println("Communication error");
    return p;
  } else if (p == FINGERPRINT_BADLOCATION) {
    Serial.println("Could not store in that location");
    return p;
  } else if (p == FINGERPRINT_FLASHERR) {
    Serial.println("Error writing to flash");
    return p;
  } else {
    Serial.println("Unknown error");
    return p;
  }   

  DisplayMessage(20,80,"Register done");
  SendDataToServer(2, _id);
  }
}