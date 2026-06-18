using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using nearby.Classes;
using nearby.Interfaces;
using nearby.Models;
using nearby.Models.Api;
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

                var updateRequest = new UpdateUserRequest
                {
                    Id = User.Id.ToString(),
                    FirstName = User.Name,
                    SecondName = User.Surname,
                    LastName = User.Patronymic,
                    Phone = User.Phone,
                    Email = User.Email,
                    BirthDate = User.BirthDate?.ToString("yyyy-MM-dd"),
                    City = User.City,
                    About = User.About,
                    EducationPlace = User.EducationInstitution,
                    EducationStartYear = User.EducationStartYear?.ToString(),
                    EducationEndYear = User.EducationEndYear?.ToString(),
                    EducationField = User.EducationField,
                    Vk = User.VK,
                    Tg = User.TG,
                    Max = User.MAX,
                    CurrentPassword = CurrentPassword,
                    NewPassword = null   // если не меняем пароль, оставляем пустым
                };

                await _userService.UpdateUserByIdAsync(User.Id, updateRequest);
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