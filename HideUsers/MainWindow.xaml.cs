using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Windows;

namespace HideUsers
{
    public partial class MainWindow : Window
    {
        public IList<string> UserList = new List<string>();
        RegistryKey UserListKey;

        public const string UserAlreadyHiddenMSG = "This User is hidden from the Welcome Screen";
        public const string UserVisibleMSG = "This User is Visible on the Welcome Screen";
        public const string AdministratorRightsRequiredMsg = "You need to be an administrator to run this Application.";

        public MainWindow()
        {
            if (!HasAdministratorPrivileges())
            {
                MessageBox.Show(AdministratorRightsRequiredMsg);
                Application.Current.Shutdown();
            }
            else
            {
                InitializeComponent();
                UserListKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\SpecialAccounts\UserList");

                var path = string.Format("WinNT://{0},computer", Environment.MachineName);
                using (var computerEntry = new DirectoryEntry(path))
                {
                    UserList = (from DirectoryEntry childEntry in computerEntry.Children
                                where childEntry.SchemaClassName == "User" && IsUserActive(childEntry)
                                select childEntry.Name).ToList();
                }
                UserCombo.ItemsSource = UserList;
            }
        }

        private static bool HasAdministratorPrivileges()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private bool IsUserActive(DirectoryEntry de)
        {
            if (de.NativeGuid == null) return false;

            int flags = (int)de.Properties["UserFlags"].Value;

            if (!Convert.ToBoolean(flags & 0x0002))
                return true;
            else return false;
        }

        public void HideUser(string username)
        {
            UserListKey.SetValue(username, "0", RegistryValueKind.DWord);
            MessageBox.Show("The registry key has been switched", "Title Goes Here");
        }

        private void UserCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateWindowFieldsto("",Visibility.Collapsed, Visibility.Collapsed);
            var selectedUser = UserCombo.SelectedValue.ToString();
            var key = UserListKey.GetValue(selectedUser);

            if (key == null)
                UpdateWindowFieldsto(UserVisibleMSG,Visibility.Visible, Visibility.Collapsed);
            else
            {
                if (Convert.ToInt32(key) == 0)
                    UpdateWindowFieldsto(UserAlreadyHiddenMSG,Visibility.Collapsed, Visibility.Visible);
                else if (Convert.ToInt32(key) == 1)
                    UpdateWindowFieldsto(UserVisibleMSG,Visibility.Visible, Visibility.Collapsed);
            }
        }

        private void UpdateWindowFieldsto(string msgText, Visibility HideUserbtnVisibility, Visibility ShowUserbtnVisibility)
        {
            MsgLbl.Content = msgText;
            HideUserbtn.Visibility = HideUserbtnVisibility;
            ShowUserbtn.Visibility = ShowUserbtnVisibility;
        }

        private void HideUserbtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UserCombo.SelectedValue.ToString();
            UserListKey.SetValue(selectedUser,0);
            UpdateWindowFieldsto(UserAlreadyHiddenMSG, Visibility.Collapsed, Visibility.Visible);
        }

        private void ShowUserbtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UserCombo.SelectedValue.ToString();
            UserListKey.SetValue(selectedUser, 1);
            UpdateWindowFieldsto(UserVisibleMSG, Visibility.Visible, Visibility.Collapsed);
        }
    }
}
