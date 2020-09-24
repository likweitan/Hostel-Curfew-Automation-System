/*
* Read a card using a mfrc522 reader on your SPI interface
* Pin layout should be as follows (on Arduino Uno):
* MOSI: Pin 11 / ICSP-4
* MISO: Pin 12 / ICSP-1
* SCK: Pin 13 / ISCP-3
* SS: Pin 10
* RST: Pin 9
*/

#include <SPI.h>
#include <RFID.h>

#define SS_PIN 10
#define RST_PIN 9

#define STEPS 32

#define STEPPER_PIN_1 3
#define STEPPER_PIN_2 4
#define STEPPER_PIN_3 5
#define STEPPER_PIN_4 6
int step_number = 0;

RFID rfid(SS_PIN,RST_PIN);



int startAlarm = false;
int resetAlarm = 4;
int relay = 7;
int alarm = 8;

int serNum[5];

int cards[][5] = {
  {209,128,106,69,126},
  {101,220,213,229,137},
  {6,213,56,27,240}
};

bool access = false;

int val = 0;

void setup(){

    Serial.begin(9600);
    SPI.begin();
    rfid.init();
    pinMode(resetAlarm, INPUT);
    pinMode(relay, OUTPUT);
    pinMode(alarm, OUTPUT);
    digitalWrite(relay, HIGH);
    attachInterrupt(0, reset_alarm, LOW);

    pinMode(STEPPER_PIN_1, OUTPUT);
    pinMode(STEPPER_PIN_2, OUTPUT);
    pinMode(STEPPER_PIN_3, OUTPUT);
    pinMode(STEPPER_PIN_4, OUTPUT);
    
}

void loop(){

 
    if(rfid.isCard()){
    
        if(rfid.readCardSerial()){
            Serial.print(rfid.serNum[0]);
            Serial.print(" ");
            Serial.print(rfid.serNum[1]);
            Serial.print(" ");
            Serial.print(rfid.serNum[2]);
            Serial.print(" ");
            Serial.print(rfid.serNum[3]);
            Serial.print(" ");
            Serial.print(rfid.serNum[4]);
            Serial.println("");
            
            for(int x = 0; x < sizeof(cards); x++){
              for(int i = 0; i < sizeof(rfid.serNum); i++ ){
                  if(rfid.serNum[i] != cards[x][i]) {
                      access = false;
                      break;
                  } else {
                      access = true;
                      // turn off tone function for pin 8:
  noTone(8);
  // play a note on pin 6 for 200 ms:
  tone(6, 440, 200);
  delay(200);

  // turn off tone function for pin 6:
  noTone(6);
  // play a note on pin 7 for 500 ms:
  tone(7, 494, 500);
  delay(500);

  // turn off tone function for pin 7:
  noTone(7);
  // play a note on pin 8 for 300 ms:
  tone(8, 523, 300);
  delay(300);
  OneStep(false);
  delay(2);
                  }
              }
              if(access) break;
            }
           
        }
        
       if(access){
          Serial.println("Welcome!"); 
          startAlarm = false;
          digitalWrite(relay, LOW);          
       } else {
           Serial.println("Not allowed!"); 
           startAlarm = true; 
            digitalWrite(relay, HIGH);         
       }        
    }
    
    if(startAlarm) {
       digitalWrite(alarm, HIGH); 
    } else {
      digitalWrite(alarm, LOW); 
    }
    
    rfid.halt();

}

void reset_alarm(){
    startAlarm = false;
}
