package src.Comm 
{
    import com.codecatalyst.promise.Deferred;
    import com.codecatalyst.promise.Promise;

    import flash.events.*;
    import flash.net.Socket;
    import flash.utils.ByteArray;
    import flash.utils.Endian;

    import src.Constants;
    import src.Global;
    import src.Util.Util;

    public class TcpSession implements Session, IEventDispatcher
	{		
		//Members
		private var dispatcher: EventDispatcher;
		private var socket:Socket;
		private var pending:Array;
		private var seq: int;
		
		private var streamingPacket: ByteArray;
		private var headerPacket: Packet;
		
		private var loginSuccess: Boolean;
		
		//Events
		private var onConnectCallback: Function;
		private var onDisconnectCallback: Function;
		private var onLoginCallback: Function;
		private var onSecurityErrorCallback: Function;
		
		public function TcpSession() 
		{
			dispatcher = new EventDispatcher(this);
			seq = 1;
			
			socket = new Socket();
			socket.endian = Endian.LITTLE_ENDIAN;
			
			streamingPacket = null;
			headerPacket = null;
			
			pending = [];
			
			socket.addEventListener(Event.CLOSE,closeHandler);
			socket.addEventListener(Event.CONNECT, connectHandler);			
			socket.addEventListener(IOErrorEvent.IO_ERROR,ioErrorHandler);
			socket.addEventListener(SecurityErrorEvent.SECURITY_ERROR,securityErrorHandler);
			socket.addEventListener(ProgressEvent.SOCKET_DATA, socketDataHandler);
		}
		
		public function hasLoginSuccess() : Boolean {
			return loginSuccess;
		}
		
		public function setLoginSuccess(bool: Boolean): void {
			loginSuccess = bool;
		}

		public function setConnect(callback: Function): void
		{
			onConnectCallback = callback;
		}
		
		public function setDisconnect(callback : Function): void
		{
			onDisconnectCallback = callback;
		}
		
		public function setLogin(callback: Function): void
		{
			onLoginCallback = callback;
		}
		
		public function setSecurityErrorCallback(callback: Function):void
		{
			onSecurityErrorCallback = callback;
		}
		
		public function login(useLoginKey: Boolean, username:String, passwdOrLoginKey:String): void
		{			
			var packet: Packet = new Packet();
			packet.cmd = Commands.LOGIN;			
			
			var version: int = Constants.version == 0 && Constants.revision == 0 ? 999 : Constants.version;
			var revision: int = Constants.version == 0 && Constants.revision == 0 ? 999 : Constants.revision;
			
			packet.writeShort(version);
			packet.writeShort(revision);

			packet.writeUByte(useLoginKey ? 0 : 1);
			packet.writeString(username);
			packet.writeString(passwdOrLoginKey);			
			
			write(packet, function(packet:Packet, custom:*):void { 
				if (onLoginCallback != null)
					onLoginCallback(packet);
			});
		}
		
		public function connect(hostname: String):void
		{			
			socket.connect(hostname, Constants.session.serverPort);
		}
		
		public function logout():void 
		{
			socket.close();
		}
		
		public function write(packet:Packet, callback: Function = null, custom: * = null):Promise
		{			
			packet.seq = seq++;

            var deferred: Deferred = new Deferred();

            pending.push(new PendingPacket(packet.seq, callback, custom, deferred));

			var bytes:ByteArray = packet.getBytes();
			
			if (Constants.debug >= 3)
				Util.log("Sending " + packet.toString());
			
			socket.writeBytes(bytes,0,bytes.length);
			
			socket.flush();

            return deferred.promise;
		}
		
		private function connectHandler(event:Event):void 
		{
			if (Constants.debug >= 3)
			{
				Util.log("connectHandler: " + event);
				Util.log("The socket is now connected...");
			}		
			
			if (onConnectCallback != null)
				onConnectCallback(event, true);
		}
		
		private function closeHandler(event:Event):void {
			if (Constants.debug >= 3)
				Util.log("closeHandler: " + event);
				
			if (onDisconnectCallback != null)
				onDisconnectCallback(event);				
		}

		private function ioErrorHandler(event:IOErrorEvent):void {
			if (Constants.debug >= 3)
				Util.log("ioErrorHandler: " + event);
				
			if (onDisconnectCallback != null)
				onDisconnectCallback(event);	
		}

		private function securityErrorHandler(event:SecurityErrorEvent):void {
			if (Constants.debug >= 3)
				Util.log("securityErrorHandler: " + event);
				
			if (onSecurityErrorCallback != null)
				onSecurityErrorCallback(event);
		}

		private function socketDataHandler(event:ProgressEvent):void 
		{		
			while ( socket.bytesAvailable > 0 ) 
			{		
				if (streamingPacket == null)
				{
					streamingPacket = new ByteArray();
					streamingPacket.endian = Endian.LITTLE_ENDIAN;
				}
			
				var incomingPacket: Packet = null;
				
				if (streamingPacket.length < Constants.headerSize)
				{
					socket.readBytes(streamingPacket, streamingPacket.length, Math.min(socket.bytesAvailable, Constants.headerSize - streamingPacket.length));
					
					if (streamingPacket.length == Constants.headerSize)					
						headerPacket = new Packet(streamingPacket);					
					else
						continue;
					
					if (headerPacket.length > 0)
						continue;
				}
				
				if (headerPacket.length > 0)
				{
					socket.readBytes(streamingPacket, streamingPacket.length, Math.min(socket.bytesAvailable, headerPacket.length - (streamingPacket.length - Constants.headerSize)));
						
					if (streamingPacket.length == headerPacket.length  + Constants.headerSize)
					{
							streamingPacket.position = 0;
							incomingPacket = new Packet(streamingPacket);
					}
					else
						continue;
				}
				else
					incomingPacket = headerPacket;
				
					
				streamingPacket = null;
				headerPacket = null;
							
				if (incomingPacket != null)
				{				
					if (Constants.debug >= 2)
						Util.log("Received: " + incomingPacket.toString());
					
					if (Constants.debug > 0) 
						Global.main.packetCounter.updateCounter();
					
					if ( (incomingPacket.option & Options.REPLY) != Options.REPLY)
					{
						if (Constants.debug > 1)
							Util.log("Channel notification: " + incomingPacket.toString());							
						
						dispatcher.dispatchEvent(new PacketEvent(Commands.CHANNEL_NOTIFICATION, incomingPacket));
					}
					else
					{						
						for (var i: int = 0; i < pending.length; ++i) 
						{				
							if (pending[i].seq == incomingPacket.seq)
							{
								var pendingPacket: PendingPacket = pending[i];
								pending.splice(i, 1);
								if (pendingPacket.callback != null)
                                {
									pendingPacket.callback(incomingPacket, pendingPacket.custom);
                                }

                                pendingPacket.resolve(incomingPacket);

								break;
							}
						}
					}
				}
			}
		}
		
		public function addEventListener(type:String, listener:Function, useCapture:Boolean = false, priority:int = 0, useWeakReference:Boolean = false):void{
			dispatcher.addEventListener(type, listener, useCapture, priority);
		}
		
		public function dispatchEvent(evt:Event):Boolean{
			return dispatcher.dispatchEvent(evt);
		}
    
		public function hasEventListener(type:String):Boolean{
			return dispatcher.hasEventListener(type);
		}
		
		public function removeEventListener(type:String, listener:Function, useCapture:Boolean = false):void{
			dispatcher.removeEventListener(type, listener, useCapture);
		}
					   
		public function willTrigger(type:String):Boolean {
			return dispatcher.willTrigger(type);
		}		
	}
}