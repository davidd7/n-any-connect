﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace NAnyConnect_test1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private MainController controller;
        private List<Button> buttons = new List<Button>();
        private List<ButtonState> buttonStates = new List<ButtonState>( );

        public enum ButtonState { Disabled = 0, EnabledUnselected = 1, EnabledSelected = 2 };

        public MainWindow()
        {
            InitializeComponent();

            buttons.Add(button_account_1);
            buttons.Add(button_account_2);

            buttonStates.Add(ButtonState.Disabled);
            buttonStates.Add(ButtonState.Disabled);

            controller = new MainController(this);
            controller.SetUpMain();

            if (controller != null)
                SystemEvents.PowerModeChanged += OnPowerChange;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);


        }



        public void SetUpConnectButton(int slot, string content, ButtonState state) {
            buttons.ElementAt(slot).Content = content;
            switch (state) {
                case ButtonState.Disabled:
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[0]).Visibility = Visibility.Visible; // Create
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[1]).Visibility = Visibility.Collapsed; // Connect
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[2]).Visibility = Visibility.Collapsed; // Disconnect
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[4]).Visibility = Visibility.Collapsed; // Edit
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[5]).Visibility = Visibility.Collapsed; // Delete
                    break;
                case ButtonState.EnabledUnselected:
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[0]).Visibility = Visibility.Collapsed;
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[1]).Visibility = Visibility.Visible;
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[2]).Visibility = Visibility.Collapsed;
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[4]).Visibility = Visibility.Visible;
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[5]).Visibility = Visibility.Visible;
                    buttons.ElementAt(slot).Background = Brushes.White;
                    buttons.ElementAt(slot).Foreground = Brushes.Black;
                    break;
                case ButtonState.EnabledSelected:
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[0]).Visibility = Visibility.Collapsed;
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[1]).Visibility = Visibility.Collapsed;
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[2]).Visibility = Visibility.Visible;
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[4]).Visibility = Visibility.Visible;
                    ((MenuItem)buttons.ElementAt(slot).ContextMenu.Items[5]).Visibility = Visibility.Visible;
                    buttons.ElementAt(slot).Background = new SolidColorBrush( (Color)ColorConverter.ConvertFromString("#097dff") );
                    buttons.ElementAt(slot).Foreground = Brushes.White;
                    break;
            }
            buttonStates[slot] = state;
        }

        public void SetUpNoConnectionButton(ButtonState state)
        {
            switch (state)
            {
                case ButtonState.EnabledUnselected:
                    button_noConnection.Background = Brushes.White;
                    button_noConnection.Foreground = Brushes.Black;
                    break;
                case ButtonState.EnabledSelected:
                    button_noConnection.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#097dff"));
                    button_noConnection.Foreground = Brushes.White;
                    break;
            }
        }

        public void SetLoading(bool b) {
            if (b) {
                text_loading.Visibility = Visibility.Visible;
            }
            else
            {
                text_loading.Visibility = Visibility.Collapsed;
            }

        }


        #region Click events
        private void ConnectButton_Connect_Click(object sender, RoutedEventArgs e)
        {
            int slot = int.Parse((string)(sender as Control).Tag);
            if (!buttonStates[slot].Equals(ButtonState.Disabled))
            {
                controller.VpnStart(slot);
            }
            else
            {
                EditWindow w = new EditWindow(slot, controller);
                w.ShowDialog();
            }
        }
        private void ConnectButton_Edit_Click(object sender, RoutedEventArgs e)
        {
            int slot = int.Parse((string) (sender as Control).Tag);
            EditWindow w = new EditWindow(slot, controller);
            w.ShowDialog();
        }
        private void ConnectButton_Delete_Click(object sender, RoutedEventArgs e)
        {
            int slot = int.Parse((string)(sender as Control).Tag);
            controller.DeleteSlot(slot);
        }
        private void NoConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            controller.VpnEnd();
        }


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

            SettingsWindow w = new SettingsWindow(controller);
            w.ShowDialog();
        }

        #endregion



        void OnProcessExit(object sender, EventArgs e)
        {
            controller.VpnEnd(false); // false necessary, because ui-thread is not the caller of this event (and unnecessary to update ui at program exit anyway)
            SystemEvents.PowerModeChanged -= OnPowerChange;
        }


        public void OnPowerChange(object s, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    controller.VpnWakeup();
                    break;
                case PowerModes.Suspend:
                    controller.VpnSleep();
                    break;
            }
        }

        private void Checkbox_reconnectAfterSleep_Checked(object sender, RoutedEventArgs e)
        {
            controller.RecognizeChangeReconnectAfterSleep();
        }


    }





}
