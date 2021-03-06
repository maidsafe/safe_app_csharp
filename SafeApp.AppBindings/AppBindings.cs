﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SafeApp.Core;

#if __IOS__
using ObjCRuntime;
#endif

namespace SafeApp.AppBindings
{
    internal partial class AppBindings : IAppBindings
    {
        #if __IOS__
        private const string DllName = "__Internal";
        #else
        private const string DllName = "sn_api";
        #endif

        #region App Level

        public bool AppIsMock()
        {
            var ret = AppIsMockNative();
            return ret;
        }

        [DllImport(DllName, EntryPoint = "app_is_mock")]
        private static extern bool AppIsMockNative();

        public Task AppSetConfigDirPathAsync(string newPath)
        {
            var (ret, userData) = BindingUtils.PrepareTask();
            AppSetConfigDirPathNative(newPath, userData, DelegateOnFfiResultCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "set_config_dir_path")]
        private static extern void AppSetConfigDirPathNative(
            [MarshalAs(UnmanagedType.LPStr)] string newPath,
            IntPtr userData,
            FfiResultCb oCb);

        #endregion

        #region IPC

        public Task<(uint, string)> EncodeAuthReqAsync(ref AuthReq req)
        {
            var reqNative = req.ToNative();
            var (ret, userData) = BindingUtils.PrepareTask<(uint, string)>();
            EncodeAuthReqNative(ref reqNative, userData, DelegateOnFfiResultUIntStringCb);
            reqNative.Free();
            return ret;
        }

        [DllImport(DllName, EntryPoint = "encode_auth_req")]
        private static extern void EncodeAuthReqNative(ref AuthReqNative req, IntPtr userData, FfiResultUIntStringCb oCb);

        public Task<(uint, string)> EncodeContainersReqAsync(ref ContainersReq req)
        {
            var reqNative = req.ToNative();
            var (ret, userData) = BindingUtils.PrepareTask<(uint, string)>();
            EncodeContainersReqNative(ref reqNative, userData, DelegateOnFfiResultUIntStringCb);
            reqNative.Free();
            return ret;
        }

        [DllImport(DllName, EntryPoint = "encode_containers_req")]
        private static extern void EncodeContainersReqNative(ref ContainersReqNative req, IntPtr userData, FfiResultUIntStringCb oCb);

        public Task<(uint, string)> EncodeUnregisteredReqAsync(byte[] extraData)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(uint, string)>();
            EncodeUnregisteredReqNative(extraData?.ToArray(), (UIntPtr)(extraData?.Length ?? 0), userData, DelegateOnFfiResultUIntStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "encode_unregistered_req")]
        private static extern void EncodeUnregisteredReqNative(
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
            byte[] extraData,
            UIntPtr extraDataLen,
            IntPtr userData,
            FfiResultUIntStringCb oCb);

        public Task<(uint, string)> EncodeShareMDataReqAsync(ref ShareMDataReq req)
        {
            var reqNative = req.ToNative();
            var (ret, userData) = BindingUtils.PrepareTask<(uint, string)>();
            EncodeShareMDataReqNative(ref reqNative, userData, DelegateOnFfiResultUIntStringCb);
            reqNative.Free();
            return ret;
        }

        [DllImport(DllName, EntryPoint = "encode_share_mdata_req")]
        private static extern void EncodeShareMDataReqNative(ref ShareMDataReqNative req, IntPtr userData, FfiResultUIntStringCb oCb);

        [DllImport(DllName, EntryPoint = "decode_ipc_msg")]
        private static extern void DecodeIpcMsgNative(
            [MarshalAs(UnmanagedType.LPStr)] string msg,
            IntPtr userData,
            UIntAuthGrantedCb oAuth,
            UIntByteListCb oUnregistered,
            UIntCb oContainers,
            UIntCb oShareMData,
            NoneCb oRevoked,
            FfiResultUIntCb oErr);

        private delegate void FfiResultUIntCb(IntPtr userData, IntPtr result, uint reqId);

