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
    public class AddPrefixRule : IRule
    {
        public RuleFormat Format { get; set; }

        public RuleParameterEditDialog ruleParameterEditDialog { get; set; }

        public AddPrefixRule()
        {
            Format = new RuleFormat();
        }

        public bool EditFormat()
        {
            return true;
        }

        public RuleParameterEditDialog EditDialog()
        {
            ruleParameterEditDialog = new RuleParameterEditDialog(Format, RuleName, new List<string>() { "Add characters as prefix" });
            ruleParameterEditDialog.addBtn.Click += AddBtn_Clicked;
            return ruleParameterEditDialog;
        }

        public string RuleName => "Add.Prefix.Rule";

        public string ReName(string name, bool isFile = true)
        {
            if (string.IsNullOrEmpty(name))
                return "";

            var Name = Format.Result + name;

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
            AddPrefixRule rule = new AddPrefixRule();
            rule.Format = (RuleFormat)this.Format.Clone();
            return rule;
        }

    }
}
