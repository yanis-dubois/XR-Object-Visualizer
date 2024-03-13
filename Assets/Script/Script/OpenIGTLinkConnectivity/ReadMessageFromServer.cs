// This code is based on the one provided in: https://github.com/franklinwk/OpenIGTLink-Unity
// Modified by Alicia Pose, from Universidad Carlos III de Madrid
// This script defines de structure to read transform and image messages using the OpenIGTLink communication protocol

using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

// Structure to handle the points of the polydata
public struct Point
{
    public float x;
    public float y;
    public float z;
}

/*

STRUCT_ARRAY

STRUCT_ARRAY contains an array of point indices that represent a geometric structure, including vertices, lines, polygons, and triangle strips. The number of structures (N_STRUCT) are specified by either NVERTICES, NLINES, NPOLYGONS, or NTRIANGLE_STRIPS (see the table above).
Data 	Type 	Description
STRUCT_0 	POINT_INDICES 	Point indices for 0th structure
... 	... 	...
STRUCT_(N_STRUCT-1) 	POINT_INDICES 	Point indices for N_STRUCT-1 th structure
*/
public struct pointIndices
{
    public UInt16 pointIndex;
}

public class ReadMessageFromServer
{
    // Information to send the transform
    static Matrix4x4 matrix = new Matrix4x4();
    
    //////////////////////////////// READING INCOMING MESSAGE ////////////////////////////////
    /// Define structure of the incoming message's header ///
    public struct HeaderInfo
    {
        public uint headerSize;
        public UInt16 versionNumber;
        public string msgType;
        public string deviceName;
        public UInt64 timestamp;
        public UInt64 bodySize;
        public UInt64 crc64;
        public UInt16 extHeaderSize;
    }
    /// Read incoming message's header ///
    public static HeaderInfo ReadHeaderInfo(byte[] iMSGbyteArray)
    {
        // Define the size of each of the components of the header 
        // according to the the OpenIGTLink protocol
        // See documentation: https://github.com/openigtlink/OpenIGTLink/blob/master/Documents/Protocol/header.md
        byte[] byteArray_Version = new byte[2];
        byte[] byteArray_MsgType = new byte[12];
        byte[] byteArray_DeviceName = new byte[20];
        byte[] byteArray_TimeStamp = new byte[8];
        byte[] byteArray_BodySize = new byte[8];
        byte[] byteArray_CRC = new byte[8];
        byte[] byteArray_ExtHeaderSize = new byte[2];

        // Define the offset to skip in the reader to reach the next variable (SP = starting point)
        int version_SP = 0;
        int msgType_SP = version_SP + byteArray_Version.Length;
        int deviceName_SP = msgType_SP + byteArray_MsgType.Length;
        int timeStamp_SP = deviceName_SP + byteArray_DeviceName.Length;
        int bodySize_SP = timeStamp_SP + byteArray_TimeStamp.Length;
        int crc_SP = bodySize_SP + byteArray_BodySize.Length;
        int extHeaderSize_SP = crc_SP + byteArray_CRC.Length;

        // Check if the incoming message has enough length to include the extHeader
        bool hasExtHeader = iMSGbyteArray.Length >= extHeaderSize_SP + byteArray_ExtHeaderSize.Length;

        // Store the information into the variables
        Buffer.BlockCopy(iMSGbyteArray, version_SP, byteArray_Version, 0, byteArray_Version.Length);
        Buffer.BlockCopy(iMSGbyteArray, msgType_SP, byteArray_MsgType, 0, byteArray_MsgType.Length);
        Buffer.BlockCopy(iMSGbyteArray, deviceName_SP, byteArray_DeviceName, 0, byteArray_DeviceName.Length);
        Buffer.BlockCopy(iMSGbyteArray, timeStamp_SP, byteArray_TimeStamp, 0, byteArray_TimeStamp.Length);
        Buffer.BlockCopy(iMSGbyteArray, bodySize_SP, byteArray_BodySize, 0, byteArray_BodySize.Length);
        Buffer.BlockCopy(iMSGbyteArray, crc_SP, byteArray_CRC, 0, byteArray_CRC.Length);

        if (hasExtHeader)
        {
            Buffer.BlockCopy(iMSGbyteArray, extHeaderSize_SP, byteArray_ExtHeaderSize, 0, byteArray_ExtHeaderSize.Length);
        }


        // If the message is Little Endian, convert it to Big Endian
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(byteArray_Version);
            //Array.Reverse(byteArray_MsgType);     // No need to reverse strings
            //Array.Reverse(byteArray_DeviceName);  // No need to reverse strings
            Array.Reverse(byteArray_TimeStamp);
            Array.Reverse(byteArray_BodySize);
            Array.Reverse(byteArray_CRC);
            Array.Reverse(byteArray_ExtHeaderSize);
        }

