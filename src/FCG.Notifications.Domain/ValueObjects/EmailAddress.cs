using System.Text.RegularExpressions;
using FCG.Notifications.Domain.Exceptions;

namespace FCG.Notifications.Domain.ValueObjects;

public sealed class EmailAddress
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("E-mail é obrigatório.");

        var normalizedEmail = value.Trim().ToLower();

        if (!EmailRegex.IsMatch(normalizedEmail))
            throw new DomainException("Formato de e-mail inválido.");

        Value = normalizedEmail;
    }

    public override string ToString()
    {
        return Value;
    }
}