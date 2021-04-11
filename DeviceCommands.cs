using System;
using System.Collections.Generic;
using System.Text;

namespace ZenggewifiAPI
{
    public static class DeviceCommands
    {
        public const Int32 SETPOWER = 0x71;
        public const Int32 ON = 0x23;
        public const Int32 OFF = 0x24;

        public const Int32 TRUE = 0xF0;
        public const Int32 FALSE = 0x0F;

        public const Int32 SETTIME = 0x10;
        //public const Int32 SETTIME2 = 0x14; -no command = 20 of year 2021
        public const Int32 GETTIME = 0x11;
        public const Int32 GETTIME2 = 0x1A;
        public const Int32 GETTIME3 = 0x1B;
        public const Int32 GETTIMERS = 0x22;
        public const Int32 GETTIMERS2 = 0x2A;
        public const Int32 GETTIMERS3 = 0x2B;

        public const Int32 SETCOLOR = 0x31;
        public const Int32 SETCOLORMUSIC = 0x41;

        public const Int32 SETMODE = 0x61;

        public const Int32 GETSTATE = 0x81;
        public const Int32 GETSTATE2 = 0x8A;
        public const Int32 GETSTATE3 = 0x8B;

        /*
            MODECOLOR    = 97
            MODEMUSIC    = 98
            MODECUSTOM   = 35
            MODEPRESET1  = 37
            MODEPRESET2  = 38
            MODEPRESET3  = 39
            MODEPRESET4  = 40
            MODEPRESET5  = 41
            MODEPRESET6  = 42
            MODEPRESET7  = 43
            MODEPRESET8  = 44
            MODEPRESET9  = 45
            MODEPRESET10 = 46
            MODEPRESET11 = 47
            MODEPRESET12 = 48
            MODEPRESET13 = 49
            MODEPRESET14 = 50
            MODEPRESET15 = 51
            MODEPRESET16 = 52
            MODEPRESET17 = 53
            MODEPRESET18 = 54
            MODEPRESET19 = 55
            MODEPRESET20 = 56
         * */
    }
}
