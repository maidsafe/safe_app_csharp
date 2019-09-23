﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SafeApp.Core;

namespace SafeApp.AppBindings
{
    public partial interface IAppBindings
    {
        void SafeConnect(
            string appId,
            string authCredentials,
            Action<FfiResult, IntPtr, GCHandle> oCb);

        #region Keys

        Task<BlsKeyPair> GenerateKeyPairAsync(ref IntPtr app);

        Task<(string, BlsKeyPair?)> CreateKeysAsync(ref IntPtr app, string from, string preloadAmount, string pk);

        Task<(string, BlsKeyPair)> KeysCreatePreloadTestCoinsAsync(ref IntPtr app, string preloadAmount);

        Task<string> KeysBalanceFromSkAsync(ref IntPtr app, string sk);

        Task<string> KeysBalanceFromUrlAsync(ref IntPtr app, string url, string sk);

        Task<string> ValidateSkForUrlAsync(ref IntPtr app, string sk, string url);

        Task<ulong> KeysTransferAsync(ref IntPtr app, string amount, string fromSk, string toUrl, ulong txId);

        #endregion Keys
    }
}