        // Convert the byte arrays to the corresponding data type
        UInt16 versionNumber_iMSG = BitConverter.ToUInt16(byteArray_Version);
        string msgType_iMSG = Encoding.ASCII.GetString(byteArray_MsgType);
        string deviceName_iMSG = Encoding.ASCII.GetString(byteArray_DeviceName);
        UInt64 timestamp_iMSG = BitConverter.ToUInt64(byteArray_TimeStamp);
        UInt64 bodySize_iMSG = BitConverter.ToUInt64(byteArray_BodySize);
        Debug.Log("BodySize: " + bodySize_iMSG);
        UInt64 crc_iMSG = BitConverter.ToUInt64(byteArray_CRC);
        UInt16 extHeaderSize_iMSG = BitConverter.ToUInt16(byteArray_ExtHeaderSize);
        

        
        // Store all this values in the HeaderInfo structure
        HeaderInfo incomingHeaderInfo = new HeaderInfo();
        incomingHeaderInfo.headerSize = 58;
        incomingHeaderInfo.versionNumber = versionNumber_iMSG;
        incomingHeaderInfo.msgType = msgType_iMSG;
        incomingHeaderInfo.deviceName = deviceName_iMSG;
        incomingHeaderInfo.timestamp = timestamp_iMSG;
        incomingHeaderInfo.bodySize = bodySize_iMSG;
        incomingHeaderInfo.crc64 = crc_iMSG;
        incomingHeaderInfo.extHeaderSize = extHeaderSize_iMSG;

