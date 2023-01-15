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
    public class ChangeExtensionRule : IRule
    {
        public RuleFormat Format { get; set; }

        public RuleParameterEditDialog ruleParameterEditDialog { get; set; }

        public ChangeExtensionRule()
        {
            Format = new RuleFormat();
        }

        public bool EditFormat()
        {
            return true;
        }

        public RuleParameterEditDialog EditDialog()
        {
            ruleParameterEditDialog = new RuleParameterEditDialog(Format, RuleName, new List<string>() { "Current extension to change (Leave this field blank if you want extension changes in all items)", "New extension to replace with" });
            ruleParameterEditDialog.addBtn.Click += AddBtn_Clicked;
            return ruleParameterEditDialog;
        }

        public string RuleName => "Change.Extension.Rule";

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

                if (string.IsNullOrEmpty(extension)) return "";
                if (Format.Parameter.Contains(extension) || Format.Parameter.Contains("All Items"))
                    Name = $"{name.Substring(0, lastDot)}.{Format.Result}";
                else
                    Name = $"{name.Substring(0, lastDot)}.{extension}";
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

            if (parameters[1].Length != 0)
            {
                var res = parameters[0].Trim();
                ruleParameterEditDialog.RuleFormat.Parameter.Add((res == null || res.Length == 0 || res == "All Items") ? "All Items" : res);
                ruleParameterEditDialog.RuleFormat.Result = parameters[1];
                ruleParameterEditDialog.DialogResult = true;
            }
        }

        public IRule Instance()
        {
            ChangeExtensionRule rule = new ChangeExtensionRule();
            rule.Format = (RuleFormat)this.Format.Clone();
            return rule;
        }
    }
}
