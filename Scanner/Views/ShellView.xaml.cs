﻿using Scanner.ViewModels;
using Scanner.Views.Dialogs;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using static Utilities;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace Scanner.Views
{
    public sealed partial class ShellView : Page
    {
        public ShellView()
        {
            this.InitializeComponent();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
        }

        private async void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            await RunOnUIThreadAsync(CoreDispatcherPriority.Low, () =>
            {
                // prevent titlebar from bleeding into buttons area
                ColumnTitlebarButtons.Width = 
                    new GridLength(sender.SystemOverlayLeftInset + sender.SystemOverlayRightInset);
            });
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.DisplayedView):
                    ViewModel_DisplayedViewChanged(sender, e);
                    break;
                case nameof(ViewModel.WindowActivationState):
                    await RunOnUIThreadAsync(CoreDispatcherPriority.Low, () =>
                    {
                        if (ViewModel.WindowActivationState == CoreWindowActivationState.Deactivated)
                        {
                            ((WinUI.NavigationViewItem) NavigationViewMain.SettingsItem).Opacity = 0.5;
                        }
                        else
                        {
                            ((WinUI.NavigationViewItem)NavigationViewMain.SettingsItem).Opacity = 1;
                        }
                    });
                    break;
                default:
                    break;
            }
        }

        private void ViewModel_DisplayedViewChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            WinUI.NavigationViewItem requestedItem = ConvertNavigationItem(ViewModel.DisplayedView);

            if (requestedItem.IsEnabled == true)
            {
                NavigationViewMain.SelectedItem = requestedItem;
            }
            else
            {
                // can not select requested item ~> resynchronize ViewModel
                var currentlySelectedItem = NavigationViewMain.SelectedItem as WinUI.NavigationViewItem;
                ViewModel.DisplayedView = ConvertNavigationItem(currentlySelectedItem);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationViewMain.SelectedItem = NavigationViewItemMainScanOptions;

            FrameMainContentSecond.Navigate(typeof(EditorView));

            ((WinUI.NavigationViewItem)(NavigationViewMain.SettingsItem)).RightTapped +=
                NavigationViewItemMainSettings_RightTapped;
        }

        private void NavigationViewMain_SelectionChanged(WinUI.NavigationView sender, WinUI.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem == NavigationViewItemMainScanOptions)
            {
                FrameMainContentFirst.Navigate(typeof(ScanOptionsView));
            }
            else if (args.SelectedItem == NavigationViewItemMainPageList)
            {
                FrameMainContentFirst.Navigate(typeof(PageListView));
            }
            else if (args.SelectedItem == NavigationViewItemMainEditor)
            {
                FrameMainContentSecond.Content = null;
                FrameMainContentFirst.Navigate(typeof(EditorView));
            }
            else if (args.SelectedItem == NavigationViewItemMainHelp)
            {
                FrameMainContentFirst.Navigate(typeof(HelpView));
            }
            else if (args.SelectedItem == NavigationViewMain.SettingsItem)
            {
                FrameMainContentFirst.Navigate(typeof(SettingsView));
            }

            ViewModel.DisplayedView = ConvertNavigationItem(args.SelectedItem as WinUI.NavigationViewItem);
        }

        private void VisualStateGroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            // ensure expected layout when the app is resized
            if (e.OldState == NarrowState)
            {
                FrameMainContentSecond.Navigate(typeof(EditorView), null, new SuppressNavigationTransitionInfo());
            }

            if (e.OldState == NarrowState && NavigationViewMain.SelectedItem == null || NavigationViewItemMainEditor.IsSelected)
            {
                NavigationViewItemMainScanOptions.IsSelected = true;
            }

            if (e.NewState == WideState && NavigationViewItemMainPageList.IsSelected)
            {
                FrameMainContentFirst.Content = null;
                NavigationViewItemMainScanOptions.IsSelected = true;
            }

            if (e.NewState == WideState)
            {
                FrameMainContentThird.Navigate(typeof(PageListView), null,
                    new SuppressNavigationTransitionInfo());
            }
        }

        /// <summary>
        ///     Maps a <see cref="ShellNavigationSelectableItem"/> to the corresponding
        ///     <see cref="WinUI.NavigationViewItem"/>.
        /// </summary>
        public WinUI.NavigationViewItem ConvertNavigationItem(ShellNavigationSelectableItem item)
        {
            switch (item)
            {
                case ShellNavigationSelectableItem.ScanOptions:
                    return NavigationViewItemMainScanOptions;
                case ShellNavigationSelectableItem.PageList:
                    return NavigationViewItemMainPageList;
                case ShellNavigationSelectableItem.Editor:
                    return NavigationViewItemMainEditor;
                case ShellNavigationSelectableItem.Help:
                    return NavigationViewItemMainHelp;
                case ShellNavigationSelectableItem.Donate:
                    return NavigationViewItemMainDonate;
                case ShellNavigationSelectableItem.Settings:
                    return (WinUI.NavigationViewItem)NavigationViewMain.SettingsItem;
                default:
                    throw new ArgumentException(String.Format(
                        "Unable to convert ShellNavigationSelectableItem {1} to NavigationViewItem.",
                        item.ToString()));
            }
        }

        /// <summary>
        ///     Maps a <see cref="WinUI.NavigationViewItem"/> to the corresponding
        ///     <see cref="ShellNavigationSelectableItem"/>.
        /// </summary>
        public ShellNavigationSelectableItem ConvertNavigationItem(WinUI.NavigationViewItem item)
        {
            if (item == NavigationViewItemMainScanOptions) return ShellNavigationSelectableItem.ScanOptions;
            else if (item == NavigationViewItemMainPageList) return ShellNavigationSelectableItem.PageList;
            else if (item == NavigationViewItemMainEditor) return ShellNavigationSelectableItem.Editor;
            else if (item == NavigationViewItemMainHelp) return ShellNavigationSelectableItem.Help;
            else if (item == NavigationViewItemMainDonate) return ShellNavigationSelectableItem.Donate;
            else if (item == NavigationViewMain.SettingsItem) return ShellNavigationSelectableItem.Settings;
            else throw new ArgumentException(String.Format(
                "Unable to convert NavigationViewItem {1} to ShellNavigationSelectableItem.",
                item.Name));
        }

        private async void NavigationViewMain_ItemInvoked(WinUI.NavigationView sender, WinUI.NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer == NavigationViewItemMainSaveLocation)
            {
                await ViewModel.ShowScanSaveLocationCommand.ExecuteAsync(null);
            }
            else if (args.InvokedItemContainer == NavigationViewItemMainDonate)
            {
                DonateDialogView dialog = new DonateDialogView();
                await RunOnUIThreadAsync(CoreDispatcherPriority.Normal, async () => await dialog.ShowAsync());
            }
        }

        private async void NavigationViewItemMainSettings_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
#if DEBUG
            await RunOnUIThreadAsync(CoreDispatcherPriority.Low, () =>
            {
                FlyoutBase.ShowAttachedFlyout(NavigationViewMain);
            });
#endif
        }

        private void InfoBarAppWideMessages_Closing(WinUI.InfoBar sender, WinUI.InfoBarClosingEventArgs args)
        {
            args.Cancel = true;
        }
    }
}