        return incomingHeaderInfo;
    }
    
    /// Define structure of the incoming image information ///
    public struct ImageInfo
    {
        public UInt16 versionNumber;
        public int imComp;
        public int scalarType;
        public int endian;
        public int imCoord;
        public UInt16 numPixX;
        public UInt16 numPixY;
        public UInt16 numPixZ;
        public float xi;
        public float yi;
        public float zi;
        public float xj;
        public float yj;
        public float zj;
        public float xk;
        public float yk;
        public float zk;
        public float centerPosX;
        public float centerPosY;
        public float centerPosZ;
        public UInt16 startingIndexSVX;
        public UInt16 startingIndexSVY;
        public UInt16 startingIndexSVZ;
        public UInt16 numPixSVX;
        public UInt16 numPixSVY;
        public UInt16 numPixSVZ;
        public int offsetBeforeImageContent;
    }
    
    /// Read incoming image's information ///
    public static ImageInfo ReadImageInfo(byte[] iMSGbyteArrayComplete, uint headerSize, UInt16 extHeaderSize_iMSG)
    {
        // Define the variables stored in the body of the message
        int[] bodyArrayLengths = new int[]{2, 1, 1, 1, 1, 2, 2, 2, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 2, 2, 2, 2, 2};
        
        ImageInfo incomingImageInfo = new ImageInfo();

        int skipTheseBytes = (int)headerSize + (int)extHeaderSize_iMSG - 2;
        for (int index = 0; index < bodyArrayLengths.Length; index++)
        {
            byte[] sectionByteArray = new byte[bodyArrayLengths[index]];
            skipTheseBytes = skipTheseBytes + bodyArrayLengths[index];
            Buffer.BlockCopy(iMSGbyteArrayComplete, skipTheseBytes, sectionByteArray, 0, bodyArrayLengths[index]);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(sectionByteArray);
            }

            switch (index)
            {
                case 0: 
                    UInt16 versionNumber_bodyIm = BitConverter.ToUInt16(sectionByteArray);
                    incomingImageInfo.versionNumber = versionNumber_bodyIm; break;
                case 1:
                    byte[] bodyArray_ImComp = sectionByteArray;
                    incomingImageInfo.imComp = bodyArray_ImComp[0]; break;
                case 2: 
                    byte[] bodyArray_scalarType = sectionByteArray;
                    incomingImageInfo.scalarType = bodyArray_scalarType[0]; break;
                case 3:
                    byte[] bodyArray_Endian = sectionByteArray;
                    incomingImageInfo.endian = bodyArray_Endian[0]; break;
                case 4:
                    byte[] bodyArray_ImCoord = sectionByteArray;
                    incomingImageInfo.imCoord = bodyArray_ImCoord[0]; break;
                case 5:
                    UInt16 numPixX_bodyIm = BitConverter.ToUInt16(sectionByteArray);
                    incomingImageInfo.numPixX = numPixX_bodyIm; break;
                case 6:
                    UInt16 numPixY_bodyIm = BitConverter.ToUInt16(sectionByteArray);
                    incomingImageInfo.numPixY = numPixY_bodyIm; break;
                case 7:
                    UInt16 numPixZ_bodyIm = BitConverter.ToUInt16(sectionByteArray);
                    incomingImageInfo.numPixZ = numPixZ_bodyIm; break;
                case 8:
                    float xi_bodyIm = BitConverter.ToUInt16(sectionByteArray);
                    incomingImageInfo.xi = xi_bodyIm; break;
                case 9:
                    float yi_bodyIm = BitConverter.ToUInt16(sectionByteArray);
                    incomingImageInfo.yi = yi_bodyIm; break;
                case 10:
                    float zi_bodyIm = BitConverter.ToUInt16(sectionByteArray);
                    incomingImageInfo.zi = zi_bodyIm; break;
                case 11:
                    float xj_bodyIm = BitConverter.ToUInt16(sectionByteArray);
                    incomingImageInfo.xj = xj_bodyIm; break;
                case 12:
                    float yj_bodyIm = BitConverter.ToSingle(sectionByteArray, 0);
                    incomingImageInfo.yj = yj_bodyIm; break;
                case 13:
                    float zj_bodyIm = BitConverter.ToSingle(sectionByteArray, 0);
                    incomingImageInfo.zj = zj_bodyIm; break;
                case 14:
                    float xk_bodyIm = BitConverter.ToSingle(sectionByteArray, 0);
                    incomingImageInfo.xk = xk_bodyIm; break;
                case 15:
                    float yk_bodyIm = BitConverter.ToSingle(sectionByteArray, 0);
                    incomingImageInfo.yj = yk_bodyIm; break;
                case 16:
                    float zk_bodyIm = BitConverter.ToSingle(sectionByteArray, 0);
                    incomingImageInfo.zj = zk_bodyIm; break;
                case 17:
                    float centerPosX_bodyIm = BitConverter.ToSingle(sectionByteArray, 0);
                    incomingImageInfo.centerPosX = centerPosX_bodyIm; break;
                case 18:
                    float centerPosY_bodyIm = BitConverter.ToSingle(sectionByteArray, 0);
                    incomingImageInfo.centerPosY = centerPosY_bodyIm; break;
                case 19:
                    float centerPosZ_bodyIm = BitConverter.ToSingle(sectionByteArray, 0);
                    incomingImageInfo.centerPosZ = centerPosZ_bodyIm; break;
                case 20:
                    UInt16 startingIndexSVX_bodyIm = BitConverter.ToUInt16(sectionByteArray, 0);
                    incomingImageInfo.startingIndexSVX = startingIndexSVX_bodyIm; break;
                case 21:
                    UInt16 startingIndexSVY_bodyIm = BitConverter.ToUInt16(sectionByteArray, 0);
                    incomingImageInfo.startingIndexSVY = startingIndexSVY_bodyIm; break;
                case 22:
                    UInt16 startingIndexSVZ_bodyIm = BitConverter.ToUInt16(sectionByteArray, 0);
                    incomingImageInfo.startingIndexSVZ = startingIndexSVZ_bodyIm; break;
                case 23:
                    UInt16 numPixSVX_bodyIm = BitConverter.ToUInt16(sectionByteArray, 0);
                    incomingImageInfo.numPixSVX = numPixSVX_bodyIm; break;
                case 24:
                    UInt16 numPixSVY_bodyIm = BitConverter.ToUInt16(sectionByteArray, 0);
                    incomingImageInfo.numPixSVY = numPixSVY_bodyIm; break;
                case 25:
                    UInt16 numPixSVZ_bodyIm = BitConverter.ToUInt16(sectionByteArray, 0);
                    incomingImageInfo.numPixSVZ = numPixSVZ_bodyIm; break;
                default:
                    break;
                
            }
        }
        
        int offsetBeforeImageContent = skipTheseBytes;
        incomingImageInfo.offsetBeforeImageContent = offsetBeforeImageContent;
        
        return incomingImageInfo;
    }


    //////////////////////////////// INCOMING TRANSFORM MESSAGE ////////////////////////////////
    /// Extract transform information ///
    public static Matrix4x4 ExtractTransformInfo(byte[] iMSGbyteArray, int scaleMultiplier, int headerSize)
    {
        byte[] matrixBytes = new byte[4];
        float[] m = new float[16];
        for (int i = 0; i < 16; i++)
        { 
            Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 12 + i * 4, matrixBytes, 0, 4); // We add +12 to skip the extended header
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(matrixBytes);
            }

            m[i] = BitConverter.ToSingle(matrixBytes, 0);
            
        }
        
        matrix.SetRow(0, new Vector4(m[0], m[3], m[6], m[9] / scaleMultiplier));
        matrix.SetRow(1, new Vector4(m[1], m[4], m[7], m[10] / scaleMultiplier));
        matrix.SetRow(2, new Vector4(m[2], m[5], m[8], m[11] / scaleMultiplier));
        matrix.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
        
        return matrix;
    }

    public static String ExtractStatusInfo(byte[] iMSGbyteArray, ReadMessageFromServer.HeaderInfo iHeaderInfo)
    {
        /*
        C 	        uint16 	            Status code groups: 1-Ok, 2-Generic Error, ... (see below)
        SUB_CODE 	int64 	            Sub-code for the error (ex. 0x200 - file not found)
        ERROR_NAME 	char[20] 	        "Error", "OK", "Warning" - can be anything, don't relay on this
        MESSAGE 	char[BODY_SIZE-30] 	Optional (English) description (ex. "File C:\test.ini not found")
        */

        // uint headerSize = iHeaderInfo.headerSize + iHeaderInfo.extHeaderSize;
        uint headerSize = iHeaderInfo.extHeaderSize;
        headerSize = headerSize - 2;

        byte[] statusBytes = new byte[2];
        byte[] subCodeBytes = new byte[8];
        byte[] errorNameBytes = new byte[20];
        byte[] messageBytes = new byte[iMSGbyteArray.Length - headerSize - 30];

        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize, statusBytes, 0, 2);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 2, subCodeBytes, 0, 8);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 10, errorNameBytes, 0, 20);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 30, messageBytes, 0, iMSGbyteArray.Length - (int)headerSize - 30);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(statusBytes);
            Array.Reverse(subCodeBytes);
        }

        UInt16 status = BitConverter.ToUInt16(statusBytes);
        Int64 subCode = BitConverter.ToInt64(subCodeBytes);
        String errorName = Encoding.ASCII.GetString(errorNameBytes);
        String message = Encoding.ASCII.GetString(messageBytes);

        return "Status: " + status + " SubCode: " + subCode + " ErrorName: " + errorName + " Message: " + message;
    }

    public static string ExtractPolydataInfo(byte[] iMSGbyteArray, ReadMessageFromServer.HeaderInfo iHeaderInfo)
    {
        /*
        Data 	                Type 	        Description
        NPOINTS 	            uint32 	        Number of points
        NVERTICES 	            uint32 	        Number of vertices
        SIZE_VERTICES 	        uint32 	        Total size of vertices data
        NLINES 	                uint32 	        Number of lines
        SIZE_LINES 	            uint32 	        Total size of line data
        NPOLYGONS 	            uint32 	        Number of polygons
        SIZE_POLYGONS 	        uint32 	        Total size of polygon data
        NTRIANGLE_STRIPS 	    uint32 	        Number of triangle strips
        SIZE_TRIANGLE_STRIPS 	uint32 	        Total size of triangle strips data
        NATTRIBUTES 	        uint32 	        Number of dataset attributes
        POINTS 	                POINTS      	Coordinates of point 0 - (NPOINTS-1)
        VERTICES 	            STRUCT_ARRAY    Array of vertices
        LINES 	                STRUCT_ARRAY    Array of lines
        POLYGONS 	            STRUCT_ARRAY    Array of polygons
        TRIANGLE_STRIPS 	    STRUCT_ARRAY    Array of triangle strips
        ATTRIBUTES 	            ATTRIBUTES 	    Attributes
        */
        uint headerSize = iHeaderInfo.extHeaderSize;
        headerSize = headerSize - 2;

        byte[] nPointsBytes = new byte[4];
        byte[] nVerticesBytes = new byte[4];
        byte[] sizeVerticesBytes = new byte[4];
        byte[] nLinesBytes = new byte[4];
        byte[] sizeLinesBytes = new byte[4];
        byte[] nPolygonsBytes = new byte[4];
        byte[] sizePolygonsBytes = new byte[4];
        byte[] nTriangleStripsBytes = new byte[4];
        byte[] sizeTriangleStripsBytes = new byte[4];
        byte[] nAttributesBytes = new byte[4];

        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize, nPointsBytes, 0, 4);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 4, nVerticesBytes, 0, 4);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 8, sizeVerticesBytes, 0, 4);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 12, nLinesBytes, 0, 4);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 16, sizeLinesBytes, 0, 4);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 20, nPolygonsBytes, 0, 4);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 24, sizePolygonsBytes, 0, 4);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 28, nTriangleStripsBytes, 0, 4);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 32, sizeTriangleStripsBytes, 0, 4);
        Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 36, nAttributesBytes, 0, 4);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(nPointsBytes);
            Array.Reverse(nVerticesBytes);
            Array.Reverse(sizeVerticesBytes);
            Array.Reverse(nLinesBytes);
            Array.Reverse(sizeLinesBytes);
            Array.Reverse(nPolygonsBytes);
            Array.Reverse(sizePolygonsBytes);
            Array.Reverse(nTriangleStripsBytes);
            Array.Reverse(sizeTriangleStripsBytes);
            Array.Reverse(nAttributesBytes);
        }

        UInt32 nPoints = BitConverter.ToUInt32(nPointsBytes);
        UInt32 nVertices = BitConverter.ToUInt32(nVerticesBytes);
        UInt32 sizeVertices = BitConverter.ToUInt32(sizeVerticesBytes);
        UInt32 nLines = BitConverter.ToUInt32(nLinesBytes);
        UInt32 sizeLines = BitConverter.ToUInt32(sizeLinesBytes);
        UInt32 nPolygons = BitConverter.ToUInt32(nPolygonsBytes);
        UInt32 sizePolygons = BitConverter.ToUInt32(sizePolygonsBytes);
        UInt32 nTriangleStrips = BitConverter.ToUInt32(nTriangleStripsBytes);
        UInt32 sizeTriangleStrips = BitConverter.ToUInt32(sizeTriangleStripsBytes);
        UInt32 nAttributes = BitConverter.ToUInt32(nAttributesBytes);

        /*
        Data 	                                        Type 	        Description
        P0X,P0Y,P0Z 	                                float32[3] 	    Coordinates for point 0
        ... 	                                        ... 	        ...
        P(NPOINTS-1)X,P(NPOINTS-1)Y,P(NPOINTS-1)Z 	    float32[3] 	    Coordinates for point (NPOINTS-1)
        */
        List<Point> points = new List<Point>();

        for (int i = 0; i < nPoints; i++)
        {
            Point point = new Point();
            byte[] pointBytes = new byte[12];
            Buffer.BlockCopy(iMSGbyteArray, (int)headerSize + 40 + i * 12, pointBytes, 0, 12);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(pointBytes);
            }
            point.x = BitConverter.ToSingle(pointBytes, 0);
            point.y = BitConverter.ToSingle(pointBytes, 4);
            point.z = BitConverter.ToSingle(pointBytes, 8);
            points.Add(point);
        }

        StringBuilder polydataValues = new StringBuilder();
        polydataValues.AppendLine("Number of points: " + nPoints);
        polydataValues.AppendLine("Number of vertices: " + nVertices);
        polydataValues.AppendLine("Total size of vertices data: " + sizeVertices);
        polydataValues.AppendLine("Number of lines: " + nLines);
        polydataValues.AppendLine("Total size of line data: " + sizeLines);
        polydataValues.AppendLine("Number of polygons: " + nPolygons);
        polydataValues.AppendLine("Total size of polygon data: " + sizePolygons);
        polydataValues.AppendLine("Number of triangle strips: " + nTriangleStrips);
        polydataValues.AppendLine("Total size of triangle strips data: " + sizeTriangleStrips);
        polydataValues.AppendLine("Number of dataset attributes: " + nAttributes);

        // foreach (Point point in points)
        // {
        //     polydataValues.AppendLine("Point: (" + point.x + ", " + point.y + ", " + point.z + ")");
        // }

        return polydataValues.ToString();
        
    }
}
