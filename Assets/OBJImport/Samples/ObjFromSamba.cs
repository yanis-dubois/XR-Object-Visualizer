using Dummiesman;
using SMBLibrary;
using SMBLibrary.Client;
using System;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjFromSamba : MonoBehaviour
{

    public GameObject objectSpawner; 

    // Start is called before the first frame update
    void Start()
    {
        // Server details and credentials
        string serverIP = "192.168.43.132";
        string shareName = "dicom";
        string username = "user";
        string password = "password";
        string domain = "";

        SMB2Client client = connectToServer(serverIP, username, password, domain);

        if(client != null){
            List<string> shares = ListShares(client);
            // Print shares
            foreach(string share in shares){
                Debug.Log("shares : " + share);
            }

            List<string> files = listFiles(client, shareName);
            // Print files
            foreach(string file in files){
                Debug.Log("files : " + file);
            }

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
                status = fileStore.CloseFile(directoryHandle);

                // cast each item in the fileList to a FileDirectoryInformation
                foreach (FileDirectoryInformation file in tmp_fileList)
                {
                    fileList.Add(file.FileName);
                }
            }
        }

        status = fileStore.Disconnect();
        return fileList;
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

            // make object interactable
            loadedObj.transform.parent = objectSpawner.transform;
            
            foreach (Transform child in loadedObj.transform) {
                // rescale
                Vector3 size = child.GetComponent<Renderer>().bounds.size;
                child.transform.localScale = new Vector3(1.0f/size.x, 1.0f/size.y, 1.0f/size.z);

                // move 
                Vector3 position = child.GetComponent<Renderer>().bounds.center;
                child.transform.position += Vector3.one - position;

                // temp
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "base" };
                child.GetComponent<Renderer>().material = mat; 
            }
        }
        status = fileStore.CloseFile(fileHandle);
        status = fileStore.Disconnect();
    }
}
