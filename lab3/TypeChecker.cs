using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab3 {

    class TypeChecker {
        public Parser p;
        public List<VariableEnvironment> environments = new List<VariableEnvironment>();
        public List<ASTstmtFuncDecl> functions = new List<ASTstmtFuncDecl>();
        public TokenType returnType;
        public int offsetCount;

        public TypeChecker(Parser p) {
            this.p = p;
            p.ast.typeCheck(this);
        }

        public VariableEnvironment addEnvironment() {
            VariableEnvironment env = new VariableEnvironment();
            if (environments.Count > 0)
                env.prev = environments[environments.Count - 1];
            environments.Add(env);
            return env;
        }

        public void exitEnvironment() {
            offsetCount -= environments[environments.Count-1].var.Count;
            environments.RemoveAt(environments.Count-1);
        }

        public bool declareVariable(string name, TokenType dataType) {
            offsetCount++;
            return environments[environments.Count - 1].add(name, dataType, offsetCount);
        }

        public TokenType getVariable(string name) {
            for (int i = environments.Count - 1; i >= 0; i--) {
                TokenType t = environments[i].find(name);
                if (t != TokenType.NOPE)
                    return t;
            }
            return TokenType.NOPE;
        }

        public bool declareFunction(ASTstmtFuncDecl func) {
            foreach (ASTstmtFuncDecl f in functions)
                if (func.id.name == f.id.name)
                    return false;
            functions.Add(func);
            returnType = TokenType.VOID;
            return true;
        }

        public bool findFunction(string name) {
            foreach (ASTstmtFuncDecl f in functions)
                if (name == f.id.name)
                    return true;
            return false;
        }

        public ASTstmtFuncDecl getFunction(string name) {
            foreach (ASTstmtFuncDecl f in functions)
                if (name == f.id.name)
                    return f;
            return null;
        }

        public ASTstmtFuncDecl getCurrentFunction() {
            if (functions.Count == 0 || environments.Count == 0)
                return null;
            return functions[functions.Count - 1];
        }
    }
}
