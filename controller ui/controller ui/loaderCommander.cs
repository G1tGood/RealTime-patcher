using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace controller_ui
{
    abstract class loaderCommander
    {
        private Dictionary<string, (string, object)> variables;
        public loaderCommander()
        {
            this.variables = new Dictionary<string, (string, object)>();
        }
        private (bool,Int64) calc(string[] substrings)
        {
            Int64 sum=0;
            string[] ops = { "+", "-", "*", "/","%"};
            for (int i=0;i< substrings.Length; i++)
            {
                if (ops.Contains(substrings[i])) ;
                else if (substrings[i].All(char.IsNumber)) ;
                else if (variables.ContainsKey(substrings[i]))
                {
                    (string, object) val = variables[substrings[0]];
                    if (val.Item1 == "long")
                    {
                        substrings[i] = ((Int64)val.Item2).ToString();
                    }else{
                        return (false, 0);
                    }
                }
                else
                {
                    return (false,0);
                }
            }
            sum= (Int64)new System.Data.DataTable().Compute(string.Join("", substrings), null);
            return (true,sum);
            //if (substrings[i] == "-" && substrings[1].All(char.IsNumber))
            //{
            //    sum = -Int64.Parse(substrings[1]);
            //    i++;
            //}
            //else if (substrings[0].All(char.IsNumber))
            //{
            //    sum = Int64.Parse(substrings[0]);
            //}
            //else if (variables.ContainsKey(substrings[0]))
            //{
            //    (string, object) val = variables[substrings[0]];
            //    if (val.Item1 == "long")
            //    {
            //        sum = (Int64)val.Item2;

            //    }
            //}
        }
        private bool set_variable(string[] substrings)
        {
            (bool, Int64) calcRet= calc(substrings.Skip(2).ToArray());
            variables[substrings[0]] = ("long",calcRet.Item2);
            return calcRet.Item1;
        }

        private (bool,long) identifiyconstant(string constant){
            long res;
            try
            {
                if (constant.StartsWith("0x"))
                {
                    res = Convert.ToInt64(constant.Skip(2).ToString(), 16);
                }
                else
                {
                    res = Convert.ToInt64(constant, 10);
                }
            }
            catch
            {
                return (false,0);
            }
            return (true,res);
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
                        byte[] vv = BitConverter.GetBytes((UInt64)var.Item2);
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
                else if (substrings[i] == ",");
                else
                {
                    (bool,long) t= identifiyconstant(substrings[i]);
                    if (t.Item1)
                        data.AddRange(utils.longToBytes(t.Item2));
                    else
                        return null;
                }
            }
            return data.ToArray();
        }

        public abstract ulong find(string regex,uint index=0);
        protected abstract void writeMemmory(UInt64 address, byte[] data);
        protected abstract byte[] readMemmory(UInt64 address, uint len);
        private bool do_find(string[] substrings)
        {
            throw new NotImplementedException();
        }
        private bool do_write(string[] substrings)
        {
            byte[] data = enumrateByteRange(substrings.Skip(1).ToArray());
            if(data==null)
                return false;
            UInt64 address;
            if (variables.ContainsKey(substrings[0]))
            {
                (string, object) var = variables[substrings[0]];
                if (var.Item1 == "long")
                {
                    address=(UInt64)var.Item2;
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
        private bool do_wait(string[] substrings)
        {
            throw new NotImplementedException();
        }
        public bool Run(string command){
            string pattern = @"(\W)";
            string[] holes = { "", " ", "\t", };
            string[] substrings = Regex.Split(command, pattern).Where(item => !holes.Contains(item)).ToArray();

            switch (substrings[0])
            {
                case "find":
                    return do_find(substrings.Skip(1).ToArray());
                case "write":
                    return do_write(substrings.Skip(1).ToArray());
                case "wait":
                    return do_wait(substrings.Skip(1).ToArray());
                default:
                    if(Regex.IsMatch(substrings[0], "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_1234567890")&& substrings[1]=="=")
                    {
                        return set_variable(substrings);
                    }
                    else
                    {
                        return false;
                    }
            }
        }
    }
}