        private delegate void FfiResultUIntStringCb(IntPtr userData, IntPtr result, uint reqId, string encoded);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultUIntStringCb))]
        #endif
        private static void OnFfiResultUIntStringCb(IntPtr userData, IntPtr result, uint reqId, string encoded)
        {
            BindingUtils.CompleteTask(userData, Marshal.PtrToStructure<FfiResult>(result), () => (reqId, encoded));
        }

        private static readonly FfiResultUIntStringCb DelegateOnFfiResultUIntStringCb = OnFfiResultUIntStringCb;

        private delegate void NoneCb(IntPtr userData);

        private delegate void UIntAuthGrantedCb(IntPtr userData, uint reqId, IntPtr authGranted);

        private delegate void UIntByteListCb(IntPtr userData, uint reqId, IntPtr serialisedCfgPtr, UIntPtr serialisedCfgLen);

        private delegate void UIntCb(IntPtr userData, uint reqId);

        #endregion

        #region Logging

        public Task AppInitLoggingAsync(string outputFileNameOverride)
        {
            var (ret, userData) = BindingUtils.PrepareTask();
            AppInitLoggingNative(outputFileNameOverride, userData, DelegateOnFfiResultCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "init_logging")]
        private static extern void AppInitLoggingNative(
            [MarshalAs(UnmanagedType.LPStr)] string outputFileNameOverride,
            IntPtr userData,
            FfiResultCb oCb);

        public Task<string> AppConfigDirPathAsync()
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            AppConfigDirPathNative(userData, DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "config_dir_path")]
        private static extern void AppConfigDirPathNative(IntPtr userData, FfiResultStringCb oCb);

        private delegate void FfiResultCb(IntPtr userData, IntPtr result);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultCb))]
        #endif
        private static void OnFfiResultCb(IntPtr userData, IntPtr result)
        {
            BindingUtils.CompleteTask(userData, Marshal.PtrToStructure<FfiResult>(result));
        }

        private static readonly FfiResultCb DelegateOnFfiResultCb = OnFfiResultCb;

        private delegate void FfiResultStringCb(IntPtr userData, IntPtr result, string logPath);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultStringCb))]
        #endif
        private static void OnFfiResultStringCb(IntPtr userData, IntPtr result, string logPath)
        {
            BindingUtils.CompleteTask(userData, Marshal.PtrToStructure<FfiResult>(result), () => logPath);
        }

        private static readonly FfiResultStringCb DelegateOnFfiResultStringCb = OnFfiResultStringCb;

        #endregion

        #region Authenticate

        public Task<string> AuthAppAsync(
            string appId,
            string appName,
            string appVendor,
            string endpoint)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            AuthAppNative(appId, appName, appVendor, endpoint, userData, DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "auth_app")]
        private static extern void AuthAppNative(
            [MarshalAs(UnmanagedType.LPStr)] string appId,
            [MarshalAs(UnmanagedType.LPStr)] string appName,
            [MarshalAs(UnmanagedType.LPStr)] string appVendor,
            [MarshalAs(UnmanagedType.LPStr)] string endpoint,
            IntPtr userData,
            FfiResultStringCb oCb);

        #endregion

        #region Connect
        public void ConnectApp(
            string appId,
            string authCredentials,
            Action<FfiResult, IntPtr, GCHandle> oCb)
        {
            var userData = BindingUtils.ToHandlePtr(oCb);
            ConnectAppNative(appId, authCredentials, userData, DelegateOnFfiResultSafeCb);
        }

        [DllImport(DllName, EntryPoint = "connect_app")]
        private static extern void ConnectAppNative(
            [MarshalAs(UnmanagedType.LPStr)] string appId,
            [MarshalAs(UnmanagedType.LPStr)] string authCredentials,
            IntPtr userData,
            FfiResultSafeCb oCb);

        private delegate void FfiResultSafeCb(IntPtr userData, IntPtr result, IntPtr app);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultSafeCb))]
        #endif
        private static void OnFfiResultSafeCb(IntPtr userData, IntPtr result, IntPtr app)
        {
            var action = BindingUtils.FromHandlePtr<Action<FfiResult, IntPtr, GCHandle>>(userData, false);
            action(Marshal.PtrToStructure<FfiResult>(result), app, GCHandle.FromIntPtr(userData));
        }

        private static readonly FfiResultSafeCb DelegateOnFfiResultSafeCb = OnFfiResultSafeCb;

        #endregion

        #region SafeUrl
        public Task<string> SafeUrlEncodeAsync(
            byte[] name,
            string nrsName,
            ulong typeTag,
            DataType dataType,
            ContentType contentType,
            string path,
            List<string> subNames,
            string queryString,
            string fragment,
            ulong contentVersion,
            SafeUrlBase baseEncoding)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            SafeUrlEncodeNative(
                name,
                nrsName,
                typeTag,
                (ushort)dataType,
                (ushort)contentType,
                path,
                subNames?.ToArray(),
                (UIntPtr)(subNames?.Count ?? 0),
                queryString,
                fragment,
                contentVersion,
                (ushort)baseEncoding,
                userData,
                DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "safe_url_encode")]
        private static extern void SafeUrlEncodeNative(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = (int)AppConstants.XorNameLen)] byte[] name,
            [MarshalAs(UnmanagedType.LPStr)] string nrsName,
            ulong typeTag,
            ulong dataType,
            ushort contentType,
            [MarshalAs(UnmanagedType.LPStr)] string path,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 7)] string[] subNames,
            UIntPtr subNamesLen,
            [MarshalAs(UnmanagedType.LPStr)] string queryString,
            [MarshalAs(UnmanagedType.LPStr)] string fragment,
            ulong contentVersion,
            ushort baseEncoding,
            IntPtr userData,
            FfiResultStringCb oCb);

        public Task<SafeUrl> NewSafeUrlAsync(
            byte[] name,
            string nrsName,
            ulong typeTag,
            DataType dataType,
            ContentType contentType,
            string path,
            List<string> subNames,
            string queryString,
            string fragment,
            ulong contentVersion)
        {
            var (ret, userData) = BindingUtils.PrepareTask<SafeUrl>();
            NewSafeUrlNative(
                name,
                nrsName,
                typeTag,
                (ulong)dataType,
                (ushort)contentType,
                path,
                subNames?.ToArray(),
                (UIntPtr)(subNames?.Count ?? 0),
                queryString,
                fragment,
                contentVersion,
                userData,
                DelegateOnFfiResultSafeUrlCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "new_safe_url")]
        private static extern void NewSafeUrlNative(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = (int)AppConstants.XorNameLen)] byte[] name,
            [MarshalAs(UnmanagedType.LPStr)] string nrsName,
            ulong typeTag,
            ulong dataType,
            ushort contentType,
            [MarshalAs(UnmanagedType.LPStr)] string path,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 7)] string[] subNames,
            UIntPtr subNamesLen,
            [MarshalAs(UnmanagedType.LPStr)] string queryString,
            [MarshalAs(UnmanagedType.LPStr)] string fragment,
            ulong contentVersion,
            IntPtr userData,
            FfiResultSafeUrlCb oCb);

        public Task<SafeUrl> SafeUrlFromUrlAsync(string safeUrl)
        {
            var (ret, userData) = BindingUtils.PrepareTask<SafeUrl>();
            SafeUrlFromUrlNative(safeUrl, userData, DelegateOnFfiResultSafeUrlCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "safe_url_from_url")]
        private static extern void SafeUrlFromUrlNative(
            [MarshalAs(UnmanagedType.LPStr)] string safeUrl,
            IntPtr userData,
            FfiResultSafeUrlCb oCb);

        public Task<string> EncodeSafekeyAsync(byte[] name, SafeUrlBase baseEncoding)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            EncodeSafekeyNative(name, (ushort)baseEncoding, userData, DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "encode_safekey")]
        private static extern void EncodeSafekeyNative(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = (int)AppConstants.XorNameLen)] byte[] name,
            ushort baseEncoding,
            IntPtr userData,
            FfiResultStringCb oCb);

        public Task<string> EncodeImmutableDataAsync(
            byte[] name,
            ContentType contentType,
            SafeUrlBase baseEncoding)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            EncodeImmutableDataNative(
                name,
                (ushort)contentType,
                (ushort)baseEncoding,
                userData,
                DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "encode_immutable_data")]
        private static extern void EncodeImmutableDataNative(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = (int)AppConstants.XorNameLen)] byte[] name,
            ushort contentType,
            ushort baseEncoding,
            IntPtr userData,
            FfiResultStringCb oCb);

        public Task<string> EncodeMutableDataAsync(
            byte[] name,
            ulong typeTag,
            ContentType contentType,
            SafeUrlBase baseEncoding)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            EncodeMutableDataNative(
                name,
                typeTag,
                (ushort)contentType,
                (ushort)baseEncoding,
                userData,
                DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "encode_mutable_data")]
        private static extern void EncodeMutableDataNative(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = (int)AppConstants.XorNameLen)] byte[] name,
            ulong typeTag,
            ushort contentType,
            ushort baseEncoding,
            IntPtr userData,
            FfiResultStringCb oCb);

        public Task<string> EncodeSequenceDataAsync(
            byte[] name,
            ulong typeTag,
            ContentType contentType,
            SafeUrlBase baseEncoding,
            bool isPrivate)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            EncodeSequenceDataNative(
                name,
                typeTag,
                (ushort)contentType,
                (ushort)baseEncoding,
                isPrivate,
                userData,
                DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "encode_sequence_data")]
        private static extern void EncodeSequenceDataNative(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = (int)AppConstants.XorNameLen)] byte[] name,
            ulong typeTag,
            ushort contentType,
            ushort baseEncoding,
            [MarshalAs(UnmanagedType.U1)] bool isPrivate,
            IntPtr userData,
            FfiResultStringCb oCb);

        private delegate void FfiResultSafeUrlCb(IntPtr userData, IntPtr result, IntPtr safeUrl);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultSafeUrlCb))]
        #endif
        private static void OnFfiResultSafeUrlCb(IntPtr userData, IntPtr result, IntPtr safeUrl)
        {
            BindingUtils.CompleteTask(userData, Marshal.PtrToStructure<FfiResult>(result), () => new SafeUrl(Marshal.PtrToStructure<SafeUrlNative>(safeUrl)));
        }

        private static readonly FfiResultSafeUrlCb DelegateOnFfiResultSafeUrlCb = OnFfiResultSafeUrlCb;

        #endregion

        #region Keys

        public Task<BlsKeyPair> GenerateKeyPairAsync(IntPtr app)
        {
            var (ret, userData) = BindingUtils.PrepareTask<BlsKeyPair>();
            GenerateKeyPairNative(app, userData, DelegateOnFfiResultBlsKeyPairCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "generate_keypair")]
        private static extern void GenerateKeyPairNative(
            IntPtr app,
            IntPtr userData,
            FfiResultBlsKeyPairCb oCb);

        private delegate void FfiResultBlsKeyPairCb(IntPtr userData, IntPtr result, IntPtr safeKey);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultBlsKeyPairCb))]
        #endif
        private static void OnFfiResultBlsKeyPairCb(IntPtr userData, IntPtr result, IntPtr safeKey)
            => BindingUtils.CompleteTask(
                userData,
                Marshal.PtrToStructure<FfiResult>(result),
                () => Marshal.PtrToStructure<BlsKeyPair>(safeKey));

        private static readonly FfiResultBlsKeyPairCb DelegateOnFfiResultBlsKeyPairCb = OnFfiResultBlsKeyPairCb;

        public Task<(string, BlsKeyPair)> CreateKeysAsync(
            IntPtr app,
            string from,
            string preloadAmount,
            string pk)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(string, BlsKeyPair)>();
            CreateKeysNative(app, from, preloadAmount, pk, userData, DelegateOnFfiResultStringBlsKeyPairCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "keys_create")]
        private static extern void CreateKeysNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string from,
            [MarshalAs(UnmanagedType.LPStr)] string preload,
            [MarshalAs(UnmanagedType.LPStr)] string pk,
            IntPtr userData,
            FfiResultStringBlsKeyPairCb oCb);

        public Task<(string, BlsKeyPair)> KeysCreatePreloadTestCoinsAsync(string preloadAmount)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(string, BlsKeyPair)>();
            KeysCreatePreloadTestCoinsNative(preloadAmount, userData, DelegateOnFfiResultStringBlsKeyPairCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "keys_create_preload_test_coins")]
        private static extern void KeysCreatePreloadTestCoinsNative(
            [MarshalAs(UnmanagedType.LPStr)] string preload,
            IntPtr userData,
            FfiResultStringBlsKeyPairCb oCb);

        private delegate void FfiResultStringBlsKeyPairCb(
            IntPtr userData,
            IntPtr result,
            string xorUrl,
            IntPtr safeKey);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultStringBlsKeyPairCb))]
        #endif
        private static void OnFfiResultStringBlsKeyPairCb(
            IntPtr userData,
            IntPtr result,
            string xorUrl,
            IntPtr safeKey)
            => BindingUtils.CompleteTask(
                userData,
                Marshal.PtrToStructure<FfiResult>(result),
                () => (xorUrl, Marshal.PtrToStructure<BlsKeyPair>(safeKey)));

        private static readonly FfiResultStringBlsKeyPairCb DelegateOnFfiResultStringBlsKeyPairCb = OnFfiResultStringBlsKeyPairCb;

        public Task<string> KeysBalanceFromSkAsync(IntPtr app, string sk)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            KeysBalanceFromSkNative(app, sk, userData, DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "keys_balance_from_sk")]
        private static extern void KeysBalanceFromSkNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string sk,
            IntPtr userData,
            FfiResultStringCb oCb);

        public Task<string> KeysBalanceFromUrlAsync(IntPtr app, string url, string sk)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            KeysBalanceFromUrlNative(app, url, sk, userData, DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "keys_balance_from_url")]
        private static extern void KeysBalanceFromUrlNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            [MarshalAs(UnmanagedType.LPStr)] string sk,
            IntPtr userData,
            FfiResultStringCb oCb);

        public Task<string> ValidateSkForUrlAsync(IntPtr app, string sk, string url)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            ValidateSkForUrlNative(app, sk, url, userData, DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "validate_sk_for_url")]
        private static extern void ValidateSkForUrlNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string sk,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            IntPtr userData,
            FfiResultStringCb oCb);

        public Task<ulong> KeysTransferAsync(IntPtr app, string amount, string fromSk, string toUrl, ulong txId)
        {
            var (ret, userData) = BindingUtils.PrepareTask<ulong>();
            KeysTransferNative(app, amount, fromSk, toUrl, txId, userData, DelegateOnFfiResultULongCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "keys_transfer")]
        private static extern void KeysTransferNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string amount,
            [MarshalAs(UnmanagedType.LPStr)] string from,
            [MarshalAs(UnmanagedType.LPStr)] string to,
            ulong id,
            IntPtr userData,
            FfiResultULongCb oCb);

        private delegate void FfiResultULongCb(IntPtr userData, IntPtr result, ulong handle);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultULongCb))]
        #endif
        private static void OnFfiResultULongCb(IntPtr userData, IntPtr result, ulong handle)
        {
            BindingUtils.CompleteTask(userData, Marshal.PtrToStructure<FfiResult>(result), () => handle);
        }

        private static readonly FfiResultULongCb DelegateOnFfiResultULongCb = OnFfiResultULongCb;

        #endregion Keys

        #region Wallet

        public Task<string> WalletCreateAsync(IntPtr app)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            WalletCreateNative(app, userData, DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "wallet_create")]
        private static extern void WalletCreateNative(
           IntPtr app,
           IntPtr userData,
           FfiResultStringCb oCb);

        public Task<string> WalletInsertAsync(IntPtr app, string keyUrl, string name, bool setDefault, string secretKey)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            WalletInsertNative(app, keyUrl, name, setDefault, secretKey, userData, DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "wallet_insert")]
        private static extern void WalletInsertNative(
           IntPtr app,
           [MarshalAs(UnmanagedType.LPStr)] string keyUrl,
           [MarshalAs(UnmanagedType.LPStr)] string name,
           [MarshalAs(UnmanagedType.U1)] bool setDefault,
           [MarshalAs(UnmanagedType.LPStr)] string secretKey,
           IntPtr userData,
           FfiResultStringCb oCb);

        public Task<string> WalletBalanceAsync(IntPtr app, string url)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            WalletBalanceNative(app, url, userData, DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "wallet_balance")]
        private static extern void WalletBalanceNative(
           IntPtr app,
           [MarshalAs(UnmanagedType.LPStr)] string url,
           IntPtr userData,
           FfiResultStringCb oCb);

        public Task<(WalletSpendableBalance, ulong)> WalletGetDefaultBalanceAsync(IntPtr app, string url)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(WalletSpendableBalance, ulong)>();
            WalletGetDefaultBalanceNative(app, url, userData, DelegateOnFfiResultWalletSpendableBalanceULongCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "wallet_get_default_balance")]
        private static extern void WalletGetDefaultBalanceNative(
           IntPtr app,
           [MarshalAs(UnmanagedType.LPStr)] string url,
           IntPtr userData,
           FfiResultWalletSpendableBalanceULongCb oCb);

        private delegate void FfiResultWalletSpendableBalanceULongCb(
           IntPtr userData,
           IntPtr result,
           IntPtr spendableWalletBalance,
           ulong version);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultWalletSpendableBalanceULongCb))]
        #endif
        private static void OnFfiResultWalletSpendableBalanceULongCb(
           IntPtr userData,
           IntPtr result,
           IntPtr spendableWalletBalance,
           ulong version)
           => BindingUtils.CompleteTask(
               userData,
               Marshal.PtrToStructure<FfiResult>(result),
               () => (Marshal.PtrToStructure<WalletSpendableBalance>(spendableWalletBalance), version));

        private static readonly FfiResultWalletSpendableBalanceULongCb DelegateOnFfiResultWalletSpendableBalanceULongCb = OnFfiResultWalletSpendableBalanceULongCb;

        public Task<ulong> WalletTransferAsync(IntPtr app, string from, string to, string amount, ulong id)
        {
            var (ret, userData) = BindingUtils.PrepareTask<ulong>();
            WalletTransferNative(app, from, to, amount, id, userData, DelegateOnFfiResultULongCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "wallet_transfer")]
        private static extern void WalletTransferNative(
           IntPtr app,
           [MarshalAs(UnmanagedType.LPStr)] string from,
           [MarshalAs(UnmanagedType.LPStr)] string to,
           [MarshalAs(UnmanagedType.LPStr)] string amount,
           ulong id,
           IntPtr userData,
           FfiResultULongCb oCb);

        public Task<WalletSpendableBalances> WalletGetAsync(IntPtr app, string url)
        {
            var (ret, userData) = BindingUtils.PrepareTask<WalletSpendableBalances>();
            WalletGetNative(app, url, userData, DelegateOnFfiResultWalletSpendableBalancesCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "wallet_get")]
        private static extern void WalletGetNative(
           IntPtr app,
           [MarshalAs(UnmanagedType.LPStr)] string url,
           IntPtr userData,
           FfiResultWalletSpendableBalancesCb oCb);

        private delegate void FfiResultWalletSpendableBalancesCb(
           IntPtr userData,
           IntPtr result,
           IntPtr spendableWalletBalance);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultWalletSpendableBalancesCb))]
        #endif
        private static void OnFfiResultWalletSpendableBalancesCb(
           IntPtr userData,
           IntPtr result,
           IntPtr spendableWalletBalance)
           => BindingUtils.CompleteTask(
               userData,
               Marshal.PtrToStructure<FfiResult>(result),
               () => new WalletSpendableBalances(Marshal.PtrToStructure<WalletSpendableBalancesNative>(spendableWalletBalance)));

        private static readonly FfiResultWalletSpendableBalancesCb DelegateOnFfiResultWalletSpendableBalancesCb = OnFfiResultWalletSpendableBalancesCb;

        #endregion Wallet

        #region Files

        public Task<(string, ProcessedFiles, FilesMap)> FilesContainerCreateAsync(
            IntPtr app,
            string location,
            string dest,
            bool recursive,
            bool followLinks,
            bool dryRun)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(string, ProcessedFiles, FilesMap)>();
            FilesContainerCreateNative(
                app,
                location,
                dest,
                recursive,
                followLinks,
                dryRun,
                userData,
                DelegateOnFfiResultStringProcessedFilesFilesMapCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "files_container_create")]
        private static extern void FilesContainerCreateNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string location,
            [MarshalAs(UnmanagedType.LPStr)] string dest,
            [MarshalAs(UnmanagedType.U1)] bool recursive,
            [MarshalAs(UnmanagedType.U1)] bool followLinks,
            [MarshalAs(UnmanagedType.U1)] bool dryRun,
            IntPtr userData,
            FfiResultStringProcessedFilesFilesMapCb oCb);

        public Task<(ulong, FilesMap)> FilesContainerGetAsync(IntPtr app, string url)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(ulong, FilesMap)>();
            FilesContainerGetNative(app, url, userData, DelegateOnFfiResultULongFilesMapCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "files_container_get")]
        private static extern void FilesContainerGetNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            IntPtr userData,
            FfiResultULongFilesMapCb oCb);

        private delegate void FfiResultULongFilesMapCb(IntPtr userData, IntPtr result, ulong version, IntPtr filesMap);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultULongFilesMapCb))]
        #endif
        private static void OnFfiResultULongFilesMapCb(IntPtr userData, IntPtr result, ulong version, IntPtr filesMap)
        {
            BindingUtils.CompleteTask(userData, Marshal.PtrToStructure<FfiResult>(result), () => (version, new FilesMap(Marshal.PtrToStructure<FilesMapNative>(filesMap))));
        }

        private static readonly FfiResultULongFilesMapCb DelegateOnFfiResultULongFilesMapCb = OnFfiResultULongFilesMapCb;

        public Task<(ulong, ProcessedFiles, FilesMap)> FilesContainerSyncAsync(
            IntPtr app,
            string location,
            string url,
            bool recursive,
            bool followLinks,
            bool delete,
            bool updateNrs,
            bool dryRun)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(ulong, ProcessedFiles, FilesMap)>();
            FilesContainerSyncNative(
                app,
                location,
                url,
                recursive,
                followLinks,
                delete,
                updateNrs,
                dryRun,
                userData,
                DelegateOnFfiResultULongProcessedFilesFilesMapCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "files_container_sync")]
        private static extern void FilesContainerSyncNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string location,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            [MarshalAs(UnmanagedType.U1)] bool recursive,
            [MarshalAs(UnmanagedType.U1)] bool followLinks,
            [MarshalAs(UnmanagedType.U1)] bool delete,
            [MarshalAs(UnmanagedType.U1)] bool updateNrs,
            [MarshalAs(UnmanagedType.U1)] bool dryRun,
            IntPtr userData,
            FfiResultULongProcessedFilesFilesMapCb oCb);

        public Task<(ulong, ProcessedFiles, FilesMap)> FilesContainerAddAsync(
            IntPtr app,
            string sourceFile,
            string url,
            bool force,
            bool updateNrs,
            bool followLinks,
            bool dryRun)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(ulong, ProcessedFiles, FilesMap)>();
            FilesContainerAddNative(
                app,
                sourceFile,
                url,
                force,
                updateNrs,
                followLinks,
                dryRun,
                userData,
                DelegateOnFfiResultULongProcessedFilesFilesMapCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "files_container_add")]
        private static extern void FilesContainerAddNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string sourceFile,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            [MarshalAs(UnmanagedType.U1)] bool force,
            [MarshalAs(UnmanagedType.U1)] bool updateNrs,
            [MarshalAs(UnmanagedType.U1)] bool followLinks,
            [MarshalAs(UnmanagedType.U1)] bool dryRun,
            IntPtr userData,
            FfiResultULongProcessedFilesFilesMapCb oCb);

        public Task<(ulong, ProcessedFiles, FilesMap)> FilesContainerAddFromRawAsync(
            IntPtr app,
            byte[] data,
            string url,
            bool force,
            bool updateNrs,
            bool dryRun)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(ulong, ProcessedFiles, FilesMap)>();
            FilesContainerAddFromRawNative(
                app,
                data,
                (UIntPtr)data.Length,
                url,
                force,
                updateNrs,
                dryRun,
                userData,
                DelegateOnFfiResultULongProcessedFilesFilesMapCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "files_container_add_from_raw")]
        private static extern void FilesContainerAddFromRawNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] data,
            UIntPtr dataLen,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            [MarshalAs(UnmanagedType.U1)] bool force,
            [MarshalAs(UnmanagedType.U1)] bool updateNrs,
            [MarshalAs(UnmanagedType.U1)] bool dryRun,
            IntPtr userData,
            FfiResultULongProcessedFilesFilesMapCb oCb);

        public Task<(ulong, ProcessedFiles, FilesMap)> FilesContainerRemovePathAsync(
            IntPtr app,
            string url,
            bool recursive,
            bool updateNrs,
            bool dryRun)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(ulong, ProcessedFiles, FilesMap)>();
            FilesContainerRemovePathNative(
                app,
                url,
                recursive,
                updateNrs,
                dryRun,
                userData,
                DelegateOnFfiResultULongProcessedFilesFilesMapCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "files_container_remove_path")]
        private static extern void FilesContainerRemovePathNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            [MarshalAs(UnmanagedType.U1)] bool recursive,
            [MarshalAs(UnmanagedType.U1)] bool updateNrs,
            [MarshalAs(UnmanagedType.U1)] bool dryRun,
            IntPtr userData,
            FfiResultULongProcessedFilesFilesMapCb oCb);

        private delegate void FfiResultULongProcessedFilesFilesMapCb(IntPtr userData, IntPtr result, ulong version, IntPtr processFiles, IntPtr filesMap);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultULongProcessedFilesFilesMapCb))]
        #endif
        private static void OnFfiResultULongProcessedFilesFilesMapCb(IntPtr userData, IntPtr result, ulong version, IntPtr processFiles, IntPtr filesMap)
        {
            BindingUtils.CompleteTask(userData, Marshal.PtrToStructure<FfiResult>(result), () => (version, new ProcessedFiles(Marshal.PtrToStructure<ProcessedFilesNative>(processFiles)), new FilesMap(Marshal.PtrToStructure<FilesMapNative>(filesMap))));
        }

        private static readonly FfiResultULongProcessedFilesFilesMapCb DelegateOnFfiResultULongProcessedFilesFilesMapCb = OnFfiResultULongProcessedFilesFilesMapCb;

        private delegate void FfiResultStringProcessedFilesFilesMapCb(IntPtr userData, IntPtr result, string xorurl, IntPtr processFiles, IntPtr filesMap);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultStringProcessedFilesFilesMapCb))]
        #endif
        private static void OnFfiResultStringProcessedFilesFilesMapCb(IntPtr userData, IntPtr result, string xorurl, IntPtr processFiles, IntPtr filesMap)
        {
            BindingUtils.CompleteTask(userData, Marshal.PtrToStructure<FfiResult>(result), () => (xorurl, new ProcessedFiles(Marshal.PtrToStructure<ProcessedFilesNative>(processFiles)), new FilesMap(Marshal.PtrToStructure<FilesMapNative>(filesMap))));
        }

        private static readonly FfiResultStringProcessedFilesFilesMapCb DelegateOnFfiResultStringProcessedFilesFilesMapCb = OnFfiResultStringProcessedFilesFilesMapCb;

        public Task<string> FilesPutPublicImmutableAsync(
            IntPtr app,
            byte[] data,
            string mediaType,
            bool dryRun)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            FilesPutPublicImmutableNative(
                app,
                data,
                (UIntPtr)data.Length,
                mediaType,
                dryRun,
                userData,
                DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "files_put_public_immutable")]
        private static extern void FilesPutPublicImmutableNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] data,
            UIntPtr dataLen,
            [MarshalAs(UnmanagedType.LPStr)] string mediaType,
            [MarshalAs(UnmanagedType.U1)] bool dryRun,
            IntPtr userData,
            FfiResultStringCb oCb);

        public Task<byte[]> FilesGetPublicImmutableAsync(IntPtr app, string url, ulong start, ulong end)
        {
            var (ret, userData) = BindingUtils.PrepareTask<byte[]>();
            FilesGetPublicImmutableNative(
                app,
                url,
                start,
                end,
                userData,
                DelegateOnFfiResultByteListCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "files_get_public_immutable")]
        private static extern void FilesGetPublicImmutableNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            ulong start,
            ulong end,
            IntPtr userData,
            FfiResultByteListCb oCb);

        private delegate void FfiResultByteListCb(
            IntPtr userData,
            IntPtr result,
            IntPtr imDataPtr,
            UIntPtr imDataLen);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultByteListCb))]
        #endif
        private static void OnFfiResultByteListCb(
            IntPtr userData,
            IntPtr result,
            IntPtr imDataPtr,
            UIntPtr imDataLen)
            => BindingUtils.CompleteTask(
                userData,
                Marshal.PtrToStructure<FfiResult>(result),
                () => BindingUtils.CopyToByteArray(imDataPtr, (int)imDataLen));

        private static readonly FfiResultByteListCb DelegateOnFfiResultByteListCb =
                                                                   OnFfiResultByteListCb;

        #endregion Files

        #region NRS

        public Task<SafeUrl> ParseUrlAsync(string url)
        {
            var (ret, userData) = BindingUtils.PrepareTask<SafeUrl>();
            ParseUrlNative(url, userData, DelegateOnFfiResultSafeUrlCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "parse_url")]
        private static extern void ParseUrlNative([MarshalAs(UnmanagedType.LPStr)] string url, IntPtr userData, FfiResultSafeUrlCb oCb);

        public Task<(string, ProcessedEntries, string)> CreateNrsMapContainerAsync(
            IntPtr app,
            string name,
            string link,
            bool directLink,
            bool dryRun,
            bool setDefault)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(string, ProcessedEntries, string)>();
            CreateNrsMapContainerNative(
                app,
                name,
                link,
                directLink,
                dryRun,
                setDefault,
                userData,
                DelegateOnFfiResultStringProcessedEntriesStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "nrs_map_container_create")]
        private static extern void CreateNrsMapContainerNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.LPStr)] string link,
            [MarshalAs(UnmanagedType.U1)] bool directLink,
            [MarshalAs(UnmanagedType.U1)] bool dryRun,
            [MarshalAs(UnmanagedType.U1)] bool setDefault,
            IntPtr userData,
            FfiResultStringProcessedEntriesStringCb oCb);

        private delegate void FfiResultStringProcessedEntriesStringCb(
            IntPtr userData,
            IntPtr result,
            string nrsMap,
            IntPtr processedEntries,
            string xorurl);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultStringProcessedEntriesStringCb))]
        #endif
        private static void OnFfiResultStringProcessedEntriesStringCb(
            IntPtr userData,
            IntPtr result,
            string nrsMap,
            IntPtr processedEntries,
            string xorurl)
        {
            BindingUtils.CompleteTask(
                userData,
                Marshal.PtrToStructure<FfiResult>(result),
                () => (
                nrsMap,
                new ProcessedEntries(Marshal.PtrToStructure<ProcessedEntriesNative>(processedEntries)), xorurl));
        }

        private static readonly FfiResultStringProcessedEntriesStringCb DelegateOnFfiResultStringProcessedEntriesStringCb =
            OnFfiResultStringProcessedEntriesStringCb;

        public Task<(string, string, ulong)> AddToNrsMapContainerAsync(
            IntPtr app,
            string name,
            string link,
            bool setDefault,
            bool directLink,
            bool dryRun)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(string, string, ulong)>();
            AddToNrsMapContainerNative(
                app,
                name,
                link,
                setDefault,
                directLink,
                dryRun,
                userData,
                DelegateOnFfiResultStringStringULongCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "nrs_map_container_add")]
        private static extern void AddToNrsMapContainerNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.LPStr)] string link,
            [MarshalAs(UnmanagedType.U1)] bool setDefault,
            [MarshalAs(UnmanagedType.U1)] bool directLink,
            [MarshalAs(UnmanagedType.U1)] bool dryRun,
            IntPtr userData,
            FfiResultStringStringULongCb oCb);

        private delegate void FfiResultStringStringULongCb(
            IntPtr userData,
            IntPtr result,
            string nrsMap,
            string xorurl,
            ulong version);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultStringStringULongCb))]
        #endif
        private static void OnFfiResultStringStringULongCb(
            IntPtr userData,
            IntPtr result,
            string nrsMap,
            string xorurl,
            ulong version)
        {
            BindingUtils.CompleteTask(
                userData,
                Marshal.PtrToStructure<FfiResult>(result),
                () => (nrsMap, xorurl, version));
        }

        private static readonly FfiResultStringStringULongCb DelegateOnFfiResultStringStringULongCb =
            OnFfiResultStringStringULongCb;

        public Task<(string, string, ulong)> RemoveFromNrsMapContainerAsync(
            IntPtr app,
            string name,
            bool dryRun)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(string, string, ulong)>();
            RemoveFromNrsMapContainerNative(
                app,
                name,
                dryRun,
                userData,
                DelegateOnFfiResultStringStringULongCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "nrs_map_container_remove")]
        private static extern void RemoveFromNrsMapContainerNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.U1)] bool dryRun,
            IntPtr userData,
            FfiResultStringStringULongCb oCb);

        public Task<(string, ulong)> GetNrsMapContainerAsync(
            IntPtr app,
            string url)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(string, ulong)>();
            GetNrsMapContainerNative(
                app,
                url,
                userData,
                DelegateOnFfiResultStringULongCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "nrs_map_container_get")]
        private static extern void GetNrsMapContainerNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            IntPtr userData,
            FfiResultStringULongCb oCb);

        private delegate void FfiResultStringULongCb(IntPtr userData, IntPtr result, string nrsMap, ulong version);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultStringULongCb))]
        #endif
        private static void OnFfiResultStringULongCb(IntPtr userData, IntPtr result, string nrsMap, ulong version)
        {
            BindingUtils.CompleteTask(userData, Marshal.PtrToStructure<FfiResult>(result), () => (nrsMap, version));
        }

        private static readonly FfiResultStringULongCb DelegateOnFfiResultStringULongCb = OnFfiResultStringULongCb;

        #endregion NRS

        #region Sequence Data

        public Task<string> CreateSequenceAsync(
            IntPtr app,
            byte[] data,
            byte[] name,
            ulong typeTag,
            bool isPrivate)
        {
            var (ret, userData) = BindingUtils.PrepareTask<string>();
            CreateSequenceNative(app, data?.ToArray(), (UIntPtr)(data?.Length ?? 0), name, typeTag, isPrivate, userData, DelegateOnFfiResultStringCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "create_sequence")]
        private static extern void CreateSequenceNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] data,
            UIntPtr dataLen,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = (int)AppConstants.XorNameLen)] byte[] name,
            ulong typeTag,
            [MarshalAs(UnmanagedType.U1)] bool isPrivate,
            IntPtr userData,
            FfiResultStringCb oCb);

        public Task<(ulong, byte[])> GetSequenceAsync(IntPtr app, string url)
        {
            var (ret, userData) = BindingUtils.PrepareTask<(ulong, byte[])>();
            GetSequenceNative(app, url, userData, DelegateOnFfiResultULongByteArrayCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "get_sequence")]
        private static extern void GetSequenceNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            IntPtr userData,
            FfiResultULongByteArrayCb oCb);

        public Task AppendSequenceAsync(IntPtr app, string url, byte[] data)
        {
            var (ret, userData) = BindingUtils.PrepareTask();
            AppendSequenceNative(
                app,
                url,
                data?.ToArray(),
                (UIntPtr)(data?.Length ?? 0),
                userData,
                DelegateOnFfiResultCb);
            return ret;
        }

        [DllImport(DllName, EntryPoint = "append_sequence")]
        private static extern void AppendSequenceNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] data,
            UIntPtr dataLen,
            IntPtr userData,
            FfiResultCb oCb);

        private delegate void FfiResultULongByteArrayCb(
            IntPtr userData,
            IntPtr result,
            ulong version,
            IntPtr dataPtr,
            UIntPtr dataLen);

        #if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultULongByteArrayCb))]
        #endif
        private static void OnFfiResultULongByteArrayCb(
            IntPtr userData,
            IntPtr result,
            ulong version,
            IntPtr dataPtr,
            UIntPtr dataLen)
        {
            BindingUtils.CompleteTask(
                userData,
                Marshal.PtrToStructure<FfiResult>(result),
                () => (version, BindingUtils.CopyToByteArray(dataPtr, (int)dataLen)));
        }

        private static readonly FfiResultULongByteArrayCb DelegateOnFfiResultULongByteArrayCb = OnFfiResultULongByteArrayCb;

        #endregion
    }
}
