using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab3 {

    /* PARSER */

    class Parser {
        LexicalAnalyzer l;
        public ASTstmtList ast;

        public Parser(LexicalAnalyzer l) {
            this.l = l;
            ast = stmtList();
        }

        public TreeNode createTree() {
            return ast.createTree();
        }

        ASTstmtList stmtList() {
            ASTstmtList sl = new ASTstmtList(l.lastPeekedLine);

            while (true) {
                TokenType t = l.peek();

                switch (t) {
                    case TokenType.VOID:
                    case TokenType.INT:
                    case TokenType.ID:
                    case TokenType.NUM:
                    case TokenType.LPAR:
                    case TokenType.RETURN:
                    case TokenType.WRITE:
                    case TokenType.READ:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.LBRACE: // stmt stmt_list
                        sl.list.Add(stmt());
                        break;

                    default:
                        return sl;
                }
            }
        }

        ASTstmt stmt() {
            switch (l.peek()) {
                case TokenType.VOID:
                case TokenType.INT: { // type ID decl_opt
                    Token t = l.eat(l.lastPeeked);
                    Token i = l.eat(TokenType.ID);
                    return declOpt(t, i);
                }

                case TokenType.NUM:
                case TokenType.ID:
                case TokenType.LPAR: { // expr SEMI
                    ASTstmt s = new ASTstmt(expr(), l.lastPeekedLine);
                    l.eat(TokenType.SEMI);
                    return s;
                }

                case TokenType.RETURN: { // RETURN return_opt SEMI
                    l.eat(TokenType.RETURN);
                    TokenType p = l.peek();
                    ASTexpr e = null;
                    if (p == TokenType.NUM || p == TokenType.ID || p == TokenType.LPAR)
                        e = expr();
                    ASTstmt s = new ASTstmtReturn(e, l.lastPeekedLine);
                    l.eat(TokenType.SEMI);
                    return s;
                }

                case TokenType.WRITE: { // WRITE expr SEMI
                    l.eat(TokenType.WRITE);
                    ASTstmt s = new ASTstmtWrite(expr(), l.lastPeekedLine);
                    l.eat(TokenType.SEMI);
                    return s;
                }


                case TokenType.READ: { // READ expr SEMI
                    l.eat(TokenType.READ);
                    ASTstmt s = new ASTstmtRead(expr(), l.lastPeekedLine);
                    l.eat(TokenType.SEMI);
                    return s;
                }

                case TokenType.IF:  { // IF LPAR expr RPAR stmt else_opt
                    l.eat(TokenType.IF);
                    l.eat(TokenType.LPAR);
                    ASTexpr e = expr();
                    l.eat(TokenType.RPAR);
                    ASTstmt s = stmt();
                    ASTstmt es = null;
                    if (l.peek() == TokenType.ELSE) {
                        l.eat(TokenType.ELSE);
                        es = stmt();
                    }
                    return new ASTstmtIf(e, s, es, l.lastPeekedLine);
                }

                case TokenType.WHILE: { // WHILE LPAR expr RPAR stmt
                    l.eat(TokenType.WHILE); 
                    l.eat(TokenType.LPAR);
                    ASTexpr e = expr();
                    l.eat(TokenType.RPAR);
                    ASTstmt s = stmt();
                    ASTstmtWhile w = new ASTstmtWhile(e, s, l.lastPeekedLine);
                    return w;
                }

                case TokenType.LBRACE: { // LBRACE stmt_list RBRACE
                    l.eat(TokenType.LBRACE);
                    ASTstmtList sl = stmtList();
                    l.eat(TokenType.RBRACE);
                    return sl;
                }

            }

            l.eat(TokenType.ERROR);
            return null;
        }

        ASTstmt declOpt(Token type, Token id) {
            switch (l.peek()) {
                case TokenType.ASSOP: { // ASSOP expr SEMI
                    l.eat(TokenType.ASSOP);
                    ASTexpr e = expr();
                    ASTstmtVarDecl s = new ASTstmtVarDecl(type.type, new ASTid(id.attr, l.lastPeekedLine), e, l.lastPeekedLine);
                    l.eat(TokenType.SEMI);
                    return s;
                }

                case TokenType.SEMI: { // SEMI
                    ASTstmtVarDecl s = new ASTstmtVarDecl(type.type, new ASTid(id.attr, l.lastPeekedLine), null, l.lastPeekedLine);
                    l.eat(TokenType.SEMI); 
                    return s;
                }

                case TokenType.LPAR:  { // LPAR par_list RPAR LBRACE stmt_list RBRACE
                    l.eat(TokenType.LPAR);
                    ASTparList pl = parList();
                    l.eat(TokenType.RPAR);
                    l.eat(TokenType.LBRACE);
                    ASTstmtList sl = stmtList();
                    l.eat(TokenType.RBRACE);
                    ASTstmtFuncDecl s = new ASTstmtFuncDecl(type.type, new ASTid(id.attr, l.lastPeekedLine), pl, sl, l.lastPeekedLine);
                    return s;
                }
            }

            l.eat(TokenType.ERROR);
            return null;
        }

        ASTparList parList() {
            ASTparList pl = new ASTparList(l.lastPeekedLine);
            while (true) { // par par_list_opt
                switch (l.peek()) {
                    case TokenType.VOID: // VOID
                        l.eat(TokenType.VOID);
                        break;

                    case TokenType.INT: // INT ID
                        l.eat(TokenType.INT);
                        Token i = l.eat(TokenType.ID);
                        pl.list.Add(new ASTpar(TokenType.INT, new ASTid(i.attr, l.lastPeekedLine), l.lastPeekedLine));
                        break;

                    case TokenType.RPAR:
                        break;

                    default:
                        l.eat(TokenType.ERROR);
                        break;
                        
                }

                if (l.peek() == TokenType.COMMA)
                    l.eat(TokenType.COMMA);
                else
                    break;
            }

            return pl;

        }

        ASTargList argList() {
            ASTargList al = new ASTargList(l.lastPeekedLine);
            while (true) { // expr arg_list_opt
                switch (l.peek()) {
                    case TokenType.NUM:
                    case TokenType.ID:
                    case TokenType.LPAR:
                        al.list.Add(expr());
                        break;
                }

                if (l.peek() == TokenType.COMMA)
                    l.eat(TokenType.COMMA);
                else
                    break;
            }

            return al;
        }

        ASTexpr expr() {
            return exprAss();
        }

        ASTexpr exprAss() {
            ASTexpr e = exprEq();
            if (l.peek() == TokenType.ASSOP) {
                l.eat(l.lastPeeked);
                return new ASTexpr(l.lastPeeked, e, exprAss(), l.lastPeekedLine);
            }
            return e;
        }

        ASTexpr exprEq() {
            ASTexpr e = exprComp();
            if (l.peek() == TokenType.EQOP) {
                l.eat(l.lastPeeked);
                return new ASTexpr(l.lastPeeked, e, exprEq(), l.lastPeekedLine);
            }
            return e;
        }

        ASTexpr exprComp() {
            ASTexpr e = exprAddSub();
            if (l.peek() == TokenType.LTOP || l.peek() == TokenType.LEOP) {
                l.eat(l.lastPeeked);
                return new ASTexpr(l.lastPeeked, e, exprComp(), l.lastPeekedLine);
            }
            return e;
        }

        ASTexpr exprAddSub() {
            ASTexpr e = exprMulDiv();
            if (l.peek() == TokenType.ADDOP || l.peek() == TokenType.SUBOP) {
                l.eat(l.lastPeeked);
                return new ASTexpr(l.lastPeeked, e, exprAddSub(), l.lastPeekedLine);
            }
            return e;
        }

        ASTexpr exprMulDiv() {
            ASTexpr e = exprNot();
            if (l.peek() == TokenType.MULOP || l.peek() == TokenType.DIVOP) {
                l.eat(l.lastPeeked);
                return new ASTexpr(l.lastPeeked, e, exprMulDiv(), l.lastPeekedLine);
            }
            return e;
        }

        ASTexpr exprNot() {
            if (l.peek() == TokenType.NOTOP) {
                l.eat(l.lastPeeked);
                return new ASTexpr(l.lastPeeked, null, exprValue(), l.lastPeekedLine);
            }
            return exprValue();
        }

        ASTexpr exprValue() {
            TokenType t = l.peek();
            switch (l.peek()) {
                case TokenType.NUM: // NUM
                    Token n = l.eat(TokenType.NUM);
                    return new ASTexprValue(new ASTint(Convert.ToInt32(n.attr), l.lastPeekedLine), l.lastPeekedLine);

                case TokenType.ID: // ID call_opt
                    Token i = l.eat(TokenType.ID);
                    if (l.peek() == TokenType.LPAR) {
                        l.eat(TokenType.LPAR);
                        ASTexprCall c = new ASTexprCall(new ASTid(i.attr, l.lastPeekedLine), argList(), l.lastPeekedLine);
                        l.eat(TokenType.RPAR);
                        return c;
                    }
                    return new ASTexprValue(new ASTid(i.attr, l.lastPeekedLine), l.lastPeekedLine);

                case TokenType.LPAR: // LPAR expr RPAR
                    l.eat(TokenType.LPAR);
                    ASTexpr e = expr();
                    l.eat(TokenType.RPAR);
                    return e;
            }

            l.eat(TokenType.ERROR);
            return null;
        }
    }
}
