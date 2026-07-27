using System.Runtime.InteropServices;
using System.Security;

namespace PaymentApi.Security;

public sealed class SecureStr : IDisposable
{
    private readonly SecureString _inner;

    public SecureStr(string raw)
    {
        _inner = new SecureString();
        foreach (var ch in raw)
        {
            _inner.AppendChar(ch);
        }
        _inner.MakeReadOnly();
    }

    public int Length => _inner.Length;

    public T Use<T>(Func<string, T> fn)
    {
        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.SecureStringToGlobalAllocUnicode(_inner);
            var plain = Marshal.PtrToStringUni(ptr) ?? string.Empty;
            return fn(plain);
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }
    }

    public override string ToString() => "***";

    public void Dispose() => _inner.Dispose();
}
