using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using NLog.Config;

namespace NLog.Targets
{
    [NLogConfigurationItem]
    public class WpfRichTextBoxWordColoringRule
    {
        private Regex compiledRegex;

        public WpfRichTextBoxWordColoringRule()
        {
            this.FontColor = "Empty";
            this.BackgroundColor = "Empty";
        }

        public WpfRichTextBoxWordColoringRule(string text, string fontColor, string backgroundColor)
        {
            this.Text = text;
            this.FontColor = fontColor;
            this.BackgroundColor = backgroundColor;
            this.Style = FontStyles.Normal;
            this.Weight = FontWeights.Normal;
        }

        public WpfRichTextBoxWordColoringRule(string text, string textColor, string backgroundColor, FontStyle fontStyle, FontWeight fontWeight)
        {
            this.Text = text;
            this.FontColor = textColor;
            this.BackgroundColor = backgroundColor;
            this.Style = fontStyle;
            this.Weight = fontWeight;
        }

        public string Regex { get; set; }

        public string Text { get; set; }

        [DefaultValue(false)]
        public bool WholeWords { get; set; }

        [DefaultValue(false)]
        public bool IgnoreCase { get; set; }

        public FontStyle Style { get; set; }

        public FontWeight Weight { get; set; }

        public Regex CompiledRegex
        {
            get
            {
                if (this.compiledRegex == null)
                {
                    string regexpression = this.Regex;
                    if (regexpression == null && this.Text != null)
                    {
                        regexpression = System.Text.RegularExpressions.Regex.Escape(this.Text);
                        if (this.WholeWords)
                        {
                            regexpression = "\b" + regexpression + "\b";
                        }
                    }

                    RegexOptions regexOptions = RegexOptions.Compiled;
                    if (this.IgnoreCase)
                    {
                        regexOptions |= RegexOptions.IgnoreCase;
                    }

                    this.compiledRegex = new Regex(regexpression, regexOptions);
                }

                return this.compiledRegex;
            }
        }

        [DefaultValue("Empty")]
        public string FontColor { get; set; }

        [DefaultValue("Empty")]
        public string BackgroundColor { get; set; }
    }
}