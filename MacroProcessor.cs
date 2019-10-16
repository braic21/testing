using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NSLOutcomes.Utilities
{
    public class MacroProcessor
    {
        const char openMacro = '{';
        const char closeMacro = '}';

        public string Result
        {
            get;
            private set;
        }

        public bool IsValid
        {
            get;
            private set;
        }

        private MacroProcessor(string command)
        {
            // IsValid = true;

            if (IsValid = command != null)
            {
                int opened = command.Count(f => f == openMacro);
                int closed = command.Count(f => f == closeMacro);
                IsValid = opened == closed;

                int cmdStartCommand = command.LastIndexOf(openMacro);

                while (cmdStartCommand >= 0)
                {
                    int cmdEndCommand = command.IndexOf(closeMacro, cmdStartCommand);

                    if (cmdEndCommand < 0)
                    {
                        command = command.Substring(0, cmdStartCommand) + command.Substring(cmdStartCommand + 1);
                        IsValid = false;
                    }
                    else
                    {
                        string commandToCompare = command.Substring(cmdStartCommand + 1, cmdEndCommand - cmdStartCommand - 1);
                        string replaceTo = Compile(commandToCompare);

                        IsValid = IsValid && !String.IsNullOrEmpty(replaceTo);

                        command = command.Replace(openMacro + commandToCompare + closeMacro, replaceTo);
                    }

                    cmdStartCommand = command.LastIndexOf(openMacro);
                }
            }

            Result = command;
        }

        private string Compile(string originalValue)
        {
            string data = String.Empty;

            var lstDateKeywords = new List<string> { "TIME", "TODAY", "NOW", "QUARTER", "HALFHOUR", "HOUR" };

            if (lstDateKeywords.Any(originalValue.ToUpper().Contains))
            {
                data = CompileDate(originalValue);
            }

            return data;
        }

        private string CompileProcedure(string originalValue)
        {

            return null;
        }

        private string CompileDate(string originalValue)
        {
            string data = null;
            string value = originalValue;

            string tmp = null;

            try
            {
                string format;
                char part = new char();
                string name;
                int number = 0;

                int valuePos = value.IndexOfAny(new char[] { ':', '-', '+' });

                if (valuePos > 0)
                {
                    name = value.Substring(0, valuePos).ToUpper();
                    value = value.Substring(valuePos).Trim();

                    tmp = value.Substring(0, 1);

                    switch (tmp)
                    {
                        case ":":
                            format = GetDateFormatFromMacro(value);
                            data = CompileKeyWords(name, format);
                            break;
                        case "-":
                            part = Convert.ToChar(GetNumberAndCharFromMacro(value)[1]);
                            number = Convert.ToInt16(GetNumberAndCharFromMacro(value)[0]);
                            break;
                        case "+":
                            part = Convert.ToChar(GetNumberAndCharFromMacro(value)[1]);
                            number = Convert.ToInt16(GetNumberAndCharFromMacro(value)[0].Replace('+', ' ').Trim());
                            break;
                    }

                    if (tmp == "-" || tmp == "+")
                    {
                        valuePos = value.IndexOf(':', 1);
                        if (valuePos > 0)
                        {
                            value = value.Substring(valuePos).Trim();
                            format = GetDateFormatFromMacro(value);
                            data = CompileKeyWords(name, number, part, format);
                        }
                        else
                        {
                            data = CompileKeyWords(name, number, part);
                        }
                    }
                }
                else
                {
                    name = value.Trim();
                    data = CompileKeyWords(name);
                    value = "";
                }

            }
            catch
            {

            }

            return data;
        }

        private string[] GetNumberAndCharFromMacro(string value)
        {
            string[] numAndChar = new string[2];

            int valuePos = value.IndexOf(':');

            if (valuePos > 0)
            {
                numAndChar[1] = value.Substring(valuePos - 1, 1);
                numAndChar[0] = value.Substring(0, valuePos - 1);
            }
            else
            {
                numAndChar[0] = value.Substring(0, value.Length - 1);
                numAndChar[1] = value.Substring(value.Length - 1);
            }

            return numAndChar;
        }

        private string GetDateFormatFromMacro(string value)
        {
            return value.Substring(1);
        }

        private DateTime? GetDateFromKeyword(string word)
        {
            DateTime? data = null;

            switch (word.ToUpper())
            {
                case "NOW":
                    data = DateTime.Now;
                    break;
                case "TODAY":
                    data = DateTime.Today;
                    break;
                case "TIME":
                    data = DateTime.Now; ;
                    break;
                case "QUARTER":
                    var tmp = DateTime.Now;
                    data = new DateTime(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, ((int)(tmp.Minute / 15)) * 15, 0);
                    break;
                case "HALFHOUR":
                    tmp = DateTime.Now;
                    data = new DateTime(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, ((int)(tmp.Minute / 30)) * 30, 0);
                    break;
                case "HOUR":
                    tmp = DateTime.Now;
                    data = new DateTime(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, 0, 0);
                    break;

            }

            return data;
        }

        private DateTime? CalculateDateFromKeyword(string word, int number, char part)
        {
            DateTime? date = null;

            try
            {

                switch (part)
                {
                    case 's':
                        date = GetDateFromKeyword(word).Value.AddSeconds(number);
                        break;
                    case 'm':
                        date = GetDateFromKeyword(word).Value.AddMinutes(number);
                        break;
                    case 'H':
                    case 'h':
                        date = GetDateFromKeyword(word).Value.AddHours(number);
                        break;
                    case 'D':
                    case 'd':
                        date = GetDateFromKeyword(word).Value.AddDays(number);
                        break;
                    case 'M':
                        date = GetDateFromKeyword(word).Value.AddMonths(number);
                        break;
                    case 'y':
                        date = GetDateFromKeyword(word).Value.AddYears(number);
                        break;
                }

                return date;
            }

            catch
            {
                return null;
            }
        }

        private string CompileKeyWords(string word)
        {
            string data = null;

            switch (word.ToUpper())
            {
                case "NOW":
                case "QUARTER":
                case "HALFHOUR":
                case "HOUR":
                    data = GetDateFromKeyword(word).ToString();
                    break;
                case "TODAY":
                    data = GetDateFromKeyword(word).Value.ToString("d");
                    break;
                case "TIME":
                    data = GetDateFromKeyword(word).Value.ToLongTimeString();
                    break;
            }

            return data;
        }

        private string CompileKeyWords(string word, string format)
        {
            try
            {
                return GetDateFromKeyword(word).Value.ToString(format);
            }
            catch
            {
                return "Unsuported string format";
            }
        }

        private string CompileKeyWords(string word, int number, char part)
        {
            string data = null;

            switch (word.ToUpper())
            {
                case "NOW":
                    data = CalculateDateFromKeyword(word, number, part).ToString();
                    break;
                case "TODAY":
                    data = CalculateDateFromKeyword(word, number, part).Value.ToString("d");
                    break;
                case "TIME":
                    data = CalculateDateFromKeyword(word, number, part).Value.ToLongTimeString();
                    break;
            }

            return data;
        }

        private string CompileKeyWords(string word, int number, char part, string format)
        {
            try
            {
                return CalculateDateFromKeyword(word, number, part).Value.ToString(format);
            }
            catch
            {
                return "Unsuported string format";
            }
        }

        public static String Parse(string command)
        {
            return Parse(command, command);
        }

        public static String Parse(string command, out bool valid)
        {
            var p = new MacroProcessor(command);
            valid = p.IsValid;

            return p.Result;
        }

        public static String Parse(string command, string defaultValue)
        {
            var p = new MacroProcessor(command);
            if (p.IsValid)
                return p.Result;

            return defaultValue;
        }

        public static String Parse(string command, string defaultValue, out bool valid)
        {
            var p = new MacroProcessor(command);

            if (valid = p.IsValid)
                return p.Result;

            return defaultValue;
        }

        /// <summary>
        /// Vraca dio input stringa koji matcha zadani regex.
        /// </summary>
        /// <param name="sourceName">Input string</param>
        /// <param name="regex">Regex koji treba matchat.</param>
        /// <returns>Dio input stringa koji matcha zadani regex</returns>
        public static string RenameByRegex(string sourceName, string regex)
        {
            // tu se treba ugraditi konverzija iz source name u nekaj pomoću regex-a
            // privremeno vraćamo ByPattern - treba zakomentirati kasnije

            Match targetName = Regex.Match(sourceName, regex);

            if (String.IsNullOrEmpty(targetName.ToString()))
                return null;

            return targetName.ToString();
            //return RenameByPattern(sourceName, regex);
        }

        public static string RenameByPattern(string sourceName, string target)
        {
            if (target == null)
                return sourceName;

            // Ako pattern sadrzi delimiter (;) onda je target dio prije delimitera, a regex je dio iza
            // Ako pattern ne sadrzi delimiter (;) cijeli pattern je target
            string regString = null;
            int regDelimiter = target.IndexOf(";");
            if (regDelimiter >= 0)
            {
                target = target.Substring(0, regDelimiter);

                // Pokusaj matchati regex
                regString = RenameByRegex(sourceName, target.Substring(regDelimiter + 1, target.Length - regDelimiter - 1));

                // Ako filename ne zadovoljava regex, vrati nepromijenjeni filename
                // Inace postavi matchani regex kao novi sourceName
                if (regString == null) 
                    return sourceName;  
                else 
                    sourceName = regString;
            }
       
            string targetName;
            string fileNamePrefix;
            string fileNameSufix;

            // Iz targeta parsiraj target name i opcionalno ako postoje prefix i sufix
            var hasPrefixOrSufix = GetTargetParts(target, out targetName, out fileNamePrefix, out fileNameSufix);
            fileNamePrefix = fileNamePrefix ?? string.Empty;
            fileNameSufix = fileNameSufix ?? string.Empty;

            // TargetName ce bit null or empty ako MacroProcessor nije uspio procesirat macro u targetu?
            // Mislim da ovo nije dobro jer se dodaju i prefix i sufix na sourcename. Jel 100% da ce oni bit prazni ako je targetname null or empty?
            if (String.IsNullOrEmpty(targetName))
                targetName = sourceName;
            else
                targetName = ReplaceTarget(sourceName, targetName);

            return fileNamePrefix + targetName + fileNameSufix;
        }

        private static string ReplaceTarget(string sourceName, string targetName)
        {
            // Ako targetname ne sadrzi tocku, dodaj je
            bool dot = true;
            if (!targetName.Contains("."))
            {
                targetName += ".";
                dot = false;
            }

            int i = 0;
            int lastPos = 0;
            int countFlag = targetName.Count(f => f == '*');
            var newtarget = new StringBuilder();
            var filterTarget = new StringBuilder();

            // ako targetname sadrzi '*'
            int pos = targetName.IndexOf("*");
            if (pos >= 0)
            {
                filterTarget.Append(targetName.Substring(0, pos));
                targetName = targetName.Remove(0, pos);
                targetName = targetName.Replace("?", string.Empty);
                targetName = filterTarget.ToString() + targetName;
            }
            // inace, ako ne sadrzi "*" i sadrzi tocku, makni je
            else
            {
                if (!dot)
                    targetName = targetName.Replace(".", string.Empty);
            }

            // ide po targetnameu i trazi svaki iduci occurance nekog od ovih znakova
            while ((i = targetName.IndexOfAny(new char[] { '?', '*' }, i)) != -1)
            {
                // ako postoji '?' 
                if (targetName[i] == '?')
                {
                    // ako je '?' u sourceu do točke ili nema točke, u newtarget dodaj dio targeta do '?' i element iz sourceName-a na i-tom indeksu
                    if ((sourceName.Substring(0, sourceName.LastIndexOf('.')).Length >= i) || (sourceName.IndexOf('.') < 0 && sourceName.Length >= i))
                    {
                        newtarget.Append(targetName.Substring(lastPos, i)); //to do ili i - provjeriti
                        newtarget.Append(sourceName[i]);
                        lastPos = i;
                    }
                    // inace makni '?'
                    else
                    {
                        targetName = targetName.Replace("?", string.Empty);
                    }
                }

                // ako postoji vise '*' i nisu poslije tocke 
                if (targetName[i] == '*' & i < targetName.LastIndexOf('.') & countFlag > 1)
                {
                    if (lastPos == 0)
                    {
                        newtarget.Append(targetName.Substring(lastPos, i - lastPos));
                    }
                    else
                    {
                        newtarget.Append(targetName.Substring(lastPos + 1, i - lastPos - 1));
                    }

                    // IVICA: remove if ?
                    if (sourceName.Contains("."))
                        newtarget.Append(sourceName.Substring(0, sourceName.LastIndexOf('.')));
                    else newtarget.Append(sourceName.Substring(0));

                    lastPos = i;
                }

                // ako postoji jedan '*' i nije poslije tocke
                if (targetName[i] == '*' & i < targetName.LastIndexOf('.') & countFlag == 1)
                {
                    if (lastPos == 0)
                    {
                        newtarget.Append(targetName.Substring(lastPos, i - lastPos));
                    }
                    else
                    {
                        newtarget.Append(targetName.Substring(lastPos + 1, i - lastPos - 1));
                    }
                    if (sourceName.Contains("."))
                    {
                        newtarget.Append(sourceName.Substring(0, sourceName.LastIndexOf('.')));
                        newtarget.Append(targetName.Substring(i + 1, targetName.LastIndexOf('.') - i - 1));
                        if (dot)
                            newtarget.Append(targetName.Substring(targetName.LastIndexOf('.'),
                                                                  targetName.Length - targetName.LastIndexOf('.')));
                    }
                    else
                    {
                        newtarget.Append(sourceName.Substring(0));
                        if (dot)
                            newtarget.Append(targetName.Substring(targetName.LastIndexOf('.'),
                                                                  targetName.Length - targetName.LastIndexOf('.')));
                    }
                }


                // ako postoji vise '*' i nalaze se poslije tocke
                if (targetName[i] == '*' & i > targetName.LastIndexOf('.') & countFlag > 1)
                {
                    if (i - 1 != targetName.IndexOf('*', i - 1))
                    {
                        if (lastPos == 0)
                        {
                            newtarget.Append(targetName.Substring(lastPos, i - lastPos - 1));
                        }
                        else
                        {
                            newtarget.Append(targetName.Substring(lastPos + 1, i - lastPos - 2));
                        }
                    }
                    if (i + 1 != targetName.IndexOf('*', i + 1))
                    {
                        newtarget.Append(targetName.Substring(i - 1, targetName.Length - i));
                        newtarget.Append(sourceName.Substring(sourceName.LastIndexOf('.'),
                                                              sourceName.Length - sourceName.LastIndexOf('.')));
                        newtarget.Append(targetName.Substring(i + 1, targetName.IndexOf('*', i) - i - 1));
                    }

                    else
                    {
                        if (i - 1 != targetName.IndexOf('*', i - 1))
                        {
                            newtarget.Append(targetName.Substring(i - 1, 1));
                        }
                        newtarget.Append(sourceName.Substring(sourceName.LastIndexOf('.') + 1,
                                                              sourceName.Length - sourceName.LastIndexOf('.') - 1));
                    }

                    lastPos = i;
                }

                // ako postoji jedan '*' i nalazi se poslije tocke
                if (targetName[i] == '*' & i > targetName.LastIndexOf('.') & countFlag == 1)
                // if (targetName[i] == '*' & i >= dotPos & countFlag == 1)
                {
                    if (i - 1 != targetName.IndexOf('*', i - 1))
                    {
                        newtarget.Append(targetName.Substring(lastPos,
                                                              i - lastPos - 1));
                        newtarget.Append(targetName.Substring(i - 1, targetName.Length - i));
                    }

                    newtarget.Append(sourceName.Substring(sourceName.LastIndexOf('.') + 1, sourceName.Length - sourceName.LastIndexOf('.') - 1));
                    newtarget.Append(targetName.Substring(i + 1, targetName.Length - i - 1));
                }

                if (targetName[i] == '*')
                {
                    countFlag--;
                }
                i++;
            }

            //provjera da je index * veći od inexa ? da ne bi u petlju upao
            if (targetName.IndexOf('*') == -1 & targetName.IndexOf('?') != -1)
            {
                newtarget.Append(targetName.Substring(lastPos + 1, targetName.Length - lastPos - 1));
            }

            if (targetName.IndexOfAny(new char[] { '?', '*' }) == -1)
            {
                newtarget.Append(targetName);
            }

            targetName = newtarget.ToString().Replace("*", string.Empty);
            return targetName;
        }


        /// <summary>
        /// Parses macros from target and gets target name, prefix and suffix.
        /// Example: "TEST_[*].csv"
        /// fileNamePrefix = "TEST_"
        /// targetName = "*"
        /// fileNameSufix = ".csv"
        /// </summary>
        /// <returns>True if target contains prefix or suffix, false otherwise.</returns>
        public static bool GetTargetParts(string target, out string targetName, out string fileNamePrefix, out string fileNameSufix)
        {
            // Parse target to get targetName
            targetName = MacroProcessor.Parse(target);

            // Check if prefix or sufix exist
            fileNamePrefix = string.Empty;
            fileNameSufix = string.Empty;

            int patternStart = targetName.IndexOf("[");
            int patternEnd = targetName.IndexOf("]");

            // If prefix or suffix exist
            if (patternStart >= 0 && patternEnd >= 0)
            {
                fileNamePrefix = targetName.Substring(0, patternStart);
                fileNameSufix = targetName.Substring(patternEnd + 1);

                targetName = targetName.Substring(patternStart + 1).Trim();
                patternEnd = targetName.IndexOf("]");
                targetName = targetName.Substring(0, patternEnd).Trim();

                return true;
            }

            return false;
        }
    }
}
