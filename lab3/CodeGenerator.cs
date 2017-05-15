using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace lab3 {
    class CodeGenerator {

        public List<string> code;
        public TypeChecker t;
        public int labelCounter;
        public VariableEnvironment curEnv = null, curParEnv = null;
        public bool isAss;

        // TODO Fix if/while statement counter to be translated to adresses which will be used by GOTOs

        public CodeGenerator(TypeChecker t) {
            this.t = t;
            code = new List<string>();
            labelCounter = 1;
            isAss = false;
            t.p.ast.generateCode(this);
            trac42();

            labelToAddress();
        }

        private void trac42() {
            ASTstmtFuncDecl main = t.getFunction("trac42");

            if (main == null)
                throw new Exception("trac42 function not declared.");

            List<string> init = new List<string>();

            init.Add("LINK");
            if(main.dataType == TokenType.INT)
                init.Add("DECL 1");
            init.Add("BSR trac42");
            if (main.dataType == TokenType.INT)
                init.Add("POP 1");
            init.Add("UNLINK");
            init.Add("END");

            code.InsertRange(0, init);
        }

        public void PushInt(int value) {
            code.Add("PUSHINT " + value);
        }

        public void IntOP(TokenType t) {
            switch (t) {
                case TokenType.ADDOP:
                    code.Add("ADD");
                    break;
                case TokenType.SUBOP:
                    code.Add("SUB");
                    break;
                case TokenType.DIVOP:
                    code.Add("DIV");
                    break;
                case TokenType.MULOP:
                    code.Add("MULT");
                    break;
                case TokenType.EQOP:
                    code.Add("EQINT");
                    break;
                case TokenType.LTOP:
                    code.Add("LTINT");
                    break;
                case TokenType.LEOP:
                    code.Add("LEINT");
                    break;
                case TokenType.ASSOP:
                    code.Add("ASSINT");
                    break;
            }
        }

        public void DeclareFunction(string name) {
            code.Add("[" + name.Trim() + "]");
        }

        public void Unary(TokenType t) {
            if (t == TokenType.NOTOP)
                code.Add("NOT");
        }

        public void declareVar(TokenType t) {
            if (t == TokenType.INT) {
                code.Add("DECL 1");
            }
        }

        private void labelToAddress() {
            int codeLine, x;
            Match match;

            Regex pattern = new Regex("(BSR |BRA |BRF )([a-zA-Z0-9_]+)");

            for (x = 0; x < code.Count; x++ ) {
                match = pattern.Match(code[x]);

                if (!match.Success)
                    continue;

                for (codeLine = 0; codeLine < code.Count; codeLine++) {
                    if (code[codeLine].Equals("[" + match.Groups[2].Value + "]")) {
                        code[x] = match.Groups[1].Value + codeLine;
                        break;
                    }
                }
            }

        }

        public int findParOffset(string name) {
            int i;

            if (curParEnv == null)
                throw new Exception("Parameter environement is null");

            for (i = curParEnv.var.Count(); i > 0; i--) {
                if (curParEnv.var[i].name.Equals(name))
                    return i+1;
            }
            return 0;
        }

        public int findLocalOffset(string name) {
            //BAD!
            
            int i = 1;
            VariableEnvironment iter = curEnv;

            while (iter != null) {
                foreach (Variable v in iter.var) {
                    if (v.name.Equals(name)) {
                        return -1 * i;
                    }
                    i++;
                }
                iter = iter.prev;
            }

            return 0;
        }

        public override string ToString() {
            string codeString = System.String.Empty;

            if (code.Count == 0)
                return "";

            codeString += 0 + " " + code[0];

            for (int i = 1; i < code.Count; i++) {
                codeString += "\n" + i + " " + code[i];
            }

            return codeString;
        }
    }
}
