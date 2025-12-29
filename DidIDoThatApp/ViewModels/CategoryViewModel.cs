using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DidIDoThatApp.Models;
using DidIDoThatApp.Services.Interfaces;

namespace DidIDoThatApp.ViewModels;

/// <summary>
/// ViewModel for a category item in the list.
/// </summary>
public partial class CategoryItemViewModel : ObservableObject
{
    private readonly Category _category;

    public CategoryItemViewModel(Category category, int taskCount)
    {
        _category = category;
        TaskCount = taskCount;
    }

    public Guid Id => _category.Id;
    public string Name => _category.Name;
    public string? Icon => _category.Icon;
    public bool IsDefault => _category.IsDefault;
    public string DisplayName => $"{Icon} {Name}";
    public Category Category => _category;

    [ObservableProperty]
    private int _taskCount;

    public string TaskCountText => TaskCount == 1 ? "1 task" : $"{TaskCount} tasks";
}

/// <summary>
/// ViewModel for the Category Management page.
/// </summary>
public partial class CategoryViewModel : BaseViewModel
{
    private readonly ICategoryService _categoryService;
    private readonly ITaskService _taskService;

    public CategoryViewModel(ICategoryService categoryService, ITaskService taskService)
    {
        _categoryService = categoryService;
        _taskService = taskService;
        Title = "Categories";
    }

    [ObservableProperty]
    private ObservableCollection<CategoryItemViewModel> _categories = [];

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private string _newCategoryIcon = string.Empty;

    [ObservableProperty]
    private bool _isAddingCategory;

    [ObservableProperty]
    private CategoryItemViewModel? _editingCategory;

    [ObservableProperty]
    private string _editCategoryName = string.Empty;

    [ObservableProperty]
    private string _editCategoryIcon = string.Empty;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var allTasks = await _taskService.GetAllTasksAsync();

            var categoryVms = categories.Select(c => new CategoryItemViewModel(
                c,
                allTasks.Count(t => t.CategoryId == c.Id)));

            Categories = new ObservableCollection<CategoryItemViewModel>(categoryVms);
        }, allowReentry: true);
    }

    [RelayCommand]
    private void StartAddCategory()
    {
        IsAddingCategory = true;
        NewCategoryName = string.Empty;
        NewCategoryIcon = string.Empty;
    }

    [RelayCommand]
    private void CancelAddCategory()
    {
        IsAddingCategory = false;
        NewCategoryName = string.Empty;
        NewCategoryIcon = string.Empty;
    }

    [RelayCommand]
    private async Task SaveNewCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
            return;

        bool created = false;
        await ExecuteAsync(async () =>
        {
            var exists = await _categoryService.CategoryExistsAsync(NewCategoryName);
            if (exists)
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    "A category with that name already exists.",
                    "OK");
                return;
            }

            await _categoryService.CreateCategoryAsync(
                NewCategoryName,
                string.IsNullOrWhiteSpace(NewCategoryIcon) ? null : NewCategoryIcon);

            IsAddingCategory = false;
            NewCategoryName = string.Empty;
            NewCategoryIcon = string.Empty;
            created = true;
        });

        // Reload data after ExecuteAsync completes to avoid nested IsBusy blocking
        if (created)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private void StartEditCategory(CategoryItemViewModel? categoryVm)
    {
        if (categoryVm == null) return;

        EditingCategory = categoryVm;
        EditCategoryName = categoryVm.Name;
        EditCategoryIcon = categoryVm.Icon ?? string.Empty;
    }

    [RelayCommand]
    private void CancelEditCategory()
    {
        EditingCategory = null;
        EditCategoryName = string.Empty;
        EditCategoryIcon = string.Empty;
    }

    [RelayCommand]
    private async Task SaveEditCategoryAsync()
    {
        if (EditingCategory == null || string.IsNullOrWhiteSpace(EditCategoryName))
            return;

        bool saved = false;
        await ExecuteAsync(async () =>
        {
            // Check if name changed and if new name already exists
            if (EditCategoryName.Trim().ToLower() != EditingCategory.Name.ToLower())
            {
                var exists = await _categoryService.CategoryExistsAsync(EditCategoryName);
                if (exists)
                {
                    await Shell.Current.DisplayAlert(
                        "Error",
                        "A category with that name already exists.",
                        "OK");
                    return;
                }
            }

            await _categoryService.UpdateCategoryAsync(
                EditingCategory.Id,
                EditCategoryName,
                string.IsNullOrWhiteSpace(EditCategoryIcon) ? null : EditCategoryIcon);

            EditingCategory = null;
            EditCategoryName = string.Empty;
            EditCategoryIcon = string.Empty;
            saved = true;
        });

        // Reload data after ExecuteAsync completes to avoid nested IsBusy blocking
        if (saved)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(CategoryItemViewModel? categoryVm)
    {
        if (categoryVm == null) return;

        if (categoryVm.IsDefault)
        {
            await Shell.Current.DisplayAlert(
                "Cannot Delete",
                "Default categories cannot be deleted.",
                "OK");
            return;
        }

        if (categoryVm.TaskCount > 0)
        {
            await Shell.Current.DisplayAlert(
                "Cannot Delete",
                $"This category has {categoryVm.TaskCount} task(s). Please delete or move them first.",
                "OK");
            return;
        }

        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Category",
            $"Are you sure you want to delete '{categoryVm.Name}'?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            await ExecuteAsync(async () =>
            {
                await _categoryService.DeleteCategoryAsync(categoryVm.Id);
            });

            // Reload data after ExecuteAsync completes to avoid nested IsBusy blocking
            await LoadDataAsync();
        }
    }
}
