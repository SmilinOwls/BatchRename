using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class RuleFactory
    {
        
        static Dictionary<string, IRule> _prototypes = null;

        public void Register(IRule prototype)
        {
            _prototypes.Add(prototype.RuleName, prototype);
        }

        public IRule RuleItem(RuleFormat rule)
        {
            return _prototypes[rule.Type];
        }

        private static RuleFactory? _instance = null;
        public static RuleFactory Instance()
        {
            if (_instance == null)
            {
                _instance = new RuleFactory();
            }

            return _instance;
        }

        private RuleFactory()
        {
           _prototypes = new Dictionary<string, IRule>();
        } 
    }
}
