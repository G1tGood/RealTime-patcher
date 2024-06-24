using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    abstract class loaderCommander
    {
        private Dictionary<string, (string, object)> variables;
        public loaderCommander()
        {
            this.variables = new Dictionary<string, (string, object)>();
        }
        
        private bool set_variable(string[] substrings)
        {
            if (substrings[2] == "[")
            {
                if (substrings[substrings.Length - 1] != "]")
                    return false;

                byte[] data = enumrateByteRange(substrings[3..(substrings.Length- 1)]);
                if (data == null)
                    return false;
                variables[substrings[0]] = ("bytes", data);
                return true;
            }
            (bool, Int64) calcRet= calc(substrings.Skip(2).ToArray());
            variables[substrings[0]] = ("long",calcRet.Item2);
            return calcRet.Item1;
        }
        public abstract ulong find(string regex,uint index=0);
        protected abstract void writeMemmory(UInt64 address, byte[] data);
        protected abstract byte[] readMemmory(UInt64 address, uint len);

        private bool do_write(string[] substrings)
        {
            byte[] data;
            if (substrings[1] == "[")
            {
                if (substrings[substrings.Length - 1] != "]")
                    return false;

                data = enumrateByteRange(substrings[2..(substrings.Length - 1)]);
                if (data == null)
                    return false;
            }
            else if (variables.ContainsKey(substrings[1]))
            {
                (string, object) var = variables[substrings[1]];
                if (var.Item1 == "long")
                {
                    byte[] vv = BitConverter.GetBytes((Int64)var.Item2);
                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(vv);
                    }
                    data=vv;
                }
                else if (var.Item1 == "bytes")
                    data=var.Item2 as byte[];
                else
                    return false;
            }
            else
            {
                return false;
            }
            UInt64 address;
            if (variables.ContainsKey(substrings[0]))
            {
                (string, object) var = variables[substrings[0]];
                if (var.Item1 == "long")
                {
                    address=Convert.ToUInt64(var.Item2);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                (bool,long) t= identifiyconstant(substrings[0]);
                if (!t.Item1)
                    return false;
                address = (ulong)t.Item2;
            }
            writeMemmory(address, data);
            return true;
        }

        private bool do_find(string[] substrings)
        {
            throw new NotImplementedException();
        }
        private bool do_wait(string[] substrings)
        {
            throw new NotImplementedException();
        }
        private string clearBlanks(string str)
        {
            return Regex.Replace(str, @"\s+", "");
        }
        public (bool,int) Run(string commands){
            string[] lines = commands.Split("\n");
            for (int i = 0; i < lines.Length; i++)
            {
                string pattern = @"(\W)";
                string[] holes = { "", " ", "\t", "\r", "\n" };
                string[] substrings = Regex.Split(lines[i], pattern).Where(item => !holes.Contains(item)).ToArray();
                if (substrings.Length == 0)
                    continue;
                switch (substrings[0])
                {
                    case "find":
                        if (!do_find(substrings[1..]))
                            return (false, i);
                        break;
                    case "write":
                        if (!do_write(substrings[1..]))
                            return (false, i);
                        break;
                    case "wait":
                        if (!do_wait(substrings[1..]))
                            return (false, i);
                        break;
                    case "print":
                        if (!do_print(substrings))
                            return (false, i);
                        break;
                    case "if":
                        if(lines[i+1].Trim()!="{")
                            return (false, i);
                        int j = Array.FindIndex(lines,i, x => x == "}");
                        if(j==-1)
                            return (false, i);
                        var t = this.Run(lines[(i+2)..j].Aggregate((current, next) => current + next));
                        i = j + 1;
                        if(!t.Item1)
                            return (false, t.Item2);
                        break;

                    default:
                        if (substrings.Length >= 3 && Regex.IsMatch(substrings[0], @"^[a-zA-Z0-9_]+$") && substrings[1] == "=")
                        {
                            if (!set_variable(substrings))
                                return (false, i);
                        }
                        else
                        {
                            return (false, i);
                        }
                        break;
                }
            }
            return (true,-1);
        }
        private bool isSpace(string str)
        {
            string[] holes = { "", " ", "\t", "\r", "\n" };
            return holes.Contains(str);
        }
        private bool do_print(string[] substrings)
        {
            if (substrings.Length >= 2)
            {
                if (substrings.Length == 2 && variables.ContainsKey(substrings[1]))
                {
                    (string, object) val = variables[substrings[1]];
                    if (val.Item1=="bytes")
                    {
                        System.Windows.Forms.MessageBox.Show(BitConverter.ToString(val.Item2 as byte[]));
                        return true;
                    }
                    System.Windows.Forms.MessageBox.Show(val.ToString());
                    return true;
                }
                (bool, long) t = calc(substrings.Skip(1).ToArray());
                if (!t.Item1) return false;
                System.Windows.Forms.MessageBox.Show(t.Item2.ToString());
                return true;
            }
            else
                return false;
        }
        private (bool, Int64) calc(string[] substrings)
        {
            Int64 sum = 0;
            string[] ops = { "+", "-", "*", "/", "%", "(", ")" };
            for (int i = 0; i < substrings.Length; i++)
            {
                if (ops.Contains(substrings[i]));
                else if (substrings[i].All(char.IsNumber));
                else if (variables.ContainsKey(substrings[i]))
                {
                    (string, object) val = variables[substrings[i]];
                    if (val.Item1 == "long")
                    {
                        substrings[i] = ((Int64)val.Item2).ToString();
                    }
                    else
                    {
                        return (false, 0);
                    }
                }
                else
                {
                    var t = identifiyconstant(substrings[i]);
                    if(!t.Item1)
                        return (false, 0);
                    substrings[i] = t.Item2.ToString();
                }
            }
            sum = Convert.ToInt64(new System.Data.DataTable().Compute(string.Join("", substrings), null));
            return (true, sum);
        }
        private (bool, long) identifiyconstant(string constant)
        {
            long res;
            try
            {
                if (constant.StartsWith("0x"))
                {
                    res = Convert.ToInt64(constant[2..], 16);
                }
                else
                {
                    res = Convert.ToInt64(constant, 10);
                }
            }
            catch
            {
                return (false, 0);
            }
            return (true, res);
        }
        private byte[] enumrateByteRange(string[] substrings)
        {
            List<byte> data = new List<byte>();
            for (int i = 0; i < substrings.Length; i++)
            {
                if (substrings[i] == "\"")
                {
                    for (i++; substrings[i] != "\"" && i < substrings.Length; i++)
                    {
                        data.AddRange(Encoding.ASCII.GetBytes(substrings[i]));
                    }
                    if (i == substrings.Length)
                        return null;
                }
                else if (variables.ContainsKey(substrings[i]))
                {
                    (string, object) var = variables[substrings[i]];
                    if (var.Item1 == "long")
                    {
                        byte[] vv = BitConverter.GetBytes((Int64)var.Item2);
                        if (!BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(vv);
                        }
                        data.AddRange(vv);
                    }
                    else if (var.Item1 == "bytes")
                    {
                        data.AddRange(var.Item2 as byte[]);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (substrings[i] == ",") ;
                else
                {
                    (bool, long) t = identifiyconstant(substrings[i]);
                    if (t.Item1)
                        data.Add(Convert.ToByte(t.Item2));
                    else
                        return null;
                }
            }
            return data.ToArray();
        }
    }
}



/*
 
x=5
x=x*x

y=2*x-(20+2*x)/2

print y

z=["this",0x20,"is",0x20,"working",0x20,]


print z
x=0x7FF669920040+y+1


write x [ z, "!!!"  ]
 
 */