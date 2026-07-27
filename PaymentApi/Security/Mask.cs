namespace PaymentApi.Security;

public static class Mask
{
    public static string Pan(string pan)
    {
        if (string.IsNullOrEmpty(pan) || pan.Length < 10)
        {
            return "************";
        }
        var head = pan[..6];
        var tail = pan[^4..];
        var mid = new string('*', pan.Length - 10);
        return head + mid + tail;
    }

    public static string Email(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1)
        {
            return "***" + (at >= 0 ? email[at..] : string.Empty);
        }
        return email[0] + new string('*', at - 1) + email[at..];
    }
}
