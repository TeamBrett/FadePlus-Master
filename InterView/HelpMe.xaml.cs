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
using System.Windows.Shapes;

namespace InterView
{
    /// <summary>
    /// Interaction logic for HelpMe.xaml
    /// </summary>
    public partial class HelpMe : Window
    {
        public HelpMe()
        {
            InitializeComponent();
            this.textBox1.Text =
            "This program requires two files.\n" +
            "\t- Data File\n" +
            "\t- Template(Describes the data)\n" +
            "\nThe template can contain the following data types:\n" +
            "\t - CHAR\n" +
            "\t - VARCHAR\n" +
            "\t - SMALLINT\n" +
            "\t - INTEGER\n" +
            "\t - DOUBLE\n" +
            "\t - LONG\n" +
            "\t - DATETIME\n" +
            "\t - EBCDIC\n" +
            "\nThe following is an example of a template:\n" +
            "\tMSIS_ID                        at   0 for  20 type CHAR\n" +
            "\tADJ_IND                        at  20 for   2 type SMALLINT    NOTNULL\n" +
            "\tTOS                            at  22 for   2 type SMALLINT\n" +
            "\tTYPE_CLM                       at  24 for   2 type SMALLINT    NULL\n" +
            "\tDATE_PAID                      at  26 for   4 type INTEGER\n" +
            "\tAMT_PAID                       at  30 for   8 type DOUBLE\n\n" +
            "\tDATE_BEGIN                     at  38 for  14 type DATETIME \"YYYYMMDDHH24MISS\"\n" +
            "\tSEGMENT_IND                    at  52 for   4 type CHAR NOTNULL  SUB_IND\n" +
            "\tFILLER_FOR_SUB                 at  56 for 146 type FILLER\n" +
            "\n" +
            "\tSUB PARA\n" +
            "\tID_SEQUENCE                    at  53 for   9 type LONG\n" +
            "\tMY_SENTENCE                    at  62 for 141 type VARCHAR\n" +
            "\n" +
            "\tSUB SENT\n" + 
            "\tMY_SMALLER_SENTENCE            at  53 for  67 type VARCHAR\n" +
            "\tNORMAL_RECORD_FILLER           at 120 for  83 type CHAR\n" +
            "\n Notice the filler on the end of the first part and the SUB_IND on the field before.\n" +
            "\n These are used for dynamic templates where the layout changes \n" +
            "\n based on the data in the SUB_IND field.  It's not been tested, use at your own risk\n" +
            "\n\n For such files in the mean time you can make a different mask for each sub segment\n" +
            "\n with the same header on each, then load the template for your subsegment and search\n" +
            "\n on the indicator field.  That's the only field you'll be able to search on though\n" +
            "\t\t\tbrett.fleming@hp.com";
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
