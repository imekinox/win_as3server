﻿/*
 * This file is part of the as3server Project. http://www.as3server.org
 *
 * Copyright (c) 2010 individual as3server contributors. See the CONTRIB file
 * for details.
 *
 * This code is licensed to you under the terms of the Apache License, version
 * 2.0, or, at your option, the terms of the GNU General Public License,
 * version 2.0. See the APACHE20 and GPL2 files for the text of the licenses,
 * or the following URLs:
 * http://www.apache.org/licenses/LICENSE-2.0
 * http://www.gnu.org/licenses/gpl-2.0.txt
 *
 * If you redistribute this file in source form, modified or unmodified, you
 * may:
 *   1) Leave this header intact and distribute it under the same terms,
 *      accompanying it with the APACHE20 and GPL20 files, or
 *   2) Delete the Apache 2.0 clause and accompany it with the GPL2 file, or
 *   3) Delete the GPL v2 clause and accompany it with the APACHE20 file
 * In all cases you must keep the copyright notice intact and include a copy
 * of the CONTRIB file.
 *
 * Binary distributions must follow the binary distribution requirements of
 * either License.
 */

using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Runtime.InteropServices;

namespace as3server
{
  class Server
  {
    private static IntPtr motor = IntPtr.Zero;
    private static IntPtr camera = IntPtr.Zero;
	private static int _devCount;
    private static String devSerial;
	private static String MotorSerial;
		
    private TcpListener depthSocket;
    private TcpListener rgbSocket;
    private TcpListener dataSocket;
    private Thread depthListenerThread;
    private Thread rgbListenerThread;
    private Thread dataListenerThread;
    private Boolean depth_is_connected = false;
    private Boolean rgb_is_connected = false;
    private Boolean data_is_connected = false;
    private Boolean sent_data_policy = false;
	private Boolean sent_depth_policy = false;
	private Boolean sent_rgb_policy = false;
	
