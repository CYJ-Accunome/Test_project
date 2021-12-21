using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_project
{
    public  class CBase
    {
        public int a;
        public CBase(int na)
        {
            a = na;
        }
        public int GetA() { return a; }
    };
    // CBase的派生类CDerive
    public class CDerive : CBase
     {
        public int b;
        public CDerive(int nb, int na):base(na)
        {
            b = nb;
            na = nb + 1;
        }
        public int GetB() { return b; }
    };

    public class Inheritance
    {
        public static void sss()
        {
            CDerive SNUM = new CDerive(10, 11);
            int a = SNUM.GetA();
            int b=SNUM.GetB();
        }
        
        
    }
}
