﻿using System;
using System.Windows;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.SqlClient;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Data.SQLite;
using System.Data;
using System.Configuration;
using OtpNet;

namespace Schedule_Mgr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        public ManagerWindow()
        {
            InitializeComponent();
            
        }

        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }


        private void calendar_CalendarFormat(object sender, RoutedEventArgs e) 
        {

            
            calendar.DisplayDateStart = DateTime.Today;
            calendar.DisplayDateEnd = DateTime.Today.AddDays(14);



            DateTime selectedDay = DateTime.Today;
            DateTime maxDay = DateTime.Today.AddDays(14);

            while (selectedDay != maxDay) 
            {
                if ((selectedDay.DayOfWeek == DayOfWeek.Saturday) || (selectedDay.DayOfWeek == DayOfWeek.Sunday)) 
                {
                    calendar.BlackoutDates.Add(new CalendarDateRange(selectedDay));
                }
                selectedDay = selectedDay.AddDays(1);
            }
        }


        private void employeeList_GetDoctors(object sender, RoutedEventArgs e) 
        {
            String sqlPath = LoadConnectionString();
            SQLiteConnection connection = new SQLiteConnection(sqlPath);
            string sqlQuery = $"SELECT Firstname, Middlename, Lastname FROM Accounts WHERE Account_Type = 2;";

            connection.Open();
            var cmd = new SQLiteCommand(sqlQuery, connection);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read()) 
            {
                string middlename = "";
                if (!(reader["Middlename"] == null))
                    middlename = reader["Middlename"].ToString();
                
                string name = "Dr. " + reader["Firstname"].ToString() + " " + middlename + " " + reader["Lastname"].ToString();

                employeeList.Items.Add(name);
            }
        }


        private void comboBox_SetTimes(object sender, RoutedEventArgs e) 
        {
            for (int i = 0; i < 25; i++) 
            {
                hoursComboBox.Items.Add(i.ToString("00"));
                hoursComboBox2.Items.Add(i.ToString("00"));
            }
            for (int i = 0; i < 60; i = i + 5) 
            {
                minsComboBox.Items.Add(i.ToString("00"));
                minsComboBox2.Items.Add(i.ToString("00"));
            }
        }
    }
}
