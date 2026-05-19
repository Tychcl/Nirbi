using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace nearby.ContentViews.Headers
{
    public partial class BaseHeaderView : ContentView
    {
        public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(BaseHeaderView),
            default(string), BindingMode.OneWay);
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly BindableProperty InnerMarginProperty =
        BindableProperty.Create(nameof(InnerMargin), typeof(Thickness), typeof(BaseHeaderView),
            new Thickness(10, 2, 10, 2), BindingMode.OneWay);
        public Thickness InnerMargin
        {
            get => (Thickness)GetValue(InnerMarginProperty);
            set => SetValue(InnerMarginProperty, value);
        }

        public static readonly BindableProperty BackCommandProperty =
        BindableProperty.Create(nameof(BackCommand), typeof(ICommand), typeof(BaseHeaderView),
            defaultValue: null, BindingMode.OneWay);
        public ICommand BackCommand
        {
            get => (ICommand)GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }
        private async Task GoBackDefault()
        {
            await Shell.Current.GoToAsync("..");
        }

        public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(BaseHeaderView),
            null, BindingMode.OneWay);
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly BindableProperty MenuIsVisibleProperty =
        BindableProperty.Create(nameof(MenuIsVisible), typeof(bool), typeof(BaseHeaderView),
            default(bool), BindingMode.OneWay);
        public bool MenuIsVisible
        {
            get => (bool)GetValue(MenuIsVisibleProperty);
            set => SetValue(MenuIsVisibleProperty, value);
        }

        public static readonly BindableProperty BackIsVisibleProperty =
        BindableProperty.Create(nameof(BackIsVisible), typeof(bool), typeof(BaseHeaderView),
            false, BindingMode.OneWay, propertyChanged: OnBackIsVisibleChanged);
        public bool BackIsVisible
        {
            get => (bool)GetValue(BackIsVisibleProperty);
            set => SetValue(BackIsVisibleProperty, value);
        }
        public static void OnBackIsVisibleChanged(BindableObject bin, object nV, object oV)
        {
            if (bin is BaseHeaderView BHV && BHV.BackIsVisible && BHV.BackCommand is null)
            {
                BHV.BackCommand = new Command(async () => await BHV.GoBackDefault());
            }
        }

        public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(BaseHeaderView),
            default(string), BindingMode.OneWay);
        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly BindableProperty ExtraContentProperty =
        BindableProperty.Create(nameof(ExtraContent), typeof(View), typeof(BaseHeaderView), null, propertyChanged: OnEXChanged);
        public View? ExtraContent
        {
            get => (View?)GetValue(ExtraContentProperty);
            set => SetValue(ExtraContentProperty, value);
        }
        public static void OnEXChanged(BindableObject bin, object newValue, object oldValue)
        {
            (bin as BaseHeaderView).Extra();
        }
        private void Extra()
        {
            if (ExtraContent is not null)
            {
                ExtraContentView.IsVisible = true;
                TitleLabel.IsVisible = false;
                ExtraContentView.SetBinding(ContentView.ContentProperty, new Binding(nameof(ExtraContent), source: this));
            }
        }

        public BaseHeaderView()
        {
            InitializeComponent();
            Extra();
        }
    }
}