using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
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
using System.IO;
using System.Collections;
using System.Globalization;
using FadePlus;
using FadePlus.Model;

namespace InterView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int MaxRecords { get; set; }

        FadePlus.FadePlus fp;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RecordChange(object sender, int currentRecord, string record)
        {
            // First record
            if (fp.CurrentRecord == 1)
                ((Button)prevButton).IsEnabled = false;

            // no next record
            if (fp.CurrentRecord < fp.RecordCount)
                nextButton.IsEnabled = true;

            // last record
            if (fp.CurrentRecord == fp.RecordCount)
                ((Button)nextButton).IsEnabled = false;

            if (fp.CurrentRecord > 1)
                prevButton.IsEnabled = true;

            if (fp.RecordCount > 2)
                searchButton.IsEnabled = true;

            recordTextBox.Text = record;

            recNumTextBox.Text = currentRecord.ToString();
        }
        
        private void recNumTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (fp == null)
            {
                warningTextBox.Text += "\n No data loaded. \n";
                return;
            }

            string query = ((TextBox)sender).Text;
            if (query == string.Empty)
                return;

            int newrec;

            if (int.TryParse(((TextBox)sender).Text, out newrec))
            {
                if (newrec > 0 && newrec <= fp.RecordCount)
                    fp.GetRecord(newrec);
                else if (e.Key == Key.Enter)
                {
                    warningTextBox.Text += "\nNot that many records!\n";
                    recNumTextBox.Text = fp.CurrentRecord.ToString();
                    recNumTextBox.Focus();
                }
            }
            else
            {
                warningTextBox.Text += "\nYes, we're zero based whole numbers here.\n";
                fp.CurrentRecord.ToString();
                recNumTextBox.Focus();
            }
        }

        private void recordTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // display's highlighted text as ascii
            string text = recordTextBox.SelectedText;

            StringBuilder nvalue = new StringBuilder(1000);
            nvalue.Append("\t\t");
            foreach (var c in text)
            {
                nvalue.Append(" ");
                nvalue.Append(((int)c).ToString().PadLeft(3));
            }
            asciiTextBox.Text = nvalue.ToString();

            selectedCountLabel.Content = text.Length.ToString();
        }

        private void dateFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dateFormatComboBox.Text != "" && fp != null)
            {
                fp.DateFormat = (string)((ComboBoxItem)e.AddedItems[0]).Content;
                    fp.Refresh();
            }
        }

        private void convertSaksCheckBox_Checked(object sender, RoutedEventArgs e)
        {
//            if (fp != null )
//            {
//                // Set fade's option
//                fp.ConvertDoubleSaksToLong = (bool)((CheckBox)sender).IsChecked;

//                // Refresh the template with the option checked
////                fp.LoadTemplate(fp.TemplateFileName);
                
//                // Refresh our view
//                fp.Refresh();
//            }
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpMe hm = new HelpMe();
            hm.Show();
        }

        string maskFP;
        private void maskButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.CheckFileExists = true;
            fd.ShowDialog();
            if (!File.Exists(fd.FileName))
                warningTextBox.Text += ("\nMask file doesn't exist");

            FadePlus.FadePlus.ParseResults pr = FadePlus.FadePlus.ValidateTemplate(fd.FileName);

            if (pr.success == false)
            {
                warningTextBox.Text += ("\nNot a valid mask file\n");
                warningTextBox.Text += pr.Error;
                return;
            }

            FileInfo fi = new FileInfo(fd.FileName);
            maskFileTextBox.Text = fi.Name;
            maskFP = fd.FileName;
            CanLoad();
        }

        string dataFP;
        private void dataButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.CheckFileExists = true;
            fd.ShowDialog();
            if (!File.Exists(fd.FileName))
            {
                warningTextBox.Text += ("\nData file doesn't exist");
                return;
            }
            FileInfo fi = new FileInfo(fd.FileName);
            dataFileTextBox.Text = fi.Name;
            dataFP = fd.FileName;
            
            CanLoad();
        }

        private void CanLoad()
        {
            loadButton.IsEnabled = (File.Exists(dataFileTextBox.Text) 
                                 && File.Exists(maskFileTextBox.Text) 
                                 && maskFileTextBox.Text != dataFileTextBox.Text );
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            // Parse Mask
            fp = new FadePlus.FadePlus(maskFP);
            fp.Warning += new WarningEventHandler(fp_Warning);
            fp.RightEnd = rightEndTextBox.Text;
            fp.LeftEnd = leftEndTextBox.Text;
            fp.ShowEnds = (bool)endsCheckBox.IsChecked;
            fp.ShowAscii = (bool)asciiCheckBox.IsChecked;

            if (fp == null)
            {
                warningTextBox.Text += "\nCould not load template\n";
                return;
            }

            // load all records and Read the first
            if (!fp.AddDataFile(dataFP))
            {
                warningTextBox.Text += "\n Could not load data file\n";
                return;
            }

            this.Title = "Fade.Plus - " + new FileInfo(dataFP).Name;
            
            // populate the search field drop down
            searchFieldComboBox.Items.Clear();

            foreach (var field in fp.RecordTemplate.GetSearchFields())
	        {
		        searchFieldComboBox.Items.Add(field);
	        }

            // Display the details
            recordSizeLabel.Content = fp.RecordTemplate.RecLength.ToString();
            recordInfoLabel.Content = "/" + fp.RecordCount;
            maskTextBox.Text = fp.RecordTemplate.TemplateToString();

            fp.RecordChange += new RecordChangeEventHandler(RecordChange);
            fp.GetRecord(1);

        }

        /// <summary>
        /// Handle warnings thrown by FadePlus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="warning"></param>
        void fp_Warning(object sender, string warning)
        {
            warningTextBox.Text = warning;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (fp.CurrentRecord < fp.RecordCount)
                fp.GetRecord(fp.CurrentRecord + 1);
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            if (fp.CurrentRecord > 1)
                fp.GetRecord(fp.CurrentRecord - 1);
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchField rf = (SearchField)searchFieldComboBox.SelectedItem;
            if (rf == null)
                warningTextBox.Text = "\n Please choose a field to search on.\n";
            rf.Query = searchTextBox.Text;
            long result = fp.Search(rf);
            if (result < 1)
            {
                warningTextBox.Text += "\n No Result \n";
            }
            else { fp.GetRecord((int)result); }
        }

        private void likeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //fp.SubStringSearch = (bool)((CheckBox)sender).IsChecked;
        }

        private void ends_Toggle(object sender, RoutedEventArgs e)
        {
            if (fp == null)
                return;
            fp.RightEnd = rightEndTextBox.Text;
            fp.LeftEnd = leftEndTextBox.Text;
            fp.ShowEnds = (bool)((CheckBox)sender).IsChecked;
            fp.Refresh();
        }

        private void ascii_Toggle(object sender, RoutedEventArgs e)
        {
            if (fp == null)
                return;
            fp.ShowAscii = (bool)((CheckBox)sender).IsChecked;
            fp.Refresh();
        }

        /// <summary>
        /// Update the fadeplus module with new ends
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftEndTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(fp != null)
                if (fp.ShowEnds) 
                {
                    fp.RightEnd = rightEndTextBox.Text;
                    fp.LeftEnd = leftEndTextBox.Text;
                    fp.Refresh();
                }
        }

        /// <summary>
        /// Clear the warning text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void warningTextBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            warningTextBox.Text = "";
        }

        private void translateButton_Click(object sender, RoutedEventArgs e)
        {
            string idir = @"C:\Documents and Settings\fleminbr\Desktop\temp\BIAR Code Tables\";
            string odir = @"C:\Documents and Settings\fleminbr\Desktop\temp\BIAR ascii tables\";
            foreach ( var dir in Directory.GetFiles(idir, "*out"))
            {
                FileInfo di = new FileInfo(dir);
                
                string temp = di.Name;
                string dat = di.FullName.Replace(".out", ".dat");
                string asc = di.Name.Replace(".out", ".cvs");
                recordTextBox.Text += temp + Environment.NewLine;

                FadePlus.FadePlus nfp = new FadePlus.FadePlus(di.FullName);

                if (nfp == null)
                {
                    warningTextBox.Text += "\nCould not load template\n";
                    return;
                }

                nfp.RightEnd = "";
                nfp.LeftEnd = "";
                nfp.ShowAscii = false;
                nfp.ShowEnds = false;

                // load all records and Read the first
                if (!nfp.AddDataFile(dat))
                {
                    warningTextBox.Text += "\n Could not load data file" + dat + "\n";
                    continue;
                }

                FileStream fs = new FileStream(odir + asc, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);

                for (int i = 1; i <= nfp.RecordCount; i++)
                {
                    string commastring = nfp.GetRecord(i, true);
                    sw.WriteLine(commastring);
                }
                sw.Close();
                fs.Close();
            }
        }
    }
}
