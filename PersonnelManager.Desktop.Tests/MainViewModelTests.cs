using PersonnelManager.Application.Abstractions;
using PersonnelManager.Application.Personnel;
using PersonnelManager.Desktop.ViewModels;
using PersonnelManager.Infrastructure;

namespace PersonnelManager.Desktop.Tests;

/// <summary>
/// The view-model is testable with NO Avalonia UI running, because it depends only on the core
/// IPersonnelService abstraction. That's the payoff of the presentation/core separation.
/// </summary>
public class MainViewModelTests
{
    private static IPersonnelService NewService() =>
        new PersonnelService(new InMemoryPersonalRepository(), new PersonalValidator());

    [Fact]
    public async Task Add_ValidPerson_AppearsInTheList()
    {
        var vm = new MainViewModel(NewService()) { Name = "Grace", Surname = "Hopper" };

        await vm.AddCommand.ExecuteAsync(null);

        Assert.Contains(vm.People, p => p.Surname == "Hopper");
        Assert.Null(vm.Name); // form cleared after a successful add
    }

    [Fact]
    public async Task Add_Invalid_ShowsError_AndAddsNothing()
    {
        var vm = new MainViewModel(NewService()); // no name or surname entered

        await vm.AddCommand.ExecuteAsync(null);

        Assert.StartsWith("Error", vm.StatusMessage);
        Assert.Empty(vm.People);
    }

    [Fact]
    public async Task Delete_RemovesTheSelectedPerson()
    {
        var vm = new MainViewModel(NewService()) { Name = "Ada", Surname = "Lovelace" };
        await vm.AddCommand.ExecuteAsync(null);
        vm.SelectedPerson = vm.People.First();

        await vm.DeleteCommand.ExecuteAsync(null);

        Assert.Empty(vm.People);
    }
}
