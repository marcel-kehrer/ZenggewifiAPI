using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ZenggewifiAPI
{
    public class Device
    {
        private readonly IPEndPoint ledHost;
        TcpClient connection;
        NetworkStream stream;

        const int PORT = 5577;
        //const int TIMEOUT = 5;
        const int BUFFERMIDDLE = 14;
        public Device(String hostname)
        {
            ledHost = new IPEndPoint(Dns.GetHostEntry(hostname).AddressList[0], PORT);
        }

        public Boolean Connect()
        {
            connection = new TcpClient();
            try
            {
                connection.Connect(ledHost);
                stream = connection.GetStream();
                return connection.Connected;
            }
            catch
            {
                return false;
            }
        }
        public Boolean Close()
        {
            connection.Close();
            return !connection.Connected;
        }

        [System.Diagnostics.ConditionalAttribute("DEBUG")]
        public static void DebugConsoleMessage(String msg)
        {
            Console.WriteLine(msg);
        }

        public static byte GetChecksum(byte[] inputData)
        {
            byte sum = 0;
            foreach (byte data in inputData) {
                sum += data;
            }
            return (byte)(sum % 256);
        }
        public class State
        {
            //getState 81 8a 8b 96

            private byte command; //0->81(COMMANDGETSTATE)
            public byte type;//1->33(51)(DeviceType)
            public bool isOn; //2->24(OFF)/23(ON)
            public byte mode;//3->61(COMMANDSETMODE)->97
            private byte unknownValue1;//4->23(ON)->35
            public byte slowness;//5->01->16
            public Color color; // 6->E6; 7->1C; 8->00; 9->00; 12->0F
            public byte ledVersionNum; //10->09(9)[VERSION?]; 11->00

            public State(byte [] data)
            {
                this.command = data[0];
                this.type = data[1];
                if (data[2] == DeviceCommands.OFF)
                {
                    this.isOn = false;
                } else if(data[2] == DeviceCommands.ON)
                {
                    this.isOn = true;
                }
                this.mode = data[3];
                this.unknownValue1 = data[4];
                this.slowness = data[5];
                this.color = new Color(data[6], data[7], data[8], data[9], data[12]);
                this.ledVersionNum = data[10];
            }

            public String GetDataAsString()
            {
                return "Command: " + this.command + "\n Type: " + this.type + "\n isOn: " + this.isOn + "\n Mode: " + this.mode + "\n UnknownValue: " + this.unknownValue1 + "\n Slowness: " + this.slowness + "\n Color: " + this.color + "\n ledVersionNum: " + this.ledVersionNum;
            }

        }
        public class Color
        {
            public byte r, g, b, w, ignoreWRaw;
            public bool ignoreW;
            
            public Color(byte r, byte g, byte b, byte w, byte ignoreW)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.w = w;
                this.ignoreWRaw = ignoreW;

                if (ignoreW == DeviceCommands.OFF)
                {
                    this.ignoreW = false;
                }
                else if (ignoreW == DeviceCommands.ON)
                {
                    this.ignoreW = true;
                }

            }
            public byte[] Getcolor() {

                return new byte[] { r, g, b, w, ignoreWRaw };
            }
        }
        public Boolean SendChecked(byte[] sendData)
        {
            byte[] tmp = sendData.Concat(new byte[] { DeviceCommands.FALSE }).ToArray();
            tmp = tmp.Concat(new byte[] { GetChecksum(tmp) }).ToArray();
            try
            {
                stream.Write(tmp, 0, tmp.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

            public Boolean SetPower(bool state)
        {
            byte[] sendData;
            if (state)
            {
                sendData = new byte[] { DeviceCommands.SETPOWER, DeviceCommands.ON };
            }  else {
                sendData = new byte[] { DeviceCommands.SETPOWER, DeviceCommands.OFF };
            }
            try
            {
                DebugConsoleMessage("Send setPower: " + BitConverter.ToString(sendData));
                stream.Write(sendData, 0, sendData.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public byte[] GetStateRaw()
        {
            byte[] sendData;
            sendData = new byte[] { DeviceCommands.GETSTATE, DeviceCommands.GETSTATE2, DeviceCommands.GETSTATE3, GetChecksum(new byte[] { DeviceCommands.GETSTATE, DeviceCommands.GETSTATE2, DeviceCommands.GETSTATE3 }) };
            DebugConsoleMessage("Send getState: " + BitConverter.ToString(sendData));
            try
            {
                stream.Write(sendData, 0, sendData.Length);
                if (stream.CanRead)
                {
                    byte[] readData = new byte[BUFFERMIDDLE];
                    stream.Read(readData, 0, BUFFERMIDDLE);
                    DebugConsoleMessage("Resv getState: " + BitConverter.ToString(readData));
                    return readData;
                } else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        public State GetState()
        {
            byte[] rawState = GetStateRaw();
            if (rawState.Length > 0)
            {
                State currentState = new State(rawState);
                return currentState;
            } else
            {
                return null;
            }
            
        }
        private static byte[] FormatColor(Color color)
        {
            byte[] preData;
            byte ignoreW;
            if (color.ignoreW){
                ignoreW = DeviceCommands.TRUE;
            }
            else
            {
                ignoreW = DeviceCommands.FALSE;
            }

            preData = new byte[] { DeviceCommands.SETCOLOR, (byte)color.r, (byte)color.g, (byte)color.b, (byte)color.w, ignoreW};

            return preData;
        }
        public Boolean SetColorRGB(byte red, byte green, byte blue)
        {
            
            byte[] sendData;
            sendData = FormatColor(new Color(red, green, blue, 0, DeviceCommands.TRUE));
            DebugConsoleMessage("Send setColorRGB: " + BitConverter.ToString(sendData));
            if (SendChecked(sendData))
            {
                return true;
            } else
            {
                return false;
            }
        }
        public Boolean SetColorWhite(byte white)
        {
            byte[] sendData;
            sendData = FormatColor(new Color(0, 0, 0, white, DeviceCommands.FALSE));
            DebugConsoleMessage("Send setColorWhite: " + BitConverter.ToString(sendData));
            if (SendChecked(sendData))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public byte[] GetTimeRaw() // 11 1a 1b 0f 55
        {
            byte[] sendData;
            sendData = new byte[] { DeviceCommands.GETTIME, DeviceCommands.GETTIME2, DeviceCommands.GETTIME3, DeviceCommands.FALSE };
            sendData = sendData.Concat(new byte[] { GetChecksum(sendData) }).ToArray();
            DebugConsoleMessage("Send getTimeRaw: " + BitConverter.ToString(sendData));
            try
            {
                stream.Write(sendData, 0, sendData.Length);
                if (stream.CanRead)
                {
                    byte[] readData = new byte[BUFFERMIDDLE];
                    stream.Read(readData, 0, BUFFERMIDDLE);
                    DebugConsoleMessage("Resv getTimeRaw: " + BitConverter.ToString(readData));
                    return readData; // 0f 11 14 15 04 0a 0a 2D 31 06 00 c5 // 10.4.2021 10:45:49
                    // as decimal       15 17 20 21 04 10 10 45 49 06 
                    //                        2021.04.10 10:45:49 DayOfWeek(0-6)6=>Saturday;0=>Sunday 00(dont know what this is) Checksum
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public DateTime GetTime()
        {
            byte[] rawTime = GetTimeRaw();
            int year = int.Parse(string.Concat(rawTime[2],rawTime[3]));
            int month = rawTime[4];
            int day = rawTime[5];
            int hour = rawTime[6];
            int minutes = rawTime[7];
            int seconds = rawTime[8];
            int dayOfWeek = rawTime[9];

            DebugConsoleMessage("GetTime year: " + year + " month: "+ month+" day: "+day+"  hour: "+hour+ " minutes: "+minutes+" seconds: "+seconds+ " dayOfWeek: "+dayOfWeek);


            return new DateTime(year, month, day, hour, minutes, seconds);
        }

        public Boolean SetTime(DateTime newTime) // (10) 14 15 04 0a 0a 2d 36 06 00 0f c9
                                                 // 10-14-15-04-0B-0A-16-0E-00
        {
            byte[] sendData;
            char[] yearC = newTime.Year.ToString().ToCharArray();
            Int16 year1 = Int16.Parse(yearC[0].ToString() + yearC[1].ToString());
            Int16 year2 = Int16.Parse(yearC[2].ToString() + yearC[3].ToString());


            /*int year = int.Parse(string.Concat(BitConverter.GetBytes(year1)[0], BitConverter.GetBytes(year2)[0]));
            int month = (byte)newTime.Month;
            int day = (byte)newTime.Day;
            int hour = (byte)newTime.Hour;
            int minutes = (byte)newTime.Minute;
            int seconds = (byte)newTime.Second;
            int dayOfWeek = (byte)newTime.DayOfWeek;
            DebugConsoleMessage("GetTime year: " + year + " month: " + month + " day: " + day + "  hour: " + hour + " minutes: " + minutes + " seconds: " + seconds + " dayOfWeek: " + dayOfWeek);*/

            // dont know what the last byte 0x00 is
            sendData = new byte[] { DeviceCommands.SETTIME, BitConverter.GetBytes(year1)[0], BitConverter.GetBytes(year2)[0], (byte)newTime.Month, (byte)newTime.Day, (byte)newTime.Hour, (byte)newTime.Minute, (byte)newTime.Second, (byte)newTime.DayOfWeek, 0x00 }; // vorletzte ist false und letzte checksum
            DebugConsoleMessage("SendChecked SetTimeRaw: " + BitConverter.ToString(sendData));
            return SendChecked(sendData); // 0f 11 14 15 04 0a 0a 2d 36 06 00 ca

            //the device now send back the new time a thew times, but not checked yet
        }

        public byte[] GetTimersRaw() // 22 2a 2b 0f 86
        {
            byte[] sendData;
            sendData = new byte[] { DeviceCommands.GETTIMERS, DeviceCommands.GETTIMERS2, DeviceCommands.GETTIMERS3, DeviceCommands.FALSE };
            sendData = sendData.Concat(new byte[] { GetChecksum(sendData) }).ToArray();
            DebugConsoleMessage("Send getTimersRaw: " + BitConverter.ToString(sendData));
            try
            {
                stream.Write(sendData, 0, sendData.Length);
                if (stream.CanRead)
                {
                    byte[] readData = new byte[connection.ReceiveBufferSize];
                    stream.Read(readData, 0, (int)connection.ReceiveBufferSize);
                    DebugConsoleMessage("Resv getTimersRaw: " + BitConverter.ToString(readData));
                    return readData; // 0F-22-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-31- and a lot of 00
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
