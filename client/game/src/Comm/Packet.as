package src.Comm {
    import flash.utils.*;

    import src.Constants;
    import src.Util.Util;

    public class Packet {
		public var seq:int=0;
		public var option:int=0;
		public var cmd:int=0;
		public var length:int=0;
		public var parameters:Array;
		public var bytes:ByteArray;
		
		public static const OPTIONS_COMPRESSED: int = 1;
		public static const OPTIONS_FAILED: int = 2;
		public static const OPTIONS_REPLY: int = 4;
		
		public function Packet(incomingBytes: ByteArray = null)
		{
			parameters = [];
			bytes = new ByteArray();			
			bytes.endian = Endian.LITTLE_ENDIAN;
			
			if (incomingBytes != null)
			{
				if (incomingBytes.length < Constants.headerSize)
					return;
					
				seq = incomingBytes.readUnsignedShort();
				option = incomingBytes.readUnsignedShort();
				cmd = incomingBytes.readUnsignedShort();
				length = incomingBytes.readUnsignedShort();
				
				if (incomingBytes.length != Constants.headerSize + length)
					return;

				if ( (option & Options.COMPRESSED) == Options.COMPRESSED)
				{
					var payloadBytes:ByteArray = new ByteArray();
					payloadBytes.endian = Endian.LITTLE_ENDIAN;
					incomingBytes.readBytes(payloadBytes, 0, length);
					payloadBytes.uncompress();					
					payloadBytes.readBytes(bytes, 0, payloadBytes.length);
				}
				else
				{
					incomingBytes.readBytes(bytes, 0, length);
				}
			}
		}
				
		/***********************   write supports  *************************/		
		public function writeByte(param:int):void {
			parameters.push(new Parameter(param,Parameter.INT4,Parameter.INT1));
			length+=1;
		}

        public function writeBoolean(param: Boolean): void {
            writeByte(param ? 1 : 0);
        }

		public function writeUByte(param:int):void {
			parameters.push(new Parameter(param,Parameter.INT4,Parameter.UINT1));
			length+=1;
		}
		
		public function writeShort(param:int):void {
			parameters.push(new Parameter(param,Parameter.INT4,Parameter.INT2));
			length+=2;
		}
		
		public function writeUShort(param:int):void {
			parameters.push(new Parameter(param,Parameter.INT4,Parameter.UINT2));
			length+=2;
		}		
		
		public function writeInt(param:int):void {
			parameters.push(new Parameter(param,Parameter.INT4,Parameter.INT4));
			length+=4;
		}
		
		public function writeUInt(param:int):void {
			parameters.push(new Parameter(param,Parameter.INT4,Parameter.UINT4));
			length+=4;
		}		
		
		public function writeFloat(param:Number):void {
			parameters.push(new Parameter(param,Parameter.FLOAT,Parameter.FLOAT));
			length+=4;
		}
		
		public function writeString(param:String):void {
			parameters.push(new Parameter(param, Parameter.STRING, Parameter.STRING));
			var tmp: ByteArray = new ByteArray();
			tmp.writeUTF(param);
			length += tmp.length;
		}
		
		/***********************   read supports  *************************/
		public function readByte():int {
			return bytes.readByte();
		}
		
		public function read2dShortArray(w: int, h: int): Array
		{
			var rows:Array = [];
			for (var a:int = 0; a < h; a++)
			{
				var cols: Array = [];
				for (var b:int = 0; b < w; b++)
					cols.push(bytes.readUnsignedShort());
				rows.push(cols);
			}			
			
			return rows;
		}
		
		public function readUByte():int {
			return bytes.readUnsignedByte();
		}
		
		public function readShort():int {
			return bytes.readShort();
		}
		
		public function readUShort():int {
			return bytes.readUnsignedShort();
		}
		
		public function readInt():int {
			return bytes.readInt();
		}
		
		public function readUInt():int {
			return bytes.readUnsignedInt();
		}		
		
		public function readFloat():Number {			
			return bytes.readFloat();
		}
		
		public function readString():String {			
			return bytes.readUTF();
		}

        public function readBoolean(): Boolean {
            return bytes.readByte() == 1;
        }

        public function readDate(): Date {
            return new Date(readUInt() * 1000);
        }

		public function hasData(): Boolean {
			return bytes.bytesAvailable > 0;
		}

        public function hasError(): Boolean {
            return (option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED;
        }

		/******************************************************************/
	
		public function getBytes():ByteArray {
			bytes = new ByteArray();	
			bytes.endian = Endian.LITTLE_ENDIAN;
			bytes.writeShort(seq);
			bytes.writeShort(option);
			bytes.writeShort(cmd);
			bytes.writeShort(length);
			
			if (Constants.debug >= 3)
				Util.log("header wrote:"+bytes.length);
			
			for (var i: int = 0; i < parameters.length; ++i ) 
			{				
				switch( parameters[i].srcType ) 
				{					
					case Parameter.INT4:
						if (parameters[i].destType == Parameter.INT1 ) 						
							bytes.writeByte(parameters[i].obj);													
						
						else if (parameters[i].destType == Parameter.UINT1)
							bytes.writeByte(parameters[i].obj);
							
						else if (parameters[i].destType == Parameter.INT2)
							bytes.writeShort(parameters[i].obj);						
							
						else if (parameters[i].destType == Parameter.UINT2)
							bytes.writeShort(parameters[i].obj);						

						else if (parameters[i].destType == Parameter.INT4 ) 						
							bytes.writeInt(parameters[i].obj);													
						
						else if (parameters[i].destType == Parameter.UINT4)
							bytes.writeInt(parameters[i].obj);
							
						if (Constants.debug >= 3)
							Util.log("done writing int4");
						break;
						
					case Parameter.STRING:						
						bytes.writeUTF(parameters[i].obj);
						
						if (Constants.debug >= 3)
							Util.log("done writing string:"+bytes.length);
						break;
				}
			}
			
			if ( (option & Options.COMPRESSED) == Options.COMPRESSED) {
				bytes.deflate();
			}
			
			return bytes;
			
		}
		
		public function toString(): String
		{
			var str: String;
			
			str = "seq[" + seq + "] option[" + option + "] cmd[" + cmd + "] length[" + length + "]\n";
			
			for (var i: int = 0; i < parameters.length; ++i)			
			{
				str += parameters[i].obj + "\n";				
			}
			
			return str;
		}

    }
}