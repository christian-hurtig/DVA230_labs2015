using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab3 {
    class Variable {
        public string name;
        public TokenType tokenType;
        public int offset;

        public Variable(string name, TokenType tokenType, int offset) {
            this.name = name;
            this.tokenType = tokenType;
            this.offset = offset;
        }
    }
}
