# C# Control for Zengge Wi-Fi LED Strips ("Magic Home")

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

```C# Control for Zengge Wi-Fi LED bulbs
ZenggewifiAPI.Device.State state = bulb.GetState();
```
Returns a status object which contains of these members:

```C#
state.isOn // Boolean whether device is on
state.type // Byte device type
state.color // Color object with current color
```

### Set Color

```C#
bulb.SetColorRGB(255,255,255); // set new RGB color
```

## To do's
- [ ] fully implement Getstate
- [ ] GetTimeRaw parser
- [ ] SetTimeRaw/SetTime
- [ ] GetTimersRaw parser
- [ ] SetTimersRaw
- [ ] code improvements
- [ ] SetColorWhite ???
- [ ] setting up new devices
- [ ] check checksum (currently using TCP, so not important)
- [ ] get ![image](https://user-images.githubusercontent.com/10454554/114270759-6b93cc80-9a0e-11eb-8673-127f1a282c35.png)
- [ ] get ![image](https://user-images.githubusercontent.com/10454554/114270779-7cdcd900-9a0e-11eb-9dd1-98c81eafd321.png)

