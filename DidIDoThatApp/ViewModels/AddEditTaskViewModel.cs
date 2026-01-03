using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using DidIDoThatApp.Services.Interfaces;

namespace DidIDoThatApp.ViewModels;

/// <summary>
/// ViewModel for adding or editing a task.
/// </summary>
[QueryProperty(nameof(TaskIdString), "taskId")]
public partial class AddEditTaskViewModel : BaseViewModel
{
    private readonly ITaskService _taskService;
    private readonly ICategoryService _categoryService;
    private readonly IDataPrefetchService? _prefetchService;

    public AddEditTaskViewModel(ITaskService taskService, ICategoryService categoryService)
    {
        _taskService = taskService;
        _categoryService = categoryService;
        _prefetchService = App.DataPrefetchService;
    }

    /// <summary>
    /// String version of TaskId for Shell navigation query property.
    /// </summary>
    public string? TaskIdString
    {
        get => TaskId?.ToString();
        set
        {
            if (Guid.TryParse(value, out var guid))
            {
                TaskId = guid;
            }
            else
            {
                TaskId = null;
            }
        }
    }

    [ObservableProperty]
    private Guid? _taskId;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private int _frequencyValue = 1;

    [ObservableProperty]
    private FrequencyUnit _frequencyUnit = FrequencyUnit.Months;

    [ObservableProperty]
    private bool _isReminderEnabled = true;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private ObservableCollection<FrequencyUnit> _frequencyUnits = new(
        Enum.GetValues<FrequencyUnit>());

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _canSave;

    partial void OnTaskIdChanged(Guid? value)
    {
        if (value.HasValue && value != Guid.Empty)
        {
            IsEditMode = true;
            Title = "Edit Task";
            LoadTaskCommand.Execute(null);
        }
        else
        {
            IsEditMode = false;
            Title = "New Task";
        }
    }

    partial void OnNameChanged(string value)
    {
        ValidateCanSave();
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        ValidateCanSave();
    }

    private void ValidateCanSave()
    {
        CanSave = !string.IsNullOrWhiteSpace(Name) && SelectedCategory != null;
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            Categories = new ObservableCollection<Category>(categories);

            if (!IsEditMode && Categories.Count > 0)
            {
                SelectedCategory = Categories[0];
            }
        });
    }

    [RelayCommand]
    private async Task LoadTaskAsync()
    {
        if (!TaskId.HasValue || TaskId == Guid.Empty) return;

        await ExecuteAsync(async () =>
        {
            var task = await _taskService.GetTaskByIdAsync(TaskId.Value);
            if (task == null) return;

            Name = task.Name;
            Description = task.Description;
            FrequencyValue = task.FrequencyValue;
            FrequencyUnit = task.FrequencyUnit;
            IsReminderEnabled = task.IsReminderEnabled;

            // Load categories directly (not via command to avoid IsBusy guard)
            if (Categories.Count == 0)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                Categories = new ObservableCollection<Category>(categories);
            }

            SelectedCategory = Categories.FirstOrDefault(c => c.Id == task.CategoryId);
        });
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (SelectedCategory == null || string.IsNullOrWhiteSpace(Name))
            return;

        await ExecuteAsync(async () =>
        {
            if (IsEditMode && TaskId.HasValue)
            {
                await _taskService.UpdateTaskAsync(
                    TaskId.Value,
                    Name,
                    Description,
                    FrequencyValue,
                    FrequencyUnit,
                    IsReminderEnabled);
            }
            else
            {
                await _taskService.CreateTaskAsync(
                    SelectedCategory.Id,
                    Name,
                    Description,
                    FrequencyValue,
                    FrequencyUnit,
                    IsReminderEnabled);
            }

            _prefetchService?.InvalidateCache();
        });

        // Navigate back after ExecuteAsync completes to ensure save is finished
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
