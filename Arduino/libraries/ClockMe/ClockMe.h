#include <Adafruit_Fingerprint.h>
#include <Adafruit_ILI9341.h>
#include <WiFi101.h>

class ClockMe{
	public:
		ClockMe();
		void Init();
		void SetId();
		void DisplayMessage(int16_t x, int16_t y, String message);
		void Draw(char *filename, int16_t x, int16_t y);
		void DrawQr();
		void ClearScreen();
		uint8_t GetFingerprintId();
		uint8_t GetFingerprintEnroll();
		uint8_t DeleteFingerprint(uint8_t id);
		String ArduinoRequest(uint8_t requestType, uint8_t fingerId);
	private:
		Adafruit_Fingerprint _finger = Adafruit_Fingerprint(&Serial1);;
		Adafruit_ILI9341 _tft = Adafruit_ILI9341(10, 9);
		WiFiClient _client;
		uint8_t _id;
		String ProcessIncomingMessages();
};