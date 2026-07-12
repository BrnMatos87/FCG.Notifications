using FCG.Notifications.Domain.Exceptions;
using FCG.Notifications.Domain.ValueObjects;

namespace FCG.Notifications.Tests.Domain.ValueObjects;

public class EmailAddressTests
{
    [Fact(DisplayName = "Validando criação de e-mail com sucesso")]
    [Trait("Categoria", "Domain - Email")]
    public void EmailAddress_Create_Success()
    {
        var email = new EmailAddress(" BRUNO@EMAIL.COM ");

        Assert.Equal("bruno@email.com", email.Value);
        Assert.Equal("bruno@email.com", email.ToString());
    }

    [Fact(DisplayName = "Validando e-mail vazio")]
    [Trait("Categoria", "Domain - Email")]
    public void EmailAddress_Empty()
    {
        var result = Assert.Throws<DomainException>(() =>
            new EmailAddress(string.Empty));

        Assert.Equal("E-mail é obrigatório.", result.Message);
    }

    [Fact(DisplayName = "Validando e-mail inválido")]
    [Trait("Categoria", "Domain - Email")]
    public void EmailAddress_Invalid()
    {
        var result = Assert.Throws<DomainException>(() =>
            new EmailAddress("email-invalido"));

        Assert.Equal("Formato de e-mail inválido.", result.Message);
    }
}
