/*
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
	//declaring motor,camera,device count,
    private static IntPtr motor = IntPtr.Zero;
    private static IntPtr camera = IntPtr.Zero;
	private static int _devCount;
    private static String devSerial;
	private static String MotorSerial;
	//declaring the 4 socket listeners  (SECURITY,DEPTH,RGB,DATA)
    private TcpListener securitySocket;
    private TcpListener depthSocket;
    private TcpListener rgbSocket;
    private TcpListener dataSocket;
	//declaring the 4 Threads  (SECURITY,DEPTH,RGB,DATA)
	private Thread securityListenerThread;	
    private Thread depthListenerThread;
    private Thread rgbListenerThread;
    private Thread dataListenerThread;
	//declaring the 2 Threads for data communication
	private Thread dataInThread;
	private Thread dataOutThread;
	//declaring other threads
	private Thread securityOutThread;
	private Thread depthOutThread;
	private Thread rgbOutThread;
 	//declaring the 3 socket connection status flags  (DEPTH,RGB,DATA)
    private Boolean depth_is_connected = false;
    private Boolean rgb_is_connected = false;
    private Boolean data_is_connected = false;
	//camera is running
	private Boolean camera_is_started = false;
	
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
				CLNUIDevice.SetMotorLED(motor, (byte)1); //Green LED
			}
			//Start server if everything went OK with the device
			new Server();
		}
		catch (System.Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}
		
    public Server()
    {
		//starts Listener and Thread for Flash Security Policy
    	/*this.securitySocket = new TcpListener(IPAddress.Any, 843);
		this.securityListenerThread = new Thread(new ThreadStart(securityWaitForConnection));
		this.securityListenerThread.Start();*/
		//starts Listener and Thread for Depth
    	this.depthSocket = new TcpListener(IPAddress.Any, 6001);
		this.depthListenerThread = new Thread(new ThreadStart(depthWaitForConnection));
		this.depthListenerThread.Start();
		//starts Listener and Thread for RGB
		this.rgbSocket = new TcpListener(IPAddress.Any, 6002);
		this.rgbListenerThread = new Thread(new ThreadStart(rgbWaitForConnection));
		this.rgbListenerThread.Start();
		//starts Listener and Thread for Data
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

	/*
     * Listen for security policy request
     */
    private void securityWaitForConnection()
    {
        this.securitySocket.Start();
        while(true){
			//blocks until a client has connected to the server
			Console.WriteLine("Waiting for Security Policy Request: {0} {1} ", securitySocket.Server.ProtocolType, securitySocket.LocalEndpoint);
            TcpClient client = this.depthSocket.AcceptTcpClient();

            Console.WriteLine("Security Request Client Conected" + client.Client.LocalEndPoint);

			//create a thread to handle communication 
            //with connected client
            securityOutThread = new Thread(new ParameterizedThreadStart(security_out));
            securityOutThread.Start(client);
        }
    }
		
	/*
	 * If security policy is requested in port 843 send it
	 */
	private void security_out(object client)
    {
        TcpClient tcpClient = (TcpClient)client;
        NetworkStream clientStream = tcpClient.GetStream();

        byte[] message = new byte[1024];
        int bytesRead;
 		Console.WriteLine("Policy Client Connected: " + tcpClient.Client.LocalEndPoint);	
        while (true)
        {
            bytesRead = 0;
            try
            {
                //blocks until a client sends a message
                bytesRead = clientStream.Read(message, 0, 1024);
            }
            catch
            {
                //a socket error has occured
                break;
            }
            if (bytesRead == 0)
            {
                //the client has disconnected from the server
                Console.WriteLine("Policy Client Disconnected: " + tcpClient.Client.LocalEndPoint);
                break;
            }
            //master policy stream
            if (new UTF8Encoding().GetString(message, 0, bytesRead).Contains("<policy-file-request/>"))
            {
				//policy file for every domain on 6001, 6002 and 6003 sockets
		        string str = "<?xml version='1.0'?>" +
		        	"<!DOCTYPE cross-domain-policy SYSTEM '/xml/dtds/cross-domain-policy.dtd'>" +
		        	"<cross-domain-policy><site-control permitted-cross-domain-policies='all'/>" +
		        	"<allow-access-from domain='*' to-ports='6001-6003'/>" +
		        	"</cross-domain-policy>";
				System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
		        Console.WriteLine("Sending Policy Stream. Length = {0}", str.Length);
		        clientStream.Write(encoding.GetBytes(str), 0, str.Length);
		        clientStream.WriteByte(0);
		        clientStream.WriteByte(13); //very important to terminate XmlSocket data in this way, otherwise Flash can't read it. 
		        clientStream.Flush();
            }
        }
        tcpClient.Close();
		securityOutThread.Abort();
    }
		
	/*
     * Listen for depth client
     */
    private void depthWaitForConnection()
    {
        this.depthSocket.Start();
        while(true){
			//blocks until a client has connected to the server
			Console.WriteLine("Waiting for DEPTH Client: {0} {1} ", depthSocket.Server.ProtocolType, depthSocket.LocalEndpoint);
            TcpClient depthClient = this.depthSocket.AcceptTcpClient();
			
			//Changing depth connection status flag
            depth_is_connected = true;
            Console.WriteLine("Depth Client Conected: " + depthClient.Client.LocalEndPoint);

			//create a thread to handle communication 
            //with connected client
            depthOutThread = new Thread(new ParameterizedThreadStart(depth_out));
            depthOutThread.Start(depthClient);
        }
    }
		
	/*
     * Sending depth data to the client
     */
    private unsafe void depth_out(object client)
    {
        TcpClient theClient = (TcpClient)client;
        NetworkStream clientStream = theClient.GetStream();
		
		//Starting the NUI camera
		if(!camera_is_started) {
			CLNUIDevice.StartCamera(camera);
			camera_is_started = true;
		}
        int i;
			
		//raw_depth buffer managed memory
        short[] raw_depth = new short[640*480];
		//Allocate depthRAW unmanaged memory 
        IntPtr depthRAW = Marshal.AllocHGlobal(640 * 480 * 2);
		//buff depth sent to the server (BGRA)
        byte[] buf_depth = new byte[640*480*4];

        while (depth_is_connected)
        {
			//putting frame into depthRAW unmanaged memory
            CLNUIDevice.GetCameraDepthFrameRAW(camera, depthRAW, 0);
			//Copying deptRAW unmanaged memory to raw_depth managed memroy
            Marshal.Copy(depthRAW, raw_depth, 0, 640 * 480);
			//For every pixel in the frame
		    for (i=0; i<640 * 480; i++) {
				//Black pixels
			    buf_depth[4 * i + 0] = 0x00; //B
                buf_depth[4 * i + 1] = 0x00; //G
			    buf_depth[4 * i + 2] = 0x00; //R
			    buf_depth[4 * i + 3] = 0xFF;
				//If pixel is between 800 and 600 depth
                if (raw_depth[i] < 800 && raw_depth[i] > 600)
                {
					//white pixels
				    buf_depth[4 * i + 0] = 0xFF;
				    buf_depth[4 * i + 1] = 0xFF;
				    buf_depth[4 * i + 2] = 0xFF;
				    buf_depth[4 * i + 3] = 0xFF;
			    }
		    }
			//send buffer to client
			try{
            	clientStream.Write(buf_depth, 0, buf_depth.Length);
			} catch {
				depth_is_connected = false;		
			}
        }
		//free memory
        Marshal.FreeHGlobal(depthRAW);
		raw_depth = null;
		buf_depth = null;
		//stop NUI camera
		if(camera_is_started) {
			CLNUIDevice.StopCamera(camera);
			camera_is_started = false;
		}
        Console.WriteLine("depth_out: closed");
        theClient.Close();
		depthOutThread.Abort();
    }
		
	/*
     * Listen for RGB client 
     */
    private void rgbWaitForConnection()
    {
        this.rgbSocket.Start();
        while (true)
        {
			//blocks until a client has connected to the server
			Console.WriteLine("Waiting for RGB Client: {0} {1} ", rgbSocket.Server.ProtocolType, rgbSocket.LocalEndpoint);
            TcpClient rgbClient = this.rgbSocket.AcceptTcpClient();
				
			//Changing RGB connection status flag
            rgb_is_connected = true;
            Console.WriteLine("RGB Client Conected: " + rgbSocket.LocalEndpoint);
				
			//create a thread to handle communication 
            //with connected client
            rgbOutThread = new Thread(new ParameterizedThreadStart(rgb_out));
            rgbOutThread.Start(rgbClient);
            Console.WriteLine("rgb_out: thread created");
        }
    }

    private unsafe void rgb_out(object client)
    {
        TcpClient theClient = (TcpClient)client;
        NetworkStream clientStream = theClient.GetStream();
		
		//Starting the NUI camera
		if(!camera_is_started) {
			CLNUIDevice.StartCamera(camera);
			camera_is_started = true;
		}
			
		// buf_rgb managed memory buffer
        byte[] buf_rgb = new byte[640*480*4];
		// rgb32 unmanaged memory allocation
        IntPtr rgb32 = Marshal.AllocHGlobal(640 * 480 * 4);

        while (rgb_is_connected)
        {
			//putting frame into unmanaged memory
            CLNUIDevice.GetCameraColorFrameRGB32(camera, rgb32, 0);
			//copy unmanaged memory to managed memory buffer
            Marshal.Copy(rgb32, buf_rgb, 0, 640 * 480 * 4);
			//send managed memory buffer to client
			try{
            	clientStream.Write(buf_rgb, 0, buf_rgb.Length);
			} catch {
				rgb_is_connected = false;		
			}
        }
		//Free memory
        Marshal.FreeHGlobal(rgb32);
		buf_rgb = null;
		//stop NUI camera
		if(camera_is_started) {
			CLNUIDevice.StopCamera(camera);
			camera_is_started = false;
		}
        Console.WriteLine("rgb_out: closed");
        theClient.Close();
		rgbOutThread.Abort();
    }
	
	/*
     * Listen for data client
     */
    private void dataWaitForConnection()
    {
        this.dataSocket.Start();
        while (true)
        {
            //blocks until a client has connected to the server
			Console.WriteLine("Waiting for Data Client: {0} {1} ", dataSocket.Server.ProtocolType, dataSocket.LocalEndpoint);
            TcpClient client = this.dataSocket.AcceptTcpClient();
				
			//Changing Data connection status flag
            data_is_connected = true;
				
			//create 2 thread to handle data in/out communication 
            //data in thread
            dataInThread = new Thread(new ParameterizedThreadStart(data_in));
            dataInThread.Start(client);
			//data out thread
            dataOutThread = new Thread(new ParameterizedThreadStart(data_out));
            dataOutThread.Start(client);
        }
    }

    private void data_in(object client)
    {
        TcpClient theClient = (TcpClient)client;
        NetworkStream clientStream = theClient.GetStream();
			
        Console.WriteLine("Data Client Conected: " + theClient.Client.LocalEndPoint);
			
        byte[] buffer = new byte[1024];
        int bytesRead;
        while (data_is_connected)
        {
            bytesRead = 0;
            try
            {
                //blocks until a client sends a message
            	Console.WriteLine("data_in: waiting for data");
                bytesRead = clientStream.Read(buffer, 0, 1024);
            }
            catch
            {
                //a socket error has occured
				data_is_connected = false;
            }
            if (bytesRead == 0)
            {
                data_is_connected = false;
            }
            if (bytesRead == 6)
            {
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
		dataOutThread.Abort();
        Console.WriteLine("data_in: Client Disconected");
        theClient.Close();
		dataInThread.Abort();
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
            CLNUIDevice.GetMotorAccelerometer(motor, ref _x, ref _y, ref _z);
            b_ax = ByteOperation.RSwapBytes(BitConverter.GetBytes(ax));
            b_ay = ByteOperation.RSwapBytes(BitConverter.GetBytes(ay));
            b_az = ByteOperation.RSwapBytes(BitConverter.GetBytes(az));
            b_dx = ByteOperation.RSwapBytes(BitConverter.GetBytes((double)_x));
            b_dy = ByteOperation.RSwapBytes(BitConverter.GetBytes((double)_y));
            b_dz = ByteOperation.RSwapBytes(BitConverter.GetBytes((double)_z));
            clientStream.Write(b_ax, 0, b_ax.Length);
            clientStream.Write(b_ay, 0, b_ay.Length);
            clientStream.Write(b_az, 0, b_az.Length);
            clientStream.Write(b_dx, 0, b_dx.Length);
            clientStream.Write(b_dy, 0, b_dy.Length);
            clientStream.Write(b_dz, 0, b_dz.Length);
            clientStream.Flush();
        }
        b_ax = b_ay = b_az = b_dx = b_dy = b_dz = null;
     }
  }
}
