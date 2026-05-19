using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using nearby.Classes;
using nearby.Classes.Interface.Converters;
using nearby.Models;
using nearby.Services;

namespace nearby.ContentViews.Elements;

public class MessageView : ContentView
{
    public static readonly BindableProperty CurrentUserIdProperty =
    BindableProperty.Create(nameof(CurrentUserId), typeof(int), typeof(MessageView), -1, propertyChanged: OnMessageChanged);
    public int CurrentUserId
    {
        get => (int)GetValue(CurrentUserIdProperty);
        set => SetValue(CurrentUserIdProperty, value);
    }

    public static readonly BindableProperty IsOwnMessageProperty =
    BindableProperty.Create(nameof(IsOwnMessage), typeof(bool), typeof(MessageView), false);
    public bool IsOwnMessage
    {
        get => (bool)GetValue(IsOwnMessageProperty);
        set => SetValue(IsOwnMessageProperty, value);
    }

    public static readonly BindableProperty SenderIsVisibleProperty =
    BindableProperty.Create(nameof(SenderIsVisible), typeof(bool), typeof(MessageView), false);
    public bool SenderIsVisible
    {
        get => (bool)GetValue(SenderIsVisibleProperty);
        set => SetValue(SenderIsVisibleProperty, value);
    }

    public static readonly BindableProperty HasReplyProperty =
    BindableProperty.Create(nameof(HasReply), typeof(bool), typeof(MessageView), false);
    public bool HasReply
    {
        get => (bool)GetValue(HasReplyProperty);
        set => SetValue(HasReplyProperty, value);
    }

