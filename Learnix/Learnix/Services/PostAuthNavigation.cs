using Learnix.Views;

namespace Learnix.Services
{
    public static class PostAuthNavigation
    {
        public static async Task NavigateAsync(LearnixApiClient apiClient)
        {
            var profile = await apiClient.GetProfileAsync();

            var route = profile.User.Role == "Admin"
                ? nameof(AdminPage)
                : profile.SelectedSubjectsCount > 0
                    ? nameof(SubjectQuestionPage)
                    : nameof(CompleteRegistrationPage);

            await Shell.Current.GoToAsync(route);
        }
    }
}
