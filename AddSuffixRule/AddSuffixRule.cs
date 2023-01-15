using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BatchRename
{
    public class AddSuffixRule : IRule
    {
        public RuleFormat Format { get; set; }

        public RuleParameterEditDialog ruleParameterEditDialog { get; set; }

        public AddSuffixRule()
        {
            Format = new RuleFormat();
        }

        public bool EditFormat()
        {
            return true;
        }

        public RuleParameterEditDialog EditDialog()
        {
            ruleParameterEditDialog = new RuleParameterEditDialog(Format, RuleName, new List<string>() { "Add characters as suffix" });
            ruleParameterEditDialog.addBtn.Click += AddBtn_Clicked;
            return ruleParameterEditDialog;
        }

        public string RuleName => "Add.Suffix.Rule";

        public string ReName(string name, bool isFile = true)
        {
            if (string.IsNullOrEmpty(name))
                return "";

            var Name = "";

            if (isFile)
            {
                var lastDot = name.LastIndexOf('.');
                if (lastDot == -1) return "";
                var extension = name.Substring(lastDot + 1);
                Name = $"{name.Substring(0, lastDot)}{Format.Result}.{extension}";
            }
            else
                Name = $"{name}{Format.Result}";

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
            string res = ruleParameterEditDialog.txtBox[0].Text;

            if (!string.IsNullOrEmpty(res))
            {
                ruleParameterEditDialog.RuleFormat.Result = res;
                ruleParameterEditDialog.DialogResult = true;
            }
        }

        public IRule Instance()
        {
            AddSuffixRule rule = new AddSuffixRule();
            rule.Format = (RuleFormat)this.Format.Clone();
            return rule;
        }

    }
}
