﻿using System;
using System.Linq;
using System.Windows;
using ModernWpf.Controls;
using LeakerUtility.Pages;
using ModernWpf.Navigation;
using System.Windows.Navigation;

namespace LeakerUtility
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
            Navigate(NavView.SelectedItem);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (NavigationViewItem navigationItem in NavView.MenuItems)
                navigationItem.Content = App.LocalizedStrings.Where(x => x.Key == (string)navigationItem.Content + "NavigationItem")?.FirstOrDefault().Value;

            var settingsItem = NavView.SettingsItem as NavigationViewItem;
            settingsItem.Content = App.LocalizedStrings.Where(x => x.Key == "SettingsNavigationItem")?.FirstOrDefault().Value;

            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, TitleBar.GetSystemOverlayRightInset(this), currMargin.Bottom);
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            ContentFrame.GoBack();
            ContentFrame.ContentRendered += delegate
            {
                var pageType = ContentFrame.Content.GetType();
                NavView.Header = App.LocalizedStrings.Where(x => x.Key == pageType.Name + "Header")?.FirstOrDefault().Value;
            };
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
                Navigate(typeof(SettingsPage));
            else
                Navigate(args.InvokedItemContainer);
        }

        private void NavView_PaneOpening(NavigationView sender, object args)
            => UpdateAppTitleMargin(sender);

        private void NavView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
            => UpdateAppTitleMargin(sender);

        private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            Thickness currMargin = AppTitleBar.Margin;
            if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
                AppTitleBar.Margin = new Thickness(sender.CompactPaneLength * 2, currMargin.Top, currMargin.Right, currMargin.Bottom);
            else
                AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);

            UpdateAppTitleMargin(sender);
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType() == typeof(SettingsPage))
                NavView.SelectedItem = NavView.SettingsItem;
            else
                NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => GetPageType(x) == e.SourcePageType());
        }

        private void UpdateAppTitleMargin(NavigationView sender)
        {
            const int smallLeftIndent = 4, largeLeftIndent = 24;

            Thickness currMargin = AppTitle.Margin;

            if ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                     sender.DisplayMode == NavigationViewDisplayMode.Minimal)
                AppTitle.Margin = new Thickness(smallLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            else
                AppTitle.Margin = new Thickness(largeLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
        }

        private void Navigate(object item)
        {
            if (item is NavigationViewItem menuItem)
            {
                Type pageType = GetPageType(menuItem);
                if (ContentFrame.CurrentSourcePageType != pageType)
                    ContentFrame.Navigate(pageType);

                ContentFrame.ContentRendered += delegate
                {
                    NavView.Header = App.LocalizedStrings.Where(x => x.Key == pageType.Name + "Header")?.FirstOrDefault().Value;
                };
            }
        }

        private void Navigate(Type sourcePageType)
        {
            if (ContentFrame.CurrentSourcePageType != sourcePageType)
                ContentFrame.Navigate(sourcePageType);

            ContentFrame.ContentRendered += delegate
            {
                var pageType = ContentFrame.Content.GetType();
                NavView.Header = App.LocalizedStrings.Where(x => x.Key == pageType.Name + "Header")?.FirstOrDefault().Value;
            };
        }

        private Type GetPageType(NavigationViewItem item)
            => item.Tag as Type;
    }
}