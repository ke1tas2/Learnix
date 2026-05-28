using Learnix.Views;

namespace Learnix
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Shell.SetNavBarIsVisible(this, false);
            Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(CompleteRegistrationPage), typeof(CompleteRegistrationPage));
            Routing.RegisterRoute(nameof(AskFewQuestions),typeof(AskFewQuestions));
            Routing.RegisterRoute(nameof(HowLongQuestionPage), typeof(HowLongQuestionPage));
            Routing.RegisterRoute(nameof(HowKnowPage), typeof(HowKnowPage));
            Routing.RegisterRoute(nameof(WhatSubject), typeof(WhatSubject));
            Routing.RegisterRoute(nameof(SubjectQuestionPage), typeof(SubjectQuestionPage));
            Routing.RegisterRoute(nameof(LessonPage), typeof(LessonPage));
            Routing.RegisterRoute(nameof(LessonResultPage), typeof(LessonResultPage));
        }
    }
}
