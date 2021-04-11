# C# API for Zengge Wi-Fi LED Strips ("Magic Home")
[![CodeQL](https://github.com/itmkehrer/ZenggewifiAPI/actions/workflows/codeql-analysis.yml/badge.svg?branch=master)](https://github.com/itmkehrer/ZenggewifiAPI/actions/workflows/codeql-analysis.yml)

Tested with ZJ-MWIR-RGB, but should also work with ZJ-MWIR-RGBW. Maybe other controllers are also supported.
## Example usage

### Connection

```C#

ZenggewifiAPI.Device bulb = new ZenggewifiAPI.Device("examplehostname.local");
bulb.Connect();

//some Code

bulb.Close();
```

### Turn on/off

```C#
bulb.SetPower(true);
bulb.SetPower(false);
```

### Get State

```C#
ZenggewifiAPI.Device.State state = bulb.GetState();
```
Returns a status object which contains of these members:

```C#
state.isOn // Boolean whether device is on
state.type // Byte device type
state.color // Color object with current color
state.mode // Byte device mode
state.slowness // Byte current slowness
state.ledVersionNum // Byte version of LED
```

### Get/Set Time

```C#
bulb.GetTime(); // DateTime with current device time

bulb.SetTime(DateTime.Now); // set current device time
```

### Set Color

```C#
bulb.SetColorRGB(255,255,255); // set new RGB color
```

## To do's
- [~] fully implement Getstate
- [x] GetTime
- [x] SetTime
- [ ] GetTimersRaw parser
- [ ] SetTimersRaw
- [ ] code improvements
- [ ] SetColorWhite - not tested (own only a RGB device)
- [ ] setting up new devices
- [ ] check checksum (currently using TCP, so not important)
- [ ] get ![image](https://user-images.githubusercontent.com/10454554/114270759-6b93cc80-9a0e-11eb-8673-127f1a282c35.png)
- [ ] get ![image](https://user-images.githubusercontent.com/10454554/114270779-7cdcd900-9a0e-11eb-9dd1-98c81eafd321.png)

