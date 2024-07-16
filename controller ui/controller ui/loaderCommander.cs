using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.HashFunction.xxHash;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Reflection;

// TODO: 
//add space supprot in strings
//wait command
//find command
//read command
//if loops etc
//to bytes command
//functions? too much i think... probably goto will be enough

namespace controller_ui
{
    class Tokenizer
    {
        private (string, int)[] tokens;
        public int index { get; set; }
        public Tokenizer(string script)
        {
            int currentCodeline = 1;
            this.index = 0;
            // tokenize
            string[] temp = Regex.Split(script, @"(\W)");

            // correctly make string tokens
            List<(String, int)> tokens = new List<(string, int)>();
            bool isString = false;
            string computedString = "";
            foreach (string s in temp)
            {
                if (s == "\"")
                {
                    if (isString)
                    {
                        isString = false;
                        tokens.Add((computedString, currentCodeline));
                        computedString = "";
                    }
                    else
                    {
                        isString = true;
                    }
                    tokens.Add((s, currentCodeline));
                }
                else
                {
                    if (isString)
                    {
                        computedString += s;
                    }
                    else if (s == "\n")
                    {
                        currentCodeline += 1;
                    }
                    else if (s.Trim().Length != 0)
                    {
                        tokens.Add((s, currentCodeline));
                    }
                }
                this.tokens = tokens.ToArray();
            }
            MessageBox.Show(string.Join(" | ", this.tokens.Select(tok => tok.Item1)));
        }

        public void advanceTokenizer()
        {
            if (!finishedParse())
            {
                index += 1;
            }
        }

        public bool finishedParse()
        {
            return index >= tokens.Length;
        }

        public string currentToken()
        {
            if (finishedParse())
                return "";
            return tokens[index].Item1;
        }

        public int currentLine()
        {
            if (finishedParse())
                return tokens.Last().Item2;
            return this.tokens[index].Item2;
        }
    }

    abstract class loaderCommander
    {
        // tokenizer
        Tokenizer tokenizer;
        // defined variable. map: name => (type, value).
        private Dictionary<string, (string, object)> variables;
        private string[] functionNames = {"find", "write", "print", "checksum", "bytes", "wait"};
        /*// defined functions. map: name => function.
        private Dictionary<string, MethodInfo> functions;*/

        public loaderCommander()
        {
            this.variables = new Dictionary<string, (string, object)>();
            /*this.functions = new Dictionary<string, MethodInfo>();

            registerFunction("find", GetType().GetMethod("Find", BindingFlags.NonPublic | BindingFlags.Instance));
            registerFunction("write", GetType().GetMethod("Write", BindingFlags.NonPublic | BindingFlags.Instance));
            registerFunction("print", GetType().GetMethod("Print", BindingFlags.NonPublic | BindingFlags.Instance));
            registerFunction("checksum", GetType().GetMethod("Checksum", BindingFlags.NonPublic | BindingFlags.Instance));
            registerFunction("bytes", GetType().GetMethod("Bytes", BindingFlags.NonPublic | BindingFlags.Instance));
            registerFunction("wait", GetType().GetMethod("Wait", BindingFlags.NonPublic | BindingFlags.Instance));*/
        }
/*
        private void registerFunction(string name, MethodInfo func)
        {
            if (functions.ContainsKey(name))
                throw error("can't create function {0} - function named {0} already exists.",
                    name);
            functions.Add(name, func);
        }

        private (string, object) doFunction(string name, params object[] args)
        {
            if (!functions.ContainsKey(name))
                throw error("can't run function '{0}' - function '{0}' doesn't exist.",
                    name);
            functions[name].Invoke(this, args);
        }*/

        public void Run(string script)
        {
            // if script is empty, return
            if (script.Trim() == "")
                return;

            this.tokenizer = new Tokenizer(script);

            while (!tokenizer.finishedParse())
            {
                parseCommand();
            }
        }

        private void parseCommand()
        {
            string name = tokenizer.currentToken();
            if (this.functionNames.Contains(name))
            {
                parseFunctionCall();
            }
            else if (this.tokenizer.currentToken() == "if")
            {
                parseIf();
            }
            else if (this.tokenizer.currentToken() == "while")
            {
                parseWhile();
            }
            else
            {
                parseAssignment();
            }
        }

        private void parseWhile()
        {
            proccess("while");
            long cond = 1;
            var condIndex = this.tokenizer.index;

            while (cond != 0)
            {
                cond = toLong(this.parseExpression());
                proccess(":");
                // ignore the commands
                proccess("{");
                if (cond == 0)
                {
                    int barCount = 1;
                    while (barCount > 0)
                    {
                        if (tokenizer.currentToken() == "{")
                            barCount++;
                        else if (tokenizer.currentToken() == "}")
                            barCount--;
                        tokenizer.advanceTokenizer();
                    }
                    break;
                }
                else
                {
                    while (this.tokenizer.currentToken() != "}")
                        parseCommand();
                    this.tokenizer.index = condIndex;
                }
            }
        }

