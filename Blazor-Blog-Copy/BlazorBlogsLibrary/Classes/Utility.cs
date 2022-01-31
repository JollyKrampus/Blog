using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BlazorBlogsLibrary.Classes
{
    internal class Utility
    {
        #region ConvertToText

        public static string ConvertToText(string sHTML)
        {
            var sContent = sHTML;
            sContent = sContent.Replace("<br />", Environment.NewLine);
            sContent = sContent.Replace("<br>", Environment.NewLine);
            sContent = FormatText(sContent, true);
            return StripTags(sContent, true);
        }

        #endregion

        #region FormatText

        public static string FormatText(string HTML, bool RetainSpace)
        {
            //Match all variants of <br> tag (<br>, <BR>, <br/>, including embedded space
            var brMatch = "\\s*<\\s*[bB][rR]\\s*/\\s*>\\s*";
            //Replace Tags by replacement String and return mofified string
            return Regex.Replace(HTML, brMatch, Environment.NewLine);
        }

        #endregion

        #region CreateRandomKey

        public static string CreateRandomKey(int KeyLength)
        {
            const string valid = "012389ABCDEFGHIJKLMN4567OPQRSTUVWXYZ";
            var res = new StringBuilder();
            var rnd = new Random();
            while (0 < KeyLength--) res.Append(valid[rnd.Next(valid.Length)]);
            return res.ToString();
        }

        #endregion

        #region CastToDate

        public static DateTime? CastToDate(string strDateTime)
        {
            DateTime? dtFinalDateTime = null;
            DateTime dtTempDateTime;
            if (DateTime.TryParse(strDateTime, out dtTempDateTime)) dtFinalDateTime = dtTempDateTime;

            return dtFinalDateTime;
        }

        #endregion

        #region CastToNullableInteger

        public static int? CastToNullableInteger(string strInteger)
        {
            int? dtFinalInteger = null;
            int intInteger;
            if (int.TryParse(strInteger, out intInteger)) dtFinalInteger = intInteger;

            return dtFinalInteger;
        }

        #endregion

        #region CastStringToPossibleNegativeOneInteger

        public static int CastStringToPossibleNegativeOneInteger(string strInteger)
        {
            var dtFinalInteger = -1;
            int intInteger;
            if (int.TryParse(strInteger, out intInteger)) dtFinalInteger = intInteger;

            return dtFinalInteger;
        }

        #endregion

        #region CastNullableIntegerToPossibleNegativeOneInteger

        public static int CastNullableIntegerToPossibleNegativeOneInteger(int? intInteger)
        {
            var dtFinalInteger = -1;
            if (intInteger != null) dtFinalInteger = Convert.ToInt32(intInteger);

            return dtFinalInteger;
        }

        #endregion

        #region StripTags

        public static string StripTags(string HTML, bool RetainSpace)
        {
            //Set up Replacement String
            string RepString;
            if (RetainSpace)
                RepString = " ";
            else
                RepString = "";

            //Replace Tags by replacement String and return mofified string
            return Regex.Replace(HTML, "<[^>]*>", RepString);
        }

        #endregion

        #region public static string CleanOutlookFontDefinitions(string HtmlContent)

        public static string CleanOutlookFontDefinitions(string HtmlContent)
        {
            // Strip out Font Definitions from Outlook emails
            var strResponse = "";

            // Get range
            var intStart = HtmlContent.IndexOf(@"/* Font Definitions */");
            if (intStart < 0) return strResponse;
            intStart = intStart - 5;
            var intStop = HtmlContent.IndexOf(@"-->", intStart + 28);
            if (intStop < 0) return strResponse;

            if (intStart >= intStop) return strResponse;

            // Strip contents
            strResponse = HtmlContent.Remove(intStart, intStop - intStart);

            // Strip empty Style tags
            strResponse = strResponse.Replace(@"<style><--></style>", "");

            return strResponse;
        }

        #endregion

        #region public static bool ValidateFileExtension(string strFileExtension)

        public static bool ValidateFileExtension(string strFileExtension)
        {
            strFileExtension = strFileExtension.Replace(".", "").ToLower();

            var colExtensions = new List<string>();
            colExtensions.Add("png");
            colExtensions.Add("gif");
            colExtensions.Add("jpg");
            colExtensions.Add("jpeg");
            colExtensions.Add("doc");
            colExtensions.Add("docx");
            colExtensions.Add("xls");
            colExtensions.Add("xlsx");
            colExtensions.Add("pdf");
            colExtensions.Add("zip");
            colExtensions.Add("txt");
            colExtensions.Add("mp3");
            colExtensions.Add("htm");
            colExtensions.Add("eml");
            colExtensions.Add("html");

            var result = (from extension in colExtensions
                where extension == strFileExtension
                select extension).FirstOrDefault();

            return result != null;
        }

        #endregion
    }
}