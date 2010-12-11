/*
 * 
 * This file is part of the OpenKinect Project. http://www.openkinect.org
 * 
 * Copyright (c) 2010 individual OpenKinect contributors. See the CONTRIB file 
 * for details.
 * 
 * This code is licensed to you under the terms of the Apache License, version 
 * 2.0, or, at your option, the terms of the GNU General Public License, 
 * version 2.0. See the APACHE20 and GPL20 files for the text of the licenses, 
 * or the following URLs:
 * http://www.apache.org/licenses/LICENSE-2.0
 * http://www.gnu.org/licenses/gpl-2.0.txt
 * 
 * If you redistribute this file in source form, modified or unmodified, 
 * you may:
 * 1) Leave this header intact and distribute it under the same terms, 
 * accompanying it with the APACHE20 and GPL20 files, or
 * 2) Delete the Apache 2.0 clause and accompany it with the GPL20 file, or
 * 3) Delete the GPL v2.0 clause and accompany it with the APACHE20 file
 * In all cases you must keep the copyright notice intact and include a copy 
 * of the CONTRIB file.
 * Binary distributions must follow the binary distribution requirements of 
 * either License.
 * 
 */
package org.libfreenect
{
	import org.libfreenect.libfreenect;
	import org.libfreenect.libfreenectData;
	
	import flash.utils.ByteArray;
	import flash.events.EventDispatcher;
	
	public class libfreenectMotor
	{	
		public static function set position(position:Number):void 
		{
			var _info:libfreenectData = libfreenectData.instance;
			var data:ByteArray = new ByteArray;
			data.writeByte(libfreenect.MOTOR_ID);
			data.writeByte(1); //MOVE MOTOR
			data.writeInt(position);
			if(_info.sendData(data) != libfreenect.SUCCESS) {
				throw new Error('Data was not complete');
			}
			
		}
		
		// 0 = Turn Off
		// 1 = Green
		// 2 = Red
		// 3 = Orange
		// 4 = Blink Green-Off
		// 6 = Blink Red-Orange
		public static function set ledColor(color:Number):void 
		{
			var _info:libfreenectData = libfreenectData.instance;
			var data:ByteArray = new ByteArray;
			data.writeByte(1); //MOTOR
			data.writeByte(2); //LED
			data.writeInt(color);
			if(_info.sendData(data) == libfreenect.SUCCESS){
				//EventDispatcher.dispatchEvent(new libfreenectLedEvent(libfreenectLedEvent.TURNED_ON, color));
			} else {
				throw new Error('Data was not complete');
			}
		}
	}
}