﻿using System;
using DidacticalEnigma.Core.Models.LanguageService;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DidacticalEnigma.Xam.Services;
using DidacticalEnigma.Xam.Views;

namespace DidacticalEnigma.Xam
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
