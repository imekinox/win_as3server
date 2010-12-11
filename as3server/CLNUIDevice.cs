//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// This library is part of CL NUI SDK
// It allows the use of Microsoft Kinect cameras in your own applications
//
// For updates and file downloads go to: http://codelaboratories.com/get/kinect
//
// Copyright 2010 (c) Code Laboratories, Inc.  All rights reserved.
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace as3server
{
    public class CLNUIDevice
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // NUIDevice  API
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport("CLNUIDevice.dll", EntryPoint = "GetNUIDeviceCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetDeviceCount();
        [DllImport("CLNUIDevice.dll", EntryPoint = "GetNUIDeviceSerial", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetDeviceSerial(int index);

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CLNUIMotor  API
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport("CLNUIDevice.dll", EntryPoint = "CreateNUIMotor", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateMotor(string serial);
        [DllImport("CLNUIDevice.dll", EntryPoint = "DestroyNUIMotor", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyMotor(IntPtr motor);
        [DllImport("CLNUIDevice.dll", EntryPoint = "GetNUIMotorSerial", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetMotorSerial(IntPtr motor);
        [DllImport("CLNUIDevice.dll", EntryPoint = "SetNUIMotorPosition", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetMotorPosition(IntPtr motor, short position);
        [DllImport("CLNUIDevice.dll", EntryPoint = "GetNUIMotorAccelerometer", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetMotorAccelerometer(IntPtr motor, ref short x, ref short y, ref short z);
        [DllImport("CLNUIDevice.dll", EntryPoint = "SetNUIMotorLED", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetMotorLED(IntPtr motor, byte mode);
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CLNUICamera API
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport("CLNUIDevice.dll", EntryPoint = "CreateNUICamera", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateCamera(string serial);
        [DllImport("CLNUIDevice.dll", EntryPoint = "DestroyNUICamera", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DestroyCamera(IntPtr camera);
        [DllImport("CLNUIDevice.dll", EntryPoint = "StartNUICamera", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StartCamera(IntPtr camera);
        [DllImport("CLNUIDevice.dll", EntryPoint = "StopNUICamera", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StopCamera(IntPtr camera);

        [DllImport("CLNUIDevice.dll", EntryPoint = "GetNUICameraColorFrameRAW", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetCameraColorFrameRAW(IntPtr camera, IntPtr data, int timeout);
        [DllImport("CLNUIDevice.dll", EntryPoint = "GetNUICameraColorFrameRGB24", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetCameraColorFrameRGB24(IntPtr camera, IntPtr data, int timeout);
        [DllImport("CLNUIDevice.dll", EntryPoint = "GetNUICameraColorFrameRGB32", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetCameraColorFrameRGB32(IntPtr camera, IntPtr data, int timeout);

        [DllImport("CLNUIDevice.dll", EntryPoint = "GetNUICameraDepthFrameRAW", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetCameraDepthFrameRAW(IntPtr camera, IntPtr data, int timeout);
        [DllImport("CLNUIDevice.dll", EntryPoint = "GetNUICameraDepthFrameCorrected12", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetCameraDepthFrameCorrected12(IntPtr camera, IntPtr data, int timeout);
        [DllImport("CLNUIDevice.dll", EntryPoint = "GetNUICameraDepthFrameCorrected8", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetCameraDepthFrameCorrected8(IntPtr camera, IntPtr data, int timeout);
        [DllImport("CLNUIDevice.dll", EntryPoint = "GetNUICameraDepthFrameRGB32", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetCameraDepthFrameRGB32(IntPtr camera, IntPtr data, int timeout);
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    }
}
