// // Code retrieved from: https://github.com/BIIG-UC3M/IGT-UltrARsound

using System;
using System.Text;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;

/// The class to communicate with the server socket, compatible with multiple platforms.
public class SocketHandler
{
    /// Tcp client for server communication.
    private TcpClient tcpClient;

    /// Stream to receive and send messages.
    private NetworkStream clientStream;

    /// Constructor to create a socket to communicate.
    public SocketHandler()
    {
        tcpClient = new TcpClient();
    }

    /// Connects socket to server.
    public async Task<bool> Connect(string ip, int port)
    {
        try
        {
            await tcpClient.ConnectAsync(ip, port);
            clientStream = tcpClient.GetStream();
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Connecting exception: " + e);
            return false;
        }
    }

    /// Method to send strings to the server.
    public async Task Send(String msg)
    {
        byte[] msgAsByteArray = Encoding.ASCII.GetBytes(msg);
        await Send(msgAsByteArray);
    }

    /// Method to send bytes to the server.
    public async Task Send(byte[] msg)
    {
        if (clientStream.CanWrite)
        {
            await clientStream.WriteAsync(msg, 0, msg.Length);
        }
    }

    /// Method to receive a byte array from the server.
    public async Task<byte[]> Listen(uint msgSize)
    {
        byte[] receivedBytes = new byte[msgSize];
        int totalBytesRead = 0;
        int percentComplete = 0;
        int tenPercent = (int)(msgSize * 0.1);

        while (totalBytesRead < msgSize)
        {
            int bytesRead = await clientStream.ReadAsync(receivedBytes, totalBytesRead, (int)msgSize - totalBytesRead);
            if (bytesRead == 0)
            {
                // Connection has been closed, terminated, or no more data is available.
                break;
            }
            totalBytesRead += bytesRead;

            if (totalBytesRead >= tenPercent * percentComplete)
            {
                Debug.Log("Download progress: " + (percentComplete * 10) + "%");
                percentComplete++;
            }
        }

        // If totalBytesRead is less than msgSize, you can handle it based on your application's logic.
        // For example, throw an exception or return the partial data.
        Debug.Log("Total bytes read: " + totalBytesRead);
        return receivedBytes;
    }

    /// Disconnects the socket.
    public void Disconnect()
    {
        if (tcpClient != null)
        {
            clientStream.Close();
            tcpClient.Close();
            tcpClient = null;
        }
    }
}