    public static readonly BindableProperty ReplyCommandProperty =
        BindableProperty.Create(nameof(ReplyCommand), typeof(IAsyncRelayCommand), typeof(MessageView), null, BindingMode.OneWay, propertyChanged: OnReplyCommandChanged);
    public IAsyncRelayCommand ReplyCommand
    {
        get => (IAsyncRelayCommand)GetValue(ReplyCommandProperty);
        set => SetValue(ReplyCommandProperty, value);
    }
    private static void OnReplyCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MessageView)bindable;
        if (view.Message.Reply is null) return;
        view._replyBoxBorder.GestureRecognizers.Clear();
        var TGR = new TapGestureRecognizer();
        TGR.Tapped += async (s, e) =>
        {
            await view.ReplyCommand.ExecuteAsync(view.Message.Reply.Id);
        };
        view._replyBoxBorder.GestureRecognizers.Add(TGR);
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(IAsyncRelayCommand), typeof(MessageView), null, BindingMode.OneWay, propertyChanged: OnCommandChanged);
    public IAsyncRelayCommand Command
    {
        get => (IAsyncRelayCommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MessageView)bindable;
        view.Content.GestureRecognizers.Clear();
        var TGR = new TapGestureRecognizer();
        TGR.Tapped += async (s, e) =>
        {
            view.point = e.GetPosition(null);
            await view.Command.ExecuteAsync(view);
        };
        view.Content.GestureRecognizers.Add(TGR);
    }

    public static readonly BindableProperty MessageProperty = 
		BindableProperty.Create(nameof(Message), typeof(Message), typeof(MessageView), null, propertyChanged: OnMessageChanged);
    public Message? Message
    {
        get => (Message?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
    private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MessageView)bindable;
        view.IsOwnMessage = view.Message?.SenderId == view.CurrentUserId;
        view.SenderIsVisible = view.Message.ChatType != "personal" && !view.IsOwnMessage;
        view.HasReply = view.Message.Reply is not null;
    }

    private Label _content;
    private Label _date;
    private Grid _grid;
    private Label _sender;
    private Label _reply;
    private Label _replySender;
    private VerticalStackLayout _replyBox;
    private Border _replyBoxBorder;
    public Point? point;
    public MessageView()
	{
        Style LS = (Style)ResourceManager.Get("CommonLabel");

        _sender = new Label()
        {
            Style = LS,
            Margin = new Thickness(0),
            Padding = new Thickness(0),
            HorizontalOptions = LayoutOptions.Start
        };
        _sender.SetDynamicResource(Label.TextColorProperty, "CPrimary");
        _sender.SetBinding(Label.TextProperty, new Binding(nameof(Message.SenderName)));
        _sender.SetBinding(Label.IsVisibleProperty, new Binding(nameof(SenderIsVisible), source: this));

        _replySender = new Label()
        {
            Style = LS,
            MaxLines = 1,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(0),
            Padding = new Thickness(0),
            LineBreakMode = LineBreakMode.TailTruncation
        };
        _replySender.SetBinding(Label.TextProperty, new Binding("Reply.SenderName"));
        _replySender.SetDynamicResource(Label.TextColorProperty, "CPrimary");
        _replySender.SetDynamicResource(Label.FontSizeProperty, "SecondaryFontSize");

        _reply = new Label()
        {
            Style = LS,
            MaxLines = 1,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(0),
            Padding = new Thickness(0),
            LineBreakMode = LineBreakMode.TailTruncation
        };
        _reply.SetBinding(Label.TextProperty, new Binding("Reply.Content"));
        _reply.SetDynamicResource(Label.FontSizeProperty, "SecondaryFontSize");

        _replyBoxBorder = new Border
        {
            Style = (Style)ResourceManager.Get("EditSection"),
            Margin = new Thickness(0),
            Padding = 4,
            Content = new VerticalStackLayout()
            {
                Children =
                {
                    _replySender,
                    _reply
                }
            },
            StrokeShape = new RoundRectangle { CornerRadius = 6 }
        };
        _replyBoxBorder.SetBinding(Border.IsVisibleProperty, new Binding(nameof(HasReply), source: this));
        _replyBoxBorder.SetDynamicResource(Border.StrokeProperty, "CSuccess");

        _content = new Label()
        {
            Style = LS,
            HorizontalOptions = LayoutOptions.Start
        };
        _content.SetBinding(Label.TextProperty, new Binding(nameof(Message.Content)));

        _date = new Label()
        {
            Style = LS,
            HorizontalOptions = LayoutOptions.End,
            Margin = new Thickness(10, 5, 0, 0)
        };
        _date.SetDynamicResource(Label.FontSizeProperty, "SecondaryFontSize");
        _date.SetBinding(Label.TextProperty, new Binding(nameof(Message.CreatedAt), stringFormat: "{0:dd.MM HH:mm}"));
        _date.SetDynamicResource(Label.TextColorProperty, "CTextSecondary");

        _grid = new Grid
        {
            Padding = 5,
            RowDefinitions = new RowDefinitionCollection
                {
                    new() { Height = new GridLength(0, GridUnitType.Auto) },
                    new() { Height = new GridLength(0, GridUnitType.Auto) },
                    new() { Height = new GridLength(0, GridUnitType.Auto) },
                    new() { Height = new GridLength(0, GridUnitType.Auto) }
                },
            MaximumWidthRequest = 300
        };
        _grid.Add( _sender, row: 0);
        _grid.Add( _replyBoxBorder, row: 1);
        _grid.Add( _content, row: 2);
        _grid.Add( _date, row: 3);

        Content = new Border
        {
            Style = (Style)ResourceManager.Get("EditSection"),
            Margin = new Thickness(5),
            Padding = 0,
            Content = _grid,
            HorizontalOptions = LayoutOptions.End,
        };
        Content.SetDynamicResource(Border.BackgroundProperty, "CSurface");
        Content.SetBinding(Border.StrokeShapeProperty, new Binding(nameof(IsOwnMessage), source: this, converter: new MessageCornerConverter()));
        Content.SetBinding(Border.HorizontalOptionsProperty, new Binding(nameof(IsOwnMessage), source: this, converter: new MessageAlignmentConverter()));
        Content.SetBinding(BindableObject.BindingContextProperty, new Binding(nameof(Message), source: this));
    }
}