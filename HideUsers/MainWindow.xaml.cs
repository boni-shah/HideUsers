using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Windows;

namespace HideUsers
{
    public partial class MainWindow : Window
    {
        public IList<string> UserList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                var path = string.Format("WinNT://{0},computer", Environment.MachineName);

                using (var computerEntry = new DirectoryEntry(path))
                {
                    UserList = (from DirectoryEntry childEntry in computerEntry.Children
                                where childEntry.SchemaClassName == "User" && IsUserActive(childEntry)
                                select childEntry.Name).ToList();
                }
                UserCombo.ItemsSource = UserList;
            }
            catch { 
                //Empty catch to swallow user load error
            }            
        }

        public bool IsUserActive(DirectoryEntry de)
        {
            if (de.NativeGuid == null) 
                return false;

            int flags = (int)de.Properties["UserFlags"].Value;

            if (!Convert.ToBoolean(flags & 0x0002))
                return true;
            else 
                return false;
        }

        public void HideUser(string username)
        {
            try
            {
                RegistryKey UserKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\SpecialAccounts\UserList");
                UserKey.SetValue(username, "0", RegistryValueKind.DWord);
                MessageBox.Show("The selected user has been hidden.", "Operation Successful");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to hide selected user. Error Details: " + ex.Message, "Operation Failed");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var userName = UserCombo.SelectedValue.ToString();
            HideUser(userName); 
        }
    }
}