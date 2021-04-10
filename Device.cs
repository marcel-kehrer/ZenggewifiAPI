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

            //0->81(COMMANDGETSTATE)
            public byte type;//1->33(51)(DeviceType)
            public bool isOn; //2->24(OFF)/23(ON)
            //int mode;//3->61(COMMANDSETMODE)
            //4->23(ON)
            //int slowness;//5->01
            public Color color; // 6->E6; 7->1C; 8->00; 9->00; 12->0F
            //10->09(9)[VERSION?]; 11->00
            //int ledVersionNum; //10->77(119)

            public State(byte [] data)
            {

                if(data[2] == DeviceCommands.OFF)
                {
                    this.isOn = false;
                } else if(data[2] == DeviceCommands.ON)
                {
                    this.isOn = true;
                }
                this.type = data[1];
                //this.mode = mode;
                //this.slowness = slowness;
                this.color = new Color(data[6], data[7], data[8], data[9], data[12]);
                //this.ledVersionNum = ledVersionNum;
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
                    byte[] readData = new byte[connection.ReceiveBufferSize];
                    stream.Read(readData, 0, (int)connection.ReceiveBufferSize);
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
                    byte[] readData = new byte[connection.ReceiveBufferSize];
                    stream.Read(readData, 0, (int)connection.ReceiveBufferSize);
                    DebugConsoleMessage("Resv getTimeRaw: " + BitConverter.ToString(readData));
                    return readData; // 0f 11 14 15 04 0a 0a 2D 31 06 00 c5 // 10.4.2021 10:45:49
                    // as decimal       15 17 20 21 04 10 10 45 49 06
                    //                        2021.04.10 10:45:49
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

        /*public Boolean SetTimeRaw() // (10 14) 15 04 0a 0a 2d 36 06 00 0f c9
        {
            byte[] sendData;
            sendData = new byte[] { DeviceCommands.SETTIME, DeviceCommands.SETTIME2 }; // vorletzte ist false und letzte checksum

            return false; // 0f 11 14 15 04 0a 0a 2d 36 06 00 ca
        }*/

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
