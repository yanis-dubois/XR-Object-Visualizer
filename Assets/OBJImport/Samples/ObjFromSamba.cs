using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SMBLibrary;
using SMBLibrary.Client;
using System;
using System.Net;


public class ObjFromSamba : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Server details and credentials
        string serverIP = "192.168.1.17";
        string shareName = "dicom";
        string username = "user";
        string password = "password";
        string domain = "";

        SMB2Client client = connectToServer(serverIP, username, password, domain);

        if(client != null){
            List<string> shares = ListShares(client);

            // Print the shares
            foreach(string share in shares){
                Debug.Log(share);
            }

            List<string> files = listFiles(client, shareName);

            // read the file
            readFile(client, shareName, "obj/tw.obj");

            disconnectFromServer(client);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Connect to the server
    SMB2Client connectToServer(string serverIP, string username, string password, string domain)
    {
        SMB2Client client = new SMB2Client();
        bool isConnected = client.Connect(IPAddress.Parse(serverIP), SMBTransportType.DirectTCPTransport);
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
        NTStatus status;
        ISMBFileStore fileStore = client.TreeConnect(shareName, out status);
        if (status == NTStatus.STATUS_SUCCESS)
        {
            object directoryHandle;
            FileStatus fileStatus;
            status = fileStore.CreateFile(out directoryHandle, out fileStatus, String.Empty, AccessMask.GENERIC_READ, FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                List<QueryDirectoryFileInformation> fileList;
                status = fileStore.QueryDirectory(out fileList, directoryHandle, "*", FileInformationClass.FileDirectoryInformation);
                status = fileStore.CloseFile(directoryHandle);

                // cast each item in the fileList to a FileDirectoryInformation
                foreach (FileDirectoryInformation file in fileList)
                {
                    Debug.Log(file.FileName);
                }
            }
        }
        status = fileStore.Disconnect();
        return null;
    }

    void readFile(SMB2Client client, string shareName, string filePath)
    {
        NTStatus status;
        ISMBFileStore fileStore = client.TreeConnect(shareName, out status);
        object fileHandle;
        FileStatus fileStatus;
        if (fileStore is SMB1FileStore)
        {
            filePath = @"\\" + filePath;
        }
        status = fileStore.CreateFile(out fileHandle, out fileStatus, filePath, AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null);

        Debug.Log(status);
        if (status == NTStatus.STATUS_SUCCESS)
        {
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
            // save the file
            System.IO.File.WriteAllBytes("Assets/OBJImport/Samples/tw.obj", stream.ToArray());
        }
        status = fileStore.CloseFile(fileHandle);
        status = fileStore.Disconnect();
    }
}
