using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SocketEvent : UnityEvent<string>
{
}

public class IUTConnect
{
    private UdpClient udpClient;
    IPEndPoint remoteEndPoint;
    IPEndPoint localEndPoint;
    IPEndPoint moduleEndPoint;
    Socket client;    
    private const string CONNECTION_SUCCESSFULL_MESSAGE = "HELLO_DEVICE";
    private const string CONNECTION_IDENTIFIED_SOCKET = "CONNECTED_TOO";
    private const string HEART_BEAT = "HEART_BEAT";

    private string multicastIP { get; set; }
    private int multicastPort { get; set; }
    private string tokenID { get; set; }
    public SocketEvent callbackSocket = new SocketEvent();
    public Action<string> OnReceive = delegate { };
    public Action<bool> OnConnect = delegate { };
    private string moduleIP;
    private bool connected;
    private System.Timers.Timer timer;

    public IUTConnect()
    {        
        this.multicastIP = "227.55.77.99";
        this.multicastPort = 5000;        
    }

    private void multicastCallback(IAsyncResult ar)
    {
        IPEndPoint sender = new IPEndPoint(0, 0);
        Byte[] receivedBytes = udpClient.EndReceive(ar, ref sender);
        Debug.Log(Encoding.ASCII.GetString(receivedBytes));        
        if (!CONNECTION_SUCCESSFULL_MESSAGE.Equals(Encoding.ASCII.GetString(receivedBytes)))
        {             
            udpClient.BeginReceive(new AsyncCallback(multicastCallback), null);
        }
        else
        {
            this.moduleIP = sender.Address.ToString();
            IUTModuleProperties.ModuleAddress = this.moduleIP;            
            this.startSocket();
        }
    }

    private void sendMulticast(byte[] bufferToSend)
    {
        udpClient.Send(bufferToSend, bufferToSend.Length, remoteEndPoint);
    }

    private void startMulticast()
    {
        udpClient = new UdpClient();
        IPAddress multicastIPAddress = IPAddress.Parse(this.multicastIP);
        IPAddress localIPAddress = IPAddress.Any;
        localEndPoint = new IPEndPoint(localIPAddress, this.multicastPort);
        remoteEndPoint = new IPEndPoint(multicastIPAddress, this.multicastPort);

        udpClient.ExclusiveAddressUse = false;
        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udpClient.ExclusiveAddressUse = false;

        udpClient.Client.Bind(localEndPoint);
        udpClient.JoinMulticastGroup(multicastIPAddress, localIPAddress);

        udpClient.BeginReceive(new AsyncCallback(multicastCallback), null);
        this.sendMulticast(Encoding.ASCII.GetBytes(this.tokenID));
    }

    private void socketCallback(IAsyncResult ar)
    {                                
        StateObject state = (StateObject)ar.AsyncState;
        Socket client = state.workSocket;
        try
        {
            int bytesReceived = client.EndReceive(ar);

            if (bytesReceived > 0)
            {
                //Debug.Log("Bytes received: " + bytesReceived);
                string message = Encoding.ASCII.GetString(state.buffer, 0, bytesReceived).Trim();
                if (!message.Equals(""))
                {
                    if (message.Trim().Equals(CONNECTION_IDENTIFIED_SOCKET))
                    {
                        this.connected = true;
                        Debug.Log("Receive \"CONNECTED_TOO\"");
                        OnConnect(this.connected);
                    }
                    else if (message.Trim().Equals(HEART_BEAT))
                    {
                        this.connected = true;
                        //Debug.Log("Heart beat");
                    }
                    else
                    {
                        OnReceive(message);
                    }
                }
            }
            else
            {
                Debug.Log("Resetou");
                state = new StateObject();
                state.workSocket = client;
            }
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(socketCallback), state);
        } catch (Exception e)
        {
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(socketCallback), state);
        }
    }

    private void receiveSocket()
    {
        StateObject state = new StateObject();
        state.workSocket = client;
        
        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(socketCallback), state);
    }

    public void sendSocket(string message)
    {
        byte[] byteMessage = Encoding.ASCII.GetBytes(message);
        client.Send(byteMessage);
    }

    private void startSocket()
    {
        IPAddress moduleIPAddress = IPAddress.Parse(this.moduleIP);
        moduleEndPoint = new IPEndPoint(moduleIPAddress, 8080);
        client = new Socket(moduleIPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);        
        Debug.Log("Socket Started");
        client.Connect(moduleEndPoint);       
        this.receiveSocket();
        System.Threading.Thread.Sleep(1500);
        Debug.Log("Send \"CONNECTED\"");
        sendSocket("CONNECTED");
        timer = new Timer(1000);
        timer.Elapsed += new ElapsedEventHandler(heartBeat);

        timer.Interval = 1000;
        timer.Enabled = true;
    }

    private void heartBeat(object source, ElapsedEventArgs e)
    {
        sendSocket(HEART_BEAT);
        polling();
    }

    public void stop()
    {
        udpClient.Close();
        timer.Stop();
        timer.Enabled = false;
        client.Close();        
    }

    public void start(string multicastIP, int multicastPort, string tokenID)
    {
        this.multicastIP = multicastIP;
        this.multicastPort = multicastPort;
        this.start(tokenID);
    }

    public void start(string tokenID)
    {
        this.tokenID = tokenID;
        this.start();
    }

    public void start()
    {
        if (this.multicastIP != null && !this.multicastIP.Equals("") 
            && this.multicastPort > 0
            && this.tokenID != null && !this.tokenID.Equals(""))
        {
            this.startMulticast();            
        }        
    }

    public bool isConnected()
    {
        return this.connected;
    }

    public void polling()
    {
        if (!client.Poll(-1, SelectMode.SelectRead)) {
            client.Connect(moduleEndPoint);
        }
    }
}