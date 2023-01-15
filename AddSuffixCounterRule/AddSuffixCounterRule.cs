using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BatchRename
{
    public class AddSuffixCounterRule : IRule
    {
        public RuleFormat Format { get; set; }

        public RuleParameterEditDialog ruleParameterEditDialog { get; set; }

        public AddSuffixCounterRule()
        {
            Format = new RuleFormat();
        }

        public bool EditFormat()
        {
            return true;
        }

        public RuleParameterEditDialog EditDialog()
        {
            ruleParameterEditDialog = new RuleParameterEditDialog(Format, RuleName, new List<string>() { "Start", "Step", "Number of digits" });
            ruleParameterEditDialog.addBtn.Click += AddBtn_Clicked;
            return ruleParameterEditDialog;
        }

        public string RuleName => "Add.Suffix.Counter.Rule";

        public int index = 0;

        public string ReName(string name, bool isFile = true)
        {

            if (string.IsNullOrWhiteSpace(name))
                return "";

            var Name = "";

            if (isFile)
            {
                var lastDot = name.LastIndexOf('.');
                if (lastDot == -1) return "";
                var extension = name.Substring(lastDot + 1);

                int start = 0, step = 0, digit = 0;
                bool isParsableForStart = Int32.TryParse(Format.Parameter[0], out start);
                bool isParsableForStep = Int32.TryParse(Format.Parameter[1], out step);
                bool isParsableForDigit = Int32.TryParse(Format.Parameter[2], out digit);

                if (isParsableForStart && isParsableForStep && isParsableForDigit)
                    Name = $"{name.Substring(0, lastDot)} {(start + (index++) * step).ToString().PadLeft(digit, '0')}.{extension}";
                else
                {
                    return "";
                }
            }
            else
                Name = name;

            return Name;
        }
        public RuleFormat GetFormat()
        {
            return Format;
        }

        public void SetFormat(RuleFormat ruleFormat)
        {
            Format = ruleFormat;
        }

        public void AddBtn_Clicked(object sender, RoutedEventArgs e)
        {
            List<String> parameters = ruleParameterEditDialog.txtBox.Select(x => x.Text).ToList();

            var res = (from parameter in parameters where string.IsNullOrEmpty(parameter) == true select parameter);
            if (!res.Any())
            {
                ruleParameterEditDialog.RuleFormat.Parameter = parameters;
                ruleParameterEditDialog.DialogResult = true;
            }
        }

        public IRule Instance()
        {
            AddSuffixCounterRule rule = new AddSuffixCounterRule();
            rule.Format = (RuleFormat)this.Format.Clone();
            return rule;
        }
    }
}
