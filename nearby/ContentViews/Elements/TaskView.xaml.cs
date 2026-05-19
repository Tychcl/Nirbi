using System.Windows.Input;
using nearby.Models;

namespace nearby.ContentViews.Elements;

public partial class TaskView : ContentView
{
    public static readonly BindableProperty TaskProperty =
        BindableProperty.Create(nameof(Task), typeof(TaskItem), typeof(TaskView),
            default(TaskItem), BindingMode.OneWay);

    public static readonly BindableProperty GoToDetailCommandProperty =
        BindableProperty.Create(nameof(GoToDetailCommand), typeof(ICommand), typeof(TaskView),
            default(ICommand), BindingMode.OneWay);

    public TaskItem Task
    {
        get => (TaskItem)GetValue(TaskProperty);
        set => SetValue(TaskProperty, value);
    }

    public ICommand GoToDetailCommand
    {
        get => (ICommand)GetValue(GoToDetailCommandProperty);
        set => SetValue(GoToDetailCommandProperty, value);
    }

    public TaskView()
    {
        InitializeComponent();
    }
}