// This code is based on the one provided in: https://github.com/franklinwk/OpenIGTLink-Unity
// Modified by Alicia Pose Díez de la Lastra, from Universidad Carlos III de Madrid

using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime;
using System.Threading.Tasks;
using TMPro;


public class OpenIGTLinkConnect : MonoBehaviour
{
    public GameObject interactableObjectPrefab;
    public GameObject objectSpawner;

    ///////// CONNECT TO 3D SLICER PARAMETERS /////////
    uint headerSize = 58; // Size of the header of every OpenIGTLink message
    private SocketHandler socket; // Socket to connect to Slicer
    bool isConnected; // Boolean to check if the socket is connected
    string ipString; // IP address of the computer running Slicer
    int port; // Port of the computer running Slicer
    public TextMeshProUGUI serverIP_text;
    public TextMeshProUGUI port_text;

    Coroutine listeningRoutine; // Coroutine to control the listening part (3D Slicer -> Unity)
    Coroutine sendingRoutine; // Coroutine to control the sending part (Unity -> 3D Slicer)

    ///////// GENERAL VARIABLES /////////
    int scaleMultiplier = 1000; // Help variable to transform meters to millimeters and vice versa
       
    ///////// SEND /////////
    public List<ModelInfo> infoToSend; // Array of Models to send to Slicer
    
    /// CRC ECMA-182 to send messages to Slicer ///
    CRC64 crcGenerator;
    string CRC;
    ulong crcPolynomial;
    string crcPolynomialBinary = "0100001011110000111000011110101110101001111010100011011010010011";

    void Start()
    {
        // Initialize CRC Generator
        crcGenerator = new CRC64();
        crcPolynomial = Convert.ToUInt64(crcPolynomialBinary, 2);
        crcGenerator.Init(crcPolynomial);

    }

    // This function is called when the user activates the connectivity switch to start the communication with 3D Slicer
    public void OnConnectToSlicerClick()
    {
        ipString = serverIP_text.text.Substring(0, serverIP_text.text.Length-1);
        port = int.Parse(port_text.text.Substring(0, port_text.text.Length-1));

        isConnected = ConnectToSlicer(ipString, port);
    }

    // Create a new socket handler and connect it to the server with the ip address and port provided in the function
    public bool ConnectToSlicer(string ipString, int port)
    {
        socket = new SocketHandler();

        Debug.Log("ipString: " + ipString);
        Debug.Log("port: " + port);
        // Assets/OpenIGTLinkConnectivity/OpenIGTLinkConnect.cs(79,28): error CS0029: Cannot implicitly convert type 'System.Threading.Tasks.Task<bool>' to 'bool'
        // bool isConnected = socket.Connect(ipString, port);
        socket.Connect(ipString, port);

        // start a coroutine
        listeningRoutine = StartCoroutine(ListenSlicerInfo());
        // sendingRoutine = StartCoroutine(SendTransformInfo());

        return isConnected;

    }

    // Routine that continuously sends the transform information of every model in infoToSend to 3D Slicer
    public IEnumerator SendTransformInfo()
    {
        while (true)
        {
            Debug.Log("Sending...");
            yield return null; // If you had written yield return new WaitForSeconds(1); it would have waited 1 second before executing the code below.
            // Loop foreach element in infoToSend
            foreach (ModelInfo element in infoToSend)
            {
                Debug.Log(element);
                SendMessageToServer.SendTransformMessage(element, scaleMultiplier, crcGenerator, CRC, socket);
            }
        }
    }

