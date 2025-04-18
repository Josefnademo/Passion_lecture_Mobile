﻿using System.Security.Cryptography.X509Certificates;

namespace Passion_lecture_Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //Save all pages for navigation
            Routing.RegisterRoute(nameof(TagsPage), typeof(TagsPage));
            Routing.RegisterRoute(nameof(LibraryPage), typeof(LibraryPage));
            Routing.RegisterRoute(nameof(EditPage), typeof(EditPage));
            Routing.RegisterRoute(nameof(ImportPage), typeof(ImportPage));
            Routing.RegisterRoute(nameof(ImportPage), typeof(EditPage));
            Routing.RegisterRoute(nameof(ImportPage), typeof(ReadBookPage));
        }
        // Static method for back navigation (can be called from any pages)
        public static async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync(".."); //navigation vers page précédente
        }

       
    }
}
