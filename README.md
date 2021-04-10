# C# Control for Zengge Wi-Fi LED bulbs

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
