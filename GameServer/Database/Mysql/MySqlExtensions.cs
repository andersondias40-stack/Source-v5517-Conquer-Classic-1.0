// * Created by ElmistRo
// * Copyright © 2010-2014
// * ElmistRo - Project

using System.Text.RegularExpressions;

namespace COServer
{
    public unsafe static class MySqlExtensions
    {
        public static string MySqlEscape(this string usString)
        {
            if (usString == null) return null;
            // SQL Encoding for MySQL Recommended here:
            // http://au.php.net/manual/en/function.mysql-real-escape-string.php
            // it escapes \r, \n, \x00, \x1a, baskslash, single quotes, and double quotes
            return Regex.Replace(usString, @"[\r\n\x00\x1a\\'""]", @"\$0");
        }
    }
}