using Dummiesman;
using SMBLibrary;
using SMBLibrary.Client;
using System;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjFromSamba : MonoBehaviour
{
    public GameObject interactableObjectPrefab;
    public GameObject objectSpawner; 

    // Server details and credentials
    public TextMeshProUGUI serverIP_text;
    public TextMeshProUGUI shareName_text;
    public TextMeshProUGUI filePath_text;
    public TextMeshProUGUI username_text;
    public TextMeshProUGUI password_text;

    public void LoadObject()
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

        SMB2Client client = connectToServer(serverIP, username, password, domain);
        if (client != null) {
            List<string> shares = ListShares(client);
            // Print shares
            foreach (string share in shares) {
                Debug.Log("shares : " + share);
            }

            List<string> files = listFiles(client, shareName);
            // Print files
            foreach (string file in files) {
                Debug.Log("files : " + file);
            }

            // read the file
            readFile(client, shareName, filePath);
            disconnectFromServer(client);
        }
    }

    // Connect to the server
    SMB2Client connectToServer(string serverIP, string username, string password, string domain)
    {
        SMB2Client client = new SMB2Client();
        bool isConnected = client.Connect(serverIP, SMBTransportType.DirectTCPTransport);
        if (isConnected)
        {
            NTStatus status = client.Login(domain, username, password);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                return client;
            }
        }
        return null;
    }

    // Disconnect from the server
    void disconnectFromServer(SMB2Client client)
    {
        client.Logoff();
        client.Disconnect();
    }

    // List all the share from the server
    List<string> ListShares(SMB2Client client)
    {
        NTStatus status;
        List<string> shares = client.ListShares(out status);
        return shares;
    }

    List<string> listFiles(SMB2Client client, string shareName, string directoryPath = "./")
    {
        List<string> fileList = new List<string>();
        NTStatus status;

        ISMBFileStore fileStore = client.TreeConnect(shareName, out status);

        if (status == NTStatus.STATUS_SUCCESS)
        {
            object directoryHandle;
            FileStatus fileStatus;
            status = fileStore.CreateFile(out directoryHandle, out fileStatus, String.Empty, AccessMask.GENERIC_READ, FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                List<QueryDirectoryFileInformation> tmp_fileList;
                status = fileStore.QueryDirectory(out tmp_fileList, directoryHandle, "*", FileInformationClass.FileDirectoryInformation);

                // cast each item in the fileList to a FileDirectoryInformation
                foreach (FileDirectoryInformation file in tmp_fileList)
                {
                    fileList.Add(file.FileName);
                }

                status = fileStore.CloseFile(directoryHandle);
            }

            status = fileStore.Disconnect();
        } 
        else 
        {
            Debug.LogError("unable to open "+shareName+" share name");
        }
        
        return fileList;
    }

    void readFile(SMB2Client client, string shareName, string filePath)
    {
        NTStatus status;
        ISMBFileStore fileStore = client.TreeConnect(shareName, out status);

        if (status == NTStatus.STATUS_SUCCESS)
        {
            object fileHandle;
            FileStatus fileStatus;
            if (fileStore is SMB1FileStore)
            {
                filePath = @"\\" + filePath;
            }
            status = fileStore.CreateFile(out fileHandle, out fileStatus, filePath, AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null);
            
            // read file and save in stream
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            byte[] data;
            long bytesRead = 0;
            while (true)
            {
                status = fileStore.ReadFile(out data, fileHandle, bytesRead, (int)client.MaxReadSize);
                if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_END_OF_FILE)
                {
                    throw new Exception("Failed to read from file");
                }

                if (status == NTStatus.STATUS_END_OF_FILE || data.Length == 0)
                {
                    break;
                }
                bytesRead += data.Length;
                stream.Write(data, 0, data.Length);
            }
            // save and reload file
            string path = "Assets/OBJImport/Samples/tw.obj";
            System.IO.File.WriteAllBytes(path, stream.ToArray());
            var loadedObj = new OBJLoader().Load(path);
            OBJInstantiate.instantiate(interactableObjectPrefab, objectSpawner, loadedObj);

            status = fileStore.CloseFile(fileHandle);
            status = fileStore.Disconnect();
        }
        else 
        {
            Debug.LogError("unable to open "+shareName+" share name");
        }
    }
}
