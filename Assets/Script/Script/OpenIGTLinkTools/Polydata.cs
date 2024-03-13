using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct pointIndices
{
    public uint NINDICES;
    public uint[] POINT_INDEX;

    public pointIndices(uint nIndices, uint[] pointIndex)
    {
        NINDICES = nIndices;
        POINT_INDEX = pointIndex;
    }
}
public class Polydata
{
    private UInt32 nPoints;
    private UInt32 nVertices;
    private UInt32 sizeVertices;
    private UInt32 nLines;
    private UInt32 sizeLines;
    private UInt32 nPolygons;
    private UInt32 sizePolygons;
    private UInt32 nTriangleStrips;
    private UInt32 sizeTriangleStrips;
    private UInt32 nAttributes;

    private Vector3[] points;
    private List<pointIndices> vertices = new List<pointIndices>();
    private List<pointIndices> lines = new List<pointIndices>();
    private List<pointIndices> polygons = new List<pointIndices>();
    private List<pointIndices> triangleStrips = new List<pointIndices>();

    // Setters and Getters for attributes
    public UInt32 NPoints
    {
        get { return nPoints; }
        set { nPoints = value; }
    }

    public UInt32 NVertices
    {
        get { return nVertices; }
        set { nVertices = value; }
    }

    public UInt32 SizeVertices
    {
        get { return sizeVertices; }
        set { sizeVertices = value; }
    }

    public UInt32 NLines
    {
        get { return nLines; }
        set { nLines = value; }
    }

    public UInt32 SizeLines
    {
        get { return sizeLines; }
        set { sizeLines = value; }
    }

    public UInt32 NPolygons
    {
        get { return nPolygons; }
        set { nPolygons = value; }
    }

    public UInt32 SizePolygons
    {
        get { return sizePolygons; }
        set { sizePolygons = value; }
    }

    public UInt32 NTriangleStrips
    {
        get { return nTriangleStrips; }
        set { nTriangleStrips = value; }
    }

    public UInt32 SizeTriangleStrips
    {
        get { return sizeTriangleStrips; }
        set { sizeTriangleStrips = value; }
    }

    public UInt32 NAttributes
    {
        get { return nAttributes; }
        set { nAttributes = value; }
    }

    public Vector3[] Points
    {
        get { return points; }
        set { points = value; }
    }

    public List<pointIndices> Vertices
    {
        get { return vertices; }
        set { vertices = value; }
    }

    public List<pointIndices> Lines
    {
        get { return lines; }
        set { lines = value; }
    }

    public List<pointIndices> Polygons
    {
        get { return polygons; }
        set { polygons = value; }
    }

    public List<pointIndices> TriangleStrips
    {
        get { return triangleStrips; }
        set { triangleStrips = value; }
    }

    public int setPointsFromData(int offset, byte[] iMSGbyteArray)
    {
        points = new Vector3[nPoints];
        for (int i = 0; i < nPoints; i++)
        {
            byte[] xBytes = new byte[4];
            Buffer.BlockCopy(iMSGbyteArray, offset, xBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(xBytes);
            }
            float x = BitConverter.ToSingle(xBytes);

            byte[] yBytes = new byte[4];
            Buffer.BlockCopy(iMSGbyteArray, offset + 4, yBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(yBytes);
            }
            float y = BitConverter.ToSingle(yBytes);

            byte[] zBytes = new byte[4];
            Buffer.BlockCopy(iMSGbyteArray, offset + 8, zBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(zBytes);
            }
            float z = BitConverter.ToSingle(zBytes);

            points[i] = new Vector3(x, y, z);
            offset = offset + 12;
        }
        return offset;
    }

    // Fill the struct array
    public int setVerticesFromData(int offset, byte[] iMSGbyteArray){
        for(int i=0; i < nVertices; i++)
        {
            byte[] nIndicesBytes = new byte[4];
            Buffer.BlockCopy(iMSGbyteArray, offset, nIndicesBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(nIndicesBytes);
            }
            uint nIndices = BitConverter.ToUInt32(nIndicesBytes);

            uint[] pointIndex = new uint[nIndices];
            for (int j = 0; j < nIndices; j++)
            {
                byte[] pointIndexBytes = new byte[4];
                Buffer.BlockCopy(iMSGbyteArray, offset + 4 + j * 4, pointIndexBytes, 0, 4);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(pointIndexBytes);
                }
                pointIndex[j] = BitConverter.ToUInt32(pointIndexBytes);
            }
            pointIndices vertex = new pointIndices(nIndices, pointIndex);
            vertices.Add(vertex);
            offset = offset + 4 + (int)nIndices * 4;
        }
        return offset;
    }

    public int setLinesFromData(int offset, byte[] iMSGbyteArray){
        for(int i=0; i < nLines; i++)
        {
            byte[] nIndicesBytes = new byte[4];
            Buffer.BlockCopy(iMSGbyteArray, offset, nIndicesBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(nIndicesBytes);
            }
            uint nIndices = BitConverter.ToUInt32(nIndicesBytes);

            uint[] pointIndex = new uint[nIndices];
            for (int j = 0; j < nIndices; j++)
            {
                byte[] pointIndexBytes = new byte[4];
                Buffer.BlockCopy(iMSGbyteArray, offset + 4 + j * 4, pointIndexBytes, 0, 4);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(pointIndexBytes);
                }
                pointIndex[j] = BitConverter.ToUInt32(pointIndexBytes);
            }
            pointIndices line = new pointIndices(nIndices, pointIndex);
            lines.Add(line);
            offset = offset + 4 + (int)nIndices * 4;
        }
        return offset;
    }

    public int setPolygonsFromData(int offset, byte[] iMSGbyteArray){
        for(int i=0; i < nPolygons; i++)
        {
            byte[] nIndicesBytes = new byte[4];
            Buffer.BlockCopy(iMSGbyteArray, offset, nIndicesBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(nIndicesBytes);
            }
            uint nIndices = BitConverter.ToUInt32(nIndicesBytes);

            uint[] pointIndex = new uint[nIndices];
            for (int j = 0; j < nIndices; j++)
            {
                byte[] pointIndexBytes = new byte[4];
                Buffer.BlockCopy(iMSGbyteArray, offset + 4 + j * 4, pointIndexBytes, 0, 4);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(pointIndexBytes);
                }
                pointIndex[j] = BitConverter.ToUInt32(pointIndexBytes);
            }
            pointIndices polygon = new pointIndices(nIndices, pointIndex);
            polygons.Add(polygon);
            offset = offset + 4 + (int)nIndices * 4;
        }
        return offset;
    }

    public int setTriangleStripsFromData(int offset, byte[] iMSGbyteArray){
        for(int i=0; i < nTriangleStrips; i++)
        {
            byte[] nIndicesBytes = new byte[4];
            Buffer.BlockCopy(iMSGbyteArray, offset, nIndicesBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(nIndicesBytes);
            }
            uint nIndices = BitConverter.ToUInt32(nIndicesBytes);

            uint[] pointIndex = new uint[nIndices];
            for (int j = 0; j < nIndices; j++)
            {
                byte[] pointIndexBytes = new byte[4];
                Buffer.BlockCopy(iMSGbyteArray, offset + 4 + j * 4, pointIndexBytes, 0, 4);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(pointIndexBytes);
                }
                pointIndex[j] = BitConverter.ToUInt32(pointIndexBytes);
            }
            pointIndices triangleStrip = new pointIndices(nIndices, pointIndex);
            triangleStrips.Add(triangleStrip);
            offset = offset + 4 + (int)nIndices * 4;
        }
        return offset;
    }

}