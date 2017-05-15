using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab3 {

    // AST node

    class ASTobject { 
        public int line;
        public ASTobject(int line) {
            this.line = line;
        }
        public virtual TreeNode createTree() { return new TreeNode("Not implemented"); }
        public virtual TokenType typeCheck(TypeChecker t) { return TokenType.ERROR; }
        public virtual void generateCode(CodeGenerator g) { return; }
    }

    // Statement list

    class ASTstmtList : ASTstmt {   
        public List<ASTstmt> list = new List<ASTstmt>();
        public VariableEnvironment env = new VariableEnvironment();


        public ASTstmtList(int line) : base(null, line) { }

        public override TreeNode createTree() {
            TreeNode n = new TreeNode("Statement list");
            foreach (ASTstmt s in list)
                n.Nodes.Add(s.createTree());
            return n;
        }

        public override TokenType typeCheck(TypeChecker t) {

            // New environment
            env = t.addEnvironment();

            // Check statements
            foreach (ASTstmt s in list)
                s.typeCheck(t);

            // Leave environment
            t.exitEnvironment();

            return TokenType.NOPE;
        }

        public override void generateCode(CodeGenerator g) {
            g.curEnv = env;
            g.t.addEnvironment();

            foreach (ASTstmt s in list)
                s.generateCode(g);

            //Assumes all local variables are integers.
            if (g.t.environments[g.t.environments.Count - 1].var.Count > 0)
                g.code.Add("POP " + g.t.environments[g.t.environments.Count - 1].var.Count);

            g.t.exitEnvironment();
        }
    }

    // Statement

    class ASTstmt : ASTobject {
        public ASTexpr expr;

        public ASTstmt(ASTexpr expr, int line) : base(line) {
            this.expr = expr;
        }

        public override TreeNode createTree() {
            TreeNode n = new TreeNode("Expression statement");
            n.Nodes.Add(expr.createTree());
            return n;
        }

        public override TokenType typeCheck(TypeChecker t) {
            return expr.typeCheck(t);
        }

        public override void generateCode(CodeGenerator g) {
            expr.generateCode(g);
        }

    }

    // Return statement

    class ASTstmtReturn : ASTstmt {
        public ASTstmtReturn(ASTexpr expr, int line) : base(expr, line) {
            this.expr = expr;
        }

        public override TreeNode createTree() {
            TreeNode n = new TreeNode("Return statement");
            if (expr != null)
                n.Nodes.Add(expr.createTree());
            return n;
        }

        public override TokenType typeCheck(TypeChecker t) {

            ASTstmtFuncDecl func = t.getCurrentFunction();

            // No function?
            if (func == null)
                throw new Exception("Return without a function at line " + line);

            // If there's an expression, check if it's the same type as the function
            if (expr != null) {
                if (expr.typeCheck(t) != func.dataType)
                    throw new Exception("Invalid return type at line " + line);
            } else if (func.dataType != TokenType.VOID) // If no expression, check if function is a void
                throw new Exception("Invalid return type at line " + line);

            // Store the return type in the typechecker, this is used at the end of FuncDecl
            t.returnType = func.dataType;

            return t.returnType;
        }

        public override void generateCode(CodeGenerator g) {
            if (expr != null) {
                g.code.Add("LVAL " + (g.curParEnv.var.Count+2) + "(FP)");
                expr.generateCode(g);
                //Assumes int return value
                g.code.Add("ASSINT");
            }
        }
    }

    // Write statement

    class ASTstmtWrite : ASTstmt {
        public ASTstmtWrite(ASTexpr expr, int line) : base(expr, line) {
            this.expr = expr;
        }

        public override TreeNode createTree() {
            TreeNode n = new TreeNode("Write call");
            n.Nodes.Add(expr.createTree());
            return n;
        }

        public override TokenType typeCheck(TypeChecker t) {
            if (expr.typeCheck(t) != TokenType.INT)
                throw new Exception("Invalid expression type for Write statement at line " + line);

            return TokenType.NOPE;
        }

        public override void generateCode(CodeGenerator g) {
            expr.generateCode(g);
            g.code.Add("WRITEINT");
            g.code.Add("POP 1");
        }
    }

    // Read statement

    class ASTstmtRead : ASTstmt {
        public ASTstmtRead(ASTexpr expr, int line) : base(expr, line) {
            this.expr = expr;
        }

        public override TreeNode createTree() {
            TreeNode n = new TreeNode("Read call");
            n.Nodes.Add(expr.createTree());
            return n;
        }

        public override TokenType typeCheck(TypeChecker t) {
            if (expr.typeCheck(t) != TokenType.INT)
                throw new Exception("Invalid expression type for Read statement at line " + line);

            return TokenType.NOPE;
        }

        public override void generateCode(CodeGenerator g) {
            g.isAss = true;
            expr.generateCode(g);
            g.code.Add("READINT");
            g.code.Add("ASSINT");
        }
    }

    // Variable declaration

    class ASTstmtVarDecl : ASTstmt {
        public TokenType dataType;
        public ASTid id;

        public ASTstmtVarDecl(TokenType dataType, ASTid id, ASTexpr expr, int line) : base(expr, line) {
            this.dataType = dataType;
            this.id = id;
            this.expr = expr;
        }

        public override TreeNode createTree() {
            TreeNode n = new TreeNode("Declare variable " + id.name);
            if (expr != null)
                n.Nodes.Add(expr.createTree());
            return n;
        }

        public override TokenType typeCheck(TypeChecker t) {

            // We can't have void variables
            if (dataType == TokenType.VOID)
                throw new Exception("Invalid variable type at line " + line);

            // Variable already exists?
            if (!t.declareVariable(id.name, dataType))
                throw new Exception("Variable redefinition at line " + line);

            // A function with this name exists?
            if (t.findFunction(id.name))
                throw new Exception("Variable declaration with same name as a function at line " + line);

            // The given expression is the same type?
            if (expr != null && expr.typeCheck(t) != dataType)
                throw new Exception("Invalid initialization value");

            return TokenType.NOPE;

        }

        public override void generateCode(CodeGenerator g) {
            bool success = g.t.declareVariable(id.name, dataType);
            
            if(success) {
                g.declareVar(dataType);

                if (expr != null) {
                    g.code.Add("LVAL -1(FP)");
                    expr.generateCode(g);
                    if (dataType == TokenType.INT)
                        g.code.Add("ASSINT");
                }
            }
        }
    }

    // Function declaration

    class ASTstmtFuncDecl : ASTstmt {
        public TokenType dataType;
        public ASTid id;
        public ASTparList parList;
        public ASTstmtList stmtList;
        public VariableEnvironment parEnv;

        public ASTstmtFuncDecl(TokenType dataType, ASTid id, ASTparList parList, ASTstmtList stmtList, int line) : base(null, line) {
            this.dataType = dataType;
            this.id = id;
            this.parList = parList;
            this.stmtList = stmtList;
        }

        public override TreeNode createTree() {
            TreeNode n = new TreeNode("Declare " + dataType + " function " + id.name);
            TreeNode np = n.Nodes.Add("Parameters");
            foreach (ASTpar p in parList.list)
                np.Nodes.Add(p.createTree());
            n.Nodes.Add(stmtList.createTree());
            return n;
        }

        public override TokenType typeCheck(TypeChecker t) {

            // Add function to typechecker list
            if (!t.declareFunction(this))
                throw new Exception("Function redeclaration at line " + line);

            // Add parameters to new scope
            parEnv = t.addEnvironment();
            foreach (ASTpar p in parList.list) {
                if (t.findFunction(p.id.name))
                    throw new Exception("Parameter with same name as a function at line " + line);
                if (!t.declareVariable(p.id.name, p.dataType))
                    throw new Exception("Parameter redefinition at line " + line);
            }
            t.offsetCount = 0;
            // Check statement list
            stmtList.typeCheck(t);

            // Check return type (fetched from a return statement inside the list)
            if (t.returnType != dataType)
                throw new Exception("Missing return for function at line " + line);

            t.exitEnvironment();

            return dataType;
        }

        public override void generateCode(CodeGenerator g) {
            
            g.code.Add("[" + id.name + "]");

            g.code.Add("LINK");
            
            g.curParEnv = parEnv;

            /*
            foreach (ASTpar p in parList.list)
                p.generateCode(g);
            */
            stmtList.generateCode(g);


            g.code.Add("UNLINK");
            g.code.Add("RTS");

        }
    }

    // If statement

    class ASTstmtIf : ASTstmt {
        ASTstmt stmt, stmtElse;
        public ASTstmtIf(ASTexpr expr, ASTstmt stmt, ASTstmt stmtElse, int line) : base(expr, line) {
            this.stmt = stmt;
            this.stmtElse = stmtElse;
        }

        public override TreeNode createTree() {
            TreeNode n = new TreeNode("If statement");
            n.Nodes.Add("Condition").Nodes.Add(expr.createTree());
            n.Nodes.Add(stmt.createTree());
            if (stmtElse != null)
                n.Nodes.Add("Else").Nodes.Add(stmtElse.createTree());
            return n;
        }

        public override TokenType typeCheck(TypeChecker t) {

            // Check expression
            if (expr.typeCheck(t) != TokenType.INT)
                throw new Exception("Invalid expression type for If statement at line " + line);

            // Check statements
            stmt.typeCheck(t);
            if (stmtElse != null)
                stmtElse.typeCheck(t);

            return TokenType.NOPE;

        }

        public override void generateCode(CodeGenerator g) {
            
            //start label
            g.code.Add("[" + g.labelCounter + "if]");
            
            // evaluate conditional and skip statement body if it's false
            expr.generateCode(g);
            g.code.Add("BRF " + g.labelCounter + "elseif");

            //Statement body, if this part is reached the statement was true and the else statement should be skipped.
            stmt.generateCode(g);
            g.code.Add("BRA " + g.labelCounter + "endif");

            g.code.Add("[" + g.labelCounter + "elseif]");

            if (stmtElse != null)
                stmtElse.generateCode(g);

            g.code.Add("[" + g.labelCounter + "endif]");

            g.labelCounter++;
        }
    }

    // While statement

    class ASTstmtWhile : ASTstmt {
        ASTstmt stmt;
        public ASTstmtWhile(ASTexpr expr, ASTstmt stmt, int line) : base(expr, line) {
            this.stmt = stmt;
        }

        public override TreeNode createTree() {
            TreeNode n = new TreeNode("While statement");
            n.Nodes.Add("Condition").Nodes.Add(expr.createTree());
            n.Nodes.Add(stmt.createTree());
            return n;
        }

        public override TokenType typeCheck(TypeChecker t) {

            // Check expression
            if (expr.typeCheck(t) != TokenType.INT)
                throw new Exception("Invalid expression type for While statement at line " + line);

            // Check statement
            stmt.typeCheck(t);

            return TokenType.NOPE;
        }

        public override void generateCode(CodeGenerator g) {
            //start Label
            g.code.Add("[" + g.labelCounter + "while]");

            // evaluate conditional and skip statement body if it's false
            expr.generateCode(g);
            g.code.Add("BRF " + g.labelCounter + "endwhile");

            stmt.generateCode(g);
            g.code.Add("BRA " + g.labelCounter + "while");

            g.code.Add("[" + g.labelCounter + "endwhile]");

        }
    }

    // Function parameter list

    class ASTparList : ASTobject {
        public ASTparList(int line) : base(line) { }
        public List<ASTpar> list = new List<ASTpar>();
    }

    // Function parameter

    class ASTpar : ASTobject {
        public TokenType dataType;
        public ASTid id;

        public ASTpar(TokenType type, ASTid id, int line) : base(line) {
            this.dataType = type;
            this.id = id;
        }
        public override TreeNode createTree() {
            return new TreeNode(dataType + " " + id.name);
        }

        public override void generateCode(CodeGenerator g) {
            return;
        }
    }

    // Function argument list

    class ASTargList : ASTobject {
        public ASTargList(int line) : base(line) { }
        public List<ASTexpr> list = new List<ASTexpr>();
    }

    // Number

    class ASTint : ASTobject {
        int value;

        public ASTint(int value, int line) : base(line) {
            this.value = value;
        }

        public override TreeNode createTree() {
            return new TreeNode("INT: " + value);
        }

        public override TokenType typeCheck(TypeChecker t) {
            return TokenType.INT;
        }

        public override void generateCode(CodeGenerator g) {
            g.PushInt(value);
        } 
    }

    // ID

    class ASTid : ASTobject {
        public string name;
        public TokenType dataType;

        public ASTid(string name, int line) : base(line) {
            this.name = name;
        }

        public override TreeNode createTree() {
            return new TreeNode("ID: " + name);
        }

        public override TokenType typeCheck(TypeChecker t) {
            dataType = t.getVariable(name);

            if (dataType == TokenType.NOPE)
                throw new Exception("Undefined variable " + name + " at line " + line);
           
            return dataType;
        }

        public override void generateCode(CodeGenerator g) {

            int i;
            VariableEnvironment iter = g.curEnv;
            string tracOP;

            if (g.isAss == true) {
                tracOP = "LVAL ";
                g.isAss = false;
            }
            else
                tracOP = "RVALINT ";

            for (i = g.curParEnv.var.Count; i > 0; i-- ) {
                if (g.curParEnv.var[i-1].name.Equals(name)) {
                    g.code.Add(tracOP + (g.curParEnv.var[i-1].offset+1) + "(FP)");
                    return;
                }
            }

            while (iter != null) {
                foreach(Variable v in iter.var) {
                    if(v.name.Equals(name)) {
                        g.code.Add(tracOP + (v.offset * -1) + "(FP)");
                        iter = null;
                        return;
                    }
                }
                iter = iter.prev;
            }
        }
    }

    // Expression

    class ASTexpr : ASTobject {
        TokenType op;
        TokenType returnType;
        ASTexpr left;
        ASTexpr right;

        public ASTexpr(TokenType op, ASTexpr left, ASTexpr right, int line) : base(line) {
            this.op = op;
            this.left = left;
            this.right = right;
        }

        public override TreeNode createTree() {
            TreeNode n = new TreeNode("Expr " + op);
            if (left != null)
                n.Nodes.Add(left.createTree());
            if (right != null)
                n.Nodes.Add(right.createTree());
            return n;
        }

        public override TokenType typeCheck(TypeChecker t) {

            if (op == TokenType.NOTOP) { // Unary
                TokenType rt = right.typeCheck(t);

                // Right side (next to !) can't be void
                if (rt == TokenType.VOID)
                    throw new Exception("Invalid type for operation " + op + " at line " + line);

                return rt;
            } else {
                TokenType lt, rt;
                lt = left.typeCheck(t);
                rt = right.typeCheck(t);

                // Left and right side must be equal and not void
                if (lt == TokenType.VOID || rt == TokenType.VOID || lt != rt)
                    throw new Exception("Invalid type for operation " + op + " at line " + line);

                returnType = lt;
                return lt;
            }
        }

        public override void generateCode(CodeGenerator g) {

            //TODO
            if (op == TokenType.NOTOP) { //Unary
                right.generateCode(g);
                g.Unary(op);
                return;
            } 
            

            if( returnType == TokenType.INT) {
                if (op == TokenType.ASSOP)
                    g.isAss = true;

                left.generateCode(g);
                right.generateCode(g);
                g.IntOP(op);
            }

        }
    }

    // Expression value

    class ASTexprValue : ASTexpr {
        ASTobject value;

        public ASTexprValue(ASTobject value, int line) : base(0, null, null, line) {
            this.value = value;
        }

        public override TreeNode createTree() {
            return value.createTree();
        }

        public override TokenType typeCheck(TypeChecker t) {
            return value.typeCheck(t);
        }

        public override void generateCode(CodeGenerator g) {
            value.generateCode(g);
        }
    }

    // Expression function call

    class ASTexprCall : ASTexpr {
        public ASTid id;
        public ASTargList list;

        public ASTexprCall(ASTid id, ASTargList list, int line) : base(0, null, null, line) {
            this.id = id;
            this.list = list;
        }

        public override TreeNode createTree() {
            TreeNode n = new TreeNode("Call to function " + id.name);
            foreach (ASTexpr e in list.list)
                n.Nodes.Add(e.createTree());
            return n;
        }

        public override TokenType typeCheck(TypeChecker t) {

            ASTstmtFuncDecl func = t.getFunction(id.name);

            // Check if function exists
            if (func == null)
                throw new Exception("Call to undefined function " + id.name + " at line " + line);

            // Check argument count
            if (list.list.Count != func.parList.list.Count)
                throw new Exception("Invalid argument count for function " + id.name + " at line " + line);

            // Check argument types
            for (int a = 0; a < list.list.Count; a++)
                if (func.parList.list[a].dataType != list.list[a].typeCheck(t))
                    throw new Exception("Invalid type for argument " + (a + 1) + " in call to function " + id.name + " at line " + line);

            return func.dataType;

        }

        public override void generateCode(CodeGenerator g) {
            ASTstmtFuncDecl func = g.t.getFunction(id.name);

            if (func.dataType == TokenType.INT)
                g.code.Add("DECL 1");

            foreach (ASTexpr arg in list.list) {
                arg.generateCode(g);
            }
            
            g.code.Add("BSR " + func.id.name);
            if(list.list.Count > 0)
                g.code.Add("POP " + list.list.Count);
        }
    }
}