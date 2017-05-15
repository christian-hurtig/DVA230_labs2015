using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab3 {

    enum TokenType {
        RETURN,
        IF,
        ELSE,
        WHILE,
        WRITE,
        READ,
        VOID,
        INT,
        LBRACE,
        RBRACE,
        LPAR,
        RPAR,
        SEMI,
        COMMA,
        ASSOP,
        ADDOP,
        SUBOP,
        MULOP,
        DIVOP,
        NOTOP,
        EQOP,
        LTOP,
        LEOP,
        ID,
        NUM,
        ERROR,
        EOF,
        NOPE
    }

    class Token {
        public TokenType type;
        public string attr;
        public int line;
        public int column;

        public Token(TokenType type, string attr, int line, int column) {
            this.type = type;
            this.attr = attr;
            this.line = line;
            this.column = column;
        }
    }
}
