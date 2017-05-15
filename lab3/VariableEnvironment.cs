using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab3 {
    class VariableEnvironment {
        public List<Variable> var = new List<Variable>();
        public VariableEnvironment prev = null;

        public bool add(string name, TokenType dataType, int offset) {
            foreach (Variable v in var) {
                if (v.name.Equals(name))
                    return false;
            }
            var.Add(new Variable(name, dataType, offset));
            return true;
        }

        public TokenType find(string name) {
            foreach (Variable v in var) {
                if(v.name.Equals(name))
                    return v.tokenType;
            }
            return TokenType.NOPE;
        }
    }
}
