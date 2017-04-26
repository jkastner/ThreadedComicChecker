using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ComicChecker;

namespace ComicCheckerControl
{
    /// <summary>
    /// Interaction logic for SelectHTMLWindow.xaml
    /// </summary>
    public partial class SelectHTMLWindow : Window
    {
        private readonly TargetSite _curSite;
        private string _startResult;
        private string _endResult;

        public SelectHTMLWindow(TargetSite curSite)
        {
            _curSite = curSite;
            InitializeComponent();
            var myText = AddLineNumbers(curSite.FullSiteContents);
            HTMLContentTextBox.Text = myText;
        }

        private String AddLineNumbers(string allText)
        {
            var allLines = allText.Split('\n');

            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < allLines.Length; index++)
            {
                int lineDisplay = index + 1;
                var curLine = allLines[index];
                sb.AppendLine(lineDisplay + ": " + curLine);
            }
            return sb.ToString();
        }

        private void FindUnique_Click(object sender, RoutedEventArgs e)
        {
            var selected = HTMLContentTextBox.SelectedText;
            string oneLineContents = _curSite.FullSiteContents.Replace("\n", "");

            var startIndex = oneLineContents.IndexOf(selected);
            int curLength = 1;
            while (startIndex-curLength>0)
            {
                var subString = oneLineContents.Substring(startIndex - curLength, curLength);
                var count = Regex.Matches(oneLineContents, subString).Count;
                if (count == 1)
                {
                    _startResult = subString;
                    _endResult = oneLineContents.Substring(startIndex + HTMLContentTextBox.SelectedText.Length, 1);
                    Result_TextBox.Text = "Start: " + _startResult + " End: " + _endResult;
                    ApplyButton.IsEnabled = true;
                    break;
                }
                curLength++;
            }


        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            _curSite.StartTag = _startResult;
            _curSite.EndTag = _endResult;
            this.Close();

        }
    }
}
