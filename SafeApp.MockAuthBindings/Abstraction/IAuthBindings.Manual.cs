﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SafeApp.Core;

namespace SafeAuthenticator
{
    internal partial interface IAuthBindings
    {
        void CreateAccount(string locator, string secret, Action disconnectedCb, Action<FfiResult, IntPtr, GCHandle> cb);

        Task<IpcReq> DecodeIpcMessage(IntPtr authPtr, string uri);

        void Login(string locator, string secret, Action disconnectedCb, Action<FfiResult, IntPtr, GCHandle> cb);

        Task<IpcReq> UnRegisteredDecodeIpcMsgAsync(string msg);
    }
}