        private void parseIf()
        {
            proccess("if");
            long cond = toLong(this.parseExpression());
            proccess(":");
            // ignore the commands
            proccess("{");
            if (cond == 0)
            {
                int barCount = 1;
                while (barCount > 0)
                {
                    if (tokenizer.currentToken() == "{")
                        barCount++;
                    else if (tokenizer.currentToken() == "}")
                        barCount--;
                    tokenizer.advanceTokenizer();
                }
            }
            else
            {
                while (this.tokenizer.currentToken() != "}")
                    parseCommand();
                proccess("}");
            }

            // else if parse statement doesnt exist
            if (tokenizer.currentToken() != "else")
                return;

            /// parse else
            // skip the else statements
            if (cond != 0)
            {
                // can be a lot of else ifs
                while (this.tokenizer.currentToken() == "else")
                {
                    // advance to the start of the statements
                    while (this.tokenizer.currentToken() != "{")
                        this.tokenizer.advanceTokenizer();
                    proccess("{");
                    int barCount = 1;
                    // we know the else was finished when there are the same amount
                    // of brackets on both sides
                    while (barCount > 0)
                    {
                        if (tokenizer.currentToken() == "{")
                            barCount++;
                        else if (tokenizer.currentToken() == "}")
                            barCount--;
                        tokenizer.advanceTokenizer();
                    }
                }
            }
            // parse the else
            else
            {
                proccess("else");
                // else if
                if (this.tokenizer.currentToken() == "if")
                {
                    parseIf();
                    return;
                }

                // final else
                proccess(":");
                proccess("{");
                while (this.tokenizer.currentToken() != "}")
                    parseCommand();
                proccess("}");
            }
        }

        private long toLong((string, object) value)
        {
            if (value.Item1 == null)
                throw error("expected a number, got void");
            if (value.Item1 != "long")
                throw error("expected a number, got {0}", value.Item1);
            return (long)value.Item2;
        }

        private (string, object) parseFunctionCall()
        {
            switch (this.tokenizer.currentToken())
            {
                case "find":
                    proccess("find");
                    proccess("(");
                    Byte[] what = toBytes(parseExpression());
                    UInt64 address = 0;
                    bool gotAddress = false;
                    if (this.tokenizer.currentToken() == ",")
                    {
                        proccess(",");
                        address = (UInt64)parseLong();
                        gotAddress = true;
                    }
                    proccess(")");
                    return ("long", gotAddress ? findCommand(what, address) : findCommand(what));
                case "write":
                    proccess("write");
                    proccess("(");
                    what = toBytes(parseExpression());
                    proccess(",");
                    address = (UInt64)parseLong();
                    proccess(")");
                    writeMemory(address, what);
                    return ("void", null);
                case "print":
                    proccess("print");
                    proccess("(");
                    (string, object) printwhat = parseExpression();
                    proccess(")");
                    printCommand(printwhat);
                    return ("void", null);
                case "checksum":
                    proccess("checksum");
                    proccess("(");
                    what = toBytes(parseExpression());
                    proccess(")");
                    return ("long", checksumCommand(what));
                case "bytes":
                    proccess("bytes");
                    proccess("(");
                    long len = toLong(parseExpression());
                    proccess(",");
                    long number = toLong(parseExpression());
                    proccess(")");
                    return ("bytes", bytesCommand(len, number));
                case "wait":
                    proccess("wait");
                    proccess("until");
                    waitCommand();
                    return ("void", null);
                default:
                    throw error("'{0}' - no such command", tokenizer.currentToken());
            }
        }

        private void waitCommand()
        {
            int startIndex = this.tokenizer.index;
            (string, object) exp;
            long res;
            do
            {
                this.tokenizer.index = startIndex;
                exp = parseExpression();
                try // try casting to long (also boolean)
                {
                    res = (long)exp.Item2;
                }
                catch (Exception)
                {
                    throw error("expression of do wait must be of boolean (long) type, type {0} received", exp.Item1);
                }
            }
            while (res != 0);
        }

        private long checksumCommand(Byte[] bytes)
        {
            IxxHash xxHash = xxHashFactory.Instance.Create(new xxHashConfig { HashSizeInBits = 64 });
            return BitConverter.ToInt64(xxHash.ComputeHash(bytes).Hash);
        }

        private Exception error(String messageFormat, params object[] parameters)
        {
            throw new Exception(
                String.Format("syntax error in line {0}: ", this.tokenizer.currentLine()) +
                String.Format(messageFormat, parameters)
                );
        }

