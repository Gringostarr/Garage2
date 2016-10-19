﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Garage20.Utilities
{
    public class Utility
    {
        static public string RandomRegNum()
        {
            Random random = new Random();
            int number = random.Next(1, 999);
            string letters = "";

            for (int i = 0; i < 3; i++)
                letters += (char)random.Next('A', 'Z');
            return letters + number.ToString();
        }

        static public bool ValidRegNum(string regNum)
        {
            Regex regExp = new Regex("[a-zA-Z]{3}[0-9]{3}");
            return regExp.Match(regNum).Success;
        }
    }
}