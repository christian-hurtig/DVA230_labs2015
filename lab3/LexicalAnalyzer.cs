using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab3 {
    class LexicalAnalyzer {

        public List<Token> tokens = new List<Token>();
        public Dictionary<TokenType, string> tokenMatches = new Dictionary<TokenType, string>();
        public string code = "";
        public int index, lineNumber, columnNumber, lastPeekedLine;
        public TokenType lastPeeked;

        public LexicalAnalyzer(string code) {

            this.code = code;

            tokenMatches.Add(TokenType.RETURN, @"\G(return)(\W|$)");
            tokenMatches.Add(TokenType.IF, @"\G(if)(\W|$)");
            tokenMatches.Add(TokenType.ELSE, @"\G(else)(\W|$)");
            tokenMatches.Add(TokenType.WHILE, @"\G(while)(\W|$)");
            tokenMatches.Add(TokenType.WRITE, @"\G(write)(\W|$)");
            tokenMatches.Add(TokenType.READ, @"\G(read)(\W|$)");
            tokenMatches.Add(TokenType.VOID, @"\G(void)(\W|$)");
            tokenMatches.Add(TokenType.INT, @"\G(int)(\W|$)");
            tokenMatches.Add(TokenType.LBRACE, @"\G({)");
            tokenMatches.Add(TokenType.RBRACE, @"\G(})");
            tokenMatches.Add(TokenType.LPAR, @"\G(\()");
            tokenMatches.Add(TokenType.RPAR, @"\G(\))");
            tokenMatches.Add(TokenType.SEMI, @"\G(;)");
            tokenMatches.Add(TokenType.COMMA, @"\G(,)");
            tokenMatches.Add(TokenType.EQOP, @"\G(==)");
            tokenMatches.Add(TokenType.ASSOP, @"\G(=)");
            tokenMatches.Add(TokenType.ADDOP, @"\G(\+)");
            tokenMatches.Add(TokenType.SUBOP, @"\G(-)");
            tokenMatches.Add(TokenType.MULOP, @"\G(\*)");
            tokenMatches.Add(TokenType.DIVOP, @"\G(\/)");
            tokenMatches.Add(TokenType.NOTOP, @"\G(\!)");
            tokenMatches.Add(TokenType.LEOP, @"\G(<=)");
            tokenMatches.Add(TokenType.LTOP, @"\G(<)");
            tokenMatches.Add(TokenType.ID, @"\G([a-zA-Z_][a-zA-Z0-9_]*)");
            tokenMatches.Add(TokenType.NUM, @"\G([0-9]+)");
             
            tokens.Clear();
            index = 0;
            lineNumber = 1;
            columnNumber = 1;

            while (index < code.Length) {
                if (skip())
                    continue;
                tokens.Add(getNextToken());
            }
        }

        public bool skip() {
            Regex rgx = new Regex(@"\G(\s|\t|\n|\/\/.*|\/\*[\s\S]*\*\/)", RegexOptions.Multiline);
            MatchCollection matches = rgx.Matches(code, index);
            if (matches.Count > 0) {
                string match = matches[0].Groups[1].Value;
                int newLines = match.Split('\n').Length;
                index += match.Length;
                columnNumber += match.Length;
                if (newLines > 1) { 
                    lineNumber += newLines - 1;
                    columnNumber = match.Length - match.LastIndexOf('\n');
                }
                return true;
            }
            return false;
        }
        
        public Token getNextToken() {
            foreach (KeyValuePair<TokenType, string> pair in tokenMatches) {
                Regex rgx = new Regex(pair.Value);
                MatchCollection matches = rgx.Matches(code, index);
                if (matches.Count > 0) {
                    string match = matches[0].Groups[1].Value;
                    index += match.Length;
                    columnNumber += match.Length;
                    return new Token(pair.Key, match, lineNumber, columnNumber);
                }
            }

            throw new Exception("Unknown token at line " + lineNumber + ", column " + columnNumber);
        }

        public TokenType peek() {
            if (tokens.Count() == 0)
                lastPeeked = TokenType.EOF;
            else { 
                lastPeeked = tokens[0].type;
                lastPeekedLine = tokens[0].line;

            }
            return lastPeeked;
        }

        public Token eat(TokenType expected) {
            if (tokens.Count() == 0)
                throw new Exception("Unexpected end of file");
            Token t = tokens[0];
            if (t.type != expected)
                throw new Exception("Unexpected " + t.type + " at line " + t.line + ", column " + t.column);
            lastPeekedLine = t.line;
            tokens.RemoveAt(0);
            return t;
        }

    }
}
