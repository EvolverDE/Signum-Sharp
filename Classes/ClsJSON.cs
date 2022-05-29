using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signum_Sharp.Classes
{
    public class ClsJSON
    {
        private List<object> RecursiveRest { get; set; } = new List<object>();

        public List<object> JSONRecursive(string Input)
        {
            Input = Input.Trim();

            if (Input.Length > 0)
            {
                List<object> U_List = new List<object>();

                if (Input[0] == '\"')
                {
                    if (Input.Contains(':'))
                    {
                        string Prop = Input.Remove(Input.IndexOf(":"));
                        string Val = Input.Substring(Input.IndexOf(":") + 1);

                        if (Input.Length > 0)
                        {
                            bool CheckBrackets = false;
                            bool CheckCurlys = false;

                            if (Val.Length > 2)
                            {
                                string T_Val = Val.Substring(0, 3);

                                if (T_Val == "[],")
                                    CheckBrackets = true;
                                else if (T_Val == "{},")
                                    CheckCurlys = true;
                            }

                            if (CheckBrackets)
                            {
                                string T_Val = Val.Substring(3);

                                U_List.Add(new List<object>
                                {
                                    Prop.Replace("\"", ""),
                                    ""
                                });
                                U_List.AddRange(JSONRecursive(T_Val).ToArray());
                            }
                            else if (CheckCurlys)
                            {
                                string T_Val = Val.Substring(3);

                                U_List.Add(new List<object>
                                {
                                    Prop.Replace("\"", ""),
                                    ""
                                });
                                U_List.AddRange(JSONRecursive(T_Val).ToArray());
                            }
                            else if (Val[0] == '{')
                            {
                                U_List.Add(new List<object>
                                {
                                    Prop.Replace("\"", ""),
                                    JSONRecursive(Val)
                                });

                                if (!(RecursiveRest.Count == 0))
                                {
                                    foreach (var RekuRest in RecursiveRest)
                                        U_List.Add(RekuRest);
                                    RecursiveRest = new List<object>();
                                }
                            }
                            else if (Val[0] == '[')
                            {
                                U_List.Add(new List<object>
                                {
                                    Prop.Replace("\"", ""),
                                    JSONRecursive(Val)
                                });

                                if (!(RecursiveRest.Count == 0))
                                {
                                    foreach (var RekuRest in RecursiveRest)
                                        U_List.Add(RekuRest);
                                    RecursiveRest = new List<object>();
                                }
                            }
                            else
                            {
                                char Brackedfound = '0';

                                foreach (var Cha in Input)
                                {
                                    Brackedfound = Cha;
                                    if (Brackedfound == '{' | Brackedfound == '[')
                                        break;
                                }


                                if (Brackedfound == '{')
                                {
                                    string T_Vals = Input.Remove(Input.IndexOf("{"));
                                    string T_Rest = Input.Substring(Input.IndexOf("{"));

                                    List<string> T_List = T_Vals.Split(',').ToList();
                                    string Key = "";

                                    for (int i = 0; i <= T_List.Count - 1; i++)
                                    {
                                        string TL = T_List[i];
                                        if (i == T_List.Count - 1)
                                            Key = TL.Split(':')[0].Replace("\"", "");
                                        else
                                            U_List.Add(new List<object>(TL.Replace("\"", "").Split(':')));
                                    }


                                    bool EmptyEntry = false;
                                    if (T_Rest.Contains("{},"))
                                    {
                                        T_Rest = T_Rest.Substring(T_Rest.IndexOf("{},") + 3);
                                        U_List.Add(new List<object>
                                        {
                                            Key,
                                            new List<object>()
                                        });
                                        EmptyEntry = true;
                                    }

                                    if (EmptyEntry)
                                    {
                                        List<object> T_Entrys = JSONRecursive(T_Rest);

                                        foreach (var T_Entry in T_Entrys)
                                            U_List.Add(T_Entry);
                                    }
                                    else
                                        U_List.Add(new List<object>
                                        {
                                            Key,
                                            JSONRecursive(T_Rest)
                                        });


                                    if (!(RecursiveRest.Count == 0))
                                    {
                                        foreach (var RekuRest in RecursiveRest)
                                            U_List.Add(RekuRest);
                                        RecursiveRest = new List<object>();
                                    }
                                }
                                else if (Brackedfound == '[')
                                {
                                    string T_Vals = Input.Remove(Input.IndexOf("["));
                                    string T_Rest = Input.Substring(Input.IndexOf("["));

                                    List<string> T_List = T_Vals.Split(',').ToList();
                                    string Key = "";

                                    for (int i = 0; i <= T_List.Count - 1; i++)
                                    {
                                        string TL = T_List[i];
                                        if (i == T_List.Count - 1)
                                            Key = TL.Split(':')[0].Replace("\"", "");
                                        else
                                            U_List.Add(new List<object>(TL.Replace("\"", "").Split(':')));
                                    }



                                    bool EmptyEntry = false;
                                    if (T_Rest.Contains("[],"))
                                    {
                                        T_Rest = T_Rest.Substring(T_Rest.IndexOf("[],") + 3);
                                        U_List.Add(new List<object>
                                        {
                                            Key,
                                            new List<object>()
                                        });
                                        EmptyEntry = true;
                                    }

                                    if (EmptyEntry)
                                    {
                                        List<object> T_Entrys = JSONRecursive(T_Rest);

                                        foreach (var T_Entry in T_Entrys)
                                            U_List.Add(T_Entry);
                                    }
                                    else
                                        U_List.Add(new List<object>
                                        {
                                            Key,
                                            JSONRecursive(T_Rest)
                                        });



                                    if (!(RecursiveRest.Count == 0))
                                    {
                                        foreach (var RekuRest in RecursiveRest)
                                            U_List.Add(RekuRest);
                                        RecursiveRest = new List<object>();
                                    }
                                }
                                else
                                {
                                    List<string> T_List = Input.Split(',').ToList();

                                    foreach (string TL in T_List)
                                        U_List.Add(new List<object>(TL.Replace("\"", "").Split(':')));

                                    return U_List;
                                }
                            }
                        }
                    }
                    else if (Input.Contains(","))
                        U_List.Add(Input.Replace("\"", "").Split(',').ToList());
                    else
                        U_List.Add(Input.Replace("\"", ""));
                }
                else if (Input[0] == '{')
                {
                    int Opencurly = CharCounterList(Input, "{").Count;
                    int Closecurly = CharCounterList(Input, "}").Count;

                    if (Opencurly < Closecurly)
                        Input = "{" + Input;
                    else if (Opencurly > Closecurly)
                        Input += "}";

                    List<string> T_Input = Between2List(Input, "{", "}");

                    if (T_Input.Count > 0)
                        Input = T_Input[0];

                    if (!(Input.Trim() == ""))
                    {
                        if (Input[0] == ',')
                            Input = Input.Substring(1);
                    }

                    List<object> SubList = JSONRecursive(Input);

                    foreach (var SubListItem in SubList)
                        U_List.Add(SubListItem);

                    if (!(RecursiveRest.Count == 0))
                    {
                        foreach (var RekuRest in RecursiveRest)
                            U_List.Add(RekuRest);
                        RecursiveRest = new List<object>();
                    }


                    string T_Rest = T_Input[1];

                    if (T_Rest.Contains("{},"))
                    {
                        T_Rest = T_Rest.Substring(T_Rest.IndexOf("{},") + 3);

                        List<object> RestRek = JSONRecursive(T_Rest);

                        foreach (var Rest in RestRek)
                            RecursiveRest.Add(Rest);
                    }
                }
                else if (Input[0] == '[')
                {
                    int Opensquare = CharCounterList(Input, "[").Count;
                    int Closesquare = CharCounterList(Input, "]").Count;

                    if (Opensquare < Closesquare)
                        Input = "[" + Input;
                    else if (Opensquare > Closesquare)
                        Input += "]";

                    List<string> T_Input = Between2List(Input, "[", "]");

                    if (T_Input.Count > 0)
                        Input = T_Input[0];

                    if (!(Input.Trim() == ""))
                    {
                        if (Input[0] == ',')
                            Input = Input.Substring(1);
                    }


                    List<object> SubList = JSONRecursive(Input);

                    foreach (var SubListItem in SubList)
                        U_List.Add(SubListItem);


                    if (!(RecursiveRest.Count == 0))
                    {
                        foreach (var RekuRest in RecursiveRest)
                            U_List.Add(RekuRest);
                        RecursiveRest = new List<object>();
                    }

                    string T_Rest = T_Input[1];

                    if (T_Rest.Contains("[],"))
                    {
                        T_Rest = T_Rest.Substring(T_Rest.IndexOf("[],") + 3);
                        List<object> RestRek = JSONRecursive(T_Rest);

                        foreach (var Rest in RestRek)
                            RecursiveRest.Add(Rest);
                    }
                }

                return U_List;
            }
            else
                return new List<object>();
        }
        public object RecursiveListSearch(List<object> List, string Key)
        {
            object Returner = false;

            try
            {
                foreach (object Entry in List)
                {
                    if (Entry.GetType().Name == typeof(string).Name)
                    {
                        string Entry1 = (string)Entry;

                        if (Entry1.ToLower().Trim() == Key.ToLower().Trim())
                        {
                            List<object> FindList = new List<object>();

                            for (int i = 1; i <= List.Count - 1; i++)
                                FindList.Add(List[i]);

                            if (FindList.Count > 1)
                                return FindList;
                            else
                                return FindList[0];
                        }
                    }
                    else
                    {
                        List<object> Entry1 = (List<object>)Entry;

                        Returner = RecursiveListSearch(Entry1, Key);

                        if (!(Returner.GetType().Name == typeof(bool).Name))
                            return Returner;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return Returner;
        }
        public string JSONToXML(string Input)
        {
            List<object> JSONList = JSONRecursive(Input);
            string XMLStr = JSONListToXMLRecursive(JSONList);
            return XMLStr;
        }

        public string JSONListToXMLRecursive(List<object> JSONList)
        {
            string Returner = "";

            foreach (object T_Key_Vals in JSONList)
            {
                if (T_Key_Vals.GetType().Name == typeof(string).Name)
                    Returner = (string)T_Key_Vals;
                else if (T_Key_Vals.GetType().Name == typeof(List<>).Name)
                {
                    List<object> T_List = new List<object>();

                    T_List.AddRange(T_Key_Vals as List<object>);

                    if (T_List.Count > 2)
                    {
                        if (T_List[0].GetType().Name == typeof(string).Name)
                        {
                            string T_Key = T_List[0].ToString();

                            for (int i = 0; i <= T_List.Count - 1; i++)
                            {
                                object T_Obj = T_List[i];

                                if (T_Obj.GetType().Name == typeof(string).Name)
                                    Returner += "<" + i.ToString() + ">" + T_Obj.ToString() + "</" + i.ToString() + ">";
                                else
                                    Returner += JSONListToXMLRecursive((List<object>)T_Obj);
                            }
                        }
                        else
                            Returner += JSONListToXMLRecursive((List<object>)T_List[0]);
                    }
                    else if (T_List[0].GetType().Name == typeof(string).Name)
                    {
                        string T_Key = T_List[0].ToString();
                        Returner += "<" + T_Key.Trim() + ">";

                        if (T_List.Count == 2)
                        {
                            if (T_List[1].GetType().Name == typeof(string).Name)
                                Returner += T_List[1].ToString();
                            else
                                Returner += JSONListToXMLRecursive((List<object>)T_List[1]);
                        }

                        Returner += "</" + T_Key.Trim() + ">";
                    }
                    else
                        Returner += JSONListToXMLRecursive((List<object>)T_List[0]);
                }
            }

            return Returner;
        }
        public string RecursiveXMLSearch(string Input, string Key)
        {
            if (Key.Contains('/'))
            {
                List<string> KeyPathList = new List<string>(Key.Split('/'));

                string T_Key = KeyPathList[0];
                string T_Path = Key.Substring(Key.IndexOf("/") + 1);

                if (Input.Contains("<" + T_Key + ">"))
                    return RecursiveXMLSearch(GetStringBetween(Input, "<" + T_Key + ">", "</" + T_Key + ">"), T_Path);
                else
                    return "";
            }
            else if (Input.Contains("<" + Key + ">"))
                return GetStringBetween(Input, "<" + Key + ">", "</" + Key + ">");
            else
                return "";
        }



        #region Get Betweens

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

        #endregion


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

                    

                    StartIdx =Input.IndexOf(StartChar, StartIdx); //Strings.InStr(StartIdx, Input, StartChar);

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
