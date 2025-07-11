//-----------------------------------------------------------------------------
//     Author : hiyohiyo
//       Mail : hiyohiyo@crystalmark.info
//        Web : http://openlibsys.org/
//    License : The modified BSD license
//
//                     Copyright 2007-2009 OpenLibSys.org. All rights reserved.
//-----------------------------------------------------------------------------
// This is support library for WinRing0 1.3.x.

namespace Universal_x86_Tuning_Utility.Services.Amd.Windows;

public class FanOls : Ols
{
    public FanOls()
    {
        DllNameX64 = "WinRing0x64_Fan";
        DllName = "WinRing0_Fan.dll";
    }
}