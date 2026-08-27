using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Desktop.ViewModels;

/// <summary>
/// The window's view-model. It depends only on IPersonnelService — the SAME core abstraction the
/// console uses — so this UI shares all the business logic, validation, logging, and persistence.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IPersonnelService _service;

    public ObservableCollection<PersonalDto> People { get; } = [];
    public EmploymentStatus[] Statuses { get; } = Enum.GetValues<EmploymentStatus>();

    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _surname;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private EmploymentStatus _selectedStatus = EmploymentStatus.Active;
    [ObservableProperty] private PersonalDto? _selectedPerson;
    [ObservableProperty] private string _statusMessage = "";

    public MainViewModel(IPersonnelService service)
    {
        _service = service;
        LoadCommand.Execute(null); // populate the list on open
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        People.Clear();
        foreach (var person in await _service.GetAllAsync())
            People.Add(person);
        StatusMessage = $"{People.Count} record(s).";
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var result = await _service.CreateAsync(
            new CreatePersonalRequest(Name, Surname, Address, Phone, SelectedStatus));

        // Result.Match — the same functional result type the console renders — drives the status line.
        StatusMessage = result.Match(
            onSuccess: dto => $"Added {dto.Name} {dto.Surname}.",
            onFailure: error => $"Error: {error}");

        if (result.IsSuccess)
        {
            Name = Surname = Address = Phone = null;
            SelectedStatus = EmploymentStatus.Active;
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedPerson is null)
        {
            StatusMessage = "Select a person to delete.";
            return;
        }

        var result = await _service.DeleteAsync(SelectedPerson.Id);
        StatusMessage = result.Match(onSuccess: _ => "Deleted.", onFailure: error => $"Error: {error}");
        await LoadAsync();
    }
}