    // Routine that continuously listents to the incoming information from 3D Slicer. In the present code, this information could be in the form of a transform or an image message
    public IEnumerator ListenSlicerInfo()
    {
        while (true)
        {
            Debug.Log("\nListening...");
            yield return null;

            ////////// READ THE HEADER OF THE INCOMING MESSAGES //////////
            byte[] iMSGbyteArray = null;

            Task<byte[]> task = socket.Listen(60);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.Log("Error listening to the server");
            }
            else
            {
                iMSGbyteArray = task.Result;
 
                if (iMSGbyteArray.Length >= (int)headerSize)
                {
                    ////////// READ THE HEADER OF THE INCOMING MESSAGES //////////
                    // Store the information of the header in the structure iHeaderInfo
                    ReadMessageFromServer.HeaderInfo iHeaderInfo = ReadMessageFromServer.ReadHeaderInfo(iMSGbyteArray);

                    Debug.Log("Message type : "+ iHeaderInfo.msgType);

                    // Get the size of the body from the header information
                    uint bodySize = Convert.ToUInt32(iHeaderInfo.bodySize);

                    ////////// READ THE BODY OF THE INCOMING MESSAGES //////////
                    task = socket.Listen(bodySize-2);
                    yield return new WaitUntil(() => task.IsCompleted);

                    iMSGbyteArray = task.Result;

                    // Compare different message types and act accordingly
                    if (iMSGbyteArray.Length >= (int)bodySize-2)
                    {
                        if ((iHeaderInfo.msgType).Contains("STATUS"))
                        {
                            string status = ReadMessageFromServer.ExtractStatusInfo(iMSGbyteArray, iHeaderInfo);
                            Debug.Log("STATUS : " + status);
                        }
                        else if ((iHeaderInfo.msgType).Contains("TRANSFORM"))
                        {
                            Matrix4x4 matrix = ReadMessageFromServer.ExtractTransformInfo(iMSGbyteArray, scaleMultiplier, (int)iHeaderInfo.headerSize);
                            Debug.Log(matrix);

                        }
                        else if ((iHeaderInfo.msgType).Contains("IMAGE"))
                        {
                            Debug.Log("Image received");
                        }
                        else if ((iHeaderInfo.msgType).Contains("POLYDATA"))
                        {
                            Polydata polydata = ReadMessageFromServer.ExtractPolydataInfo(iMSGbyteArray, iHeaderInfo);
                            Debug.Log(polydata);
                            PolydataToMesh polydataToMesh = new PolydataToMesh(objectSpawner, interactableObjectPrefab);
                            polydataToMesh.renderPolydata(polydata);
                        }
                        else
                        {
                            Debug.Log("Message type not recognized");
                        }
                    }
                    else{
                        Debug.Log("Message body not complete. Waiting for the rest of the message...");
                        //TODO : Implement a way to wait for the rest of the message
                    }
                }

            }

        }
    }
    
    /// Apply transform information to GameObject ///
    void ApplyTransformToGameObject(Matrix4x4 matrix, GameObject gameObject)
    {
        Vector3 translation = matrix.GetColumn(3);
        //gameObject.transform.localPosition = new Vector3(-translation.x, translation.y, translation.z);
        //Vector3 rotation= matrix.rotation.eulerAngles;
        //gameObject.transform.localRotation = Quaternion.Euler(rotation.x, -rotation.y, -rotation.z);
        if (translation.x > 10000 || translation.y > 10000 || translation.z > 10000)
        {
            gameObject.transform.position = new Vector3(0, 0, 0.5f);
            Debug.Log("Out of limits. Default position assigned.");
        }
        else
        {
            gameObject.transform.localPosition = new Vector3(-translation.x, translation.y, translation.z);
            Vector3 rotation= matrix.rotation.eulerAngles;
            gameObject.transform.localRotation = Quaternion.Euler(rotation.x, -rotation.y, -rotation.z);
        }
    }

    
    // Called when the user disconnects Unity from 3D Slicer using the connectivity switch
    public void OnDisconnectClick()
    {
        socket.Disconnect();
        Debug.Log("Disconnected from the server");
    }


    // Execute this function when the user exits the application
    void OnApplicationQuit()
    {
        // Release the socket.
        if (socket != null)
        {
            socket.Disconnect();
        }
    }
}
