using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nearby.Interfaces;
using nearby.Models;
using nearby.Views.Main;

namespace nearby.ViewModels;

public partial class ConfirmationsViewModel : BaseViewModel
{
    private readonly IConfirmationService _confirmationService;
    private readonly IUserService _userService;

    [ObservableProperty]
    private bool _isReceivedSelected = true;

    [ObservableProperty]
    private ObservableCollection<Confirmation> _receivedConfirmations = new();

    [ObservableProperty]
    private ObservableCollection<Confirmation> _sentConfirmations = new();

    [ObservableProperty]
    private int _receivedCount;

    [ObservableProperty]
    private int _sentCount;

    public ConfirmationsViewModel(IConfirmationService confirmationService, IUserService userService)
    {
        _confirmationService = confirmationService;
        _userService = userService;
        PageTitle = "Отклики";
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        try
        {
            // Загружаем оба списка параллельно
            var receivedTask = _confirmationService.GetConfirmationsByReviewerAsync();
            var sentTask = _confirmationService.GetConfirmationsByInitiatorAsync();
            await Task.WhenAll(receivedTask, sentTask);

            var received = receivedTask.Result;
            var sent = sentTask.Result;

            // Загружаем связанных пользователей (инициаторов для полученных, рецензентов для отправленных)
            await LoadRelatedUsersAsync(received, isReceived: true);
            await LoadRelatedUsersAsync(sent, isReceived: false);

            ReceivedConfirmations = new ObservableCollection<Confirmation>(received);
            SentConfirmations = new ObservableCollection<Confirmation>(sent);
            ReceivedCount = received.Count;
            SentCount = sent.Count;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadRelatedUsersAsync(List<Confirmation> confirmations, bool isReceived)
    {
        var ids = isReceived
            ? confirmations.Select(c => c.InitiatorId).Distinct().ToList()
            : confirmations.Select(c => c.ReviewerId).Distinct().ToList();

        if (ids.Count == 0) return;

        // Загружаем пользователей батчево (по одному – упрощённо, можно оптимизировать)
        var userTasks = ids.Select(id => _userService.LoadUserByIdAsync(id)).ToList();
        var users = await Task.WhenAll(userTasks);
        var userDict = users.Where(u => u != null).ToDictionary(u => u.Id);

        foreach (var conf in confirmations)
        {
            var key = isReceived ? conf.InitiatorId : conf.ReviewerId;
            conf.RelatedUser = userDict.TryGetValue(key, out var user) ? user : null;
        }
    }

    [RelayCommand]
    private void SwitchTab(string tab)
    {
        IsReceivedSelected = tab == "received";
    }

    [RelayCommand]
    private async Task AcceptAsync(Guid confirmationId)
    {
        try
        {
            await _confirmationService.RespondToConfirmationAsync(confirmationId, new RespondToConfirmationRequest
            {
                IsAccepted = true,
                RejectionReason = null
            });
            await LoadDataAsync();
        }
        catch (Exception ex) { await ShowErrorAsync(ex.Message); }
    }

    [RelayCommand]
    private async Task RejectAsync(Guid confirmationId)
    {
        try
        {
            await _confirmationService.RespondToConfirmationAsync(confirmationId, new RespondToConfirmationRequest
            {
                IsAccepted = false,
                RejectionReason = "Отклонено"
            });
            await LoadDataAsync();
        }
        catch (Exception ex) { await ShowErrorAsync(ex.Message); }
    }

    [RelayCommand]
    private async Task RevokeAsync(Guid confirmationId)
    {
        try
        {
            await _confirmationService.RevokeConfirmationAsync(confirmationId, (Guid)_userService.CurrentUserId!);
            await LoadDataAsync();
        }
        catch (Exception ex) { await ShowErrorAsync(ex.Message); }
    }

    [RelayCommand]
    private async Task GoToTaskAsync(Guid taskId)
    {
        await Shell.Current.GoToAsync(nameof(TaskDetailPage), new Dictionary<string, object?> { { "taskId", taskId } });
    }

    [RelayCommand]
    private async Task GoToProfileAsync(Guid userId)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage), new Dictionary<string, object?> { { "id", userId } });
    }
}