        private Exception error(String message)
        {
            throw new Exception(message);
        }

        private Byte[] bytesCommand(long len, long number)
        {
            return toBytes(("long", number)).Take((int) len).ToArray();
        }

        private Byte[] toBytes((string, object) value)
        {
            if (value.Item1 == "void")
            {
                return null;
            }
            if (value.Item1 == "bytes")
            {
                return (Byte[])value.Item2;
            }
            if (value.Item1 == "long")
            {
                Byte[] res = BitConverter.GetBytes((long)value.Item2);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(res);
                }
                return res;
            }
            return null;
        }

        private long parseLong()
        {
            string number = this.tokenizer.currentToken();
            long res;
            try
            {
                if (number.StartsWith("0x"))
                {
                    res = Convert.ToInt64(number[2..], 16);
                }
                else
                {
                    res = Convert.ToInt64(number, 10);
                }
            }
            catch
            {
                throw error("excpected a number got '{0}'.",
                    number);
            }
            this.tokenizer.advanceTokenizer();
            return res;
        }

        private (string, object) parseExpression()
        {
            List<string> expressions = new List<string>();
            string[] ops = { "+", "-", "*", "/", "=", "<", ">" };
            (string, object) temp1, temp2;
            string op;
            temp1 = parseTerm();
            string type = temp1.Item1;
            List<Byte> bytesRes = new List<byte>();    // result if the operation is on bytes

            // no operation
            if (!ops.Contains(this.tokenizer.currentToken()))
                return temp1;
            
            if (type == "long") expressions.Add(((long)temp1.Item2).ToString());
            if (type == "bytes") bytesRes.AddRange((Byte[]) temp1.Item2);

            while (ops.Contains(this.tokenizer.currentToken()))
            {
                op = this.tokenizer.currentToken();
                proccess(op);
                if (type == "long")
                    expressions.Add(op);
                temp2 = parseTerm();

                string rType = temp2.Item1;
                switch (type)
                {
                    case "long":
                        if (rType == "long")
                            expressions.Add(temp2.Item2.ToString());
                        else
                            throw error("operation 'long' {0} '{1}' is undefined",
                                op,
                                this.tokenizer.currentToken());
                        break;
                    case "bytes":
                        temp2 = computeBytesOp(bytesRes, op, temp2);
                        type = temp2.Item1;
                        if (type == "long")
                            expressions.Add(temp2.Item2.ToString());
                        else if (type == "bytes")
                            bytesRes = new List<Byte>((Byte[]) temp2.Item2);
                        break;
                    case "void":
                        throw error("can't do operation '{0}' with void type",
                            this.tokenizer.currentToken());;
                    default:
                        throw error("can't do operation '{0}' with error type",
                            this.tokenizer.currentToken());
                }
            }

            // return result
            if (type == "long") {
                long sum = Convert.ToInt64(new System.Data.DataTable().Compute(string.Join("", expressions), null));
                return ("long", sum);
            }
            if (type == "bytes")
            {
                return ("bytes", bytesRes.ToArray());
            }
            throw error("can't return error type");
        }

        private (string, object) computeBytesOp(List<Byte> bytesRes, string op, (string, object) temp2)
        {
            string rtype = temp2.Item1;
            object rvalue = temp2.Item2;
            switch (rtype)
            {
                case "long":
                    if (op == "*")
                    {
                        List<Byte> res = new List<Byte>();
                        for (long i = 0; i < (long) rvalue; i++)
                            res.AddRange(bytesRes);
                        return ("bytes", res.ToArray());
                    }
                    else throw error("operation 'bytes' {0} '{1}' is undefined",
                                op,
                                rtype);
                case "bytes":
                    if (op == "+")
                    {
                        bytesRes.AddRange((Byte[])rvalue);
                        return ("bytes", bytesRes.ToArray());
                    }
                    else if (op == "=")
                        return ("long", bytesRes.SequenceEqual((Byte[])rvalue));
                    else throw error("operation 'bytes' {0} '{1}' is undefined",
                                op,
                                rtype);
                default:
                    throw error("operation 'bytes' {0} 'error type' is undefined", op);
            }
        }

