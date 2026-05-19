using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using nearby.Classes;
using nearby.Models;
using nearby.Services;

namespace nearby.ViewModels
{
    public partial class EditProfileViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private int aboutLength;
        [ObservableProperty]
        private string about;
        partial void OnAboutChanged(string value)
        {
            User.About = value;
            AboutLength = value.Length;
        }

        [ObservableProperty]
        private User _user;

        [ObservableProperty]
        private string _currentPassword = string.Empty;
 

        public EditProfileViewModel(IUserService userService)
        {
            _userService = userService;
            PageTitle = "Редактирование профиля";
            User = _userService.CurrentUser.Copy();
            About = User.About;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentPassword))
                    throw new Exception("Введите текущий пароль для подтверждения изменений");

                if (string.IsNullOrWhiteSpace(User.FullName))
                    throw new Exception("ФИО обязательно для заполнения");

                var updatedData = new
                {
                    full_name = User.FullName,
                    city = User.City,
                    birth_date = User.BirthDate?.ToString("yyyy-MM-dd"),
                    email = User.Email,
                    phone = User.Phone,
                    about = User.About,
                    education_institution = User.EducationInstitution,
                    education_degree = User.EducationDegree,
                    education_field = User.EducationField,
                    education_start_year = string.IsNullOrWhiteSpace(User.EducationStartYear.ToString()) ? 0 : User.EducationStartYear,
                    education_end_year = string.IsNullOrWhiteSpace(User.EducationEndYear.ToString()) ? 0 : User.EducationEndYear,
                    current_password = CurrentPassword
                };

                var r = await _userService.UpdateUserByIdAsync(updatedData);
                await Application.Current.MainPage.DisplayAlert("Успех", "Данные сохранены", "OK");
                await Application.Current.MainPage.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
            
        }
    }
}