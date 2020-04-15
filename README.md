# Fermenter
This project aims at building an Raspberry Pi controlled Sourdough fermenting box.

The idea is to build a box to control and monitor the fermentation of sourdough.

Controller Equipment:
- Raspberry Pi Zero W with Raspian Buster Lite and mono-complete (5.18)
- A SSD1306 monochrom display is connected according to https://indibit.de/raspberry-pi-oled-display-128x64-mit-python-ansteuern-i2c/
- As a temperature sensor I choose a BMP280, because it is also a humidity sensor.

I searched for quite a while to find a good and simple library to display things and ended up with https://github.com/iot-sas/SSD1306-Sharp which is fast and does not have dependencies. I am adding drawing to the library, so I use my fork to build my sample.

For development of the low level stuff, I can recommend using the VSMonoDebugger: https://marketplace.visualstudio.com/items?itemName=GordianDotNet.VSMonoDebugger0d62