        private (string, object) parseTerm()
        {
            string term = this.tokenizer.currentToken();
            (string, object) value;
            // it's a function call
            if (this.functionNames.Contains(term))
            {
                return parseFunctionCall();
            }
            // it's a variable
            else if (this.variables.ContainsKey(term))
            {
                proccess(term);
                return this.variables[term];
            }
            // it's '(' expression ')'
            else if (term == "(")
            {
                proccess("(");
                value = parseExpression();
                proccess(")");
                return value;
            }
            // it's unary operator '-'
            else if (term == "-")
            {
                proccess("-");
                value = parseTerm();
                switch (value.Item1)
                {
                    case "bytes":
                        value.Item2 = ((Byte[])value.Item2).Select(val => -val).ToArray();
                        return value;
                    case "long":
                        value.Item2 = -(long)value.Item2;
                        return value;

                    default:
                        throw error("can't do unary operation '-' on value of type void");
                }
            }
            // it's unary operator '!'
            else if (term == "!")
            {
                proccess("!");
                value = parseTerm();
                switch (value.Item1)
                {
                    case "long":
                        value.Item2 = (long)value.Item2 == 0 ? (long)1 : (long)0;
                        return value;
                    case null:
                        throw error("can't do unary operation '!' on value of type void");
                    default:
                        throw error("can't do unary operation '!' on value of type {0}", value.Item1);
                }
            }
            // number, string, bytes (or error)
            else
            {
                // bytes
                if (term == "[")
                    return ("bytes", parseBytes());
                // string (bytes)
                if (term == "\"")
                    return ("bytes", parseString());
                // number
                if (isNumeric(term))
                    return ("long", parseLong());

                throw error("unknown token '{0}'", term);
            }
        }

        private bool isNumeric(string term)
        {
            int currentIndex = this.tokenizer.index;
            bool res = true;
            try
            {
                parseLong();
            }
            catch (Exception)
            {
                res = false;
            }
            this.tokenizer.index = currentIndex;
            return res;
        }

        private object parseBytes()
        {
            List<Byte> bytes = new List<byte>();
            (string, object) value;
            proccess("[");
            value = parseExpression();
            switch (value.Item1)
            {
                case "bytes":
                    foreach (Byte b in (Byte[])value.Item2)
                    {
                        bytes.Add(b);
                    }
                    break;
                case "long":
                    long num = (long)value.Item2;
                    if (num < 0 || num > 255)
                        throw error("bytes object can't accept values not in byte range (0-255): got {0}", num);
                    bytes.Add((byte)num);
                    break;
                case null:
                    throw error("can't convert 'void' value to bytes");
                default:
                    throw error("can't convert '{0}' value to bytes", value.Item1);
            }
            while (this.tokenizer.currentToken() == ",")
            {
                proccess(",");
                value = parseExpression();
                switch (value.Item1)
                {
                    case "bytes":
                        foreach (Byte b in (Byte[])value.Item2)
                        {
                            bytes.Add(b);
                        }
                        break;
                    case "long":
                        long num = (long)value.Item2;
                        if (num < 0 || num > 255)
                            throw error("bytes object can't accept values not in byte range (0-255): got {0}", num);
                        bytes.Add((byte)num);
                        break;
                    case null:
                        throw error("can't convert 'void' value to bytes");
                    default:
                        throw error("can't convert '{0}' value to bytes", value.Item1);
                }
            }
            proccess("]");
            return bytes.ToArray();
        }

        private Byte[] parseString()
        {
            proccess("\"");
            string str = this.tokenizer.currentToken();
            proccess(str);
            proccess("\"");
            return Encoding.ASCII.GetBytes(str);
        }

        private void parseAssignment()
        {
            string varName = tokenizer.currentToken();
            Regex variableRg = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$");
            if (!variableRg.IsMatch(varName))
                throw error("variable name must start with a letter and continue with letters and numbers, got '{0}'", varName);
            proccess(varName);
            proccess("=");
            (string, object) newValue = parseExpression();
            if (newValue.Item1 == null)
                throw error("variable can't be assigned a void value");
            this.variables[varName] = newValue;
        }

        private void proccess(string token)
        {
            if ( tokenizer.finishedParse() )
            {
                throw error("excpected '{0}' got end of script.", token);
            }
            if (this.tokenizer.currentToken() != token)
            {
                throw error("excpected '{0}' got '{1}'.", token, this.tokenizer.currentToken());
            }
            this.tokenizer.advanceTokenizer();
        }

        protected abstract void writeMemory(UInt64 address, byte[] data);
        protected abstract byte[] readMemory(UInt64 address, uint len);
        public abstract ulong findCommand(Byte[] what, ulong startAdress = 0);

        private void printCommand((string, object) what)
        {
            string type = what.Item1;
            object value = what.Item2;
            string msg = "";
            if (type == null)
            {
                throw error("can't print a null value; value must be string, long or bytes");
            }
            else if (type == "long")
            {
                msg = "long: " + ((long)value).ToString();
            }
            else if (type == "bytes")
            {
                msg = "bytes/string: " + BitConverter.ToString((Byte[])value);
            }
            var response = MessageBox.Show(msg + "\r\n\r\ncopy to clipboard?", "script output", MessageBoxButtons.YesNo);
            if (response == DialogResult.Yes)
            {
                Clipboard.SetText(msg);
            }
        }
    }
}