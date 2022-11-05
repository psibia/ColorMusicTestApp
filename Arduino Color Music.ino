#include <FastLED.h>

#define LED_PIN        7          // Номер пина, куда подключена лента 
#define LED_COUNT 30 

CRGB leds[LED_COUNT];

void setup() 
{
  FastLED.addLeds<WS2811, LED_PIN, GRB>(leds, LED_COUNT).setCorrection( TypicalLEDStrip );
  
  Serial.begin(9600);
  Serial.setTimeout(10);  
  
  delay(2000);
  
  fill_solid( leds, LED_COUNT, CRGB(0, 0, 255));
  FastLED.show();
}

void loop() 
{
  if (Serial.available()) 
  {
    FastLED.setBrightness(Serial.read() * 2.55);
    FastLED.show();
  }
}
