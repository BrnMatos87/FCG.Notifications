using FCG.Notifications.Application.Commands.Notifications;
using FCG.Notifications.Domain.ValueObjects;
using FCG.Notifications.Infrastructure.Services;
using NetArchTest.Rules;

namespace FCG.Notifications.Tests.Architecture;

public class LayerDependencyTests
{
    private const string DomainNamespace = "FCG.Notifications.Domain";
    private const string ApplicationNamespace = "FCG.Notifications.Application";
    private const string InfrastructureNamespace = "FCG.Notifications.Infrastructure";
    private const string WorkerNamespace = "FCG.Notifications.Worker";

    [Fact(DisplayName = "Domain não deve depender de Application")]
    [Trait("Categoria", "Architecture")]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var result = Types
            .InAssembly(typeof(EmailAddress).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Domain não deve depender de Infrastructure")]
    [Trait("Categoria", "Architecture")]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(EmailAddress).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Domain não deve depender de Worker")]
    [Trait("Categoria", "Architecture")]
    public void Domain_Should_Not_Depend_On_Worker()
    {
        var result = Types
            .InAssembly(typeof(EmailAddress).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WorkerNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Application não deve depender de Infrastructure")]
    [Trait("Categoria", "Architecture")]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(SendWelcomeEmailCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Application não deve depender de Worker")]
    [Trait("Categoria", "Architecture")]
    public void Application_Should_Not_Depend_On_Worker()
    {
        var result = Types
            .InAssembly(typeof(SendWelcomeEmailCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WorkerNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Infrastructure não deve depender de Worker")]
    [Trait("Categoria", "Architecture")]
    public void Infrastructure_Should_Not_Depend_On_Worker()
    {
        var result = Types
            .InAssembly(typeof(ConsoleEmailSender).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WorkerNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Worker não deve ser referenciado pelas camadas internas")]
    [Trait("Categoria", "Architecture")]
    public void Worker_Should_Be_Outer_Layer()
    {
        var domain = Types
            .InAssembly(typeof(EmailAddress).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WorkerNamespace)
            .GetResult();

        var application = Types
            .InAssembly(typeof(SendWelcomeEmailCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WorkerNamespace)
            .GetResult();

        var infrastructure = Types
            .InAssembly(typeof(ConsoleEmailSender).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WorkerNamespace)
            .GetResult();

        Assert.True(domain.IsSuccessful);
        Assert.True(application.IsSuccessful);
        Assert.True(infrastructure.IsSuccessful);
    }
}