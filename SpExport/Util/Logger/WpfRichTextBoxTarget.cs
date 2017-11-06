using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using NLog.Config;
using Application = System.Windows.Application;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace NLog.Targets
{
    [Target("RichTextBox")]
    public sealed class WpfRichTextBoxTarget : TargetWithLayout
    {
        private static readonly TypeConverter ColorConverter = new ColorConverter();
        private int _lineCount;

        static WpfRichTextBoxTarget()
        {
            var rules = new List<WpfRichTextBoxRowColoringRule>
            {
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Fatal", "Fuchsia", "#00000000"),
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "#00000000"),
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "#00000000"),
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Info", "#FF0A8FC9", "#00000000"),
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "#00000000"),
                new WpfRichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "#00000000"),
            };

            DefaultRowColoringRules = rules.AsReadOnly();
        }

        public WpfRichTextBoxTarget(RichTextBox richTextBox)
        {
            WordColoringRules = new List<WpfRichTextBoxWordColoringRule>();
            RowColoringRules = new List<WpfRichTextBoxRowColoringRule>();
            ToolWindow = true;
            TargetRichTextBox = richTextBox; //(RichTextBox)App.LoggerConsole.FindName(ControlName);

        }

        public static ReadOnlyCollection<WpfRichTextBoxRowColoringRule> DefaultRowColoringRules { get; private set; }

      //  public string ControlName { get; set; }

        public string FormName { get; set; }

        [DefaultValue(false)]
        public bool UseDefaultRowColoringRules { get; set; }

        [ArrayParameter(typeof(WpfRichTextBoxRowColoringRule), "row-coloring")]
        public IList<WpfRichTextBoxRowColoringRule> RowColoringRules { get; private set; }

        [ArrayParameter(typeof(WpfRichTextBoxWordColoringRule), "word-coloring")]
        public IList<WpfRichTextBoxWordColoringRule> WordColoringRules { get; private set; }

        [DefaultValue(true)]
        public bool ToolWindow { get; set; }

        public bool ShowMinimized { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool AutoScroll { get; set; }

        public int MaxLines { get; set; }

        internal Form TargetForm { get; set; }

        internal RichTextBox TargetRichTextBox { get; set; }

        internal bool CreatedForm { get; set; }

        protected override void InitializeTarget()
        {
        }

        protected override void CloseTarget()
        {
            if (CreatedForm)
            {
                TargetForm.Invoke((FormCloseDelegate)TargetForm.Close);
                TargetForm = null;
            }
        }

        protected override void Write(LogEventInfo logEvent)
        {
            WpfRichTextBoxRowColoringRule matchingRule = null;

            foreach (WpfRichTextBoxRowColoringRule rr in RowColoringRules)
            {
                if (rr.CheckCondition(logEvent))
                {
                    matchingRule = rr;
                    break;
                }
            }

            if (UseDefaultRowColoringRules && matchingRule == null)
            {
                foreach (WpfRichTextBoxRowColoringRule rr in DefaultRowColoringRules)
                {
                    if (rr.CheckCondition(logEvent))
                    {
                        matchingRule = rr;
                        break;
                    }
                }
            }

            if (matchingRule == null)
            {
                matchingRule = WpfRichTextBoxRowColoringRule.Default;
            }

            string logMessage = Layout.Render(logEvent);

            if (Application.Current.Dispatcher.CheckAccess() == false)
                Application.Current.Dispatcher.Invoke(() => SendTheMessageToRichTextBox(logMessage, matchingRule));
            else
                SendTheMessageToRichTextBox(logMessage, matchingRule);
        }

        private static Color GetColorFromString(string color, Brush defaultColor)
        {
            //  Brushes.DeepSkyBlue

            if (defaultColor == null)
                return (Color)ColorConverter.ConvertFromString("White");

            if (color == "Empty")
                return (Color)ColorConverter.ConvertFromString("White");

            return (Color)ColorConverter.ConvertFromString(color);
        }

        // -- END https://nlog.codeplex.com/workitem/6272

        private void SendTheMessageToRichTextBox(string logMessage, WpfRichTextBoxRowColoringRule rule)
        {
            RichTextBox rtbx = TargetRichTextBox;

            var tr = new TextRange(rtbx.Document.ContentEnd, rtbx.Document.ContentEnd) { Text = logMessage + "\r" };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                new SolidColorBrush(GetColorFromString(rule.FontColor,
                    (Brush)tr.GetPropertyValue(TextElement.ForegroundProperty)))
                );
            tr.ApplyPropertyValue(TextElement.BackgroundProperty,
                new SolidColorBrush(GetColorFromString(rule.BackgroundColor,
                    (Brush)tr.GetPropertyValue(TextElement.BackgroundProperty)))
                );
            tr.ApplyPropertyValue(TextElement.FontStyleProperty, rule.Style);
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, rule.Weight);

            if (MaxLines > 0)
            {
                _lineCount++;
                if (_lineCount > MaxLines)
                {
                    tr = new TextRange(rtbx.Document.ContentStart, rtbx.Document.ContentEnd);
                    tr.Text.Remove(0, tr.Text.IndexOf('\n'));
                    _lineCount--;
                }
            }

            if (AutoScroll)
            {
                rtbx.ScrollToEnd();
            }
        }

        private delegate void DelSendTheMessageToRichTextBox(string logMessage, WpfRichTextBoxRowColoringRule rule);

        private delegate void FormCloseDelegate();
    }
}