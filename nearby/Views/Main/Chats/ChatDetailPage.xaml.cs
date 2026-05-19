using CommunityToolkit.Maui.Extensions;
using nearby.Classes;
using nearby.ContentViews.Elements;
using nearby.ViewModels;

namespace nearby.Views.Main;

public partial class ChatDetailPage : ContentPage
{
    public ChatDetailPage(ChatDetailViewModel viewModel)
    {
        InitializeComponent();
        viewModel.CV = ChatCollectionView;
        BindingContext = viewModel;
    }

    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();
    //    if (BindingContext is ChatDetailViewModel vm)
    //    {
    //        if (vm.MessageNotOwnerPopup is PopupMenu MNOP && vm.MessageOwnerPopup is PopupMenu MOP)
    //        {
    //            MNOP.IsVisible = false;
    //            PopupManager.navigation.ShowPopup(MNOP);
    //            await PopupManager.navigation.ClosePopupAsync(MNOP);
    //            MNOP.IsVisible = true;
    //
    //            MOP.IsVisible = false;
    //            PopupManager.navigation.ShowPopup(MOP);
    //            await PopupManager.navigation.ClosePopupAsync(MOP);
    //            MOP.IsVisible = true;
    //            //await Task.Delay(300);
    //            //ChatCollectionView.ScrollTo(vm.Messages.Last(), position: ScrollToPosition.MakeVisible, animate: false);
    //        }
    //    }
    //}
}