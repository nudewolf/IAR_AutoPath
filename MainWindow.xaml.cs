using IAR_AutoPath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;

namespace IAR_AutoPath_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string startPath = AppDomain.CurrentDomain.BaseDirectory.ToString();
        IAR_Project iarProject;
        public MainWindow()
        {
            InitializeComponent();

            TextBox_Source.Text = startPath;

            iarProject = new IAR_Project();
            iarProject.LogshowEvent += new IAR_Project.LogshowHandler(ShowMsg);
            iarProject.SourcePath = startPath;

            DirectoryInfo root = new DirectoryInfo(startPath);
            foreach (FileInfo f in root.GetFiles())
            {
                if (f.Extension == ".ewp")
                {
                    TextBox_PrjName.Text = f.FullName;
                    UpdateEwpInfoUI(iarProject.GetEwpInfo(f.FullName));
                    break;
                }
            }
        }

        void UpdateEwpInfoUI(EwpInfo ewpInfo)
        {
            ComboBox_ConfigList.Items.Clear();

            TextBox_Version.Text = ewpInfo.version;

            if (ewpInfo.configList.Count > 0)
            {
                foreach (string item in ewpInfo.configList)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = item;
                    ComboBox_ConfigList.Items.Add(comboBoxItem);
                }
                ComboBox_ConfigList.SelectedIndex = 0;
            }
        }
        private void Button_Update_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_AutoInc.IsChecked == false && CheckBox_AutoPath.IsChecked == false)
                return;

            iarProject.isAutoInclude = (bool)CheckBox_AutoInc.IsChecked;
            iarProject.isAutoPath = (bool)CheckBox_AutoPath.IsChecked;
            iarProject.SourcePath = TextBox_Source.Text;

            ComboBoxItem item = ComboBox_ConfigList.SelectedItem as ComboBoxItem;
            iarProject.configName = item.Content.ToString();

            Thread thread = new Thread(new ThreadStart(iarProject.Update));
            thread.Start();

            //ShowMsg("\n\n更新失败!");
        }

        void ShowMsg(string msg)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
             {
                 if (msg.Contains("开始"))
                     TextBox_Msg.Clear();
                 TextBox_Msg.AppendText(msg + "\n");
             }));
            //Thread.Sleep(20);
        }

        private void TextBox_Msg_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox_Msg.ScrollToEnd();
        }

        private void Button_OpenPrj_Click(object sender, RoutedEventArgs e)
        {
            //CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            //dialog.IsFolderPicker = false;//设置为选择文件夹
            //dialog.Title = "选择IAR Workspace文件";
            //dialog.InitialDirectory = startPath;
            //dialog.Filters.Add(new CommonFileDialogFilter("IAR IDE Workspace", ".eww"));

            //if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            //{
            //    TextBox_PrjName.Text = dialog.FileName;
            //    prjPath = dialog.FileName.Substring(0, dialog.FileName.LastIndexOf('\\') + 1);
            //}

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择文件";
            openFileDialog.Filter = "IAR IDE EWP|*.ewp";
            openFileDialog.FileName = string.Empty;
            openFileDialog.InitialDirectory = startPath;

            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                TextBox_PrjName.Text = openFileDialog.FileName;
                TextBox_Source.Text = openFileDialog.FileName.Remove(openFileDialog.FileName.LastIndexOf('\\'));

                UpdateEwpInfoUI(iarProject.GetEwpInfo(openFileDialog.FileName));
            }
        }

        private void Button_OpenDir_Click(object sender, RoutedEventArgs e)
        {
            //CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            //dialog.IsFolderPicker = true;//设置为选择文件夹
            //dialog.Title = "选择文件夹";
            //dialog.InitialDirectory= startPath;

            //if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            //{
            //    TextBox_Source.Text = dialog.FileName;
            //}

            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            m_Dialog.ShowNewFolderButton = false;

            DialogResult result = m_Dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                TextBox_Source.Text = m_Dialog.SelectedPath.Trim();
            }
        }

        private void Button_Transfer_Click(object sender, RoutedEventArgs e)
        {
            string prjVersion = TextBox_Version.Text;
            ComboBoxItem item = ComboBox_Version.SelectedItem as ComboBoxItem;
            string selVersion = item.Content.ToString();
            if (prjVersion.Contains('.'))
            {
                prjVersion = prjVersion.Remove(prjVersion.LastIndexOf('.'));
                if (String.Compare(prjVersion, selVersion) <= 0)//项目版本低于选择的版本，不转换
                {
                    TextBox_Msg.AppendText("项目版本不高于选择的版本，不可降级！\n");
                }
                else
                {
                    iarProject.ChangeVersion("8.30.1");
                    UpdateEwpInfoUI(iarProject.GetEwpInfo());
                }
            }
        }




        private void TextBox_PrjName_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            //if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            //{
            //    e.Effects = DragDropEffects.Link;
            //}
            //else
            //{
            //    e.Effects = DragDropEffects.None;

            //}

            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void TextBox_PrjName_PreviewDrop(object sender, System.Windows.DragEventArgs e)
        {
            var fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

            FileInfo fileInfo = new FileInfo(fileName);
            if (fileInfo.Extension == ".ewp")
            {
                TextBox_PrjName.Text = fileName;
                TextBox_Source.Text = fileName.Remove(fileName.LastIndexOf('\\'));

                UpdateEwpInfoUI(iarProject.GetEwpInfo(fileName));
            }
        }

        private void TextBox_Source_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void TextBox_Source_PreviewDrop(object sender, System.Windows.DragEventArgs e)
        {
            var fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            if (Directory.Exists(fileName))
            {
                TextBox_Source.Text = fileName;
            }
        }
    }
}
