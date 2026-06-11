namespace ClaudeDesk.Core;

/// <summary>
/// Windows Credential Manager backed key store (CredWrite/CredRead/CredDelete).
/// </summary>
public sealed class CredentialManagerApiKeyStore : IApiKeyStore
{
    private const uint CredTypeGeneric = 1;        // CRED_TYPE_GENERIC
    private const uint CredPersistLocalMachine = 2; // survives reboot, per-user

    public void Save(string key, string secret)
    {
        byte[] blob = Encoding.Unicode.GetBytes(secret);
        var credential = new NativeCredential
        {
            Type = CredTypeGeneric,
            TargetName = Marshal.StringToCoTaskMemUni(key),
            CredentialBlobSize = (uint)blob.Length,
            CredentialBlob = Marshal.AllocCoTaskMem(blob.Length),
            Persist = CredPersistLocalMachine,
        };
        Marshal.Copy(blob, 0, credential.CredentialBlob, blob.Length);
        try
        {
            if (!CredWrite(ref credential, 0))
            {
                throw new InvalidOperationException($"CredWrite failed (Win32 error {Marshal.GetLastWin32Error()}).");
            }
        }
        finally
        {
            Marshal.FreeCoTaskMem(credential.TargetName);
            Marshal.FreeCoTaskMem(credential.CredentialBlob);
        }
    }

    public string? TryGet(string key)
    {
        if (!CredRead(key, CredTypeGeneric, 0, out nint pCredential))
        {
            return null;
        }

        try
        {
            var credential = Marshal.PtrToStructure<NativeCredential>(pCredential);
            return credential.CredentialBlobSize == 0
                ? string.Empty
                : Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);
        }
        finally
        {
            CredFree(pCredential);
        }
    }

    public void Delete(string key) => CredDelete(key, CredTypeGeneric, 0);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeCredential
    {
        public uint Flags;
        public uint Type;
        public nint TargetName;
        public nint Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public nint CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public nint Attributes;
        public nint TargetAlias;
        public nint UserName;
    }

    [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CredWriteW")]
    private static extern bool CredWrite(ref NativeCredential credential, uint flags);

    [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CredReadW")]
    private static extern bool CredRead(string target, uint type, uint flags, out nint credential);

    [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CredDeleteW")]
    private static extern bool CredDelete(string target, uint type, uint flags);

    [DllImport("advapi32", EntryPoint = "CredFree")]
    private static extern void CredFree(nint buffer);
}