	public static void Main() {
		try
		{
			//counting the attached Kinect devices
			_devCount = CLNUIDevice.GetDeviceCount();
			if (_devCount == 0)//No Device Found EXIT
			{
				Environment.Exit(0);
			}
			for (int i = 0; i < _devCount; i++) //Start Motor and Camera for every device found
			{
				//TODO adding them to an object array. For now works for one device.
				devSerial = CLNUIDevice.GetDeviceSerial(i);
				motor = CLNUIDevice.CreateMotor(devSerial);
				camera = CLNUIDevice.CreateCamera(devSerial);
				CLNUIDevice.SetMotorPosition(motor, 0000); //Reset motor to 0 degrees
				CLNUIDevice.SetMotorLED(motor, (byte)0); //ShutDown the LED
			}
			new Server();
		}
		catch (System.Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}
		
    public Server()
    {
      this.depthSocket = new TcpListener(IPAddress.Any, 6001);
      this.depthListenerThread = new Thread(new ThreadStart(depthWaitForConnection));
      this.depthListenerThread.Start();
			
      this.rgbSocket = new TcpListener(IPAddress.Any, 6002);
      this.rgbListenerThread = new Thread(new ThreadStart(rgbWaitForConnection));
      this.rgbListenerThread.Start();

      this.dataSocket = new TcpListener(IPAddress.Any, 6003);
      this.dataListenerThread = new Thread(new ThreadStart(dataWaitForConnection));
      this.dataListenerThread.Start();

    }

    private void send_motor_serial(NetworkStream clientStream)
    {
        MotorSerial = CLNUIDevice.GetMotorSerial(motor);
		System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
        clientStream.BeginWrite(encoding.GetBytes(MotorSerial), 0, MotorSerial.Length, null, null);
        Console.WriteLine(encoding.GetBytes(MotorSerial).Length);
	}

    private void send_policy_file(NetworkStream clientStream)
    {
        string str = "<?xml version='1.0'?><!DOCTYPE cross-domain-policy SYSTEM '/xml/dtds/cross-domain-policy.dtd'><cross-domain-policy><site-control permitted-cross-domain-policies='all'/><allow-access-from domain='*' to-ports='*'/></cross-domain-policy>\n";
        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
		Console.WriteLine("policy_file: size = " + str.Length);
        clientStream.BeginWrite(encoding.GetBytes(str), 0, str.Length, null, null);
    }

    private void depthWaitForConnection()
    {
        this.depthSocket.Start();
        while(true){
            Console.WriteLine("## Wait depth client");
            TcpClient depthClient = this.depthSocket.AcceptTcpClient();

            depth_is_connected = true;
            Console.WriteLine("Depth Client Conected");

            Thread depthOutThread = new Thread(new ParameterizedThreadStart(depth_out));
            depthOutThread.Start(depthClient);
            Console.WriteLine("depth_out: thread created");
        }
    }

    private unsafe void depth_out(object client)
    {
        TcpClient theClient = (TcpClient)client;
        NetworkStream clientStream = theClient.GetStream();
        CLNUIDevice.StartCamera(camera);
		if(!sent_depth_policy){
        	send_policy_file(clientStream);
			sent_depth_policy = true;
		}
        int i;
        short[] raw_depth = new short[640*480];
        IntPtr depthRAW = Marshal.AllocHGlobal(640 * 480 * 2);
        byte[] buf_depth = new byte[640*480*4];

        while (depth_is_connected)
        {
            CLNUIDevice.GetCameraDepthFrameRAW(camera, depthRAW, 0);
            Marshal.Copy(depthRAW, raw_depth, 0, 640 * 480);
		    for (i=0; i<640 * 480; i++) {
			    buf_depth[4 * i + 0] = 0x00; //B
                buf_depth[4 * i + 1] = 0x00; //G
			    buf_depth[4 * i + 2] = 0x00; //R
			    buf_depth[4 * i + 3] = 0xFF;
                //Console.WriteLine("if (" + raw_depth[i] + " < 2000");
                if (raw_depth[i] < 800 && raw_depth[i] > 600)
                {
				    buf_depth[4 * i + 0] = 0xFF;
				    buf_depth[4 * i + 1] = 0xFF;
				    buf_depth[4 * i + 2] = 0xFF;
				    buf_depth[4 * i + 3] = 0xFF;
			    }
		    }
            clientStream.Write(buf_depth, 0, buf_depth.Length);
        }
        Marshal.FreeHGlobal(depthRAW);
        CLNUIDevice.StopCamera(camera);
        Console.WriteLine("depth_out: closed");
        theClient.Close();
    }

    private void rgbWaitForConnection()
    {
        this.rgbSocket.Start();
        while (true)
        {
            Console.WriteLine("## Wait rgb client");
            TcpClient rgbClient = this.rgbSocket.AcceptTcpClient();

            rgb_is_connected = true;
            Console.WriteLine("RGB Client Conected");

            Thread rgbOutThread = new Thread(new ParameterizedThreadStart(rgb_out));
            rgbOutThread.Start(rgbClient);
            Console.WriteLine("rgb_out: thread created");
        }
    }

    private unsafe void rgb_out(object client)
    {
        TcpClient theClient = (TcpClient)client;
        NetworkStream clientStream = theClient.GetStream();
        CLNUIDevice.StartCamera(camera);
		if(!sent_rgb_policy){
        	send_policy_file(clientStream);
			sent_rgb_policy = true;
		}
        //int i;
        byte[] buf_rgb = new byte[640*480*4];
        //short[] raw_rgb = new short[640*480];
        IntPtr rgb32 = Marshal.AllocHGlobal(640 * 480 * 4);

        while (rgb_is_connected)
        {
            CLNUIDevice.GetCameraColorFrameRGB32(camera, rgb32, 0);
            Marshal.Copy(rgb32, buf_rgb, 0, 640 * 480 * 4);
		    /*for (i=0; i<640 * 480; i++) {
			    buf_depth[4 * i + 0] = 0x00; //B
                buf_depth[4 * i + 1] = 0x00; //G
			    buf_depth[4 * i + 2] = 0x00; //R
			    buf_depth[4 * i + 3] = 0xFF;
                //Console.WriteLine("if (" + raw_depth[i] + " < 2000");
                if (raw_depth[i] < 800 && raw_depth[i] > 600)
                {
				    buf_depth[4 * i + 0] = 0xFF;
				    buf_depth[4 * i + 1] = 0xFF;
				    buf_depth[4 * i + 2] = 0xFF;
				    buf_depth[4 * i + 3] = 0xFF;
			    }
		    }*/
            clientStream.Write(buf_rgb, 0, buf_rgb.Length);
        }
        Marshal.FreeHGlobal(rgb32);
        CLNUIDevice.StopCamera(camera);
        Console.WriteLine("rgb_out: closed");
        theClient.Close();
    }

    private void dataWaitForConnection()
    {
        this.dataSocket.Start();

        while (true)
        {
            Console.WriteLine("## Wait data client");
            TcpClient client = this.dataSocket.AcceptTcpClient();

            data_is_connected = true;
            Console.WriteLine("Data Client Conected");

            Thread dataInThread = new Thread(new ParameterizedThreadStart(data_in));
            dataInThread.Start(client);

            Thread dataOutThread = new Thread(new ParameterizedThreadStart(data_out));
            dataOutThread.Start(client);
        }
    }

    private void data_in(object client)
    {
        TcpClient theClient = (TcpClient)client;
        NetworkStream clientStream = theClient.GetStream();

        if (!sent_data_policy)
        {
            send_policy_file(clientStream);
            sent_data_policy = true;
        }
        //send_motor_serial(clientStream);
        byte[] buffer = new byte[1024];
        int bytesRead;
        while (data_is_connected)
        {
            bytesRead = 0;
            Console.WriteLine("data_in: waiting for data");
            try
            {
                //blocks until a client sends a message
                bytesRead = clientStream.Read(buffer, 0, 1024);
            }
            catch
            {
                //a socket error has occured
                Console.WriteLine("data_in: socket error!");
                break;
            }

            if (bytesRead == 0)
            {
                data_is_connected = false;
                Console.WriteLine("Client Disconected");
                break;
            }
            if (bytesRead == 6)
            {
                //SwapBytes(buffer);
                switch(buffer[0])
                {
					case 1: //MOTOR
						switch(buffer[1]) {
							case 1: //MOVE
								ByteOperation.SwapBytes(buffer, 2, 4);
                        		int angle = BitConverter.ToInt32(buffer, 2);
                        		if (angle < 31 && angle > -31)
                        		{                            
                       			    short motorPosition = (short)(angle * (8000 / 31));
                        		    CLNUIDevice.SetMotorPosition(motor, motorPosition);
                        		}
							break;
							case 2: //LED COLOR
								ByteOperation.SwapBytes(buffer, 2, 4);
                        		int color = BitConverter.ToInt32(buffer, 2);
                        		if (color < 7 && color > -1)
                        		{                            
									CLNUIDevice.SetMotorLED(motor, (byte)color);
                        		}
							break;
						}
					break;
                }
            }      
        }
        theClient.Close();
     }

    private void data_out(object client)
    {
        TcpClient theClient = (TcpClient)client;
        NetworkStream clientStream = theClient.GetStream();
	    short ax = 0, ay = 0, az = 0;
        short _x = 0, _y = 0, _z = 0;
        byte[] b_ax, b_ay, b_az, b_dx, b_dy, b_dz;

        while (data_is_connected)
        {
            System.Threading.Thread.Sleep(1000 / 30);
            byte[] buffer_send = new byte[30];
            CLNUIDevice.GetMotorAccelerometer(motor, ref _x, ref _y, ref _z);
            b_ax = ByteOperation.RSwapBytes(BitConverter.GetBytes(ax));
            b_ay = ByteOperation.RSwapBytes(BitConverter.GetBytes(ay));
            b_az = ByteOperation.RSwapBytes(BitConverter.GetBytes(az));
            b_dx = ByteOperation.RSwapBytes(BitConverter.GetBytes((double)_x));
            b_dy = ByteOperation.RSwapBytes(BitConverter.GetBytes((double)_y));
            b_dz = ByteOperation.RSwapBytes(BitConverter.GetBytes((double)_z));
            Buffer.BlockCopy(b_ax, 0, buffer_send, 0, b_ax.Length);
            Buffer.BlockCopy(b_ay, 0, buffer_send, 2, b_ay.Length);
            Buffer.BlockCopy(b_az, 0, buffer_send, 4, b_az.Length);
            Buffer.BlockCopy(b_dx, 0, buffer_send, 6, b_dx.Length);
            Buffer.BlockCopy(b_dy, 0, buffer_send, 14, b_dy.Length);
            Buffer.BlockCopy(b_dz, 0, buffer_send, 22, b_dz.Length);
            clientStream.BeginWrite(buffer_send, 0, 30, null, null); 
        }
        b_ax = b_ay = b_az = b_dx = b_dy = b_dz = null;
        Console.WriteLine("data_out: closed");
        theClient.Close();
     }
  }
}
