
#if !NET_CF && !MONO && !SILVERLIGHT

namespace NLog.Targets
{
    using System.ComponentModel;
    using System.Windows;
    using NLog.Conditions;
    using NLog.Config;

    [NLogConfigurationItem]
    public class WpfRichTextBoxRowColoringRule
    {
        static WpfRichTextBoxRowColoringRule()
        {
            Default = new WpfRichTextBoxRowColoringRule();
        }

        public WpfRichTextBoxRowColoringRule()
            : this(null, "Empty", "Empty", FontStyles.Normal, FontWeights.Normal)
        {
        }

        public WpfRichTextBoxRowColoringRule(string condition, string fontColor, string backColor, FontStyle fontStyle, FontWeight fontWeight)
        {
            this.Condition = condition;
            this.FontColor = fontColor;
            this.BackgroundColor = backColor;
            this.Style = fontStyle;
            this.Weight = fontWeight;
        }

        public WpfRichTextBoxRowColoringRule(string condition, string fontColor, string backColor)
        {
            this.Condition = condition;
            this.FontColor = fontColor;
            this.BackgroundColor = backColor;
            this.Style = FontStyles.Normal;
            this.Weight = FontWeights.Normal;
        }

        public static WpfRichTextBoxRowColoringRule Default { get; private set; }

        [RequiredParameter]
        public ConditionExpression Condition { get; set; }

        [DefaultValue("Empty")]
        public string FontColor { get; set; }

        [DefaultValue("Empty")]
        public string BackgroundColor { get; set; }

        public FontStyle Style { get; set; }

        public FontWeight Weight { get; set; }

        public bool CheckCondition(LogEventInfo logEvent)
        {
            return true.Equals(this.Condition.Evaluate(logEvent));
        }
    }
}
#endif
 