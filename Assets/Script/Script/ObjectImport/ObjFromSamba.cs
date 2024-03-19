using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using SMBLibrary.Client;
using SMBLibrary;

public class ObjFromSamba : MonoBehaviour
{
    public TextMeshProUGUI serverIP_text;
    public TextMeshProUGUI shareName_text;
    public TextMeshProUGUI filePath_text;
    public TextMeshProUGUI username_text;
    public TextMeshProUGUI password_text;
    public GameObject validButton;
    public GameObject loadingAnimation;

    public async Task<byte[]> LoadObject()
    {
        string domain = "";
        string serverIP = serverIP_text.text.Substring(0, serverIP_text.text.Length-1);
        string shareName = shareName_text.text.Substring(0, shareName_text.text.Length-1);
        string filePath = filePath_text.text.Substring(0, filePath_text.text.Length-1);
        string username = username_text.text.Substring(0, username_text.text.Length-1);
        string password = password_text.text.Substring(0, password_text.text.Length-1);
        Debug.Log("server IP: "+serverIP);
        Debug.Log("share name: "+shareName);
        Debug.Log("file path: "+filePath);
        Debug.Log("username: "+username);
        Debug.Log("password: "+password);

        SMB2AsyncClient client = await ConnectToServer(serverIP, username, password, domain);
        if (client == null) {
            Debug.LogError("unable to connect to server");
            return null;
        }

        // Print shares
        // [broken by async feature]
        // List<string> shares = await ListShares(client);
        // foreach (string share in shares) {
        //     Debug.Log("shares : " + share);
        // }

        // Print files
        List<string> files = await ListFiles(client, shareName);
        foreach (string file in files) {
            Debug.Log("files : " + file);
        }

        // download the file
        byte[] results = await DownloadObject(client, shareName, filePath);
        DisconnectFromServer(client);

        return results;
    }

    // Connect to the server
    async Task<SMB2AsyncClient> ConnectToServer(string serverIP, string username, string password, string domain)
    {
        SMB2AsyncClient client = new SMB2AsyncClient();
        bool isConnected = await client.Connect(serverIP, SMBTransportType.DirectTCPTransport);
        if (isConnected)
        {
            NTStatus status = await client.Login(domain, username, password);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                return client;
            }
        }
        return null;
    }

    // Disconnect from the server
    async void DisconnectFromServer(SMB2AsyncClient client)
    {
        await client.Logoff();
        client.Disconnect();
    }

    // [broken by async feature]
    // List all the share from the server
    // async Task<List<string>> ListShares(SMB2Client client)
    // {
    //     Tuple<List<string>, NTStatus> tuple = await client.ListShares();
    //     List<string> shares = tuple.Item1;
    //     NTStatus status = tuple.Item2;

    //     if (status != NTStatus.STATUS_SUCCESS)
    //     {
    //         Debug.LogError("unable to connect to client");
    //         return null;
    //     }

    //     return shares;
    // }

    async Task<List<string>> ListFiles(SMB2AsyncClient client, string shareName, string directoryPath = "./")
    {
        List<string> fileList = new List<string>();

        Tuple<SMB2AsyncFileStore, NTStatus> tuple = await client.TreeConnect(shareName);
        SMB2AsyncFileStore fileStore = tuple.Item1;
        NTStatus status = tuple.Item2;

        if (status != NTStatus.STATUS_SUCCESS)
        {
            Debug.LogError("unable to open "+shareName+" share name");
            return null;
        }
        
        Tuple<NTStatus, object, FileStatus> tuple_ = await fileStore.CreateFile(String.Empty, AccessMask.GENERIC_READ, FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
        status = tuple_.Item1;
        object directoryHandle = tuple_.Item2;
        FileStatus fileStatus = tuple_.Item3;
        
        if (status == NTStatus.STATUS_SUCCESS)
        {
            Tuple<NTStatus, List<QueryDirectoryFileInformation>> tuple__ = await fileStore.QueryDirectory(directoryHandle, "*", FileInformationClass.FileDirectoryInformation);
            status = tuple__.Item1;
            List<QueryDirectoryFileInformation> tmp_fileList = tuple__.Item2;

            // cast each item in the fileList to a FileDirectoryInformation
            foreach (FileDirectoryInformation file in tmp_fileList)
            {
                fileList.Add(file.FileName);
            }

            status = await fileStore.CloseFile(directoryHandle);
        }
        status = await fileStore.Disconnect();

        return fileList;
    }

    private async Task<byte[]> DownloadObject(SMB2AsyncClient client, string shareName, string filePath)
    {
        Tuple<SMB2AsyncFileStore, NTStatus> tuple = await client.TreeConnect(shareName);
        SMB2AsyncFileStore fileStore = tuple.Item1;
        NTStatus status = tuple.Item2;

        if (status != NTStatus.STATUS_SUCCESS) {
            Debug.LogError("unable to open "+shareName+" share name");
            return null;
        }

        Tuple<NTStatus, object, FileStatus> tuple_ = await fileStore.CreateFile(filePath, AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null);
        status = tuple_.Item1;
        object fileHandle = tuple_.Item2;
        FileStatus fileStatus = tuple_.Item3;

        // read data file
        byte[] buffer, data;
        List<byte> data_list = new List<byte>();
        long bytesRead = 0;
        while (true)
        {
            Tuple<NTStatus, byte[]> tuple__ = await fileStore.ReadFile(fileHandle, bytesRead, (int)client.MaxReadSize);
            status = tuple__.Item1;
            buffer = tuple__.Item2;

            if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_END_OF_FILE)
            {
                throw new Exception("Failed to read from file");
            }

            if (status == NTStatus.STATUS_END_OF_FILE || buffer.Length == 0)
            {
                break;
            }
            bytesRead += buffer.Length;
            data_list.AddRange(buffer);

            await Task.Yield();
        }
        data = data_list.ToArray();

        status = await fileStore.CloseFile(fileHandle);
        status = await fileStore.Disconnect();

        return data;
    }
}
