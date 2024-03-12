/* Copyright (C) 2017-2023 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using SMBLibrary.SMB2;
using System.Threading.Tasks;
using Utilities;

namespace SMBLibrary.Client
{
    public class SMB2FileStore
    {
        private const int BytesPerCredit = 65536;

        private SMB2Client m_client;
        private uint m_treeID;
        private bool m_encryptShareData;

        public SMB2FileStore(SMB2Client client, uint treeID, bool encryptShareData)
        {
            m_client = client;
            m_treeID = treeID;
            m_encryptShareData = encryptShareData;
        }

        public async Task<Tuple<NTStatus, object, FileStatus>> CreateFile(string path, AccessMask desiredAccess, FileAttributes fileAttributes, ShareAccess shareAccess, CreateDisposition createDisposition, CreateOptions createOptions, SecurityContext securityContext)
        {
            object handle = null;
            FileStatus fileStatus = FileStatus.FILE_DOES_NOT_EXIST;
            CreateRequest request = new CreateRequest();
            request.Name = path;
            request.DesiredAccess = desiredAccess;
            request.FileAttributes = fileAttributes;
            request.ShareAccess = shareAccess;
            request.CreateDisposition = createDisposition;
            request.CreateOptions = createOptions;
            request.ImpersonationLevel = ImpersonationLevel.Impersonation;
            TrySendCommand(request);

            SMB2Command response = await m_client.WaitForCommand(request.MessageID);
            if (response != null)
            {
                if (response.Header.Status == NTStatus.STATUS_SUCCESS && response is CreateResponse)
                {
                    CreateResponse createResponse = ((CreateResponse)response);
                    handle = createResponse.FileId;
                    fileStatus = ToFileStatus(createResponse.CreateAction);
                }
                return new Tuple<NTStatus, object, FileStatus> (response.Header.Status, handle, fileStatus);
            }

            return new Tuple<NTStatus, object, FileStatus> (NTStatus.STATUS_INVALID_SMB, handle, fileStatus);
        }

        public async Task<NTStatus> CloseFile(object handle)
        {
            CloseRequest request = new CloseRequest();
            request.FileId = (FileID)handle;
            TrySendCommand(request);
            SMB2Command response = await m_client.WaitForCommand(request.MessageID);
            if (response != null)
            {
                return response.Header.Status;
            }

            return NTStatus.STATUS_INVALID_SMB;
        }

        public async Task<Tuple<NTStatus, byte[]>> ReadFile(object handle, long offset, int maxCount)
        {
            byte [] data = null;
            ReadRequest request = new ReadRequest();
            request.Header.CreditCharge = (ushort)Math.Ceiling((double)maxCount / BytesPerCredit);
            request.FileId = (FileID)handle;
            request.Offset = (ulong)offset;
            request.ReadLength = (uint)maxCount;
            
            TrySendCommand(request);
            SMB2Command response = await m_client.WaitForCommand(request.MessageID);
            if (response != null)
            {
                if (response.Header.Status == NTStatus.STATUS_SUCCESS && response is ReadResponse)
                {
                    data = ((ReadResponse)response).Data;
                }
                return new Tuple<NTStatus, byte[]> (response.Header.Status, data);
            }

            return new Tuple<NTStatus, byte[]> (NTStatus.STATUS_INVALID_SMB, data);
        }

        public async Task<Tuple<NTStatus, int>> WriteFile(object handle, long offset, byte[] data)
        {
            int numberOfBytesWritten = 0;
            WriteRequest request = new WriteRequest();
            request.Header.CreditCharge = (ushort)Math.Ceiling((double)data.Length / BytesPerCredit);
            request.FileId = (FileID)handle;
            request.Offset = (ulong)offset;
            request.Data = data;

            TrySendCommand(request);
            SMB2Command response = await m_client.WaitForCommand(request.MessageID);
            if (response != null)
            {
                if (response.Header.Status == NTStatus.STATUS_SUCCESS && response is WriteResponse)
                {
                    numberOfBytesWritten = (int)((WriteResponse)response).Count;
                }
                return new Tuple<NTStatus, int> (response.Header.Status, numberOfBytesWritten);
            }

            return new Tuple<NTStatus, int> (NTStatus.STATUS_INVALID_SMB, numberOfBytesWritten);
        }

        public async Task<NTStatus> FlushFileBuffers(object handle)
        {
            FlushRequest request = new FlushRequest();
            request.FileId = (FileID) handle;

            TrySendCommand(request);
            SMB2Command response = await m_client.WaitForCommand(request.MessageID);
            if (response != null)
            {
                if (response.Header.Status == NTStatus.STATUS_SUCCESS && response is FlushResponse)
                {
                    return response.Header.Status;
                }
            }

            return NTStatus.STATUS_INVALID_SMB;
        }

        public NTStatus LockFile(object handle, long byteOffset, long length, bool exclusiveLock)
        {
            throw new NotImplementedException();
        }

        public NTStatus UnlockFile(object handle, long byteOffset, long length)
        {
            throw new NotImplementedException();
        }

        public async Task<Tuple<NTStatus, List<QueryDirectoryFileInformation>>> QueryDirectory(object handle, string fileName, FileInformationClass informationClass)
        {
            List<QueryDirectoryFileInformation> result = new List<QueryDirectoryFileInformation>();
            QueryDirectoryRequest request = new QueryDirectoryRequest();
            request.Header.CreditCharge = (ushort)Math.Ceiling((double)m_client.MaxTransactSize / BytesPerCredit);
            request.FileInformationClass = informationClass;
            request.Reopen = true;
            request.FileId = (FileID)handle;
            request.OutputBufferLength = m_client.MaxTransactSize;
            request.FileName = fileName;

            TrySendCommand(request);
            SMB2Command response = await m_client.WaitForCommand(request.MessageID);
            if (response != null)
            {
                while (response.Header.Status == NTStatus.STATUS_SUCCESS && response is QueryDirectoryResponse)
                {
                    List<QueryDirectoryFileInformation> page = ((QueryDirectoryResponse)response).GetFileInformationList(informationClass);
                    result.AddRange(page);
                    request.Reopen = false;
                    TrySendCommand(request);
                    response = await m_client.WaitForCommand(request.MessageID);
                }
                return new Tuple<NTStatus, List<QueryDirectoryFileInformation>> (response.Header.Status, result);
            }

            return new Tuple<NTStatus, List<QueryDirectoryFileInformation>> (NTStatus.STATUS_INVALID_SMB, result);
        }

        public async Task<Tuple<NTStatus, FileInformation>> GetFileInformation(object handle, FileInformationClass informationClass)
        {
            FileInformation result = null;
            QueryInfoRequest request = new QueryInfoRequest();
            request.InfoType = InfoType.File;
            request.FileInformationClass = informationClass;
            request.OutputBufferLength = 4096;
            request.FileId = (FileID)handle;

            TrySendCommand(request);
            SMB2Command response = await m_client.WaitForCommand(request.MessageID);
            if (response != null)
            {
                if (response.Header.Status == NTStatus.STATUS_SUCCESS && response is QueryInfoResponse)
                {
                    result = ((QueryInfoResponse)response).GetFileInformation(informationClass);
                }
                return new Tuple<NTStatus, FileInformation> (response.Header.Status, result);
            }

            return new Tuple<NTStatus, FileInformation> (NTStatus.STATUS_INVALID_SMB, result);
        }

        public async Task<NTStatus> SetFileInformation(object handle, FileInformation information)
        {
            SetInfoRequest request = new SetInfoRequest();
            request.InfoType = InfoType.File;
            request.FileInformationClass = information.FileInformationClass;
            request.FileId = (FileID)handle;
            request.SetFileInformation(information);

            TrySendCommand(request);
            SMB2Command response = await m_client.WaitForCommand(request.MessageID);
            if (response != null)
            {
                return response.Header.Status;
            }

            return NTStatus.STATUS_INVALID_SMB;
        }

        public async Task<Tuple<NTStatus, FileSystemInformation>> GetFileSystemInformation(FileSystemInformationClass informationClass)
        {
            FileSystemInformation result = null;
            Tuple<NTStatus, object, FileStatus> tuple = await CreateFile(String.Empty, (AccessMask)DirectoryAccessMask.FILE_LIST_DIRECTORY | (AccessMask)DirectoryAccessMask.FILE_READ_ATTRIBUTES | AccessMask.SYNCHRONIZE, 0, ShareAccess.Read | ShareAccess.Write | ShareAccess.Delete, CreateDisposition.FILE_OPEN, CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT | CreateOptions.FILE_DIRECTORY_FILE, null);
            NTStatus status = tuple.Item1;
            object fileHandle = tuple.Item2;
            FileStatus fileStatus = tuple.Item3;

            if (status != NTStatus.STATUS_SUCCESS)
            {
                return new Tuple<NTStatus, FileSystemInformation> (status, result);
            }

            Tuple<NTStatus, FileSystemInformation> tuple_ = await GetFileSystemInformation(fileHandle, informationClass);
            status = tuple_.Item1;
            result = tuple_.Item2;

            await CloseFile(fileHandle);
            return new Tuple<NTStatus, FileSystemInformation> (status, result);
        }

        public async Task<Tuple<NTStatus, FileSystemInformation>> GetFileSystemInformation(object handle, FileSystemInformationClass informationClass)
        {
            FileSystemInformation result = null;
            QueryInfoRequest request = new QueryInfoRequest();
            request.InfoType = InfoType.FileSystem;
            request.FileSystemInformationClass = informationClass;
            request.OutputBufferLength = 4096;
            request.FileId = (FileID)handle;

            TrySendCommand(request);
            SMB2Command response = await m_client.WaitForCommand(request.MessageID);
            if (response != null)
            {
                if (response.Header.Status == NTStatus.STATUS_SUCCESS && response is QueryInfoResponse)
                {
                    result = ((QueryInfoResponse)response).GetFileSystemInformation(informationClass);
                }
                return new Tuple<NTStatus, FileSystemInformation> (response.Header.Status, result);
            }

            return new Tuple<NTStatus, FileSystemInformation> (NTStatus.STATUS_INVALID_SMB, result);
        }

        public NTStatus SetFileSystemInformation(FileSystemInformation information)
        {
            throw new NotImplementedException();
        }

        public async Task<Tuple<NTStatus, SecurityDescriptor>> GetSecurityInformation(object handle, SecurityInformation securityInformation)
        {
            SecurityDescriptor result = null;
            QueryInfoRequest request = new QueryInfoRequest();
            request.InfoType = InfoType.Security;
            request.SecurityInformation = securityInformation;
            request.OutputBufferLength = 4096;
            request.FileId = (FileID)handle;

            TrySendCommand(request);
            SMB2Command response = await m_client.WaitForCommand(request.MessageID);
            if (response != null)
            {
                if (response.Header.Status == NTStatus.STATUS_SUCCESS && response is QueryInfoResponse)
                {
                    result = ((QueryInfoResponse)response).GetSecurityInformation();
                }
                return new Tuple<NTStatus, SecurityDescriptor> (response.Header.Status, result);
            }

            return new Tuple<NTStatus, SecurityDescriptor> (NTStatus.STATUS_INVALID_SMB, result);
        }

        public NTStatus SetSecurityInformation(object handle, SecurityInformation securityInformation, SecurityDescriptor securityDescriptor)
        {
            return NTStatus.STATUS_NOT_SUPPORTED;
        }

        public NTStatus NotifyChange(out object ioRequest, object handle, NotifyChangeFilter completionFilter, bool watchTree, int outputBufferSize, OnNotifyChangeCompleted onNotifyChangeCompleted, object context)
        {
            throw new NotImplementedException();
        }

        public NTStatus Cancel(object ioRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<Tuple<NTStatus, byte[]>> DeviceIOControl(object handle, uint ctlCode, byte[] input, int maxOutputLength)
        {
            byte [] output = null;
            IOCtlRequest request = new IOCtlRequest();
            request.Header.CreditCharge = (ushort)Math.Ceiling((double)maxOutputLength / BytesPerCredit);
            request.CtlCode = ctlCode;
            request.IsFSCtl = true;
            request.FileId = (FileID)handle;
            request.Input = input;
            request.MaxOutputResponse = (uint)maxOutputLength;
            TrySendCommand(request);
            SMB2Command response = await m_client.WaitForCommand(request.MessageID);

            if (response != null)
            {
                if ((response.Header.Status == NTStatus.STATUS_SUCCESS || response.Header.Status == NTStatus.STATUS_BUFFER_OVERFLOW) && response is IOCtlResponse)
                {
                    output = ((IOCtlResponse)response).Output;
                }
                return new Tuple<NTStatus, byte[]> (response.Header.Status, output);
            }

            return new Tuple<NTStatus, byte[]> (NTStatus.STATUS_INVALID_SMB, output);
        }

        public async Task<NTStatus> Disconnect()
        {
            TreeDisconnectRequest request = new TreeDisconnectRequest();
            TrySendCommand(request);
            SMB2Command response = await m_client.WaitForCommand(request.MessageID);
            if (response != null)
            {
                return response.Header.Status;
            }

            return NTStatus.STATUS_INVALID_SMB;
        }

        private void TrySendCommand(SMB2Command request)
        {
            request.Header.TreeID = m_treeID;
            if (!m_client.IsConnected)
            {
                throw new InvalidOperationException("The client is no longer connected");
            }
            m_client.TrySendCommand(request, m_encryptShareData);
        }

        public uint MaxReadSize
        {
            get
            {
                return m_client.MaxReadSize;
            }
        }

        public uint MaxWriteSize
        {
            get
            {
                return m_client.MaxWriteSize;
            }
        }

        private static FileStatus ToFileStatus(CreateAction createAction)
        {
            switch (createAction)
            {
                case CreateAction.FILE_SUPERSEDED:
                    return FileStatus.FILE_SUPERSEDED;
                case CreateAction.FILE_OPENED:
                    return FileStatus.FILE_OPENED;
                case CreateAction.FILE_CREATED:
                    return FileStatus.FILE_CREATED;
                case CreateAction.FILE_OVERWRITTEN:
                    return FileStatus.FILE_OVERWRITTEN;
                default:
                    return FileStatus.FILE_OPENED;
            }
        }
    }
}
