using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signum_Sharp.Modules
{
    internal class ModJSON
    {

        public static bool GetBooleanBetweenFromList(List<string> inputList, string startchar = "(", string endchar = ")")
        {
            try
            {
                for (int i = 0; i <= inputList.Count - 1; i++)
                {
                    string Entry = inputList[i];
                    if (Entry.Contains(startchar))
                        return GetBooleanBetween(Entry, startchar, endchar);
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static int GetIntegerBetweenFromList(List<string> inputList, string startchar = "(", string endchar = ")")
        {
            try
            {
                for (int i = 0; i <= inputList.Count - 1; i++)
                {
                    string Entry = inputList[i];
                    if (Entry.Contains(startchar))
                        return GetIntegerBetween(Entry, startchar, endchar);
                }

                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static ulong GetULongBetweenFromList(List<string> inputList, string startchar = "(", string endchar = ")")
        {
            try
            {
                for (int i = 0; i <= inputList.Count - 1; i++)
                {
                    string Entry = inputList[i];
                    if (Entry.Contains(startchar))
                        return GetULongBetween(Entry, startchar, endchar);
                }

                return 0UL;
            }
            catch (Exception ex)
            {
                return 0UL;
            }
        }
        public static double GetDoubleBetweenFromList(List<string> inputList, string startchar = "(", string endchar = ")")
        {
            try
            {
                for (int i = 0; i <= inputList.Count - 1; i++)
                {
                    string Entry = inputList[i];
                    if (Entry.Contains(startchar))
                        return GetDoubleBetween(Entry, startchar, endchar);
                }

                return 0.0;
            }
            catch (Exception ex)
            {
                return 0.0;
            }
        }
        public static DateTime GetDateBetweenFromList(List<string> inputList, string startchar = "(", string endchar = ")")
        {
            try
            {
                for (int i = 0; i <= inputList.Count - 1; i++)
                {
                    string Entry = inputList[i];
                    if (Entry.Contains(startchar))
                        return GetDateBetween(Entry, startchar, endchar);
                };
                
                return new DateTime(0001, 1, 1);

            }
            catch (Exception ex)
            {
                return new DateTime(0001, 1, 1);

            }
        }
        public static string GetStringBetweenFromList(List<string> inputList, string startchar = "(", string endchar = ")")
        {
            try
            {
                for (int i = 0; i <= inputList.Count - 1; i++)
                {
                    string Entry = inputList[i];
                    if (Entry.Contains(startchar))
                        return GetStringBetween(Entry, startchar, endchar);
                }

                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static bool GetBooleanBetween(string input, string startchar = "(", string endchar = ")", bool LastIdxOf = false)
        {

            // TODO: OUT from Between
            // If GetINISetting(E_Setting.InfoOut, False) Then
            // Dim Out As ClsOut = New ClsOut(Application.StartupPath)
            // Out.ErrorLog2File(Application.ProductName + "-error in SendMessages(): -> " + ex.Message)
            // End If

            if (input.Trim() != "")
            {
                if (input.Contains(startchar) & input.Contains(endchar))
                {
                    input = input.Substring(input.IndexOf(startchar) + startchar.Length);

                    if (LastIdxOf)
                        input = input.Remove(input.LastIndexOf(endchar));
                    else
                        input = input.Remove(input.IndexOf(endchar));

                    return Convert.ToBoolean(input);
                }
            }

            return false;
        }
        public static int GetIntegerBetween(string input, string startchar = "(", string endchar = ")", bool LastIdxOf = false)
        {

            // TODO: OUT from Between
            // If GetINISetting(E_Setting.InfoOut, False) Then
            // Dim Out As ClsOut = New ClsOut(Application.StartupPath)
            // Out.ErrorLog2File(Application.ProductName + "-error in SendMessages(): -> " + ex.Message)
            // End If

            if (input.Trim() != "")
            {
                if (input.Contains(startchar) & input.Contains(endchar))
                {
                    input = input.Substring(input.IndexOf(startchar) + startchar.Length);

                    if (LastIdxOf)
                        input = input.Remove(input.LastIndexOf(endchar));
                    else
                        input = input.Remove(input.IndexOf(endchar));

                    return Convert.ToInt32(input);
                }
            }

            return 0;
        }
        public static ulong GetULongBetween(string input, string startchar = "(", string endchar = ")", bool LastIdxOf = false)
        {

            // TODO: OUT from Between
            // If GetINISetting(E_Setting.InfoOut, False) Then
            // Dim Out As ClsOut = New ClsOut(Application.StartupPath)
            // Out.ErrorLog2File(Application.ProductName + "-error in SendMessages(): -> " + ex.Message)
            // End If

            if (input.Trim() != "")
            {
                if (input.Contains(startchar) & input.Contains(endchar))
                {
                    input = input.Substring(input.IndexOf(startchar) + startchar.Length);

                    if (LastIdxOf)
                        input = input.Remove(input.LastIndexOf(endchar));
                    else
                        input = input.Remove(input.IndexOf(endchar));

                    return Convert.ToUInt64(input);
                }
            }

            return 0UL;
        }
        public static double GetDoubleBetween(string input, string startchar = "(", string endchar = ")", bool LastIdxOf = false)
        {

            // TODO: OUT from Between
            // If GetINISetting(E_Setting.InfoOut, False) Then
            // Dim Out As ClsOut = New ClsOut(Application.StartupPath)
            // Out.ErrorLog2File(Application.ProductName + "-error in SendMessages(): -> " + ex.Message)
            // End If

            if (input.Trim() != "")
            {
                if (input.Contains(startchar) & input.Contains(endchar))
                {
                    input = input.Substring(input.IndexOf(startchar) + startchar.Length);

                    if (LastIdxOf)
                        input = input.Remove(input.LastIndexOf(endchar));
                    else
                        input = input.Remove(input.IndexOf(endchar));

                    return Convert.ToDouble(input);
                }
            }

            return 0.0;
        }
        public static DateTime GetDateBetween(string input, string startchar = "(", string endchar = ")", bool LastIdxOf = false)
        {

            // TODO: OUT from Between
            // If GetINISetting(E_Setting.InfoOut, False) Then
            // Dim Out As ClsOut = New ClsOut(Application.StartupPath)
            // Out.ErrorLog2File(Application.ProductName + "-error in SendMessages(): -> " + ex.Message)
            // End If

            if (input.Trim() != "")
            {
                if (input.Contains(startchar) & input.Contains(endchar))
                {
                    input = input.Substring(input.IndexOf(startchar) + startchar.Length);

                    if (LastIdxOf)
                        input = input.Remove(input.LastIndexOf(endchar));
                    else
                        input = input.Remove(input.IndexOf(endchar));

                    return Convert.ToDateTime(input);
                }
            };
            
            return new DateTime(0001, 1, 1);
        }

        /// <summary>
        /// Einen String aus einem Bereich auslesen
        /// </summary>
        /// <param name="input">Der String, aus dem ein Bereich ausgelesen werden soll</param>
        /// <param name="startchar">die Startmarkierung ab der gelesen werden soll</param>
        /// <param name="endchar">die Endmarkierung bis der gelesen werden soll</param>
        /// <param name="LastIdxOf">Legt fest, ob bis zum letzten endchar gelesen werden soll</param>
        /// <returns>Vorzugsweise ein Double , andernfalls z.b. ein Integer</returns>
        public static string GetStringBetween(string input, string startchar = "(", string endchar = ")", bool LastIdxOf = false)
        {

            // TODO: OUT from Between
            // If GetINISetting(E_Setting.InfoOut, False) Then
            // Dim Out As ClsOut = New ClsOut(Application.StartupPath)
            // Out.ErrorLog2File(Application.ProductName + "-error in SendMessages(): -> " + ex.Message)
            // End If

            if (input.Trim() != "")
            {
                if (input.Contains(startchar) & input.Contains(endchar))
                {
                    input = input.Substring(input.IndexOf(startchar) + startchar.Length);

                    if (LastIdxOf)
                        input = input.Remove(input.LastIndexOf(endchar));
                    else
                        input = input.Remove(input.IndexOf(endchar));

                    return input;
                }
            }

            return "";
        }

        public static List<string> Between2List(string Input, string startchar = "(", string endchar = ")")
        {
            string Output = "";
            string Rest = "";
            string Temp1 = "";
            string Temp2 = "";

            try
            {
                if (Input.Trim() != "")
                {
                    if (Input.Contains(startchar) & Input.Contains(endchar))
                    {

                        // Dim StartIdx As Integer = -1
                        // Dim EndIdx As Integer = -1
                        int CntIdx = 0;


                        List<int> StartList = CharCounterList(Input, startchar);
                        List<int> EndList = CharCounterList(Input, endchar);

                        List<int> FinalList = new List<int>();

                        FinalList.AddRange(StartList.ToArray());
                        FinalList.AddRange(EndList.ToArray());

                        FinalList.Sort();

                        int SplitIdx = -1;

                        foreach (int Idx in FinalList)
                        {
                            string Ch = Input.Substring(Idx, 1);

                            if (Ch == startchar)
                                CntIdx += 1;
                            else if (Ch == endchar)
                                CntIdx -= 1;

                            if (CntIdx == 0)
                            {
                                SplitIdx = Idx;
                                break;
                            }
                        }

                        if (SplitIdx != -1)
                            Temp1 = Input.Remove(SplitIdx);

                        Temp1 = Temp1.Remove(FinalList[0], 1);


                        try
                        {
                            Temp2 = Input.Replace(Temp1, "");
                        }
                        catch (Exception ex)
                        {
                        }

                        Output = Temp1;
                        Rest = Temp2;

                        try
                        {
                            Rest = Input.Replace(Output, "");
                        }
                        catch (Exception ex)
                        {
                        }


                        if (Output.Trim() == "")
                            return new List<string>
                            {
                                Output,
                                Rest
                            };
                else
                            return new List<string>
                            {
                                Output,
                                Rest
                            };
                    }
                }
            }
            catch (Exception ex)
            {
                return new List<string>();
            }

            return new List<string>();
        }

        public static List<int> CharCounterList(string Input, string StartChar)
        {
            int StartIdx = -1;
            List<int> StartList = new List<int>();

            while (!(StartIdx == 0))
            {
                if (!(StartIdx == 0))
                {
                    if (StartIdx == -1)
                        StartIdx = 1;

                    StartIdx = Input.IndexOf(StartChar, StartIdx);// Strings.InStr(StartIdx, Input, StartChar);

                    if (StartIdx > 0)
                    {
                        if (!(StartIdx == 0))
                        {
                            StartList.Add(StartIdx - 1);
                            StartIdx += 1;
                        }
                    }
                }
            }

            return StartList;
        }

    }
}
